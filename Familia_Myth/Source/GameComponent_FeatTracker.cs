using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Feat
{
    public class GameComponent_FeatTracker : GameComponent
    {
        private const int ScanIntervalTicks = 250;

        //每场战斗一本账
        public List<BattleAccount> accounts = new();
        //已结算战斗ID
        private List<string> resolved = new();

        public static GameComponent_FeatTracker Instance =>
            Current.Game?.GetComponent<GameComponent_FeatTracker>();

        public GameComponent_FeatTracker() { }

        public GameComponent_FeatTracker(Game game) { }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % ScanIntervalTicks != 0) return;

            List<Battle> battles = Find.BattleLog?.Battles;
            if (battles == null) return;

            //被吸收的战斗曾写过账，先把账本并进它现在归属的战斗
            MigrateAbsorbed();

            for (int i = 0; i < battles.Count; i++)
            {
                Battle battle = battles[i];
                if (battle == null) continue;

                //合并链取末端
                while (battle.AbsorbedBy != null) battle = battle.AbsorbedBy;

                string id = battle.GetUniqueLoadID();
                if (resolved.Contains(id)) continue;
                if (!IsFinished(battle)) continue;

                FeatUtility.Resolve(battle, AccountById(id), Find.CurrentMap);
                resolved.Add(id);
                DropAccount(id);
            }

            PruneResolved();
            PruneOrphanAccounts();
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

        //一次伤害的三条路径共用一次归属解析
        public void RecordDamage(Pawn attacker, Pawn victim, float amount)
        {
            if (victim == null || amount <= 0f) return;
            Faction player = Faction.OfPlayer;
            if (player == null) return;

            //敌对判据一律走 Faction.HostileTo：GenHostility.HostileTo 对 Destroyed 的对象直接返回 false，
            //而致死一击的受害者在本回调里已被销毁，用它会漏记那一击
            bool enemyHit = victim.Faction != null && victim.Faction.HostileTo(player);
            bool dealt = attacker != null && attacker != victim
                && attacker.Faction == player && enemyHit;
            bool taken = victim.Faction == player
                && attacker != null && attacker.Faction != null && attacker.Faction.HostileTo(player);
            if (!dealt && !taken && !enemyHit) return;

            BattleAccount account = AccountFor(attacker, victim);
            if (account == null) return;

            int now = Find.TickManager.TicksAbs;
            account.Touch(now);
            if (enemyHit) account.enemyDamage += amount;
            if (dealt) account.AddDealt(attacker, amount);
            if (taken) account.AddTaken(victim, amount);
        }

        //治疗/buff/debuff
        public void RecordSupport(Pawn pawn, float amount)
        {
            if (pawn == null || amount <= 0f) return;
            if (pawn.Faction != Faction.OfPlayer) return;

            BattleAccount account = AccountFor(pawn, pawn);
            if (account == null) return;

            account.Touch(Find.TickManager.TicksAbs);
            account.AddSupport(pawn, amount);
        }

        //归属优先取攻击者（我方出手者必已在战斗中），回落受击者
        private BattleAccount AccountFor(Pawn attacker, Pawn victim)
        {
            Battle battle = attacker?.records?.BattleActive ?? victim?.records?.BattleActive;
            if (battle == null) return null;

            while (battle.AbsorbedBy != null) battle = battle.AbsorbedBy;
            return GetOrAddAccount(battle);
        }

        private BattleAccount GetOrAddAccount(Battle battle)
        {
            string battleId = battle.GetUniqueLoadID();
            BattleAccount account = AccountById(battleId);
            if (account != null) return account;
            account = new BattleAccount(battleId, Find.TickManager.TicksAbs);
            accounts.Add(account);
            return account;
        }

        private BattleAccount AccountById(string battleId)
        {
            for (int i = 0; i < accounts.Count; i++)
                if (accounts[i] != null && accounts[i].battleId == battleId) return accounts[i];
            return null;
        }

        //把已被吸收的战斗的账本并进它现在归属的战斗
        private void MigrateAbsorbed()
        {
            if (accounts.Count <= 1) return;

            for (int i = accounts.Count - 1; i >= 0; i--)
            {
                BattleAccount account = accounts[i];
                if (account == null) { accounts.RemoveAt(i); continue; }

                //全员都已过期时保留原 id，等结算精确匹配
                Battle current = account.CurrentBattle();
                if (current == null) continue;

                while (current.AbsorbedBy != null) current = current.AbsorbedBy;
                string newId = current.GetUniqueLoadID();
                if (newId == account.battleId) continue;

                BattleAccount target = AccountById(newId);
                if (target == null) account.battleId = newId;
                else
                {
                    target.Merge(account);
                    accounts.RemoveAt(i);
                }
            }
        }

        private void DropAccount(string battleId)
            => accounts.RemoveAll(a => a == null || a.battleId == battleId);

        //只保留当前战斗列表中的已结算ID，避免长局无限累积
        private void PruneResolved()
        {
            List<Battle> battles = Find.BattleLog?.Battles;
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

        //清理孤儿账本：战斗在结算前就被 BattleLog 挤出列表，且账本内无人还挂在战斗上
        private void PruneOrphanAccounts()
        {
            List<Battle> battles = Find.BattleLog?.Battles;
            if (accounts.Count == 0 || battles.NullOrEmpty()) return;

            List<string> alive = new();
            for (int i = 0; i < battles.Count; i++)
            {
                Battle battle = battles[i];
                if (battle == null) continue;
                while (battle.AbsorbedBy != null) battle = battle.AbsorbedBy;
                alive.Add(battle.GetUniqueLoadID());
            }

            accounts.RemoveAll(a => a == null
                || (!alive.Contains(a.battleId)
                    && !resolved.Contains(a.battleId)
                    && a.CurrentBattle() == null));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref accounts, "accounts", LookMode.Deep);
            Scribe_Collections.Look(ref resolved, "resolved", LookMode.Value);

            if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
            accounts ??= new List<BattleAccount>();
            resolved ??= new List<string>();
        }
    }

    // 一场战斗一本账：分摊分母挂在账本上，Pawn 只是账本里的一行
    public class BattleAccount : IExposable
    {
        public string battleId;
        //敌方承受的全部伤害（我方 Pawn / 炮塔 / 商队 / 陷阱 / 环境均计入）
        public float enemyDamage;
        //账本内首个 / 最后一个贡献事件的时点
        public int firstTick;
        public int lastTick;
        public Dictionary<Pawn, PawnContribution> pawns = new();

        //Scribe_Collections 的字典重载要求 key / value 的 work list 跨阶段存活，故必须是字段
        private List<Pawn> keysWorkList;
        private List<PawnContribution> valuesWorkList;

        public BattleAccount() { }

        public BattleAccount(string battleId, int now)
        {
            this.battleId = battleId;
            firstTick = now;
            lastTick = now;
        }

        // ── 记账 ──

        public void Touch(int now)
        {
            firstTick = Mathf.Min(firstTick, now);
            lastTick = Mathf.Max(lastTick, now);
        }

        public void AddDealt(Pawn pawn, float amount) => Row(pawn).dealt += amount;

        public void AddTaken(Pawn pawn, float amount) => Row(pawn).taken += amount;

        public void AddSupport(Pawn pawn, float amount) => Row(pawn).support += amount;

        // ── 查询 ──

        public PawnContribution Find(Pawn pawn)
        {
            if (pawn == null) return null;
            return pawns.TryGetValue(pawn, out PawnContribution row) ? row : null;
        }

        //账本内任一成员仍在战斗中，就能指出这本账现在的归属
        public Battle CurrentBattle()
        {
            foreach (KeyValuePair<Pawn, PawnContribution> pair in pawns)
            {
                Battle battle = pair.Key?.records?.BattleActive;
                if (battle != null) return battle;
            }
            return null;
        }

        // ── 归并 ──

        public void Merge(BattleAccount other)
        {
            if (other == null || other == this) return;

            enemyDamage += other.enemyDamage;
            firstTick = Mathf.Min(firstTick, other.firstTick);
            lastTick = Mathf.Max(lastTick, other.lastTick);

            foreach (KeyValuePair<Pawn, PawnContribution> pair in other.pawns)
            {
                if (pair.Key == null) continue;
                if (pawns.TryGetValue(pair.Key, out PawnContribution existing))
                {
                    existing.dealt += pair.Value.dealt;
                    existing.taken += pair.Value.taken;
                    existing.support += pair.Value.support;
                }
                else pawns.Add(pair.Key, pair.Value);
            }
        }

        //调用方保证 pawn 非 null（三条 Add* 都在外层判过空）
        private PawnContribution Row(Pawn pawn)
        {
            if (!pawns.TryGetValue(pawn, out PawnContribution row))
            {
                row = new PawnContribution();
                Log.Message($"[Familia Myth] Start Tracking {pawn.Name?.ToStringShort ?? pawn.Label} In Battle {pawn.records?.BattleActive?.GetName() ?? "Unknown Battle"}");
                pawns.Add(pawn, row);
            }
            return row;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref battleId, "battleId");
            Scribe_Values.Look(ref enemyDamage, "enemyDamage", 0f);
            Scribe_Values.Look(ref firstTick, "firstTick", 0);
            Scribe_Values.Look(ref lastTick, "lastTick", 0);
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference, LookMode.Deep,
                ref keysWorkList, ref valuesWorkList);

            if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
            pawns ??= new Dictionary<Pawn, PawnContribution>();
        }
    }

    // 账本里的一行：某个我方 Pawn 在这场战斗里的贡献；Pawn 由字典 key 承担
    public class PawnContribution : IExposable
    {
        public float dealt;
        public float taken;
        public float support;

        public void ExposeData()
        {
            Scribe_Values.Look(ref dealt, "dealt", 0f);
            Scribe_Values.Look(ref taken, "taken", 0f);
            Scribe_Values.Look(ref support, "support", 0f);
        }
    }
}
