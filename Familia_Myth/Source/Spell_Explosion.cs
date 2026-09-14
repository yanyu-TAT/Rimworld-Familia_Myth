using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_Explosion : Spell, ISpellWithTargetRange
    {
        private static DamageDef damageDef;
        private static DamageDef DamageDef =>
            damageDef ??= DefDatabase<DamageDef>.GetNamed("SpellExplosion");

        private static ThingDef circleDef;
        private static ThingDef CircleDef =>
            circleDef ??= DefDatabase<ThingDef>.GetNamed("Mote_ExplosionCircle");

        private static ThingDef distortionDef;
        private static ThingDef DistortionDef =>
            distortionDef ??= DefDatabase<ThingDef>.GetNamed("Mote_ExplosionDistortion");

        private static FleckDef cachedChargeSpark;
        private static FleckDef ChargeSpark =>
            cachedChargeSpark ??= DefDatabase<FleckDef>.GetNamed("ArcsRayChargeSpark"); //复用灵弓光箭的魔力粒子

        private static HediffDef manaExhaustion;
        private static HediffDef ManaExhaustion =>
            manaExhaustion ??= DefDatabase<HediffDef>.GetNamed("Hediff_ManaExhaustion");

        private const float ChargeSpawnRadiusMin = 1.5f; //汇聚粒子起始最小半径
        private const float ChargeSpawnRadiusMax = 3f; //汇聚粒子起始最大半径
        private const int ChargeSpawnIntervalTicks = 1;  //生成间隔
        private const int ChargeParticlePerBatch = 5;    //每批粒子数

        //爆炸延迟
        private const int DelayTicks = 60;
        //基础伤害
        private const float BasicDamage = 200f;
        //基础范围
        private const float BasicRadius = 20f;

        private IntVec3 targetPos;
        private Map targetMap;

        private int delayTicksLeft;
        private float damage;
        private float radius;

        private Mote_ExplosionCircle circle;
        private Mote_ExplosionDistortion distortion;

        private bool WaitingForExplosion => delayTicksLeft > 0;

        public Color HighlightColor => Color.red;

        protected override void WarmUpStart()
        {
            targetPos = verb.CurrentTarget.Cell;
            targetMap = pawn?.Map;
            SpawnCircle();

            delayTicksLeft = -1;
        }

        private void SpawnCircle()
        {
            if (targetMap == null || !targetPos.InBounds(targetMap))
                return;

            circle = (Mote_ExplosionCircle)ThingMaker.MakeThing(CircleDef);
            if (circle == null) return;
            circle.Init(WarmupTicks);
            GenSpawn.Spawn(circle, targetPos, targetMap);
            circle.Maintain();
        }

        protected override void TickWarmUp()
        {
            //目标点处法阵
            circle?.Maintain();

            //魔力汇聚粒子（同灵弓光箭）
            if (targetMap == null) return;
            if (CurrentTick % ChargeSpawnIntervalTicks != 0) return;
            var center = pawn.DrawPos;
            for (int i = 0; i < ChargeParticlePerBatch; i++)
            {
                var r = Rand.Range(ChargeSpawnRadiusMin, ChargeSpawnRadiusMax);
                var ang = Rand.Range(0f, 360f);
                var start = center + (Quaternion.AngleAxis(ang, Vector3.up) * Vector3.forward) * r;
                var data = FleckMaker.GetDataStatic(start, pawn.Map, ChargeSpark, 1f);
                data.velocity = (center - start).normalized * Rand.Range(0.35f, 0.7f);
                data.scale = Rand.Range(0.6f, 1f);
                data.rotation = Rand.Range(0f, 360f);
                data.instanceColor = Def.themeColor.WithAlpha(0.7f);
                pawn.Map.flecks.CreateFleck(data);
            }
        }

        protected override void WarmUpFinish(bool castDone)
        {
            if (!castDone) return;

            float power = GetCurrentManaControl();
            damage = power * BasicDamage;
            radius = power * BasicRadius;

            SpawnDistortionWave();

            if (pawn?.health != null && !pawn.Dead)
                pawn.health.AddHediff(ManaExhaustion);

            delayTicksLeft = DelayTicks;
            stunning = true;
        }

        private void SpawnDistortionWave()
        {
            if (targetMap == null || !targetPos.InBounds(targetMap))
                return;

            distortion = (Mote_ExplosionDistortion)ThingMaker.MakeThing(DistortionDef);
            if (distortion == null) return;
            distortion.Init(radius, DelayTicks);
            GenSpawn.Spawn(distortion, targetPos, targetMap);
            distortion.Maintain();
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (!stunning || !WaitingForExplosion) return;
            if (targetMap == null) return;

            circle?.Maintain();
            distortion?.Maintain();

            delayTicksLeft--;

            if (delayTicksLeft <= 0)
            {
                TriggerExplosion();
                stunning = false;
            }
        }

        private void TriggerExplosion()
        {
            circle?.FadeOut();
            if (targetMap == null || !targetPos.InBounds(targetMap))
                return;

            GenExplosion.DoExplosion(
                targetPos, targetMap,
                radius, DamageDef, pawn,
                (int)damage,
                armorPenetration: 0.8f,
                chanceToStartFire: 0.5f,
                damageFalloff: true,
                screenShakeFactor: 1f);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref damage, "damage");
            Scribe_Values.Look(ref radius, "radius");
            Scribe_Values.Look(ref delayTicksLeft, "delayTicksLeft");
            Scribe_Values.Look(ref targetPos, "targetPos");
            Scribe_References.Look(ref targetMap, "targetMap");
            Scribe_References.Look(ref circle, "circle");
            Scribe_References.Look(ref distortion, "distortion");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                circle?.Maintain();
                distortion?.Maintain();
            }
        }

        private float radiusForHighlight = -1;

        public IEnumerable<IntVec3> GetCells(LocalTargetInfo localTarget)
        {
            if (radiusForHighlight < 0 || Find.TickManager.TicksGame % 30 == 0)
                radiusForHighlight = GetCurrentManaControl() * BasicRadius;

            if (!localTarget.IsValid)
                return null;

            return GenRadial.RadialCellsAround(localTarget.Cell, radiusForHighlight, true);
        }

        public bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo localTarget)
        {
            throw new NotImplementedException();
        }
    }
}
