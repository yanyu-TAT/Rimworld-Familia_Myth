using FamiliaMyth.Core;
using RimWorld;
using Verse;

namespace FamiliaMyth.Pharna
{
    public class ChoiceLetter_PharnaLost : ChoiceLetter
    {
        public Pawn pawn;

        public override bool CanDismissWithRightClick => false;
        public override bool CanShowInLetterStack => base.CanShowInLetterStack && GameComponent_FamiliaMyth.Instance?.GetRecord(pawn) != null;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly)
                {
                    yield return Option_Close;
                    yield break;
                }

                var comp = GameComponent_FamiliaMyth.Instance;
                var record = comp?.GetRecord(pawn);
                if (record == null)
                {
                    yield return Option_Close;
                    yield break;
                }

                DiaOption restoreOption = new("FamiliaMyth.PharnaLost.Restore".Translate())
                {
                    action = () =>
                    {
                        AcceptanceReport report = record.Restore(pawn);
                        if (report)
                        {
                            Messages.Message("FamiliaMyth.PharnaLost.RestoreDone".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.PositiveEvent);
                            comp.Discard(pawn);
                            Find.LetterStack.RemoveLetter(this);
                        }
                        else
                        {
                            Messages.Message(report.Reason, MessageTypeDefOf.RejectInput);
                        }
                    },
                    resolveTree = true
                };
                yield return restoreOption;

                DiaOption discardOption = new("FamiliaMyth.PharnaLost.Discard".Translate())
                {
                    action = () =>
                    {
                        comp.Discard(pawn);
                        Find.LetterStack.RemoveLetter(this);
                    },
                    resolveTree = true
                };
                yield return discardOption;
            }
        }
    }
}
