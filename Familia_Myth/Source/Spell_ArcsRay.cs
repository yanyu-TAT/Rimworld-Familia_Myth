using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_ArcsRay : Spell
    {
        private static ThingDef arrowDef;
        private static ThingDef ArrowDef =>
            arrowDef ??= ThingDef.Named("Projectile_ArcsRay");

        private const int BasicPenetration = 3;

        private const float HoveringDist = 1.5f;

        private const float ChargeSpawnRadiusMin = 0.5f; //汇聚粒子起始最小半径
        private const float ChargeSpawnRadiusMax = 1.2f; //汇聚粒子起始最大半径
        private const int ChargeSpawnIntervalTicks = 4;  //每4帧生成一批
        private const int ChargeParticlePerBatch = 3;    //每批粒子数

        private static readonly Vector3 ArrowTipLocalOffset = new(-0.02f, 0f, 1f);

        private static FleckDef chargeFleckDef;
        private static FleckDef ChargeFleckDef =>
            chargeFleckDef ??= DefDatabase<FleckDef>.GetNamed("ArcsRayChargeSpark");

        public int Pentration => Math.Max(1, (int)(BasicPenetration * GetCurrentManaControl()));

        public Pawn mainTarget;

        protected override void WarmUpStart()
        {
            mainTarget = verb.CurrentTarget.Pawn;
            TrySummonArrow();
        }

        private void TrySummonArrow()
        {
            if (pawn?.Map == null) return;
            //仅可存在一支光箭
            if (activeProjectiles.Any(p => p is Projectile_ArcsRay && p.Spawned)) return;

            var dir = (pawn.Rotation.FacingCell - pawn.Position).ToVector3();
            var offset = dir.normalized * HoveringDist;

            var missile = (Projectile_ArcsRay)GenSpawn.Spawn(ArrowDef, (pawn.Position + offset.ToIntVec3()).ClampInsideMap(pawn.Map), pawn.Map);
            if (missile == null) return;
            missile.Init(this);
            missile.currentState = new HoveringState_Offset(missile, offset);
            activeProjectiles.Add(missile);
        }

        protected override void WarmUpFinish(bool castDone)
        {
            if (!castDone)
                activeProjectiles.ToList().ForEach(p => p.Destroy());
            else
                NotifyAll("Shoot");
            activeProjectiles.Clear();
        }

        protected override void TickWarmUp()
        {
            base.TickWarmUp();

            //按固定间隔生成汇聚粒子
            if (CurrentTick % ChargeSpawnIntervalTicks != 0) return;

            //取当前悬浮的光箭
            var arrow = activeProjectiles.OfType<Projectile_ArcsRay>().FirstOrDefault(p => p.Spawned);
            if (arrow?.Map == null || pawn?.Map == null) return;

            //箭头视觉位置
            var tip = arrow.ExactPosition + arrow.ExactRotation * ArrowTipLocalOffset;

            for (int i = 0; i < ChargeParticlePerBatch; i++)
            {
                //在箭头周围环形随机起点
                var r = Rand.Range(ChargeSpawnRadiusMin, ChargeSpawnRadiusMax);
                var ang = Rand.Range(0f, 360f);
                var start = tip + (Quaternion.AngleAxis(ang, Vector3.up) * Vector3.forward) * r;

                //向箭头中心汇聚
                var data = FleckMaker.GetDataStatic(start, pawn.Map, ChargeFleckDef, 1f);
                data.velocity = (tip - start).normalized * Rand.Range(0.35f, 0.7f);
                data.scale = Rand.Range(0.6f, 1f);
                data.rotation = Rand.Range(0f, 360f);
                data.instanceColor = Color.white.WithAlpha(0.5f);

                pawn.Map.flecks.CreateFleck(data);
            }
        }

        //用于传给Validator的目标过滤器
        public static bool CanTarget(TargetInfo info)
        {
            if (info.Thing is not Pawn pawn || pawn.Faction == null) return false;
            return pawn.Faction.HostileTo(Faction.OfPlayer);
        }
    }
}
