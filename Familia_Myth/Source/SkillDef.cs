using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class SkillDef : Def
    {
        public Type skillClass;

        public RewardRarity rarity;
        public SkillType skillType;

        public float cooldown = 0f;
        public float warmup = 0f;
        public float duration = 0f;
        public float checkInterval = 60f;

        public string iconPath;
        public string gizmoClass;

        public Skill CreateSkill()
        {
            var skill = (Skill)Activator.CreateInstance(skillClass);
            skill.def = this;
            return skill;
        }
    }
    public enum SkillType
    {
        Active,     //主动
        Passive,    //被动（常态）
        Triggered   //被动（条件）
    }
}