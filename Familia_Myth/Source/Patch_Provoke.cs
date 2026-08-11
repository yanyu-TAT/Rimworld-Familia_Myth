using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "UpdateEnemyTarget")]
    public class Patch_Provoke
    {
        private static float Radius => Skill_Provoke.Radius;

        public static void Postfix(Pawn pawn)
        {
            // 只处理地图上存活的敌对单位
            if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.Downed) return;
            if (pawn.Faction == null || pawn.Faction == Faction.OfPlayer) return;

            if (Skill_Provoke.Provokers.Count == 0) return;

            var taunter = FindProvokerInRange(pawn);
            if (taunter == null) return;

            Log.Message($"Enemy {pawn.Name.ToStringShort} Provoked By {taunter.Name.ToStringShort}");
            pawn.mindState.enemyTarget = taunter;
        }

        private static Pawn FindProvokerInRange(Pawn attacker)
        {
            // 惰性清理失效的施放者
            Skill_Provoke.Provokers.RemoveWhere(p => !Skill_Provoke.IsProvokeActive(p));

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
