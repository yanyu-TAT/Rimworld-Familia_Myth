using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_Ember : Skill
    {
        public static HashSet<Pawn> downedPawns = new();

        //tick方法中不触发效果，由patch监听触发效果
        public override bool CanCast(out string failReason)
        {
            failReason = null;
            return false;
        }

        public override void Tick()
        {
            //Log.Message($"Ticking for pawn {pawn?.Name}");
            base.Tick();
            if (Find.TickManager.TicksGame % (int)def.checkInterval != 0) return;

            //状态变化时触发
            bool isDowned = Owner.Downed;
            if (isDowned && !downedPawns.Contains(Owner))
            {
                //Log.Message($"Pawn {pawn.Name} is downed.");
                downedPawns.Add(Owner);
                TryApplyOnDowned(Owner);
            }
            if (!isDowned && downedPawns.Contains(Owner))
                downedPawns.Remove(Owner);
        }

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            if (Owner?.health == null) return false;

            var ember = HediffMaker.MakeHediff(HediffDef.Named("EmberInEffect"), Owner);
            var comp = ember.TryGetComp<HediffComp_Disappears>();
            if (comp != null)
                comp.ticksToDisappear = ((int)(def.duration * 60));
            Owner.health.AddHediff(ember);
            //Log.Message("Ember Effect Apllied");
            return true;
        }

        public bool TryApplyOnDowned(Pawn downed)
        {
            if (downed == null) return false;
            //检测冷却
            if (IsCooldown) return false;
            //Log.Message("Not in cooldown");
            if (downed.health?.hediffSet?.hediffs is not List<Hediff> hediffs) return false;
            //强制倒地（麻醉等）不触发
            if (downed.health.forceDowned) return false;
            //Log.Message("Not forced downed");
            //没有受伤不触发
            if (!hediffs.OfType<Hediff_Injury>().Any()) return false;
            //Log.Message("Got injuries");

            if (ApplyEffect(downed))
            {
                Messages.Message($"由于被击倒，{downed.Name.ToStringShort} 的技能：余烬 已触发", MessageTypeDefOf.PositiveEvent);
                //进入冷却
                SetCooldown();
                return true;
            }
            return false;
        }

        public override void Apply(Hediff_Pharna pharna)
        {
            base.Apply(pharna);
            var hediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("EmberState"), pharna.pawn);
            pharna.pawn.health.AddHediff(hediff);
        }
    }
}