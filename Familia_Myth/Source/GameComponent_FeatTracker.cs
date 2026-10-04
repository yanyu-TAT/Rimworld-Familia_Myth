using RimWorld;
using Verse;

namespace FamiliaMyth.Feat
{
    public class GameComponent_FeatTracker : GameComponent
    {
        private const int ScanIntervalTicks = 250;

        //每场战斗各参与者的贡献累计
        public List<ContributionEntry> contributions = new();
        //已结算战斗ID
        private List<string> resolved = new();

        public static GameComponent_FeatTracker Instance =>
            Current.Game?.GetComponent<GameComponent_FeatTracker>();

        public GameComponent_FeatTracker() { }

        public GameComponent_FeatTracker(Game game) { }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % ScanIntervalTicks != 0) return;
            Log.Message("Scanning Active Battles");

            List<Battle> battles = Find.BattleLog?.Battles;
            if (battles == null) return;

            //修复战斗引用
            RenormalizeContributions();

            //结算已结束且未结算的战斗
            for (int i = 0; i < battles.Count; i++)
            {
                Battle battle = battles[i];
                if (battle == null) continue;

                //合并链取末端
                while (battle.AbsorbedBy != null) battle = battle.AbsorbedBy;

                string id = battle.GetUniqueLoadID();
                if (resolved.Contains(id)) continue;
                if (!IsFinished(battle)) continue;

                Log.Message($"Resolving Battle: {battle.GetName()}");
                FeatUtility.Resolve(battle, contributions, Find.CurrentMap);
                resolved.Add(id);
                PruneContributions(id);
            }

            //清理
            PruneResolved();
            PruneOrphanContributions();
        }

        //结束判定
        private static bool IsFinished(Battle battle)
        {
            List<LogEntry> entries = battle.Entries;
            int last = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                LogEntry entry = entries[i];
                if (entry != null && entry.Timestamp > last) last = entry.Timestamp;
            }
            if (last == 0) return true;
            return Find.TickManager.TicksAbs - last > Battle.TicksForBattleExit;
        }

        //造成伤害
        public void RecordDealt(Pawn attacker, Pawn victim, float amount)
        {
            if (attacker == null || amount <= 0f) return;
            if (attacker.Faction != Faction.OfPlayer) return;                   
            if (victim != null && !victim.HostileTo(Faction.OfPlayer)) return;
            ContributionEntry entry = GetOrAdd(attacker);
            if (entry != null) entry.dealt += amount;
        }

        //承受伤害
        public void RecordTaken(Pawn victim, Pawn inflictor, float amount)
        {
            if (victim == null || inflictor == null || amount <= 0f) return;
            if (victim.Faction != Faction.OfPlayer) return;
            if (!inflictor.HostileTo(Faction.OfPlayer)) return;
            ContributionEntry entry = GetOrAdd(victim);
            if (entry != null) entry.taken += amount;
        }

        //治疗/buff/debuff
        public void RecordSupport(Pawn pawn, float amount)
        {
            if (pawn == null || amount <= 0f) return;
            ContributionEntry entry = GetOrAdd(pawn);
            if (entry != null) entry.support += amount;
        }

        private ContributionEntry GetOrAdd(Pawn pawn)
        {
            if (pawn == null) return null;
            Battle battle = pawn.records?.BattleActive;
            if (battle == null) return null;

            while (battle.AbsorbedBy != null) battle = battle.AbsorbedBy;
            string battleId = battle.GetUniqueLoadID();

            for (int i = 0; i < contributions.Count; i++)
            {
                ContributionEntry entry = contributions[i];
                if (entry != null && entry.pawn == pawn && entry.battleId == battleId)
                    return entry;
            }

            Log.Message($"Start Track {pawn.Name.ToStringShort} In Battle {battle.GetName()}");
            ContributionEntry added = new(){ battleId = battleId, pawn = pawn };
            contributions.Add(added);
            return added;
        }

        private void PruneContributions(string battleId)
        {
            for (int i = contributions.Count - 1; i >= 0; i--)
                if (contributions[i] == null || contributions[i].battleId == battleId)
                    contributions.RemoveAt(i);
        }

        //只保留当前战斗列表中的已结算ID，避免长局无限累积
        private void PruneResolved()
        {
            var battles = Find.BattleLog?.Battles;
            if (battles.NullOrEmpty()) return;
            if (resolved.Count <= battles.Count) return;

            List<string> alive = new();
            for (int i = 0; i < battles.Count; i++)
            {
                Battle battle = battles[i];
                if (battle == null) continue;
                while (battle.AbsorbedBy != null) battle = battle.AbsorbedBy;
                alive.Add(battle.GetUniqueLoadID());
            }
            resolved.RemoveAll(id => !alive.Contains(id));
        }

        //把条目重新归属到pawn当前有效的战斗，并归并同键条目。
        private void RenormalizeContributions()
        {
            if (contributions.Count == 0) return;

            List<ContributionEntry> merged = new();
            for (int i = 0; i < contributions.Count; i++)
            {
                ContributionEntry entry = contributions[i];
                if (entry == null || entry.pawn == null) continue;

                // BattleActive 为 null 时保留原 ID：该战斗正待结算（结算轮与失效轮同 tick）
                Battle current = entry.pawn.records?.BattleActive;
                if (current != null)
                {
                    string newId = current.GetUniqueLoadID();
                    if (newId != entry.battleId) entry.battleId = newId;
                }

                ContributionEntry target = FindEntry(merged, entry.battleId, entry.pawn);
                if (target == null)
                {
                    merged.Add(entry);
                }
                else
                {
                    // 同键归并：否则 ContributionOf 只返回第一条，其余战绩被静默丢弃
                    target.dealt += entry.dealt;
                    target.taken += entry.taken;
                    target.support += entry.support;
                }
            }

            contributions = merged;
        }

        private static ContributionEntry FindEntry(List<ContributionEntry> list, string battleId, Pawn pawn)
        {
            for (int i = 0; i < list.Count; i++)
            {
                ContributionEntry entry = list[i];
                if (entry != null && entry.pawn == pawn && entry.battleId == battleId) return entry;
            }
            return null;
        }

        //清理孤儿条目
        private void PruneOrphanContributions()
        {
            var battles = Find.BattleLog?.Battles;
            if (contributions.Count == 0 || battles.NullOrEmpty()) return;

            List<string> alive = new();
            for (int i = 0; i < battles.Count; i++)
            {
                Battle battle = battles[i];
                if (battle == null) continue;
                while (battle.AbsorbedBy != null) battle = battle.AbsorbedBy;
                alive.Add(battle.GetUniqueLoadID());
            }

            contributions.RemoveAll(e => e == null
                || (e.pawn == null)
                || (!alive.Contains(e.battleId)
                    && !resolved.Contains(e.battleId)
                    && e.pawn.records?.BattleActive == null));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref contributions, "contributions", LookMode.Deep);
            Scribe_Collections.Look(ref resolved, "resolved", LookMode.Value);

            if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
            contributions ??= new List<ContributionEntry>();
            resolved ??= new List<string>();
        }
    }

    // 单场战斗中某个参与者的贡献累计
    public class ContributionEntry : IExposable
    {
        public string battleId;
        public Pawn pawn;
        public float dealt;      //造成伤害
        public float taken;      //承受伤害
        public float support;    //支援

        public void ExposeData()
        {
            Scribe_Values.Look(ref battleId, "battleId");
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref dealt, "dealt", 0f);
            Scribe_Values.Look(ref taken, "taken", 0f);
            Scribe_Values.Look(ref support, "support", 0f);
        }
    }
}
