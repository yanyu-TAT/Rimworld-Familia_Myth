using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Quest
{
    //「物资押运」定居点交互 comp：A=领取点、B=交付点。
    //由 QuestPart_SupplyEscort 在任务装配期初始化（role + 固化信号串 + 押运清单状态），任务结束/Cleanup 时清零；
    //未初始化时不给远行队任何按钮（comp 挂在 SettlementDef 上，全定居点共享同一份 def 声明）。
    public class WorldObjectComp_SupplyEscort : WorldObjectComp
    {
        public enum Role { None, Pickup, Delivery }

        public Role role = Role.None;
        public string signalPickup;      //固化信号：在 A 点击「接收押运物资」
        public string signalDeliver;     //固化信号：在 B 点击「交付押运物资」
        public string questName;         //任务名（提示用）
        public bool manifestReady;       //押运清单是否已生成（领取后为真）
        public string manifestSummary;   //清单摘要（交付按钮提示用）

        private const string PickupIconPath = "UI/Commands/FormCaravan";
        private const string DeliverIconPath = "UI/Commands/SellableItems";

        //任务部件初始化：把本定居点标记为 A（领取）或 B（交付）
        public void Setup(Role newRole, string pickupSignalTag, string deliverSignalTag, string questNameText)
        {
            role = newRole;
            signalPickup = pickupSignalTag;
            signalDeliver = deliverSignalTag;
            questName = questNameText;
        }

        //任务部件推送押运清单状态（领取当拍变就绪）
        public void SetManifest(bool ready, string summary)
        {
            manifestReady = ready;
            manifestSummary = summary;
        }

        public void Clear()
        {
            role = Role.None;
            signalPickup = null;
            signalDeliver = null;
            questName = null;
            manifestReady = false;
            manifestSummary = null;
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            if (role == Role.None || caravan == null || parent == null) yield break;
            if (caravan.Tile != parent.Tile) yield break;

            if (role == Role.Pickup && !signalPickup.NullOrEmpty())
            {
                yield return PickupGizmo(caravan);
            }
            else if (role == Role.Delivery && !signalDeliver.NullOrEmpty())
            {
                yield return DeliveryGizmo(caravan);
            }
        }

        public override string CompInspectStringExtra()
        {
            if (role == Role.None) return null;
            return role == Role.Pickup
                ? $"押运领取点（{questName}）"
                : $"押运交付点（{questName}）";
        }

        //A 点：接收押运物资（超重确认由任务部件按清单实际重量处理）
        private Gizmo PickupGizmo(Caravan caravan)
        {
            return new Command_Action
            {
                defaultLabel = "接收押运物资",
                defaultDesc = $"从 {parent.Label} 接收公会的押运物资"
                    + $"（总重 {SupplyEscortUtility.CargoMassMin:F0}~{SupplyEscortUtility.CargoMassMax:F0}kg，"
                    + "清单由对方现场配货）。\n\n"
                    + "领取后请按清单原样送达任务指定的交付点。",
                icon = ContentFinder<Texture2D>.Get(PickupIconPath),
                action = () => SendSignal(signalPickup, caravan)
            };
        }

        //B 点：交付押运物资（清单未就绪则禁用；逐项核对由任务部件受理）
        private Gizmo DeliveryGizmo(Caravan caravan)
        {
            var gizmo = new Command_Action
            {
                defaultLabel = "交付押运物资",
                defaultDesc = $"向 {parent.Label} 交付押运清单上的物资（可分批交付）。\n\n"
                    + $"尚需交付：{manifestSummary ?? "（尚未领取）"}\n\n"
                    + "按种类与数量逐项核对，清单外的物品不会被收取。",
                icon = ContentFinder<Texture2D>.Get(DeliverIconPath),
                action = () => SendSignal(signalDeliver, caravan)
            };

            if (!manifestReady)
            {
                gizmo.Disabled = true;
                gizmo.disabledReason = "尚未从领取点接收押运物资";
            }

            return gizmo;
        }

        private static void SendSignal(string tag, Caravan caravan)
        {
            if (tag.NullOrEmpty()) return;
            Find.SignalManager.SendSignal(new Signal(tag, caravan.Named("CARAVAN")));
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref role, "escortRole", Role.None);
            Scribe_Values.Look(ref signalPickup, "escortSignalPickup");
            Scribe_Values.Look(ref signalDeliver, "escortSignalDeliver");
            Scribe_Values.Look(ref questName, "escortQuestName");
            Scribe_Values.Look(ref manifestReady, "escortManifestReady");
            Scribe_Values.Look(ref manifestSummary, "escortManifestSummary");
        }
    }
}
