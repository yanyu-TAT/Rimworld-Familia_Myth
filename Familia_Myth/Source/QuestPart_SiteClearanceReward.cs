using System.Collections.Generic;
using FamiliaMyth.Familia;
using FamiliaMyth.Pharna;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FamiliaMyth.Quest
{
    //「据点清剿」（日常/强制共用）奖励：
    //威胁清除时点快照「站点在场的受恩惠玩家成员」，结算按快照名单发放；
    //acceptor 非空且在名单（多人时）→ acceptor 50% + 名单均分；acceptor 为空 → 名单全员均分。
    //伟业值与升级点数均在本 part 内统一经 utility / tracker 发放。
    public class QuestPart_SiteClearanceReward : QuestPartCanDisable
    {
        public string inSignal;
        public string snapshotSignal;          //名单快照时点
        public MapParent site;                 //任务站点（快照来源）
        public Pawn acceptor;                 //接取者（强制任务为 null）
        public float totalExploits;
        public string letterLabel;             //结算信标题（空 → "委托完成"）
        public FamiliaCategoryDef category;    //升级点数归属分类（null → 不发点数）
        public int upgradePoints;              //发放的升级点数

        //威胁清除时点快照的参与者名单（结算按此发放）
        public List<Pawn> participants = new();

        protected override void ProcessQuestSignal(Signal signal)
        {
            //威胁清除时点：快照站点在场的受恩惠玩家成员（只记录一次）
            if (snapshotSignal != null && signal.tag == snapshotSignal)
            {
                Snapshot();
                return;
            }

            if (signal.tag == inSignal ||
                signal.tag == "rewardsReady" ||
                signal.tag.EndsWith(".rewardsReady"))
            {
                Distribute();
            }
        }

        private void Snapshot()
        {
            if (participants.Count > 0) return;
            QuestUtilities.SnapshotSiteParticipants(site, participants);
        }

        private void Distribute()
        {
            if (totalExploits > 0f)
            {
                //名单兜底：若异常未快照，结算时补一次
                if (participants.Count == 0)
                    Snapshot();

                //统一分配 + 结算信（acceptor 可为 null → 全员均分）
                string label = string.IsNullOrEmpty(letterLabel)
                    ? "FamiliaMyth.QuestPart.QuestComplete".Translate()
                    : letterLabel;
                QuestUtilities.DistributeExploits(participants, acceptor, totalExploits, label,
                    "FamiliaMyth.QuestPart.RewardReasonOutstanding".Translate());
            }

            //升级点数（眷族共享，不随参与者名单分摊）
            if (category != null && upgradePoints > 0)
                GameComponent_FamiliaRankTracker.Instance?.GainPoints(category, upgradePoints);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref snapshotSignal, "snapshotSignal");
            Scribe_References.Look(ref site, "site");
            Scribe_References.Look(ref acceptor, "acceptor");
            Scribe_Collections.Look(ref participants, "participants", LookMode.Reference);
            Scribe_Values.Look(ref totalExploits, "totalExploits");
            Scribe_Values.Look(ref letterLabel, "letterLabel");
            Scribe_Defs.Look(ref category, "category");
            Scribe_Values.Look(ref upgradePoints, "upgradePoints", 0);
        }
    }
}
