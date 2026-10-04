using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Feat
{
    public static class FeatUtility
    {
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
        public static void Resolve(Battle battle, List<ContributionEntry> contributions, Map map)
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
            FeatContext ctx = new(battle, map, battleId, enemies, friendlies, contributions);
            float threat = ctx.Threat();
            float totalContribution = ctx.TotalContribution();

            //判定额外乘数
            List<FeatTraitDef> traits = new();
            List<FeatTraitDef> allDefs = DefDatabase<FeatTraitDef>.AllDefsListForReading;
            float battleMagnitude = threat;

            for (int i = 0; i < allDefs.Count; i++)
            {
                var def = allDefs[i];
                var judger = def?.Judger;
                if (judger == null) continue;

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

            Log.Message($"Resolve Done, Result: {battleMagnitude:F3}");
            if (battleMagnitude <= 0f) return;

            //战斗地图：优先取参战者所在地图（任务站点/临时地图），回落到调用方传入的 map
            Map battleMap = null;
            for (int i = 0; i < friendlies.Count && battleMap == null; i++) battleMap = friendlies[i].Map;
            for (int i = 0; i < enemies.Count && battleMap == null; i++) battleMap = enemies[i].Map;
            battleMap ??= map;

            int tileId = battleMap != null ? battleMap.Tile.tileId : -1;
            int now = Find.TickManager.TicksAbs;

            for (int i = 0; i < participants.Count; i++)
            {
                Pawn p = participants[i];
                Hediff_Pharna pharna = p.GetPharna();
                if (pharna == null) continue;

                float share = totalContribution > 0f
                    ? ctx.ContributionOf(p) / totalContribution
                    : 1f / friendlies.Count;
                float score = battleMagnitude * share;

                int tier = TierOf(score);
                Log.Message($"Feat Recorded For {p.Name.ToStringShort}: score:{score},tier:{tier}");
                if (tier == 0) continue;

                pharna.feats.Add(new FeatRecord
                {
                    featTraits = new List<FeatTraitDef>(traits),
                    tier = tier,
                    share = share,
                    battleMagnitude = battleMagnitude,
                    tick = now,
                    tileId = tileId
                });
            }
        }
    }
}
