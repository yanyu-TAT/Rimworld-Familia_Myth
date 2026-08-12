using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Verb_CastAbility_FanTarget : Verb_CastAbility
    {
        public int fanAngle = -1;

        public override bool CanHitTarget(LocalTargetInfo targ)
        {
            return targ.Cell.DistanceTo(caster.Position) <= EffectiveRange;
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            return targ.Cell.DistanceTo(root) <= EffectiveRange;
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

        //仿照原版覆写以跳过视线检查
        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            //基础合法性校验（与父类 Verb.TryStartCastOn 一致）
            if (caster == null || !caster.Spawned) return false;
            if (state == VerbState.Bursting || !CanHitTarget(castTarg)) return false;

            this.surpriseAttack = surpriseAttack;
            canHitNonTargetPawnsNow = canHitNonTargetPawns;
            this.preventFriendlyFire = preventFriendlyFire;
            this.nonInterruptingSelfCast = nonInterruptingSelfCast;
            currentTarget = castTarg;
            currentDestination = destTarg;

            if (CasterIsPawn && WarmupTime > 0f)
            {
                //跳过 TryFindShootLineFromTo 视线检查，允许锁定不可视位置。
                var statValue = CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor);
                int ticks = (WarmupTime * statValue).SecondsToTicks();
                CasterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
                if (verbProps.stunTargetOnCastStart && castTarg.Pawn != null)
                    castTarg.Pawn.stances.stunner.StunFor(ticks, null, addBattleLog: false);
            }
            else
            {
                if (verbTracker.directOwner is Ability ability)
                    ability.lastCastTick = Find.TickManager.TicksGame;
                WarmupComplete();
            }
            return true;
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            base.DrawHighlight(target);

            if (fanAngle <= 0)
            {
                Log.Warning($"[FamiliaMyth] fanAngle should have been set for ability {ability.def.label} but not");
                fanAngle = 90; //默认值
            }

            if (target.IsValid && caster is Pawn pawn && pawn.Spawned)
            {
                //获取扇形范围内地格
                var cells = GetFanCells(pawn.Position, target.Cell, EffectiveRange, fanAngle);
                GenDraw.DrawFieldEdges(cells, Color.red);
            } 
        }

        private static List<IntVec3> GetFanCells(IntVec3 casterPos, IntVec3 targetPos, float range, float angle)
        {
            var results = new List<IntVec3>();
            var facing = (targetPos - casterPos).AngleFlat;
            float halfAngle = angle / 2f;
            int cellCount = GenRadial.NumCellsInRadius(range);

            for (int i = 0; i < cellCount; i++)
            {
                var cell = casterPos + GenRadial.RadialPattern[i];
                if (!cell.InBounds(Find.CurrentMap)) continue;

                var dir = cell - casterPos;
                if (dir.SqrMagnitude > range * range) continue;

                float cellAngle = dir.AngleFlat;
                float diff = Mathf.DeltaAngle(facing, cellAngle);
                if (Mathf.Abs(diff) <= halfAngle)
                    results.Add(cell);
            }
            return results;
        }
    }
}
