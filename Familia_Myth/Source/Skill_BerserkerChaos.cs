using FamiliaMyth.Pharna;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_BerserkerChaos : Skill
    {
        public override bool ApplyEffect(Pawn pawn, LocalTargetInfo target)
        {
            return true;
        }

        public float GetDamageFactor(Pawn pawn)
        {
            float x = 1f - pawn.health.summaryHealth.SummaryHealthPercent;
            x = Mathf.Clamp(x, 0f, 1f);
            return x;
        }
    }
}