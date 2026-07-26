using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_Ember : Skill
    {
        public override bool CheckCondition(Pawn caster)
        {
            //tick方法中不触发效果，由patch监听触发效果
            return false;
        }

        public override bool ApplyEffect(Pawn pawn, LocalTargetInfo target)
        {
            if (pawn?.health == null) return false;

            var ember = HediffMaker.MakeHediff(HediffDef.Named("EmberInEffect"), pawn);
            var comp = ember.TryGetComp<HediffComp_Disappears>();
            comp?.SetDuration((int)(def.duration * 60));
            pawn.health.AddHediff(ember);
            return true;
        }

        public bool TryApplyOnKill(Pawn pawn, DamageInfo? dinfo, Hediff exactCulprit)
        {
            if (pawn == null || pawn.Dead) return false;

            bool f = false;
            if (dinfo.HasValue)
            {
                var d = dinfo.Value.Def;

                if (d == DamageDefOf.SurgicalCut) return false; //手术不触发
                if (!d.ExternalViolenceFor(pawn)) return false; //非外伤不触发

                f = true;
            }
            if (!f) 
            {
                if (exactCulprit == null)
                {
                    Log.Warning("[FamiliaMyth] Pawn with ember skill killed by unknown damage");
                    return false;
                }
                else if (exactCulprit is not Hediff_Injury && exactCulprit.def != HediffDefOf.BloodLoss)
                {
                    return false; 
                }
            }

            //检测冷却
            if (IsCooldown) return false;

            if (ApplyEffect(pawn, pawn))
            {
                //进入冷却
                SetCooldown();
                return true;
            }
            return false;
        }

        public override void Tick(Pawn pawn)
        {
            base.Tick(pawn);

            //更新当前状态
            var state = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EmberState"));
            if (state == null)
            {
                state = HediffMaker.MakeHediff(HediffDef.Named("EmberState"), pawn);
                pawn.health.AddHediff(state);
            }

            //severity: 0.1 = 可适用 0.5 = 生效中 0.9 = 冷却中
            bool inEffect = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EmberInEffect")) != null;
            state.Severity = inEffect ? 0.5f : (IsCooldown ? 0.9f : 0.1f);
        }
    }
}