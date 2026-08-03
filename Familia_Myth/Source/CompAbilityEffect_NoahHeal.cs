using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 诺亚治愈效果组件：包扎所有伤口并赋予愈合加速光环
    /// </summary>
    public class CompAbilityEffect_NoahHeal : CompAbilityEffect
    {
        private static HediffDef cachedHediffDef;
        private static HediffDef HediffDef => cachedHediffDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_NoahHeal");

        public new CompProperties_NoahHeal Props => (CompProperties_NoahHeal)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            var caster = parent.pawn;
            var targetPawn = target.Pawn;
            if (caster == null || targetPawn == null) return;

            // 包扎所有伤口并治愈
            foreach (var hediff in targetPawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_Injury injury)
                {
                    HediffComp_TendDuration tendComp = injury.TryGetComp<HediffComp_TendDuration>();
                    tendComp?.CompTended(1f, 1f);
                    injury.Heal(Math.Min(injury.Severity, 5));
                }
            }

            // 添加愈合加速光环
            targetPawn.health.AddHediff(HediffDef);
        }
    }

    public class CompProperties_NoahHeal : CompProperties_AbilityEffect
    {
        public CompProperties_NoahHeal()
        {
            compClass = typeof(CompAbilityEffect_NoahHeal);
        }
    }
}
