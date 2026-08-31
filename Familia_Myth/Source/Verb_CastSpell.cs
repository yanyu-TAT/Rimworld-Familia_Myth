using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Verb_CastSpell : Verb_CastAbility
    {
        public ISpellWithTargetRange spell;

        protected bool CheckLOS => spell?.CheckLOS ?? false;

        public override void DrawHighlight(LocalTargetInfo target)
        {
            base.DrawHighlight(target);

            try 
            {
                var cells = spell.GetCells(target).ToList();
                GenDraw.DrawFieldEdges(cells, spell.HighlightColor);
            }
            catch { }
        }

        public override bool CanHitTarget(LocalTargetInfo targ)
            => CanHitTargetFrom(caster.Position, targ);

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            try
            {
                return spell.CanHitTargetFrom(root, targ);
            }
            catch
            {
                return CanHitTargetFromDefault(root, targ);
            }
        }

        public bool CanHitTargetFromDefault(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.Thing != null && targ.Thing == caster)
            {
                return targetParams.canTargetSelf;
            }

            if (targ.Pawn != null && targ.Pawn.IsPsychologicallyInvisible() && caster.HostileTo(targ.Pawn))
            {
                return false;
            }

            if (ApparelPreventsShooting())
            {
                return false;
            }

            if (spell.CheckLOS)
                return TryFindShootLineFromTo(root, targ, out _);

            return true;
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!CanHitTarget(target))
            {
                Messages.Message("目标不在有效范围内", MessageTypeDefOf.RejectInput);
                return false;
            }
            return true;
        }

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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref spell, "spell");
        }
    }

    public interface ISpellWithTargetRange : ILoadReferenceable
    {
        /// <summary>
        /// 获取高亮的格子，出现错误则回退为默认行为
        /// </summary>
        IEnumerable<IntVec3> GetCells(LocalTargetInfo localTarget);
        /// <summary>
        /// 目标合法性检查，出现错误则回退为默认行为
        /// </summary>
        bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo localTarget);
        /// <summary>
        /// 是否检查视线遮挡
        /// </summary>
        bool CheckLOS { get => false; }
        /// <summary>
        /// 高光颜色
        /// </summary>
        Color HighlightColor { get => Color.white; }
    }
}
