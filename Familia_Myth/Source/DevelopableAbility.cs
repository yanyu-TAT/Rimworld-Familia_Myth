using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 发展能力数据结构基类
    /// </summary>
    public abstract class DevelopableAbility : IExposable, IAscendReward
    {
        public DevelopableAbilityDef def;
        public float proficiency; //熟练度
        protected static readonly List<string> ranks = new();

        public RewardType RewardType => RewardType.DevelopableAbility;

        public RewardRarity Rarity => def.rarity;

        public string Label => def.label;

        public string Description => def.description;

        public void RewardInitialize()
        {
            def = DefDatabase<DevelopableAbilityDef>.GetNamed(GetType().Name, false);
        }

        static DevelopableAbility()
        {
            for (char c = 'I'; c >= 'A'; c--)
            {
                ranks.Add(c.ToString());
            }
            ranks.Add("S");
            ranks.Add("SS");
            ranks.Add("SSS");
        }

        /// <summary>
        /// 对指定的数据进行修正
        /// </summary>
        /// <param name="pharna">生效中的恩惠</param>
        /// <param name="stat">修正值的类型def</param>
        /// <param name="value">修正值的引用</param>
        /// <returns>修正成功/失败</returns>
        public abstract bool ApplyModifiers(Hediff_Pharna pharna, StatDef stat, ref float value);

        /// <summary>
        /// 获取数据修正中的描述，例:"魔导I12:+0.55"
        /// </summary>
        /// <param name="pharna">生效中的恩惠</param>
        /// <param name="stat">修正值的类型def</param>
        /// <returns>输出的描述</returns>
        public abstract string GetExplanation(Hediff_Pharna pharna, StatDef stat);

        protected static string GetRank(float value)
        {
            int t = ((int)value) / 100;
            t = t >= 12 ? 11 : t;
            return ranks[t];
        }

        public virtual bool GainProficiency(float amout)
        {
            proficiency += amout;
            return true;
        }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref proficiency, "proficiency", 0f);
        }

        public void Apply(Hediff_Pharna pharna)
        {
            pharna.developableAbilities.Add(this);
        }
    }
}