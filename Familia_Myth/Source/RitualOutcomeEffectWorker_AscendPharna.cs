using FamiliaMyth.Pharna;
using FamiliaMyth.UI;
using RimWorld;
using Verse;

namespace FamiliaMyth.Ritual
{
    public class RitualOutcomeEffectWorker_AscendPharna : RitualOutcomeEffectWorker_FromQuality
    {
        public RitualOutcomeEffectWorker_AscendPharna() { compDatas = new List<RitualOutcomeComp_Data>(); }
        public RitualOutcomeEffectWorker_AscendPharna(RitualOutcomeEffectDef def) : base(def) { compDatas = new List<RitualOutcomeComp_Data>(); }

        // 仪式质量部分暂不做实现
        public override RitualOutcomePossibility GetOutcome(float quality, LordJob_Ritual ritual)
        {
            if (def.outcomeChances.TryRandomElementByWeight(o => o.chance, out var result))
                return result;
            return def.outcomeChances.First();
        }

        protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
        {
            base.ApplyExtraOutcome(totalPresence, jobRitual, outcome, out extraOutcomeDesc, ref letterLookTargets);

            Pawn pawn = jobRitual.PawnWithRole("recipient");
            Hediff_Pharna pharna = Hediff_Pharna.GetPharna(pawn);

            //检查是否已有恩惠
            if (pharna == null)
            {
                extraOutcomeDesc = string.Empty;
                Log.Warning("[FamiliaMyth] The recipent haven't been granted pharna yet");
                return;
            }

            //检查是否可升华
            if (!pharna.CanAscend())
            {
                extraOutcomeDesc = string.Empty;
                Log.Warning("[FamiliaMyth] The recipent doesn't meet the acesnd requirement"); 
                return;
            }

            //升华恩惠
            pharna.Ascend();

            //根据仪式效果提供升级奖励
            var rewards = AscendHelper.GetRandomRewards(10).ToList();
            if (rewards.Count > 0)
                Find.WindowStack.Add(new Dialog_AscendReward(pharna, rewards));

            //根据仪式效果赋予 能力落差后遗症

            letterLookTargets = new LookTargets(pawn);
        }
    }
}