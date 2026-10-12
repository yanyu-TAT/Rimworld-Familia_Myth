using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_NoahHeal : Spell
    {
        private const float HealBudget = 10f;

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

            targetPawn.TendInjuries(null, 1f, caster);
            targetPawn.HealInjuries(HealBudget, caster);

            // 添加愈合加速光环；其伟业贡献由 HediffComp_FeatSupport 按实际生效时间累计
            var aura = targetPawn.health.AddHediff(HediffDef);
            aura?.TryGetComp<HediffComp_FeatSupport>()?.SetCaster(caster);
        }
    }
}
