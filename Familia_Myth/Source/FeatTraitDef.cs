using RimWorld;
using UnityEngine;
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
        //全部玩家派系参战 Pawn
        public readonly List<Pawn> friendlies;

        private readonly BattleAccount account;

        // ── 贡献路径权重：每条路径独立系数，累加得个人贡献 ──
        // 各路径量纲不同（造成伤害 / 承受伤害 / 治疗辅助），系数同时承担"权重"与"单位换算"
        public const float DamageWeight = 1f;    //造成伤害
        public const float TakenWeight = 0.5f;   //承受伤害（扛线）
        public const float SupportWeight = 1f;   //治疗 / buff / debuff

        //结算期内的纯派生量只算一次
        private float? threat;
        private float? maxCombatPower;
        private int? defeatedCount;
        private int? downedCount;
        private float? totalContribution;

        public FeatContext(Battle battle, Map map, string battleId,
            List<Pawn> enemies, List<Pawn> friendlies,
            BattleAccount account)
        {
            this.battle = battle;
            this.map = map;
            this.battleId = battleId;
            this.enemies = enemies;
            this.friendlies = friendlies;
            this.account = account;
        }

        // ── 派生量 ──

        // 敌方战力合计
        public float Threat() => threat ??= ComputeThreat();

        // 敌方最强单体战力
        public float MaxCombatPower() => maxCombatPower ??= ComputeMaxCombatPower();

        // 敌方倒下或死亡数
        public int DefeatedCount() => defeatedCount ??= ComputeDefeatedCount();

        // 我方倒地数（按全部参战者计，非仅受恩惠者）
        public int DownedCount() => downedCount ??= ComputeDownedCount();

        // 敌方承受的总伤害（不分来源：我方、炮塔、商队、环境均计入）
        public float EnemyDamageTotal() => account?.enemyDamage ?? 0f;

        // 分摊分母：敌方承受的全部伤害 + 我方承伤折算 + 我方支援
        // 我方 Pawn 打出的伤害已计入敌方受损，此处分母不再重复计入 dealt
        public float TotalContribution() => totalContribution ??= ComputeTotalContribution();

        // 个人贡献 = 输出 + 承伤折算 + 支援
        public float ContributionOf(Pawn pawn)
        {
            PawnContribution row = EntryOf(pawn);
            if (row == null) return 0f;
            return row.dealt * DamageWeight
                 + row.taken * TakenWeight
                 + row.support * SupportWeight;
        }

        public PawnContribution EntryOf(Pawn pawn) => account?.Find(pawn);

        private float ComputeThreat()
        {
            float sum = 0f;
            if (enemies == null) return sum;
            for (int i = 0; i < enemies.Count; i++) 
                sum += FeatUtility.CombatPowerOf(enemies[i]);
            return sum;
        }

        private float ComputeMaxCombatPower()
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

        private int ComputeDefeatedCount()
        {
            int count = 0;
            if (enemies == null) return count;
            for (int i = 0; i < enemies.Count; i++)
                if (enemies[i] != null && (enemies[i].Dead || enemies[i].Downed)) count++;
            return count;
        }

        private int ComputeDownedCount()
        {
            int count = 0;
            if (friendlies == null) return count;
            for (int i = 0; i < friendlies.Count; i++)
                if (friendlies[i] != null && friendlies[i].Downed) count++;
            return count;
        }

        private float ComputeTotalContribution()
        {
            float sum = EnemyDamageTotal() * DamageWeight;
            if (friendlies == null) return sum;
            for (int i = 0; i < friendlies.Count; i++)
            {
                PawnContribution row = EntryOf(friendlies[i]);
                if (row == null) continue;
                sum += row.taken * TakenWeight
                     + row.support * SupportWeight;
            }
            return sum;
        }

        private float? friendliesDamage;
        //玩家派系造成的总伤害
        public float FriendliesDamageTotal()
        {
            if (friendliesDamage.HasValue) return friendliesDamage.Value;
            if (friendlies.NullOrEmpty()) 
            {
                friendliesDamage = 0;
                return friendliesDamage.Value; 
            }

            float sum = 0f;
            for (int i = 0; i < friendlies.Count; i++)
            {
                PawnContribution row = EntryOf(friendlies[i]);
                if (row != null) sum += row.dealt;
            }
            friendliesDamage = sum;
            return sum;
        }

        private int? battleDuration;
        //无账本时返回 int.MaxValue，语义为「无法衡量」
        public int BattleDuration()
        {
            if (battleDuration.HasValue) return battleDuration.Value;
            if (account == null) 
            {
                battleDuration = int.MaxValue;
                return battleDuration.Value; 
            }

            battleDuration = account.lastTick - account.firstTick;
            return battleDuration.Value;
        }
    }

    //伟业性质的作用域：Battle 乘进战斗总得分，Pawn 只乘该参与者的个人得分
    public enum FeatTraitScope
    {
        Battle,
        Pawn,
    }

    public class FeatTraitDef : Def
    {
        public float multiplier = 1.0f;
        public Type judgerClass;
        public FeatTraitScope scope = FeatTraitScope.Battle;
        public int displayOrder;

        private FeatTraitJudger judger;
        public FeatTraitJudger Judger => judger ??= FeatTraitJudger.Create(this);

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var s in base.ConfigErrors())
                yield return s;

            FeatTraitJudger instance = Judger;
            if (instance == null)
            {
                yield return $"Invalid Judger For Feat Trait {defName}";
                yield break;
            }

            if (multiplier <= 0)
                yield return $"Non-positive Multiplier For Feat Trait {defName}";

            //作用域与判定入口不匹配会让该性质静默永不命中
            Type judgerType = instance.GetType();
            if (scope == FeatTraitScope.Battle
                && judgerType.GetMethod(nameof(FeatTraitJudger.Judge), new[] { typeof(FeatContext) })
                    ?.DeclaringType == typeof(FeatTraitJudger))
            {
                yield return $"Feat Trait {defName} Declares Battle Scope But Does Not Override Judge(FeatContext)";
            }

            if (scope == FeatTraitScope.Pawn
                && judgerType.GetMethod(nameof(FeatTraitJudger.Judge), new[] { typeof(FeatContext), typeof(Pawn) })
                    ?.DeclaringType == typeof(FeatTraitJudger))
            {
                yield return $"Feat Trait {defName} Declares Pawn Scope But Does Not Override Judge(FeatContext, Pawn)";
            }
        }
    }

    public abstract class FeatTraitJudger
    {
        public FeatTraitDef def;

        //作用域以 def 声明为准
        public FeatTraitScope Scope => def?.scope ?? FeatTraitScope.Battle;

        //战斗层入口：subject 恒为 null
        public virtual bool Judge(FeatContext ctx) => Judge(ctx, null);

        //个人层入口：subject 必为非 null 的受恩惠参与者
        public virtual bool Judge(FeatContext ctx, Pawn subject) => false;

        public virtual float Multiplier(FeatContext ctx) => Multiplier(ctx, null);

        public virtual float Multiplier(FeatContext ctx, Pawn subject) => def?.multiplier ?? 1f;

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
        public List<FeatTraitDef> featTraits;      //战斗层性质
        public List<FeatTraitDef> personalTraits;  //个人层性质
        public float personalMultiplier = 1f;      //个人层系数合计
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
            Scribe_Collections.Look(ref personalTraits, "personalTraits", LookMode.Def);
            Scribe_Values.Look(ref personalMultiplier, "personalMultiplier", 1f);
            Scribe_Values.Look(ref tier, "tier", 0);
            Scribe_Values.Look(ref share, "share", 0f);
            Scribe_Values.Look(ref battleMagnitude, "battleMagnitude", 0f);
            Scribe_Values.Look(ref battleName, "battleName");
            Scribe_Values.Look(ref tick, "tick", 0);
            Scribe_Values.Look(ref tileId, "tileId", -1);
            Scribe_Values.Look(ref usedForLevel, "usedForLevel", -1);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                featTraits ??= new();
                personalTraits ??= new();
            }
        }
    }
}
