using System.Text;
using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Feat
{
    public static class FeatUtility
    {
        //治疗路径折算：治愈按实际恢复量 1:1；包扎与恢复部位为间接收益，取 0.3
        public const float HealWeight = 1f;
        public const float TendWeight = 0.3f;
        public const float RestoreWeight = 0.3f;
        //无 Hediff 载体的控制效果（时停 / 眩晕）每秒折算的贡献点数
        public const float ControlPerSecond = 1f;

        //9档阈值表
        public static readonly float[] TierThresholds =
        {
            150f, 250f, 400f, 600f, 900f,
            1300f, 1900f, 2700f, 3800f
        };

        /// <summary>
        /// 档位判定
        /// </summary>
        public static int TierOf(float score)
        {
            int tier = 0;
            for (int i = 0; i < TierThresholds.Length; i++)
                if (score >= TierThresholds[i]) tier = i + 1;
            return tier;
        }

        /// <summary>
        /// 战斗力获取（根据原版PawnKindDef中的定义）
        /// </summary>
        public static float CombatPowerOf(Pawn pawn)
        {
            if (pawn?.kindDef == null) return 0;
            float power = pawn.kindDef.combatPower;
            if (power <= 0)
            {
                Log.WarningOnce($"[FamiliaMyth] Combat Power Of Pawn Kind {pawn.kindDef.defName} Not Defined Or Invalid", pawn.kindDef.defName.GetHashCode());
                return 0;
            }
            return power;
        }

        /// <summary>
        /// 结算已结束的战斗
        /// </summary>
        public static void Resolve(Battle battle, BattleAccount account, Map map)
        {
            if (battle == null) return;
            string battleId = battle.GetUniqueLoadID();

            // concerns 为 private，参与者只能从 Entries 的 GetConcerns() 重建
            HashSet<Thing> concerns = new();
            List<LogEntry> entries = battle.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                LogEntry entry = entries[i];
                if (entry == null) continue;
                foreach (Thing thing in entry.GetConcerns())
                    if (thing != null) concerns.Add(thing);
            }

            Faction player = Faction.OfPlayer;
            if (player == null) return;

            List<Pawn> enemies = new();
            List<Pawn> friendlies = new();
            List<Pawn> participants = new();

            foreach (Thing thing in concerns)
            {
                if (thing is not Pawn pawn) continue;
                Faction faction = pawn.Faction;
                if (faction == null) continue;

                if (faction == player)
                {
                    friendlies.Add(pawn);
                    if (pawn.IsColonist && pawn.GetPharna() != null)
                        participants.Add(pawn);
                }
                else if (faction.HostileTo(player))
                {
                    enemies.Add(pawn);
                }
            }

            if (participants.Count == 0 || enemies.Count == 0) return;

            //生成数据包
            FeatContext ctx = new(battle, map, battleId, enemies, friendlies, account);
            float threat = ctx.Threat();
            float totalContribution = ctx.TotalContribution();

            //战斗层判定：系数乘进战斗总得分，全队共享
            List<FeatTraitDef> traits = new();
            List<FeatTraitDef> allDefs = DefDatabase<FeatTraitDef>.AllDefsListForReading;
            float battleMagnitude = threat;

            for (int i = 0; i < allDefs.Count; i++)
            {
                FeatTraitDef def = allDefs[i];
                FeatTraitJudger judger = def?.Judger;
                if (judger == null || def.scope != FeatTraitScope.Battle) continue;

                bool hit;
                try
                {
                    hit = judger.Judge(ctx);
                }
                catch (Exception ex)
                {
                    Log.Error($"[FamiliaMyth] Error Judging FeatTraitDef {def.defName}：{ex}");
                    hit = false;
                }

                if (!hit) continue;
                traits.Add(def);
                battleMagnitude *= judger.Multiplier(ctx);
            }

            string battleName = SafeBattleName(battle);

            //战斗地图：优先取参战者所在地图（任务站点/临时地图），回落到调用方传入的 map
            Map battleMap = null;
            for (int i = 0; i < friendlies.Count && battleMap == null; i++) battleMap = friendlies[i].Map;
            for (int i = 0; i < enemies.Count && battleMap == null; i++) battleMap = enemies[i].Map;
            battleMap ??= map;

            int tileId = battleMap != null ? battleMap.Tile.tileId : -1;
            int now = Find.TickManager.TicksAbs;

            //逐参与者：个人得分 = 战斗总得分 × 占比 × 个人层系数
            List<ParticipantResult> results = new(participants.Count);
            for (int i = 0; i < participants.Count; i++)
            {
                Pawn p = participants[i];
                Hediff_Pharna pharna = p.GetPharna();
                if (pharna == null) continue;

                float share = totalContribution > 0f
                    ? ctx.ContributionOf(p) / totalContribution
                    : 1f / participants.Count;
                PersonalFeatResult personal = JudgePersonalTraits(ctx, allDefs, p);
                float score = battleMagnitude * share * personal.multiplier;
                int tier = TierOf(score);

                results.Add(new ParticipantResult
                {
                    pawn = p,
                    share = share,
                    tier = tier,
                    personal = personal,
                });

                if (tier == 0) continue;

                pharna.feats.Add(new FeatRecord
                {
                    featTraits = new List<FeatTraitDef>(traits),
                    personalTraits = personal.traits,
                    personalMultiplier = personal.multiplier,
                    tier = tier,
                    share = share,
                    battleMagnitude = battleMagnitude,
                    battleName = battleName,
                    tick = now,
                    tileId = tileId
                });
            }

            LogResolveReport(ctx, battleName, participants, results, traits, battleMagnitude, totalContribution);
        }

        //个人层判定：只对 subject 本人生效，不影响战斗总得分
        private static PersonalFeatResult JudgePersonalTraits(FeatContext ctx, List<FeatTraitDef> allDefs, Pawn subject)
        {
            List<FeatTraitDef> traits = null;
            float multiplier = 1f;

            for (int i = 0; i < allDefs.Count; i++)
            {
                FeatTraitDef def = allDefs[i];
                FeatTraitJudger judger = def?.Judger;
                if (judger == null || def.scope != FeatTraitScope.Pawn) continue;

                bool hit;
                try
                {
                    hit = judger.Judge(ctx, subject);
                }
                catch (Exception ex)
                {
                    Log.Error($"[FamiliaMyth] Error Judging Personal FeatTraitDef {def.defName}：{ex}");
                    hit = false;
                }

                if (!hit) continue;
                traits ??= new List<FeatTraitDef>();
                traits.Add(def);
                multiplier *= judger.Multiplier(ctx, subject);
            }

            return new PersonalFeatResult
            {
                traits = traits ?? new List<FeatTraitDef>(),
                multiplier = multiplier,
            };
        }

        //结算报告：输出战斗事实、性质判定与逐人贡献明细，便于核对数值
        private static void LogResolveReport(FeatContext ctx, string battleName, List<Pawn> participants,
            List<ParticipantResult> results, List<FeatTraitDef> traits, float battleMagnitude, float totalContribution)
        {
            float threat = ctx.Threat();
            float multiplier = threat > 0f ? battleMagnitude / threat : 1f;

            StringBuilder sb = new();
            sb.AppendLine($"[FamiliaMyth] 战斗结算：{battleName ?? "未知战斗"}");
            sb.AppendLine($"  战斗事实：敌方 {ctx.enemies.Count} 人｜战力合计 {threat:F1}｜最强单体 {ctx.MaxCombatPower():F1}｜已倒下或阵亡 {ctx.DefeatedCount()}"
                + $"；我方 {ctx.friendlies.Count} 人｜倒地 {ctx.DownedCount()}｜受恩惠参与者 {participants.Count}；战斗总时间：{ctx.BattleDuration().ToStringTicksToPeriod()}");
            sb.AppendLine($"  性质判定：{TraitText(traits)} → 战斗总得分 {battleMagnitude:F1}（敌方战力 {threat:F1} × 性质系数 {multiplier:F2}）");
            sb.AppendLine($"  总贡献：{totalContribution:F1} 总输出： {ctx.EnemyDamageTotal()}");
            float enemyDamage = ctx.EnemyDamageTotal();
            float friendliesDealt = ctx.FriendliesDamageTotal();
            sb.AppendLine($"  合账：敌方承受 {enemyDamage:F1}｜我方输出 {friendliesDealt:F1}"
                + $"｜差额 {enemyDamage - friendliesDealt:F1}｜Σ占比 {ShareSum(results) * 100f:F1}%");
            sb.AppendLine($"  贡献明细（输出｜承伤×{FeatContext.TakenWeight:F1}｜支援 → 合计｜占比｜个人得分｜档位）：");

            for (int i = 0; i < ctx.friendlies.Count; i++)
            {
                Pawn pawn = ctx.friendlies[i];
                if (pawn == null) continue;

                PawnContribution row = ctx.EntryOf(pawn);
                float dealt = row?.dealt ?? 0f;
                float taken = row?.taken ?? 0f;
                float support = row?.support ?? 0f;
                float total = ctx.ContributionOf(pawn);

                bool rewarded = participants.Contains(pawn);
                int index = rewarded ? results.FindIndex(r => r.pawn == pawn) : -1;
                float share = index >= 0
                    ? results[index].share
                    : (totalContribution > 0f ? total / totalContribution : 0f);
                float personalMultiplier = index >= 0 ? results[index].personal.multiplier : 1f;
                float score = battleMagnitude * share * personalMultiplier;
                int tier = index >= 0 ? results[index].tier : 0;

                string reward = rewarded
                    ? $"｜{score:F1}｜{(tier > 0 ? $"{tier} 级" : "未达档位")}"
                      + (index >= 0 ? PersonalText(results[index].personal) : "")
                    : "";

                sb.AppendLine($"    {(rewarded ? "[恩惠]" : "[    ]")} {pawn.Name.ToStringShort}（{pawn.KindLabel}）："
                    + $"{dealt:F1}｜{taken * FeatContext.TakenWeight:F1}｜{support:F1}"
                    + $" → {total:F1}｜{share.ToStringPercent()}{reward}");
            }

            Log.Message(sb.ToString());
        }

        //战斗层性质文本
        private static string TraitText(List<FeatTraitDef> traits)
            => traits.NullOrEmpty()
                ? "无"
                : string.Join("、", traits.Where(t => t != null).Select(t => $"{t.label}×{t.multiplier:F2}"));

        //参与者占比之和，超过 100% 说明分摊分母少算
        private static float ShareSum(List<ParticipantResult> results)
        {
            float sum = 0f;
            for (int i = 0; i < results.Count; i++) sum += results[i].share;
            return sum;
        }

        //个人层性质文本，附在该参与者的明细行末
        private static string PersonalText(PersonalFeatResult personal)
            => personal.traits.NullOrEmpty()
                ? ""
                : "｜个人：" + string.Join("、", personal.traits.Where(t => t != null).Select(t => $"{t.label}×{t.multiplier:F2}"));

        //原版战斗名是按需生成、且会被新日志清空的缓存，结算时取一次快照
        private static string SafeBattleName(Battle battle)
        {
            try
            {
                string name = battle.GetName();
                return name.NullOrEmpty() ? null : name;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] Failed To Get Battle Name: {ex}");
                return null;
            }
        }

        //单个参与者的结算快照，供记录与报告共用
        private struct ParticipantResult
        {
            public Pawn pawn;
            public float share;
            public int tier;
            public PersonalFeatResult personal;
        }

        //个人层判定结果：命中的性质与系数合计
        private struct PersonalFeatResult
        {
            public List<FeatTraitDef> traits;
            public float multiplier;
        }
    }
}
