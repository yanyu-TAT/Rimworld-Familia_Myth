using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_FusilladeFlameSpears : Spell
    {
        //扇形角度
        private const int FanAngle = 120;
        //半径/半弧长+半径
        private const float ProgressRate = 1 / (Mathf.PI / 180 * FanAngle + 1);
        //扇形弧线分段数
        private const int ArcLineCount = 60;
        //指示线宽度
        private const float IndicatorLineWIdth = 0.15f;
        //基础锁定间隔
        private const int BasicLockOnIntervalTicks = 60;

        //锁定间隔
        private int lockOnIntervalTicks = -1;
        private int lastLockOnTick = -99999;

        //射击间隔
        private const int ShootIntervalTicks = 10;
        private int lastShootTick = -99999;
        private bool isShooting = false;
        private float curPolylineAngle;
        private IntVec3 CurCell => lockedTargets.Count > 0 ? lockedTargets.First().Cell : IntVec3.Invalid;
        private IntVec3 curRandCell;
        private float prevTargetAngle;

        private static FleckDef lineIndicatorDef;
        private static FleckDef LineIndicatorDef =>
            lineIndicatorDef ??= DefDatabase<FleckDef>.GetNamed("LineIndicator");

        private static ThingDef moteDef;
        private static ThingDef MoteDef => 
            moteDef ??= DefDatabase<ThingDef>.GetNamed("Mote_TargetLock");

        private int Radius => (int)verb.EffectiveRange;

        private List<LocalTargetInfo> lockedTargets = new();

        private readonly HashSet<Pawn> pawnsLocked = new();

        private readonly Dictionary<LocalTargetInfo, Mote_TargetLock> moteTargets = new();

        private readonly static Color lockColor = new ColorInt(160, 20, 30).ToColor;

        public override void Apply(Hediff_Pharna pharna)
        {
            base.Apply(pharna);
            if (verb is Verb_CastAbility_FanTarget fanTarget)
                fanTarget.fanAngle = 120;
            else
                Log.Warning("[FamiliaMyth]Verb For Spell_FusilladeFlameSpears should have been of type Verb_CastAbility_FanTarget but not");
        }

        protected override void WarmUpStart()
        {
            base.WarmUpStart();

            //计算锁定间隔
            lockOnIntervalTicks = (int)(BasicLockOnIntervalTicks / GetCurrentSpellPower());

            //开始锁定
            lastLockOnTick = CurrentTick;
            lockedTargets.Clear();
            pawnsLocked.Clear();

            prevTargetAngle = verb.CurrentTarget.IsValid ? (verb.CurrentTarget.Cell - pawn.Position).AngleFlat : pawn.Rotation.AsAngle;
        }

        private bool CanLock(Pawn target)
        {
            //目标可锁定
            if (target == null || target.Dead || target.Destroyed || !target.Spawned)
                return false;
            //敌对
            if (!target.Faction?.HostileTo(pawn.Faction) ?? true)
                return false;

            //可视
            if (!verb.TryFindShootLineFromTo(pawn.Position, target, out _, true))
                return false;

            return CanLockPosition(target.Position);
        }

        private bool CanLockPosition(IntVec3 position)
        {
            //距离
            var distance = position.DistanceTo(pawn.Position);
            if (distance > Radius)
                return false;
            //扇形角度
            var angle = (position - pawn.Position).AngleFlat;
            var diff = Mathf.DeltaAngle(prevTargetAngle, angle);
            return Mathf.Abs(diff) <= FanAngle / 2f;
        }

        protected override void TickWarmUp()
        {
            base.TickWarmUp();

            //效果指示框每2帧重绘
            if (CurrentTick % 2 == 0)
                DrawWithProgress(verb.WarmupProgress, 0.3f);

            if (CurrentTick -  lastLockOnTick > lockOnIntervalTicks)
            {
                lastLockOnTick = CurrentTick;
                //锁定目标
                var target = pawn.Map.mapPawns.AllPawnsSpawned
                .Where(CanLock)
                .OrderBy(p => p.Position.DistanceTo(pawn.Position) * (pawnsLocked.Contains(p) ? 2 : 1))
                .FirstOrDefault();

                LocalTargetInfo localTarget;
                if (target != null){
                    localTarget = new LocalTargetInfo(target);
                    pawnsLocked.Add(target);
                }
                else
                {
                    //随机锁定范围内地格
                    var randomCell = GenRadial.RadialCellsAround(pawn.Position, Radius, true)
                        .Where(c => c.InBounds(pawn.Map) && CanLockPosition(c))
                        .RandomElementByWeight(c => 1f);
                    localTarget = new LocalTargetInfo(randomCell);
                }

                //添加锁定特效
                Mote_TargetLock mote = (Mote_TargetLock)ThingMaker.MakeThing(MoteDef);
                mote.Attach(localTarget.ToTargetInfo(pawn.Map));
                mote.exactPosition = localTarget.CenterVector3;
                mote.instanceColor = lockColor;
                GenSpawn.Spawn(mote, localTarget.Cell, pawn.Map);

                moteTargets[localTarget] = mote;
                lockedTargets.Add(localTarget);
            }
        }

        protected override void WarmUpFinish(bool castDone)
        {
            //咏唱结束时重绘完整指示线，传入更长的留存时间
            DrawWithProgress(1f, 30);
            DrawWithProgress(1f, 30);

            //初始化射击
            lastShootTick = CurrentTick - ShootIntervalTicks;
            isShooting = true;
            SetParamsForNewTarget();

            //延续僵直效果
            stunning = true;
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (!isShooting)
                return;

            //射击
            if (lockedTargets.Count > 0)
            {
                var shootProgress = (float)(CurrentTick - lastShootTick) / ShootIntervalTicks;
                if (shootProgress >= 1f)
                {
                    //过滤友军
                    var ignored = new List<Thing>();
                    foreach (var cell in GenRadial.RadialCellsAround(curRandCell, 5f, true))
                        foreach (var thing in cell.GetThingList(pawn.Map))
                            if (thing is Pawn p && p.Faction == pawn.Faction)
                                ignored.Add(p);
                    //爆
                    GenExplosion.DoExplosion(
                        curRandCell, pawn.Map, 5f, DamageDefOf.Bomb, pawn,
                        ignoredThings: ignored);
                    //删除锁定特效
                    var localTarget = lockedTargets.First();
                    while (moteTargets.TryGetValue(localTarget, out var mote))
                    {
                        mote.Destroy();
                        moteTargets.Remove(localTarget);
                    }
                    //下一个目标
                    lastShootTick = CurrentTick;
                    lockedTargets.RemoveAt(0);
                    SetParamsForNewTarget();
                }
                else  //每帧绘制
                {
                    var A = pawn.Position.ToVector3();
                    var B = curRandCell.ToVector3();
                    var O = (A + B) / 2;
                    var R = (A - B) / 2;
                    var C = O + R.magnitude * AngleToVec(curPolylineAngle + R.AngleFlat());
                    var AC = C - A;
                    var CB = B - C;
                    var a = AC.magnitude;
                    var b = CB.magnitude;
                    var pa = Mathf.Clamp01(shootProgress * (a + b) / a);
                    var pb = Mathf.Clamp01((shootProgress * (a + b) - a) / b);
                    ConnectingLine(A, A + AC * pa, 0.2f, pawn.Map);
                    ConnectingLine(C, C + CB * pb, 0.2f, pawn.Map);
                }
            }
            else
            {
                //射击结束
                isShooting = false;
                stunning = false;

                //删除所有生成的锁定特效
                foreach(var d in moteTargets) 
                    d.Value.Destroy();
                moteTargets.Clear();
            }
        }

        private void SetParamsForNewTarget()
        {
            curPolylineAngle = Rand.Range(15f, 60f);
            curRandCell = CurCell + (Rand.InsideUnitCircleVec3 * 5f).ToIntVec3();
            var angle = (curRandCell - pawn.Position).AngleFlat;
            curPolylineAngle *= Mathf.DeltaAngle(angle, prevTargetAngle) > 0 ? -1 : 1;
        }

        /// <summary>
        /// 绘制扇形指示线
        /// </summary>
        /// <param name="progress">咏唱进度</param>
        /// <param name="particleSolidTime">粒子留存时间</param>
        private void DrawWithProgress(float progress, float particleSolidTime)
        {
            if (pawn?.Map is not Map map)
                return;

            float facing = verb.CurrentTarget.IsValid ? (verb.CurrentTarget.Cell - pawn.Position).AngleFlat : pawn.Rotation.AsAngle;
            float startAngle = facing - FanAngle / 2f;
            float endAngle = facing + FanAngle / 2f;
            var center = pawn.DrawPos + Vector3.up * 0.2f;
            var curRadius = Radius * Mathf.Clamp01(progress / ProgressRate);
            var curArcAngle = FanAngle * Mathf.Clamp01((progress - ProgressRate) / (1f - ProgressRate));

            //半径
            var A = center + AngleToVec(startAngle) * curRadius;
            var B = center + AngleToVec(endAngle) * curRadius;
            ConnectingLine(center, A, particleSolidTime, map, IndicatorLineWIdth);
            ConnectingLine(center, B, particleSolidTime, map, IndicatorLineWIdth);

            if (curArcAngle > 0)
                //两端到弧中点弧线
                for (int i = 0; i < (int)(curArcAngle / FanAngle * ArcLineCount); i++)
                {
                    var t1 = (float)i / ArcLineCount;
                    var t2 = (float)(i + 1) / ArcLineCount;
                    var a1 = center + AngleToVec(Mathf.Lerp(startAngle, endAngle, t1)) * Radius;
                    var b1 = center + AngleToVec(Mathf.Lerp(startAngle, endAngle, t2)) * Radius;
                    var a2 = center + AngleToVec(Mathf.Lerp(endAngle, startAngle, t2)) * Radius;
                    var b2 = center + AngleToVec(Mathf.Lerp(endAngle, startAngle, t1)) * Radius;
                    ConnectingLine(a2, b2, particleSolidTime, map, IndicatorLineWIdth);
                    ConnectingLine(a1, b1, particleSolidTime, map, IndicatorLineWIdth);
                }
        }

        public static void ConnectingLine(Vector3 start, Vector3 end, float particleSolidTime, Map map, float width = 0.5f)
        {
            Vector3 vector = end - start;
            float x = vector.MagnitudeHorizontal();
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(start + vector * 0.5f, map, LineIndicatorDef);
            dataStatic.solidTimeOverride = particleSolidTime;
            dataStatic.exactScale = new Vector3(x, 1f, width);
            dataStatic.rotation = Mathf.Atan2(0f - vector.z, vector.x) * 57.29578f;
            dataStatic.instanceColor = new(1, 0.15f, 0.05f, 0.9f);
            map.flecks.CreateFleck(dataStatic);
        }

        private static Vector3 AngleToVec(float angleDeg) => Quaternion.AngleAxis(angleDeg, Vector3.up) * Vector3.forward;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref lockedTargets, "lockedTargets", LookMode.Reference);
        }
    }
}
