using FamiliaMyth.Familia;
using RimWorld;
using Verse;

namespace FamiliaMyth.Quest
{
    public class QuestPart_GainUpgradePoints : QuestPartCanDisable
    {
        public string inSignal;
        public FamiliaCategoryDef category;
        public int amount;

        protected override void ProcessQuestSignal(Signal signal)
        {
            if (signal.tag == inSignal ||
                signal.tag == "rewardsReady" ||
                signal.tag.EndsWith("rewardsReady"))
            {
                if (category != null && amount > 0)
                    GameComponent_FamiliaRankTracker.Instance.GainPoints(category, amount);
            }

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Defs.Look(ref category, "category");
            Scribe_Values.Look(ref amount, "amount", 0);
        }
    }
}
