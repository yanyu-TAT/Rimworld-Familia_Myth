using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public static class WeaponHelper
    {
        /// <summary>
        /// 获取对应殖民者已装备且伤害最高的锐伤近战武器
        /// </summary>
        public static Verb GetMeleeSharpWeapon(this Pawn pawn)
        {
            var verb = pawn?.equipment?.AllEquipmentVerbs?
                .Where(v => v.verbProps.IsMeleeAttack && v.verbProps.meleeDamageDef?.armorCategory == DamageArmorCategoryDefOf.Sharp)
                .OrderByDescending(v => v.verbProps.AdjustedMeleeDamageAmount(v, pawn))
                .FirstOrDefault();

            return verb;
        }

        /// <summary>
        /// 获取对应殖民者空手时伤害最高的verb
        /// </summary>
        public static Verb GetUnarmedMeleeVerb(this Pawn pawn)
        {
            var verb = pawn?.VerbTracker?.AllVerbs?
                .Where(v => v.verbProps.IsMeleeAttack)
                .OrderByDescending(v => v.verbProps.AdjustedMeleeDamageAmount(v, pawn))
                .FirstOrDefault();

            return verb;
        }

        /// <summary>
        /// 获取殖民者当前装备的、射程最远的远程射击 verb（装备且非近战、有 defaultProjectile）。
        /// </summary>
        public static Verb GetRangedEquippedVerb(this Pawn pawn)
        {
            return pawn?.equipment?.AllEquipmentVerbs?
                .Where(v => v.verbProps.LaunchesProjectile
                            && v.verbProps.defaultProjectile != null
                            && v.EquipmentSource != null)
                .OrderByDescending(v => v.verbProps.AdjustedRange(v, pawn))
                .FirstOrDefault();
        }
    }

    public static class RangeUtilities
    {
        /// <summary>
        /// 获取直线路径附近一定范围内的地格
        /// 调用方有义务检查返回值是否在地图内
        /// </summary>
        public static IEnumerable<IntVec3> GetHitCellsAroundStraightWay(IntVec3 start, IntVec3 end, float radius)
        {
            if (start == end) return GenRadial.RadialCellsAround(start, radius, true);
            var cells = new HashSet<IntVec3>();
            var d = end - start;
            int steps = Mathf.Max(Mathf.Abs(d.x), Mathf.Abs(d.z));
            for (int i = 0; i <= steps; i++)
            {
                var cell = start + new IntVec3(d.x * i / steps, 0, d.z * i / steps);
                foreach (var c in GenRadial.RadialCellsAround(cell, radius, true))
                    cells.Add(c);
            }
            return cells;
        }

        public static IEnumerable<Pawn> GetPawnsAroundStraightWay(IntVec3 start, IntVec3 end, float radius, Pawn pawn, Func<Pawn, bool> validator)
        {
            var hit = new HashSet<Pawn>();
            if (pawn?.Map == null) return hit;
            foreach (var cell in GetHitCellsAroundStraightWay(start, end, radius).ToList())
            {
                if (!cell.InBounds(pawn.Map)) continue;
                foreach (var thing in cell.GetThingList(pawn.Map).ToList())
                {
                    if (thing is Pawn enemy && enemy != pawn && enemy.Spawned && !enemy.DeadOrDowned
                        && validator(enemy))
                        hit.Add(enemy);
                }
            }
            return hit;
        }

        public static IEnumerable<Pawn> GetEnemiesAroundStraightWay(IntVec3 start, IntVec3 end, float radius, Pawn pawn)
        {
            return GetPawnsAroundStraightWay(start, end, radius, pawn, p => p.HostileTo(pawn));
        }

        public static IntVec3 GetDest(Vector3 start, Vector3 end, Map map, Func<IntVec3, bool> validator, float step = 0.5f)
        {
            HashSet<IntVec3> cells = new();

            float distance = (end - start).magnitude;
            if (distance < 1e-5f) return start.ToIntVec3();

            var dir = (end - start).normalized;
            var endCell = start.ToIntVec3();

            for (float i = 0; i <= distance; i += step)
            {
                var cell = (start + i * dir).ToIntVec3();
                if (!cells.Contains(cell))
                    cells.Add(cell);
                else
                    continue;

                if (!cell.InBounds(map) || !validator(cell))
                    break;

                endCell = cell;
            }
            return endCell;
        }

        public static IntVec3 GetDest(Vector3 start, Vector3 end, Map map, float step = 0.5f)
        {
            HashSet<IntVec3> cells = new();

            float distance = (end - start).magnitude;
            if (distance < 1e-5f) return start.ToIntVec3();

            var dir = (end - start).normalized;
            var endCell = start.ToIntVec3();

            for (float i = 0; i <= distance; i += step)
            {
                var cell = (start + i * dir).ToIntVec3();
                if (!cells.Contains(cell))
                    cells.Add(cell);
                else
                    continue;

                if (!cell.InBounds(map))
                    break;

                endCell = cell;
            }
            return endCell;
        }
    }
}
