using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_SuccessionOfJustice : Skill
    {
        private const float RangeGainRate = 1f / 2000f;

        private static HediffDef effectDef;
        private static HediffDef EffectDef =>
            effectDef ??= DefDatabase<HediffDef>.GetNamed("SuccessionOfJusticeInEffect");

        public float Exp => pharna?.experience ?? 0;
        private float Range => 5f + Exp * RangeGainRate;

        private List<Hediff_SuccessionOfJusticeInEffect> cache = new();
        public override void Tick()
        {
            if (Find.TickManager.TicksGame % def.checkInterval != 0) return;
            if (Owner?.Map is not Map map || !Owner.Spawned) return;
            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
                if (pawn.Position.DistanceToSquared(Owner.Position) <= Range * Range && pawn.GetPharna() != null)
                {
                    pawn.health.hediffSet.GetHediffs(ref cache, h => h.skill == this);
                    var hediff = cache.FirstOrDefault();
                    if (hediff == null)
                    {
                        //先设好技能与施予者再入列表，TryMergeWith 才能按施予者技能判断合并
                        if (HediffMaker.MakeHediff(EffectDef, pawn) is not Hediff_SuccessionOfJusticeInEffect created) continue;
                        created.Init(this);
                        pawn.health.AddHediff(created);

                        //被合并时新实例不进入 hediffSet，取回真正生效的那个
                        pawn.health.hediffSet.GetHediffs(ref cache, h => h.skill == this);
                        hediff = cache.FirstOrDefault();
                        if (hediff == null) continue;
                    }

                    var comp = hediff.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                        comp.ticksToDisappear = 120;
                }
        }

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            return true;
        }
    }

    public class Hediff_SuccessionOfJusticeInEffect : HediffWithComps
    {
        private const float MentalBreakThresholdRate = 2000f;

        public Skill_SuccessionOfJustice skill;
        public float Exp => skill?.Exp ?? 0;

        public void Init(Skill_SuccessionOfJustice skill)
        {
            this.skill = skill;
            from = skill?.Owner;
            stage.statFactors = modifiers;
            stage.extraTooltip = "FamiliaMyth.Ability.SuccessionOfJusticeFrom".Translate(skill?.Owner?.Name.ToStringShort ?? "Unknown");
        }

        private readonly HediffStage stage = new();
        private readonly List<StatModifier> modifiers = new() { new() { stat = StatDefOf.MentalBreakThreshold } };
        public override HediffStage CurStage
        {
            get
            {
                float exp = Exp == 0 ? 1 : Exp;
                modifiers[0].value = Mathf.Clamp01(MentalBreakThresholdRate / exp);

                return stage;
            }
        }

        public override bool TryMergeWith(Hediff other)
        {
            if (other is Hediff_SuccessionOfJusticeInEffect effect)
            {
                if (effect.skill == skill)
                    return base.TryMergeWith(other);
                else
                    return false;
            }

            return base.TryMergeWith(other);
        }

        public Pawn from;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref from, "from");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                //技能实例不参与 Scribe，读档后经施予者的恩惠反查
                skill = from?.GetPharna()?.skills?.OfType<Skill_SuccessionOfJustice>().FirstOrDefault();
                Init(skill);
                if (skill == null)
                {
                    Log.Warning($"[FamiliaMyth] Pawn Reference For Hediff {def?.defName} Of {pawn?.Name?.ToStringShort ?? "Unknown"} Lost. Now Removing.");
                    pawn?.health?.RemoveHediff(this);
                }
            }
        }
    }

    public class HediffComp_SuccessionOfJusticeInEffect : HediffComp, IStatusModifier
    {
        private const float ManaWillGainRate = 0.01f;
        private float Exp => (parent as Hediff_SuccessionOfJusticeInEffect)?.Exp ?? 0;

        public void ApplyStatusModifier(PharnaStatusType type, ref float val)
        {
            if (type == PharnaStatusType.Will || type == PharnaStatusType.Mana)
                val += ManaWillGainRate * Exp;
        }

        public string GetExplanation(PharnaStatusType type)
        {
            if (type == PharnaStatusType.Mana || type == PharnaStatusType.Will)
                return "FamiliaMyth.Ability.SuccessionOfJustice".Translate((ManaWillGainRate * Exp).ToString("F0"));
            return "";
        }
    }
}
