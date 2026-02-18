using FamiliaMyth.Core;
using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Ritual
{
    public class RitualOutcomeEffectWorker_GrantPharna : RitualOutcomeEffectWorker_FromQuality
    {
        public RitualOutcomeEffectWorker_GrantPharna() { compDatas = new List<RitualOutcomeComp_Data>(); }
        public RitualOutcomeEffectWorker_GrantPharna(RitualOutcomeEffectDef def) : base(def) { compDatas = new List<RitualOutcomeComp_Data>(); }

        private static HediffDef cachedPharnaDef;
        private static HediffDef PharnaDef => cachedPharnaDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_Pharna");

        private static string GodName => FamiliaMythMod.godName;

        // 无质量组件，直接返回固定结果
        public override RitualOutcomePossibility GetOutcome(float quality, LordJob_Ritual ritual)
        {
            return def.outcomeChances.First();
        }

        protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
        {
            base.ApplyExtraOutcome(totalPresence, jobRitual, outcome, out extraOutcomeDesc, ref letterLookTargets);

            Pawn pawn = jobRitual.PawnWithRole("recipient");
            if (pawn == null || pawn.health == null)
            {
                extraOutcomeDesc = string.Empty;
                Log.Error("[FamiliaMyth] Invalid recipent role for ritual grant pharna");
                return;
            }

            //检查是否已有恩惠
            if (Hediff_Pharna.GetPharna(pawn) != null)
            {
                extraOutcomeDesc = string.Empty;
                Log.Warning("[FamiliaMyth] The recipent have been already granted pharna");
                return;
            }

            //赋予恩惠
            pawn.health.AddHediff(PharnaDef);

            extraOutcomeDesc += TranslateHelper.Translate("FamiliaMyth.GrantPharnaRitualDone", ("pawn", pawn.Name), ("god", GodName));
            letterLookTargets = new LookTargets(pawn);
        }
    }
}