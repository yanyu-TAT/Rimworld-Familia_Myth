using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Patch
{
    /// <summary>
    /// 烈焰魔剑附魔：近战命中时消耗一次计数，并在施法者前方展开扇形火焰。
    /// </summary>
    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    public static class Patch_FlameSword
    {
        //基础伤害与扇形尺寸
        private const float BasicDamage = 30f;
        private const float Radius = 3f;
        private const float HalfAngle = 22.5f;
        private const float ArmorPenetration = 0.3f;
        //火焰特效
        private const int SparkCount = 12;
        private const int HeatGlowCount = 6;
        private const int SmokeCount = 3;
        private const float BlastFlameScaleMin = 0.75f;
        private const float BlastFlameScaleMax = 1.2f;
        private const float EffectMinDistance = 0.6f;

        private static HediffDef effectDef;
        private static HediffDef EffectDef =>
            effectDef ??= DefDatabase<HediffDef>.GetNamed("FlameSwordInEffect");

        private static StatDef manaControlDef;
        private static StatDef ManaControlDef =>
            manaControlDef ??= DefDatabase<StatDef>.GetNamed("ManaControl_Enchant");

        private static FleckDef blastFlameDef;
        private static FleckDef BlastFlameDef =>
            blastFlameDef ??= DefDatabase<FleckDef>.GetNamed("BlastFlame");

        private static FleckDef microSparksDef;
        private static FleckDef MicroSparksDef =>
            microSparksDef ??= DefDatabase<FleckDef>.GetNamed("MicroSparksFast");

        private static FleckDef heatGlowDef;
        private static FleckDef HeatGlowDef =>
            heatGlowDef ??= DefDatabase<FleckDef>.GetNamed("HeatGlow_Intense");

        private static FleckDef smokeDef;
        private static FleckDef SmokeDef =>
            smokeDef ??= DefDatabase<FleckDef>.GetNamed("SmokeGrowing");

        public static void Postfix(Verb_MeleeAttack __instance, bool __result)
        {
            try
            {
                if (!__result) return;
                if (__instance.CasterPawn is not Pawn caster || caster.Map == null || caster.health == null)
                    return;

                var hediff = caster.health.hediffSet.GetFirstHediffOfDef(EffectDef);
                if (hediff?.TryGetComp<HediffComp_DisappearsWithCounter>() is not HediffComp_DisappearsWithCounter counter)
                    return;

                //恩惠或魔法已移除时清掉残留效果
                if (caster.GetPharna()?.spells?.OfType<Spell_FlameSword>().Any() != true)
                {
                    caster.health.RemoveHediff(hediff);
                    return;
                }

                LocalTargetInfo target = __instance.CurrentTarget;
                if (!target.IsValid) return;

                IntVec3 targetCell = target.HasThing && target.Thing.Spawned ? target.Thing.Position : target.Cell;
                if (!targetCell.InBounds(caster.Map)) return;

                if (!counter.TryConsume()) return;

                Vector3 origin = caster.Position.ToVector3Shifted();
                float aim = (targetCell.ToVector3Shifted() - origin).AngleFlat();
                int damage = Mathf.Max(1, (int)(BasicDamage * caster.GetStatValue(ManaControlDef)));

                var fanCells = new List<IntVec3>();
                DamageSector(caster, origin, aim, damage, fanCells);
                SpawnFlecks(caster, origin, aim, fanCells);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_FlameSword)} error: {ex}");
            }
        }

        private static void DamageSector(Pawn caster, Vector3 origin, float aim, int damage, List<IntVec3> fanCells)
        {
            var damaged = new HashSet<Thing>();
            foreach (var cell in GenRadial.RadialCellsAround(caster.Position, Radius, true))
            {
                if (!cell.InBounds(caster.Map)) continue;

                Vector3 offset = cell.ToVector3Shifted() - origin;
                if (offset.sqrMagnitude < 0.01f) continue;
                if (Mathf.Abs(Mathf.DeltaAngle(aim, offset.AngleFlat())) > HalfAngle) continue;

                fanCells.Add(cell);

                foreach (var thing in cell.GetThingList(caster.Map).ToList())
                {
                    if (thing == caster || thing.Destroyed || !damaged.Add(thing)) continue;
                    if (thing is Pawn pawn && pawn.Faction == caster.Faction) continue;

                    thing.TakeDamage(new DamageInfo(DamageDefOf.Flame, damage, ArmorPenetration, aim, caster));
                }
            }
        }

        private static void SpawnFlecks(Pawn caster, Vector3 origin, float aim, List<IntVec3> fanCells)
        {
            Map map = caster.Map;
            foreach (var cell in fanCells)
            {
                Vector3 pos = cell.ToVector3Shifted();
                float distance = Mathf.Clamp((pos - origin).magnitude, 0f, Radius);
                float scale = Mathf.Lerp(BlastFlameScaleMin, BlastFlameScaleMax, 1f - distance / Radius);
                var data = FleckMaker.GetDataStatic(pos, map, BlastFlameDef, scale);
                map.flecks.CreateFleck(data);
            }

            SpawnFanParticles(caster, origin, aim, MicroSparksDef, SparkCount,
                minScale: 0.35f, maxScale: 0.7f, minSpeed: 1.2f, maxSpeed: 2.2f,
                color: new Color(1f, 0.55f, 0.15f, 1f));
            SpawnFanParticles(caster, origin, aim, HeatGlowDef, HeatGlowCount,
                minScale: 0.8f, maxScale: 1.4f, minSpeed: 0.4f, maxSpeed: 1.0f,
                color: new Color(1f, 0.4f, 0.1f, 0.85f));
            SpawnFanParticles(caster, origin, aim, SmokeDef, SmokeCount,
                minScale: 0.5f, maxScale: 0.9f, minSpeed: 0.2f, maxSpeed: 0.6f,
                color: new Color(0.3f, 0.28f, 0.25f, 0.55f));
        }

        private static void SpawnFanParticles(Pawn caster, Vector3 origin, float aim, FleckDef def,
            int count, float minScale, float maxScale, float minSpeed, float maxSpeed, Color color)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = aim + Rand.Range(-HalfAngle, HalfAngle);
                Vector3 offset = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward
                    * Rand.Range(EffectMinDistance, Radius);
                var data = FleckMaker.GetDataStatic(origin + offset, caster.Map, def, Rand.Range(minScale, maxScale));
                data.rotation = Rand.Range(0f, 360f);
                data.velocity = offset.normalized * Rand.Range(minSpeed, maxSpeed);
                data.instanceColor = color;
                caster.Map.flecks.CreateFleck(data);
            }
        }
    }
}
