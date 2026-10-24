using FamiliaMyth.Abilities;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class Patch_Railgun
    {
        private const float PathDamageRadius = 1f;      //伤害范围
        private const float WallDamageFactor = 2f;  //建筑伤害倍率

        private static FamiliaMyth.Abilities.SkillDef railgunDef;
        private static FamiliaMyth.Abilities.SkillDef RailgunDef =>
            railgunDef ??= DefDatabase<FamiliaMyth.Abilities.SkillDef>.GetNamed("Skill_Railgun", false);

        private static ThingDef moteDef;
        private static ThingDef MoteDef =>
            moteDef ??= DefDatabase<ThingDef>.GetNamed("Mote_RailgunImpact");

        public static bool Prefix(Verb_LaunchProjectile __instance)
        {
            try
            {
                if (__instance.Caster is not Pawn caster) return true;
                if (caster.equipment == null) return true;
                
                var effectHediff = caster.health?.hediffSet?.hediffs
                    ?.FirstOrDefault(h => h.def.defName == "RailgunInEffect");
                if (effectHediff?.TryGetComp<HediffComp_DisappearsWithCounter>() is not HediffComp_DisappearsWithCounter c || !c.TryConsume())
                    return true; //执行原发射

                var verb = caster.GetRangedEquippedVerb();
                if (verb == null) return true;
                if (verb.verbProps.defaultProjectile == null) return true;

                var weapon = verb.EquipmentSource;
                if (weapon == null) return true;

                var targetCell = __instance.CurrentTarget.Cell;
                var start = caster.Position;
                var delta = targetCell - start;

                if (delta.SqrMagnitude < 1e-5f) return true;
                var dir = delta.ToVector3().normalized;

                float range = verb.verbProps.AdjustedRange(verb, caster);
                DoImpact(caster, weapon, start, dir, range, verb);

                return false;
            }
            catch (Exception ex)
            {
                Log.Error("[FamiliaMyth] Patch_Railgun error: " + ex);
                return true; //出错则放行原逻辑
            }
        }

        private static void DoImpact(Pawn launcher, Thing weapon, IntVec3 start, Vector3 dir, float maxDist, Verb verb)
        {
            if (launcher?.Map == null) return;

            IntVec3 end = RangeUtilities.GetDest(start.ToVector3(), start.ToVector3() + dir * maxDist, launcher.Map,
                c =>
                {
                    foreach (var building in c.GetThingList(launcher.Map).OfType<Building>())
                    {
                        if (building.def.fillPercent >= 0.9f)
                            return false;
                    }

                    return true;
                });

            foreach (var building in GenRadial.RadialDistinctThingsAround(end, launcher.Map, PathDamageRadius * 2, true).OfType<Building>().ToList())
                Hit(launcher, weapon, verb, building);

            foreach (var pawn in RangeUtilities.GetPawnsAroundStraightWay(start, end, PathDamageRadius, launcher, p => p.Faction != launcher.Faction))
                Hit(launcher, weapon, verb, pawn);

            SpawnImpactMote(launcher.Map, start, end);
            return;
        }

        private static void SpawnImpactMote(Map map, IntVec3 a, IntVec3 b)
        {
            if (map == null) return;
            var mid = (a.ToVector3Shifted() + b.ToVector3Shifted()) * 0.5f;
            if (MoteMaker.MakeStaticMote(mid, map, MoteDef, 1f, false, 0f) is Mote mote)
            {
                mote.linearScale = new Vector3(1f, 1f, a.DistanceTo(b));
                float ang = (b - a).AngleFlat;
                mote.exactRotation = ang;
                mote.instanceColor = new Color(1f, 1f, 0.8f, 0.9f);
            }
        }

        private static void Hit(Pawn launcher, Thing weapon, Verb verb, Thing thing)
        {
            if (thing == null || !thing.Spawned) return;
            if (thing is Pawn pawn)
            {
                if (pawn.DeadOrDowned) return;
                pawn.stances.stunner.StunFor(Skill_Railgun.StunTicks, launcher, true, true);
            }
            float factor = 2f;
            if (thing is Building)
                factor *= WallDamageFactor;

            if (verb == null) return;
            float dmg = verb.verbProps.defaultProjectile.projectile.GetDamageAmount(weapon);
            float apt = verb.verbProps.defaultProjectile.projectile.GetArmorPenetration(weapon);
            var dinfo = new DamageInfo(
                verb.GetDamageDef(), dmg * factor, apt,
                0f, launcher, null, weapon?.def,
                DamageInfo.SourceCategory.ThingOrUnknown, null, false);
            dinfo.SetTool(verb.tool);
            var log = CombatLogHelper.MakeHitLog(launcher, thing, RailgunDef);
            thing.TakeDamage(dinfo).AssociateWithLog(log);
        }
    }
}
