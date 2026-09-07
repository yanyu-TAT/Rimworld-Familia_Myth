using FamiliaMyth.Familia;
using RimWorld;
using Verse;

namespace FamiliaMyth.Quest
{
    //强制任务失败结算：任务失败/超时信号触发时才记录重发冷却（而非任务生成期副作用）
    public class QuestPart_RankUpFailed : QuestPartCanDisable
    {
        public string inSignal;
        public FamiliaCategoryDef category;

        protected override void ProcessQuestSignal(Signal signal)
        {
            if (signal.tag == inSignal)
            {
                GameComponent_FamiliaRankTracker.Instance?.OnRankUpQuestFailed(category);
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
