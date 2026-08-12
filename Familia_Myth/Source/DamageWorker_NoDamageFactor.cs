using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class DamageFactorIgnore_Extension : DefModExtension
    {
        public float ignoreFactor = 0f;
    }

    public class DamageWorker_NoDamageFactor : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            if (thing is Pawn victim && dinfo.Def.ExternalViolenceFor(victim))
            {
                var props = dinfo.Def.GetModExtension<DamageFactorIgnore_Extension>();
                if (props != null)
                {
                    float factor = victim.GetStatValue(StatDefOf.IncomingDamageFactor);
                    //对于减少承伤的进行无视
                    if (factor > 0f && factor < 1f)
                    {
                        float neutralized = factor * (1f - props.ignoreFactor) + props.ignoreFactor;
                        float multiplier = neutralized / factor;
                        if (Math.Abs(multiplier - 1f) > 0.0001f)
                            dinfo.SetAmount(dinfo.Amount * multiplier);
                    }
                }
            }
            return base.Apply(dinfo, thing);
        }
    }
}
