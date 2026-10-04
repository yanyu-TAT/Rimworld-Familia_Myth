using System.Collections.Generic;
using FamiliaMyth.Familia;
using RimWorld;
using Verse;

namespace FamiliaMyth.Quest
{
    //「据点清剿」（日常/强制共用）奖励：结算时按分类发放升级点数。
    public class QuestPart_SiteClearanceReward : QuestPartCanDisable
    {
        public string inSignal;
        public FamiliaCategoryDef category;    //升级点数归属分类（null → 不发点数）
        public int upgradePoints;              //发放的升级点数

        protected override void ProcessQuestSignal(Signal signal)
        {
            if (signal.tag == inSignal ||
                signal.tag == "rewardsReady" ||
                signal.tag.EndsWith(".rewardsReady"))
            {
                Distribute();
            }
        }

        private void Distribute()
        {
            //升级点数（眷族共享，不随参与者名单分摊）
            if (category != null && upgradePoints > 0)
                GameComponent_FamiliaRankTracker.Instance?.GainPoints(category, upgradePoints);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Defs.Look(ref category, "category");
            Scribe_Values.Look(ref upgradePoints, "upgradePoints", 0);
        }
    }
}
