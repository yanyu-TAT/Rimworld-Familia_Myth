using FamiliaMyth.Core;
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

        private static string GodName => FamiliaMythMod.godName ?? FamiliaMythMod.DefaultGodName1;

        //每次升华提供的奖励候选数量
        private const int RewardCount = 3;

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
            Hediff_Pharna pharna = pawn.GetPharna();

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
                Log.Warning("[FamiliaMyth] The recipent can't get ascend"); 
                return;
            }

            //升华恩惠
            pharna.Ascend();

            //根据仪式效果提供升级奖励
            var session = new AscendRewardSession(pharna, RewardCount, AscendHelper.GetRerollCount(pharna));
            if (session.Rewards.Count > 0)
                Find.WindowStack.Add(new Dialog_AscendReward(session));
            else
                Messages.Message("FamiliaMyth.AscendNoReward".Translate(pawn.Name.ToStringShort), MessageTypeDefOf.NeutralEvent);

            //升华恢复一次铭刻机会，铭刻本身保留
            extraOutcomeDesc += TranslateHelper.Translate("FamiliaMyth.InscriptionRenewed", ("pawn", pawn.Name), ("god", GodName));

            //根据仪式效果赋予 能力落差后遗症

            letterLookTargets = new LookTargets(pawn);
        }
    }
}