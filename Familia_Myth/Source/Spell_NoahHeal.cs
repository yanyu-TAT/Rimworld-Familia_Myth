using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_NoahHeal : Spell
    {
        private static HediffDef cachedHediffDef;
        private static HediffDef HediffDef => cachedHediffDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_NoahHeal");

        private Pawn target;

        protected override void WarmUpStart()
        {
            target = verb.CurrentTarget.Pawn;
        }

        protected override void WarmUpFinish(bool castDone)
        {
            if (!castDone || target == null || !target.Spawned) return;

            var caster = pawn;
            var targetPawn = target;
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
}
