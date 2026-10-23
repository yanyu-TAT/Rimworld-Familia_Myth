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

            FeatBattleReport report = BuildReport(ctx, battleName, tileId, now, participants, results,
                traits, battleMagnitude, totalContribution);
            report.WriteLog();
            report.SendLetter();
        }

        //把结算结果整理成一份快照，日志与信件共用
        private static FeatBattleReport BuildReport(FeatContext ctx, string battleName, int tileId, int tickAbs,
            List<Pawn> participants, List<ParticipantResult> results,
            List<FeatTraitDef> traits, float battleMagnitude, float totalContribution)
        {
            float threat = ctx.Threat();

            int friendlyLost = 0;
            for (int i = 0; i < ctx.friendlies.Count; i++)
            {
                Pawn pawn = ctx.friendlies[i];
                if (pawn != null && (pawn.Dead || pawn.Downed)) friendlyLost++;
            }

            FeatBattleReport report = new()
            {
                battleName = battleName,
                tickAbs = tickAbs,
                tileId = tileId,
                enemyCount = ctx.enemies.Count,
                threat = threat,
                maxCombatPower = ctx.MaxCombatPower(),
                defeatedCount = ctx.DefeatedCount(),
                friendlyCount = ctx.friendlies.Count,
                downedCount = ctx.DownedCount(),
                friendlyLostCount = friendlyLost,
                participantCount = participants.Count,
                battleDuration = ctx.BattleDuration(),
                battleTraits = traits,
                battleMagnitude = battleMagnitude,
                battleMultiplier = threat > 0f ? battleMagnitude / threat : 1f,
                totalContribution = totalContribution,
                enemyDamageTotal = ctx.EnemyDamageTotal(),
                friendliesDamageTotal = ctx.FriendliesDamageTotal(),
                rows = new List<FeatReportRow>(ctx.friendlies.Count),
                participants = participants,
            };

            float shareSum = 0f;
            for (int i = 0; i < ctx.friendlies.Count; i++)
            {
                Pawn pawn = ctx.friendlies[i];
                if (pawn == null) continue;

                PawnContribution entry = ctx.EntryOf(pawn);
                float contribution = ctx.ContributionOf(pawn);

                bool rewarded = participants.Contains(pawn);
                int index = rewarded ? results.FindIndex(r => r.pawn == pawn) : -1;
                float share = index >= 0
                    ? results[index].share
                    : (totalContribution > 0f ? contribution / totalContribution : 0f);
                float personalMultiplier = index >= 0 ? results[index].personal.multiplier : 1f;

                report.rows.Add(new FeatReportRow
                {
                    pawn = pawn,
                    dealt = entry?.dealt ?? 0f,
                    taken = entry?.taken ?? 0f,
                    support = entry?.support ?? 0f,
                    contribution = contribution,
                    share = share,
                    score = battleMagnitude * share * personalMultiplier,
                    tier = index >= 0 ? results[index].tier : 0,
                    rewarded = rewarded,
                    personalTraits = index >= 0 ? results[index].personal.traits : null,
                });

                if (index >= 0) shareSum += share;
            }

            report.shareSum = shareSum;
            return report;
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
