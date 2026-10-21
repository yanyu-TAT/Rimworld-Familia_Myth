using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 烈焰魔剑：咏唱结束后为施法者附加数次近战火焰附魔。
    /// </summary>
    public class Spell_FlameSword : Spell
    {
        private static HediffDef inEffectDef;
        private static HediffDef InEffectDef =>
            inEffectDef ??= DefDatabase<HediffDef>.GetNamed("FlameSwordInEffect");

        protected override void WarmUpFinish(bool castDone)
        {
            if (!castDone || pawn?.health == null || !pawn.Spawned || pawn.DeadOrDowned)
                return;

            var hediff = pawn.health.GetOrAddHediff(InEffectDef);
            hediff?.TryGetComp<HediffComp_DisappearsWithCounter>()?.ResetCnt();
        }
    }
}
