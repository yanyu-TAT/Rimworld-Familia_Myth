using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using Verse;

namespace Familia_Myth.Source
{
    public class Skill_DesperateArmyWin : Skill, IStatusModifier
    {
        private float Ratio => (0.5f - Owner.needs.mood.CurLevelPercentage) / 0.5f;

        public override AcceptanceReport CanCast
        {
            get
            {
                var canCast = base.CanCast;
                if (!canCast) return canCast;

                return Ratio > 0 ? true : "";
            }
        }

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            return true;
        }

        public void ApplyStatusModifier(PharnaStatusType type, ref float val)
        {
            if (!CanCast) return;
            switch (type)
            {
                case PharnaStatusType.Strength or PharnaStatusType.Endurance or PharnaStatusType.Agility:
                    val += 200f * Ratio + 100f;
                    break;
                case PharnaStatusType.Will:
                    val += 300f * Ratio + 150f;
                    break;
            }
        }

        public string GetExplanation(PharnaStatusType type)
        {
            if (!CanCast) return "";
            string s = type switch
            {
                PharnaStatusType.Endurance or PharnaStatusType.Strength or PharnaStatusType.Agility
                    => "FamiliaMyth.Ability.DesperateArmyWinStatus".Translate((int)(200f * Ratio + 100f)),
                PharnaStatusType.Will
                    => "FamiliaMyth.Ability.DesperateArmyWinWill".Translate((int)(300f * Ratio + 150f)),
                _ => ""
            };
            return s;
        }
    }
}
