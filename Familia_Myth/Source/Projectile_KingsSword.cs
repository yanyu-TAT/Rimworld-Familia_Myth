using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Projectile_KingsSword : Projectile_MagicMissileBase
    {
        //索敌间隔
        private const int ScanIntevalTicks = 60;
        //追击最大速度
        private const float ChaseSpeed = 0.5f;
        //攻击范围角
        private const float StrikeAngle = 60f;
        //残影层数
        private const int TrailCount = 10;
        //攻击动画时间
        private const int AnimTicks = 60;
        //攻击动画阶段比例：前摇/摆动/后摇
        private const float AnimWindupFrac = 0.2f;
        private const float AnimSwingFrac = 0.6f;
        //后摇占比 = 1 - windup - swing = 0.2

        //尺寸插值系数
        private const float ScaleLerpParm = 0.1f;
        //攻击冷却
        private const int StrikeInterval = 60;
        //索敌半径
        private const float ScanRadius = 40f;

        private static float HoveringSpeed => Spell_KingsSword.HoveringSpeed;

        private readonly List<TrailData> trails = new();

        private int scanCooldown = 0;
        private int strikeAnimTimer = 0;
        private float curScale = 0f;

        private Pawn curTarget;
        private int strikeCoolDown = 0;
        private bool strikeDamaged;

        private Spell_KingsSword Spell => spell as Spell_KingsSword;

        struct TrailData
        {
            public Vector3 pos;
            public Quaternion rot;
            public float scale;
        }

        //仅作动画显示，不实际碰撞，伤害直接调用范围内Thing.TakeDamage处理
        protected override bool CanHitAt(Thing thing) => false;
        public override bool CanShieldBlock => false;
        protected override bool NeedTrail => false;

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            if (Spell == null || Spell.usedTimes <= 0)
                FadeOut();

            //插值计算大小
            curScale = Mathf.Lerp(curScale, Spell.SwordScale, ScaleLerpParm);
            //每隔30帧用队列保存残影位置
            if (Find.TickManager.TicksGame % 30 == 0)
            {
                trails.Add(new TrailData
                {
                    pos = ExactPosition,
                    rot = ExactRotation,
                    scale = curScale
                });
                while (trails.Count > TrailCount)
                    trails.RemoveAt(0);
            }

            if (strikeAnimTimer > 0)
                strikeAnimTimer -= delta;

            //摆动开始时判定一次伤害（前摇结束、开始挥扫的一帧）
            if (strikeAnimTimer > 0 && !strikeDamaged && strikeAnimTimer <= StrikeDamageTick)
            {
                ApplyStrikeDamage();
                strikeDamaged = true;
            }

            if (scanCooldown > 0)
                scanCooldown -= delta;

            //攻击动画结束后冷却
            if (strikeCoolDown > 0 && strikeAnimTimer <= 0){
                strikeCoolDown -= delta;
            }
            TickAutoAttack();
        }

        private void TickAutoAttack()
        {
            //正在追击
            if (curTarget != null)
            {
                //目标已失效
                if (!IsValid(curTarget))
                {
                    //重新选择目标
                    //Log.Message($"Target Lost. Trying To Retarget.");
                    TryGetTarget();
                    return;
                }

                //正在攻击
                if (strikeAnimTimer > 0) return;

                //已移动到范围内，且攻击冷却完毕
                if (Position.DistanceTo(curTarget.Position) <= Spell.AttackRange && strikeCoolDown <= 0)
                {
                    //攻击
                    TryStrike();
                    //Log.Message("Target In Range. Trying To Attack");
                    return;
                }
            }

            //待机中，依据检测间隔尝试获取目标
            if (scanCooldown <= 0)
            {
                //Log.Message("Hovering. Trying To Get Target.");
                TryGetTarget();
            }
        }

        private void TryStrike()
        {
            if (Map == null) return;
            strikeAnimTimer = AnimTicks;
            strikeCoolDown = StrikeInterval;
            strikeDamaged = false;   // 重置，等待摆动开始时判定伤害
        }

        private int StrikeDamageTick =>
            AnimTicks - Mathf.RoundToInt(AnimTicks * 0.5f);

        private void ApplyStrikeDamage()
        {
            if (Map == null || curTarget == null) return;

            var O = Position.ToVector3();
            var A = curTarget.Position.ToVector3();
            var OAangle = (A - O).normalized.ToAngleFlat();
            var r = Spell.AttackRange;
            var sqr = r * r;
            float dmg = DamageAmount * Spell.DamageMultiplier;

            foreach (var cell in GenRadial.RadialCellsAround(Position, r, true))
            {
                foreach (var thing in cell.GetThingList(Map).ToList())
                {
                    if (!thing.HostileTo(launcher)) continue;

                    var OT = thing.Position.ToVector3() - O;
                    if (OT.sqrMagnitude > sqr) continue;

                    //计算角度
                    var OTangle = OT.ToAngleFlat();
                    if (Mathf.Abs(Mathf.DeltaAngle(OAangle, OTangle)) > StrikeAngle)
                        continue;

                    //造成伤害
                    DamageInfo dinfo = new(DamageDefOf.Cut, dmg, ArmorPenetration, OTangle, launcher);
                    var log = CombatLogHelper.MakeHitLog(launcher as Pawn, thing, Spell?.Def);
                    thing.TakeDamage(dinfo).AssociateWithLog(log);
                }
            }
        }

        private void TryGetTarget()
        {
            scanCooldown = ScanIntevalTicks;

            float sqr = ScanRadius * ScanRadius;
            var enemy = Map?.mapPawns.AllPawnsSpawned
                .Where(p => IsValid(p) && p.HostileTo(launcher))
                .OrderBy(p => p.Position.DistanceToSquared(Position))
                .FirstOrDefault(p => p.Position.DistanceToSquared(Position) <= sqr);

            curTarget = enemy;

            if (curTarget == null)
            {
                //找不到目标，回到悬浮状态
                if (currentState is not HoveringState_Orbit)
                {
                    currentState =
                        new HoveringState_Orbit(currentState, Spell_KingsSword.OrbitAngularSpeed, Spell.OrbitRadius);
                    currentState.SetMaxV(HoveringSpeed);
                    //Log.Message("Enemy Not Found. Back To Hovering");
                }
            }
            else
            {
                //找到了目标，开始追踪
                currentState = new TrackingState_WithRange(currentState, curTarget, Spell.AttackRange * 0.8f);
                currentState.SetMaxV(ChaseSpeed);
                //Log.Message($"Enemy Found: {enemy.Name.ToStringShort}");
            }
        }

        public override Quaternion ExactRotation
        {
            get
            {
                if (strikeAnimTimer > 0 && curTarget != null)
                {
                    float angle = (curTarget.Position - Position).ToVector3().ToAngleFlat();
                    float p = 1f - Mathf.Clamp01((float)strikeAnimTimer / AnimTicks);

                    float offset;
                    float windupEnd = AnimWindupFrac;

                    if (p < windupEnd)  //前摇
                    {
                        float s = SmoothStep(p / windupEnd);
                        offset = Mathf.Lerp(0f, -StrikeAngle, s);
                    }
                    else if (p < windupEnd + AnimSwingFrac) //摆动
                    {
                        float s = SmoothStep((p - windupEnd) / AnimSwingFrac);
                        offset = Mathf.Lerp(-StrikeAngle, StrikeAngle, s);
                    }
                    else    //后摇
                    {
                        float after = windupEnd + AnimSwingFrac;
                        float s = SmoothStep((p - after) / (1f - after));
                        offset = Mathf.Lerp(StrikeAngle, 0f, s);
                    }
                    return Quaternion.AngleAxis(angle + offset, Vector3.up);
                }

                if (curTarget != null && IsValid(curTarget))
                {
                    float angle = (curTarget.Position - Position).ToVector3().ToAngleFlat();
                    return Quaternion.AngleAxis(angle, Vector3.up);
                }
                return Quaternion.identity;
            }
        }

        private static float SmoothStep(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            matPropertyBlock ??= new MaterialPropertyBlock();
            var mesh = MeshPool.GridPlane(def.graphicData.drawSize);

            //纯色剪影
            int count = trails.Count;
            for (int k = 0; k < count; k++)
            {
                //根据入队顺序进行插值
                float t = count <= 1 ? 0f : k / (float)(count - 1);
                float alpha = Mathf.Lerp(0.05f, 0.30f, t);
                matPropertyBlock.SetFloat("_Flat", 1f);
                matPropertyBlock.SetColor("_Color", EdgeColor.WithAlpha(alpha));
                Graphics.DrawMesh(mesh,
                    Matrix4x4.TRS(trails[k].pos, trails[k].rot, Vector3.one * trails[k].scale),
                    DrawMat, 0, null, 0, matPropertyBlock, false, false, false);
            }

            //本体
            matPropertyBlock.SetFloat("_Flat", 0f);
            float hl = Mathf.Clamp01((Spell.SwordScale - curScale) / 0.5f);
            matPropertyBlock.SetFloat("_Highlight", hl);
            matPropertyBlock.SetColor("_GlowColor", EdgeColor.WithAlpha(0.3f));
            matPropertyBlock.SetFloat("_GlowStrength", 0.5f);
            matPropertyBlock.SetFloat("_GlowWidth", Mathf.Sin(Find.TickManager.TicksGame / 30f) * 1.5f + 2f);
            Graphics.DrawMesh(mesh,
                Matrix4x4.TRS(drawLoc, ExactRotation, Vector3.one * curScale),
                DrawMat, 0, null, 0, matPropertyBlock, false, false, false);
            Comps_PostDraw();
        }

        private bool IsValid(Pawn p) 
            => p != null && p.Spawned && !p.DeadOrDowned;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref scanCooldown, "scanCooldown");
            Scribe_Values.Look(ref strikeAnimTimer, "strikeAnimTimer");
            Scribe_Values.Look(ref strikeCoolDown, "strikeCoolDown");
            Scribe_Values.Look(ref strikeDamaged, "strikeDamaged");
            Scribe_Values.Look(ref curScale, "curScale");
            Scribe_References.Look(ref curTarget, "curTarget");
        }
    }
}
