using Verse;

namespace FamiliaMyth.Abilities
{
    public class Hediff_SpellState : Hediff
    {
        private static HediffDef hediffDef;
        public static HediffDef HediffDef =>
            hediffDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_SpellState");

        public ISpellWithRemainingTime spell;

        public override string LabelBase => spell?.EffectName ?? base.LabelBase;
        public override string LabelInBrackets => "剩余" + spell?.RemainingTimeText ?? base.LabelInBrackets;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref spell, "spell");
        }
    }

    public interface ISpellWithRemainingTime : ILoadReferenceable
    {
        void NotifyStateStart(Pawn pawn)
        {
            if (pawn?.health == null) return;
            State = HediffMaker.MakeHediff(Hediff_SpellState.HediffDef, pawn) as Hediff_SpellState;
            if (State == null) return;
            State.spell = this;
            pawn.health.AddHediff(State);
        }

        void NotifyStateEnd(Pawn pawn)
        {
            if (State == null) return;
            pawn.health.RemoveHediff(State);
        }

        string RemainingTimeText { get; }
        string EffectName { get; }
        Hediff_SpellState State { get; set; }
    }
}
