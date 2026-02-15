using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Ritual
{
    public class RitualOutcomeEffectWorker_AscendPharna : RitualOutcomeEffectWorker_FromQuality
    {
        public RitualOutcomeEffectWorker_AscendPharna() { compDatas = new List<RitualOutcomeComp_Data>(); }
        public RitualOutcomeEffectWorker_AscendPharna(RitualOutcomeEffectDef def) : base(def) { compDatas = new List<RitualOutcomeComp_Data>(); }

        private static HediffDef cachedPharnaDef;
        private static HediffDef PharnaDef => cachedPharnaDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_Pharna");

        // 无质量组件，按定义的chance权重随机选择结果
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
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null)
            {
                extraOutcomeDesc = string.Empty;
                Log.Error("[FamiliaMyth] Invalid recipent role for ritual grant pharna");
                return;
            }

            //检查是否已有恩惠
            if (pawn.health.hediffSet.GetFirstHediffOfDef(PharnaDef) is not Hediff_Pharna pharna)
            {
                extraOutcomeDesc = string.Empty;
                Log.Warning("[FamiliaMyth] The recipent haven't been already granted pharna");
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

            //根据仪式效果赋予 能力落差后遗症

            letterLookTargets = new LookTargets(pawn);
        }
    }
}