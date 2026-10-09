using RimWorld;
using Verse;

namespace FamiliaMyth.Feat
{
    public class FeatContext
    {
        public readonly Battle battle;
        public readonly Map map;
        public readonly string battleId;

        //全部敌对 Pawn（含已倒下/已死亡者——倒地同样是战果）
        public readonly List<Pawn> enemies;
        //全部我方参战 Pawn（含无恩惠者、盟友、动物）
        public readonly List<Pawn> friendlies;

        private readonly List<ContributionEntry> contributions;

        // ── 贡献路径权重：每条路径独立系数，累加得个人贡献 ──
        // 各路径量纲不同（造成伤害 / 承受伤害 / 治疗辅助），系数同时承担"权重"与"单位换算"
        public const float DamageWeight = 1f;    // 造成伤害
        public const float TakenWeight = 0.5f;   // 承受伤害（扛线）
        public const float SupportWeight = 1f;   // 治疗 / buff / debuff

        public FeatContext(Battle battle, Map map, string battleId,
            List<Pawn> enemies, List<Pawn> friendlies,
            List<ContributionEntry> contributions)
        {
            this.battle = battle;
            this.map = map;
            this.battleId = battleId;
            this.enemies = enemies;
            this.friendlies = friendlies;
            this.contributions = contributions;
        }

        // ── 派生量（按需调用；判定器若重复使用，自取局部变量缓存） ──

        // 敌方战力合计
        public float Threat()
        {
            float sum = 0f;
            if (enemies == null) return sum;
            for (int i = 0; i < enemies.Count; i++) sum += FeatUtility.CombatPowerOf(enemies[i]);
            return sum;
        }

        // 敌方最强单体战力
        public float MaxCombatPower()
        {
            float max = 0f;
            if (enemies == null) return max;
            for (int i = 0; i < enemies.Count; i++)
            {
                float power = FeatUtility.CombatPowerOf(enemies[i]);
                if (power > max) max = power;
            }
            return max;
        }

        // 敌方倒下或死亡数
        public int DefeatedCount()
        {
            int count = 0;
            if (enemies == null) return count;
            for (int i = 0; i < enemies.Count; i++)
                if (enemies[i] != null && (enemies[i].Dead || enemies[i].Downed)) count++;
            return count;
        }

        // 我方倒地数（按全部参战者计，非仅受恩惠者）
        public int DownedCount()
        {
            int count = 0;
            if (friendlies == null) return count;
            for (int i = 0; i < friendlies.Count; i++)
                if (friendlies[i] != null && friendlies[i].Downed) count++;
            return count;
        }

        // 个人贡献 = 输出 + 承伤折算 + 支援
        public float ContributionOf(Pawn pawn)
        {
            ContributionEntry entry = EntryOf(pawn);
            if (entry == null) return 0f;
            return entry.dealt * DamageWeight
                 + entry.taken * TakenWeight
                 + entry.support * SupportWeight;
        }

        // 分摊分母：敌方承受的全部伤害 + 我方承伤折算 + 我方支援
        // 我方 Pawn 打出的伤害已计入敌方受损，此处分母不再重复计入 dealt
        public float TotalContribution()
        {
            float sum = EnemyDamageTotal() * DamageWeight;
            if (friendlies == null) return sum;
            for (int i = 0; i < friendlies.Count; i++)
            {
                ContributionEntry entry = EntryOf(friendlies[i]);
                if (entry == null) continue;
                sum += entry.taken * TakenWeight
                     + entry.support * SupportWeight;
            }
            return sum;
        }

        // 敌方承受的总伤害（不分来源：我方、炮塔、商队、环境均计入）
        public float EnemyDamageTotal()
        {
            float sum = 0f;
            if (enemies == null) return sum;
            for (int i = 0; i < enemies.Count; i++)
            {
                ContributionEntry entry = EntryOf(enemies[i]);
                if (entry != null) sum += entry.enemyTaken;
            }
            return sum;
        }

        internal ContributionEntry EntryOf(Pawn pawn)
        {
            if (pawn == null || contributions == null) return null;
            for (int i = 0; i < contributions.Count; i++)
            {
                ContributionEntry entry = contributions[i];
                if (entry != null && entry.pawn == pawn && entry.battleId == battleId) return entry;
            }
            return null;
        }
    }

    public class FeatTraitDef : Def
    {
        public float multiplier = 1.0f;
        public Type judgerClass;
        public int displayOrder;

        private FeatTraitJudger judger;
        public FeatTraitJudger Judger => judger ??= FeatTraitJudger.Create(this);

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var s in base.ConfigErrors())
                yield return s;

            if (Judger == null)
                yield return $"Invalid Judger For Feat Trait {defName}";

            if (multiplier <= 0)
                yield return $"Non-positive Multiplier For Feat Trait {defName}";
        }
    }

    public abstract class FeatTraitJudger
    {
        public FeatTraitDef def;
        public abstract bool Judge(FeatContext ctx);
        public virtual float Multiplier(FeatContext ctx) => def?.multiplier ?? 1f;

        public static FeatTraitJudger Create(FeatTraitDef def)
        {
            if (def?.judgerClass == null)
            {
                Log.Error($"[FamiliaMyth] FeatTraitDef {(def == null ? "??" : def.defName)} Has No Valid judgerClass.");
                return null;
            }

            if (Activator.CreateInstance(def.judgerClass) is not FeatTraitJudger judger)
            {
                Log.Error($"[FamiliaMyth] FeatTraitDef {def.defName} 的 judgerClass 未派生自 FeatTraitJudger。");
                return null;
            }

            judger.def = def;
            return judger;
        }
    }

    public class FeatRecord : IExposable
    {
        public List<FeatTraitDef> featTraits;
        public int tier;                  //档位
        public float share;               //个人贡献占比
        public float battleMagnitude;     //战斗层面总分
        public string battleName;         //战斗名（结算时快照）
        public int tick;                  //发生时点（TicksAbs）
        public int tileId = -1;           //发生地点（PlanetTile.tileId）
        public int usedForLevel = -1;     //已用于哪一级升华；-1表示未使用

        public void ExposeData()
        {
            Scribe_Collections.Look(ref featTraits, "featTraits", LookMode.Def);
            Scribe_Values.Look(ref tier, "tier", 0);
            Scribe_Values.Look(ref share, "share", 0f);
            Scribe_Values.Look(ref battleMagnitude, "battleMagnitude", 0f);
            Scribe_Values.Look(ref battleName, "battleName");
            Scribe_Values.Look(ref tick, "tick", 0);
            Scribe_Values.Look(ref tileId, "tileId", -1);
            Scribe_Values.Look(ref usedForLevel, "usedForLevel", -1);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && featTraits == null)
                featTraits = new();
        }
    }
}
