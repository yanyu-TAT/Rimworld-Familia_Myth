using FamiliaMyth.Familia;
using RimWorld;
using Verse;

namespace FamiliaMyth.Quest
{
    //评级晋升结算：任务完成信号触发时才执行晋升（而非任务生成期副作用）
    public class QuestPart_RankUpComplete : QuestPartCanDisable
    {
        public string inSignal;
        public FamiliaCategoryDef category;

        protected override void ProcessQuestSignal(Signal signal)
        {
            if (signal.tag == inSignal ||
                signal.tag == "rewardsReady" ||
                signal.tag.EndsWith("rewardsReady"))
            {
                GameComponent_FamiliaRankTracker.Instance?.OnRankUpRequestDone(category);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Defs.Look(ref category, "category");
        }
    }
}
