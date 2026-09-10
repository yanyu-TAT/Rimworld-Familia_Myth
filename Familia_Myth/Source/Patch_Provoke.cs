using FamiliaMyth.Abilities;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "UpdateEnemyTarget")]
    public static class Patch_Provoke
    {
        private static float Radius => Skill_Provoke.Radius;

        public static void Postfix(Pawn pawn)
        {
            try
            {
                // 只处理地图上存活的敌对单位
                if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.Downed) return;
                if (pawn.Faction == null || pawn.Faction == Faction.OfPlayer) return;

                if (Skill_Provoke.Provokers.Count == 0) return;

                var taunter = FindProvokerInRange(pawn);
                if (taunter == null) return;

                pawn.mindState.enemyTarget = taunter;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_Provoke)} error: {ex}");
            }
        }

        private static Pawn FindProvokerInRange(Pawn attacker)
        {
            var provokers = Skill_Provoke.Provokers
                .Where(p =>
                p.Map == attacker.Map && //同一地图
                attacker.Position.InHorDistOf(p.Position, Radius) && //效果范围内
                p.Faction.HostileTo(attacker.Faction) &&    //敌对目标
                attacker.CanReach(p, Verse.AI.PathEndMode.Touch, Danger.Deadly, true))  //路径可达
                .OrderBy(p => p.Position.DistanceTo(attacker.Position));    //最近目标

            return provokers.FirstOrDefault();
        }
    }
}
