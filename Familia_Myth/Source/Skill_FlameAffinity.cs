using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_FlameAffinity : Skill
    {
        private const float HealPerInterval = 1f;

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            if (Owner?.health?.hediffSet?.hediffs == null) return false;

            var injuries = Owner.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(h => h.Severity > 0f)
                .OrderBy(h => h.Severity)
                .ToList();
            if (injuries.NullOrEmpty()) return false;

            float buget = HealPerInterval;
            foreach(var injury in injuries)
            {
                if (buget <= 0) break;
                float heal = Mathf.Min(buget, injury.Severity);
                injury.Heal(heal);
                buget -= heal;
            }
            return true;
        }

        //仅考虑持有者是否正在燃烧
        public override AcceptanceReport CanCast => Owner.IsBurning() ? true : "";

        public static bool HasFlameAffinity(Pawn pawn)
        {
            var pharna = pawn.GetPharna();
            if (pharna?.skills == null) return false;
            return pharna.skills.Any(s => s is Skill_FlameAffinity);
        }
    }
}
