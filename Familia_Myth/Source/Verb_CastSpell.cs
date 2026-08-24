using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Verb_CastSpell : Verb_CastAbility
    {
        //是否检测视线遮挡
        protected virtual bool CheckLOS => true;

        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            if (caster == null || !caster.Spawned || !CasterIsPawn) return false;
            if (state == VerbState.Bursting || !CanHitTarget(castTarg)) return false;
            this.surpriseAttack = surpriseAttack;
            canHitNonTargetPawnsNow = canHitNonTargetPawns;
            this.preventFriendlyFire = preventFriendlyFire;
            this.nonInterruptingSelfCast = nonInterruptingSelfCast;
            currentTarget = castTarg;
            currentDestination = destTarg;
            
            if (CheckLOS)
            {
                if (!TryFindShootLineFromTo(caster.Position, castTarg, out var line))
                    return false;
                CasterPawn.Drawer.Notify_WarmingCastAlongLine(line, caster.Position);
            }

            int ticks = WarmupTime.SecondsToTicks();
            var stance = ((CasterPawn.GetPharna()?.CanConcurrentCast) ?? false) && ((ability as Spell)?.CanConcurrentCast ?? false)
                ? new Stance_Warmup_ConcurrentCasting(ticks, castTarg, this)
                : new Stance_Warmup(ticks, castTarg, this);
            CasterPawn.stances.SetStance(stance);

            if ((ability.def.stunTargetWhileCasting || verbProps.stunTargetOnCastStart) && castTarg.Pawn != CasterPawn)
            {
                castTarg.Pawn?.stances.stunner.StunFor(ticks, caster, false, false);
                if (!castTarg.Pawn?.Awake() ?? false)
                    RestUtility.WakeUp(castTarg.Pawn);
            }

            return true;
        }
    }
}
