using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_SolLight : Spell, ISpellWithTargetRange
    {
        private const float BasicRadius = 8f;
        private const float BasicHealBudget = 300f;
        private const int TendCount = 5;
        private const float TendQuality = 1f;

        private static ThingDef circleDef;
        private static ThingDef CircleDef =>
            circleDef ??= DefDatabase<ThingDef>.GetNamed("Mote_SolLightCircle");

        private static FleckDef healSparkDef;
        private static FleckDef HealSpark =>
            healSparkDef ??= DefDatabase<FleckDef>.GetNamed("HealSpark");

        private int elapsedTicks = -1;
        private IntVec3 targetPos;
        private Map targetMap;
        private float radius;
        private float healBudget;
        private Mote_SolLight circle;
        private bool healed;

        private bool IsActive => elapsedTicks >= 0 && elapsedTicks < 480;

        public Color HighlightColor => Def.themeColor;

        public bool CheckLOS => true;

        public bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo localTarget) => true;

        public IEnumerable<IntVec3> GetCells(LocalTargetInfo localTarget)
        {
            if (!localTarget.IsValid) return null;
            return GenRadial.RadialCellsAround(localTarget.Cell, BasicRadius * GetCurrentManaControl(), true);
        }

        protected override void WarmUpStart()
        {
            targetPos = verb.CurrentTarget.Cell;
            targetMap = pawn?.Map;
        }

        protected override void WarmUpFinish(bool castDone)
        {
            if (!castDone || pawn == null || targetMap == null || !targetPos.InBounds(targetMap))
                return;

            //咏唱结束后以最新目标格为准
            IntVec3 pos = verb.CurrentTarget.Cell;
            if (pos.InBounds(targetMap))
                targetPos = pos;

            float power = GetCurrentManaControl();
            radius = BasicRadius * power;
            healBudget = BasicHealBudget * power;
            elapsedTicks = 0;
            healed = false;

            circle = ThingMaker.MakeThing(CircleDef) as Mote_SolLight;
            if (circle == null) return;

            circle.SetRadius(radius);
            circle.SetThemeColor(Def.themeColor);
            circle.SetElapsed(0);
            GenSpawn.Spawn(circle, targetPos, targetMap);
            circle.Maintain();
        }

        public override void AbilityTick()
        {
            base.AbilityTick();

            if (elapsedTicks < 0 || elapsedTicks >= 480)
                return;

            elapsedTicks++;
            circle?.Maintain();
            circle?.SetElapsed(elapsedTicks);

            //展开 2s + 等待 1s 后治疗一次
            if (!healed && elapsedTicks >= 180)
            {
                healed = true;
                HealArea();
                SpawnHealParticles();
            }

            //闪烁后放大淡出 5s
            if (elapsedTicks >= 480)
                EndEffect();
        }

        private void EndEffect()
        {
            if (circle != null && !circle.Destroyed)
                circle.Destroy();
            circle = null;
            elapsedTicks = -1;
        }

        private void HealArea()
        {
            if (targetMap == null || !targetPos.InBounds(targetMap)) return;

            float r2 = radius * radius;
            var targets = targetMap.mapPawns.AllPawnsSpawned
                .Where(p => !p.Dead && p.Faction == pawn.Faction
                            && p.Position.DistanceToSquared(targetPos) <= r2)
                .ToList();
            if (targets.NullOrEmpty()) return;

            foreach (var p in targets)
                p.TendInjuries(TendCount, TendQuality, pawn);

            float budget = healBudget;
            foreach (var p in targets)
            {
                budget = p.HealInjuries(budget, pawn);
                if (budget <= 0f) break;
            }
        }

        private void SpawnHealParticles()
        {
            if (targetMap == null) return;

            Color lightGreen = Color.Lerp(Def.themeColor, Color.white, 0.35f);
            Vector3 basePos = targetPos.ToVector3Shifted() + new Vector3(0f, 3f, 0f);

            for (int i = 0; i < 30; i++)
            {
                Vector3 start = basePos + Rand.InsideUnitCircleVec3 * radius * 0.7f;
                var data = FleckMaker.GetDataStatic(start, targetMap, HealSpark, Rand.Range(0.8f, 1.6f));
                data.velocity = new Vector3(Rand.Range(-0.15f, 0.15f), -Rand.Range(2f, 3.5f), Rand.Range(-0.15f, 0.15f));
                data.rotation = Rand.Range(0f, 360f);
                data.instanceColor = lightGreen;
                targetMap.flecks.CreateFleck(data);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref elapsedTicks, "elapsedTicks", -1);
            Scribe_Values.Look(ref targetPos, "targetPos");
            Scribe_Values.Look(ref radius, "radius");
            Scribe_Values.Look(ref healBudget, "healBudget");
            Scribe_Values.Look(ref healed, "healed");
            Scribe_References.Look(ref targetMap, "targetMap");
            Scribe_References.Look(ref circle, "circle");

            if (Scribe.mode != LoadSaveMode.PostLoadInit) return;

            if (!IsActive || targetMap == null || circle == null || circle.Destroyed)
            {
                EndEffect();
                return;
            }

            circle.Maintain();
            circle.SetRadius(radius);
            circle.SetThemeColor(Def.themeColor);
            circle.SetElapsed(elapsedTicks);
        }
    }
}
