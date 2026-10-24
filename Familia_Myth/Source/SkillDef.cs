using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class SkillDef : Def, ICombatLogSource
    {
        public Type skillClass;

        public RewardRarity rarity;
        public SkillType skillType;

        public bool needTarget = false;

        public float cooldown = 0f;
        public float warmup = 0f;
        public float duration = 0f;
        public float checkInterval = 60f;
        public int maxCharge = 1;

        public string iconPath;

        /// <summary>
        /// 伤害日志用的规则包。为空时用默认包。
        /// </summary>
        public RulePackDef combatLogRule;

        /// <summary>
        /// 伤害日志里显示的名字。为空时用 label。
        /// </summary>
        public string combatLogLabel;

        private static readonly Color LogLabelColor = new(0.6f, 0.85f, 1f);

        [Unsaved(false)]
        private Texture2D icon;

        public Texture2D LogIcon => icon;
        public RulePackDef CastRule => FamiliaMythRulePackDefOf.FamiliaMyth_CastSkill;
        public RulePackDef HitRule => combatLogRule ?? FamiliaMythRulePackDefOf.FamiliaMyth_HitBySkill;
        public string LogLabel => (combatLogLabel.NullOrEmpty() ? label : combatLogLabel).Colorize(LogLabelColor);

        public override void PostLoad()
        {
            base.PostLoad();
            if (iconPath.NullOrEmpty()) return;
            LongEventHandler.ExecuteWhenFinished(() => icon = ContentFinder<Texture2D>.Get(iconPath, false));
        }

        public Skill CreateSkill()
        {
            var skill = (Skill)Activator.CreateInstance(skillClass);
            skill.def = this;
            return skill;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (skillType != SkillType.Active && needTarget)
                yield return $"Non-Active Skill {defName} Can't Have Target. \"needTarget\" Should Be False.";
        }
    }
    public enum SkillType
    {
        Active,     //主动
        Passive,    //被动（常态）
        Triggered   //被动（条件）
    }
}