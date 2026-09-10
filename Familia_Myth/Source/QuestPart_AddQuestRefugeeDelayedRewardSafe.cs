using System.Linq;
using RimWorld;
using Verse;

namespace FamiliaMyth.Quest
{
    //延迟回报子任务（原版 RefugeeDelayedReward）的安全版。
    //原版 QuestPart_AddQuestRefugeeDelayedReward.GetSlate 会从 lodgers 里挑一名
    //「未死、非玩家派系、非囚犯」的难民作为 rewardGiver；若一个都没有，生成的子任务缺少
    //rewardGiver 变量，文本解析会在 QuestGen.Generate 处抛
    //"Grammar unresolvable. Root 'root'（rewardGiver_nameDef → UNRESOLVABLE）"。
    //本任务成功线里难民可能全在穿梭机机舱内或已全部被劝加入 → 必须生成前预检，无候选则静默跳过。
    public class QuestPart_AddQuestRefugeeDelayedRewardSafe : QuestPart_AddQuestRefugeeDelayedReward
    {
        public override bool CanAdd =>
            !lodgers.NullOrEmpty()
            && lodgers.Any(p => p != null && !p.Dead && p.Faction != Faction.OfPlayer && !p.IsPrisoner);
    }
}
