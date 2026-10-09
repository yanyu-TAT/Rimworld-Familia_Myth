using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 发展能力数据结构基类
    /// </summary>
    public abstract class DevelopableAbility : IExposable, IAscendReward
    {
        public DevelopableAbilityDef def;

        protected float proficiency; //熟练度
        public float Proficiency => proficiency;

        protected static readonly List<string> ranks = new();

        public RewardType RewardType => RewardType.DevelopableAbility;

        public RewardRarity Rarity => def.rarity;

        public string DefName => def?.defName;

        public string Label => def.label;

        public string Description => def.description;

        protected Hediff_Pharna pharna;

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

        protected TraitDef traitDef;
        protected TraitDef TraitDef =>
            traitDef ??= DefDatabase<TraitDef>.GetNamed(def.traitDefName);

        /// <summary>
        /// 将 proficiency 映射到 Trait degree
        /// </summary>
        public int Degree => Mathf.Min(((int)proficiency) / 100, 11);

        /// <summary>
        /// 创建对应的 Trait 实例
        /// </summary>
        public Trait ToTrait => new(TraitDef, Degree, forced: true);

        /// <summary>
        /// 对指定的数据进行修正
        /// </summary>
        /// <param name="pharna">生效中的恩惠</param>
        /// <param name="stat">修正值的类型def</param>
        /// <param name="value">修正值的引用</param>
        /// <returns>修正成功/失败</returns>
        public abstract bool ApplyModifiers(StatDef stat, ref float value);

        /// <summary>
        /// 获取数据修正中的描述，例:"魔导I12:+0.55"
        /// </summary>
        /// <param name="pharna">生效中的恩惠</param>
        /// <param name="stat">修正值的类型def</param>
        /// <returns>输出的描述</returns>
        public abstract string GetExplanation(StatDef stat);

        protected string Rank => ranks[Degree];

        public virtual bool GainProficiency(float amout)
        {
            var prevDegree = Degree;
            proficiency += amout;
            //升级
            if (Degree !=  prevDegree)
            {
                if (pharna.pawn?.story?.traits?.HasTrait(TraitDef) ?? false)
                {
                    var trait = pharna.pawn.story.traits.GetTrait(TraitDef);
                    pharna.pawn.story.traits.RemoveTrait(trait);
                }
                pharna.pawn.story.traits.GainTrait(ToTrait);
            }
            return true;
        }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref proficiency, "proficiency", 0f);
            Scribe_References.Look(ref pharna, "pharna");
        }

        public virtual void Apply(Hediff_Pharna p)
        {
            pharna = p;
            pharna.developableAbilities.Add(this);

            if (!pharna.pawn?.story?.traits?.HasTrait(TraitDef) ?? false)
            {
                var temp = ToTrait;
                pharna.pawn.story.traits.GainTrait(temp);
            }
        }

        public virtual void Tick() { }

        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            return null;
        }
    }
}