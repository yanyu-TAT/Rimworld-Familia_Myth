using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Verb_CastAbility_FanTarget : Verb_CastAbility
    {
        public int fanAngle = -1;

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
