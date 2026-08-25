using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 魔法弹行为状态基类，默认行为为保持初始化时的加速度和速度移动
    /// </summary>
    public class BaseMagicMissileState : IExposable
    {
        protected Projectile_MagicMissileBase missile;

        //空气阻力系数
        protected const float Ck = -0.1f;
        //固定位置移动时的插值系数
        protected const float Ct = 0.5f;
        //自然减速的插值系数
        protected const float SlowDownFactor = 0.5f;

        //速度&加速度&位置
        protected Vector3 velocity;
        protected Vector3 acceleration;
        protected Vector3 position;

        //通过插值计算位置
        protected Vector3 GetLerpedPosition(int delta, Vector3 to)
        {
            Vector3 res = position;
            for (int i = 0; i < delta; i++)
            {
                res = new Vector3()
                {
                    x = Mathf.Lerp(res.x, to.x, Ct),
                    y = Mathf.Lerp(res.y, to.y, Ct),
                    z = Mathf.Lerp(res.z, to.z, Ct)
                };
            }

            //未设置速度上限，直接返回
            if (!maxV.HasValue) return res;

            return position + ClampVector(res - position, maxV.Value * delta);
        }

        protected Vector3 ClampVector(Vector3 vec, float length)
        {
            if (vec.sqrMagnitude <= length * length)
                return vec;
            return vec.normalized * Mathf.Clamp(vec.magnitude, 0, length);
        }

        //最大加速度/速度
        public float? maxA;
        public float? maxV;

        //是否自然减速
        protected bool slowDown;

        public Vector3 Velocity => velocity;
        public Vector3 Acceleration => acceleration;

        public virtual bool CanHit => true;

        public Vector3 ExactPosition => position;

        public Pawn Caster => missile?.Launcher as Pawn;

        public void SetV(Vector3 v)
        {
            velocity = v;
        }

        public void SetA(Vector3 v)
        {
            acceleration = v;
        }

        public void SetMaxV(float f)
        {
            maxV = f;
        }

        public void SetMaxA(float f)
        {
            maxA = f;
        }

        public BaseMagicMissileState(Projectile_MagicMissileBase missile, bool slowDown = false)
        {
            this.missile = missile;
            acceleration = velocity = Vector3.zero;
            position = missile.Position.ToVector3();
            this.slowDown = slowDown;
        }

        public BaseMagicMissileState(BaseMagicMissileState other, bool slowDown = false)
        {
            velocity = other.velocity;
            acceleration = other.acceleration;
            position = other.position;
            missile = other.missile;
            this.slowDown = slowDown;
        }

        public virtual void UpdateInterval(int delta)
        {
            if (missile != null && missile.Spawned)
            {
                if (slowDown)
                    for (int i = 0; i < delta; i++)
                    {
                        float f = Mathf.Lerp(acceleration.magnitude, 0, SlowDownFactor);
                        acceleration = acceleration.normalized * f;
                    }

                var a = acceleration + velocity * Ck;
                if (a.magnitude < 1e-5f)
                    a = Vector3.zero;
                if (maxA.HasValue)
                    a = ClampVector(a, maxA.Value);

                velocity += a * delta;
                if (velocity.magnitude < 1e-5f)
                    velocity = Vector3.zero;
                if (maxV.HasValue)
                    velocity = ClampVector(velocity, maxV.Value);

                position += velocity * delta;
            }
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref position, "position");
            Scribe_Values.Look(ref velocity, "velocity");
            Scribe_Values.Look(ref acceleration, "acceleration");
            Scribe_Values.Look(ref slowDown, "slowDown");
            Scribe_Values.Look(ref maxA, "maxA");
            Scribe_Values.Look(ref maxV, "maxV");
            Scribe_References.Look(ref missile, "missile");
        }
    }

    /// <summary>
    /// 静止悬浮：与施法者保持相对位置不变
    /// </summary>
    public class HoveringState_Offset : BaseMagicMissileState
    {
        private Vector3 offset;

        public override bool CanHit => false;

        public HoveringState_Offset(Projectile_MagicMissileBase missile, Vector3 offset) : base(missile)
        {
            this.offset = offset;
            position = Caster.Position.ToVector3() + offset;
            acceleration = velocity = Vector3.zero;
        }

        public override void UpdateInterval(int delta)
        {
            var prevP = position;
            var prevV = velocity;
            //通过插值改变位置
            position = GetLerpedPosition(delta, offset + Caster?.Position.ToVector3() ?? Vector3.zero);
            //模拟速度/加速度
            velocity = (position - prevP) / delta;
            acceleration = (velocity - prevV) / delta;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref offset, "offset");
        }
    }

    /// <summary>
    /// 环绕悬浮：以固定半径绕施法者做圆周运动
    /// </summary>
    public class HoveringState_Orbit : BaseMagicMissileState
    {
        //角速度
        private float w;
        //半径
        private float r;
        //当前角度
        private float angle;

        public override bool CanHit => false;

        public HoveringState_Orbit(Projectile_MagicMissileBase missile, float angularSpeed, float radius) : base(missile)
        {
            w = angularSpeed;
            r = radius;
            var vec = Rand.UnitVector3;
            angle = vec.ToAngleFlat();
        }

        public HoveringState_Orbit(BaseMagicMissileState other, float angularSpeed, float radius) : base(other)
        {
            w = angularSpeed;
            r = radius;
            var vec = Rand.UnitVector3;
            angle = vec.ToAngleFlat();
        }

        public override void UpdateInterval(int delta)
        {
            var prevP = position;
            var prevV = velocity;

            angle += w;
            position = GetLerpedPosition(delta, Caster.Position.ToVector3() + r * new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)));

            //模拟速度/加速度
            velocity = (position - prevP) / delta;
            acceleration = (velocity - prevV) / delta;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref w, "w");
            Scribe_Values.Look(ref r, "r");
            Scribe_Values.Look(ref angle, "angle");
        }
    }

    /// <summary>
    /// 追踪：锁定给定的目标移动
    /// </summary>
    public class TrackingState : BaseMagicMissileState
    {
        protected Thing target;

        public TrackingState(Projectile_MagicMissileBase missile, Thing target, float a = 0.1f) : base(missile)
        {
            this.target = target;
            acceleration = a * Vector3.forward;
        }

        public TrackingState(BaseMagicMissileState other, Thing target, float a = 0.1f) : base(other)
        {
            this.target = target;
            acceleration = a * Vector3.forward;
        }

        public override void UpdateInterval(int delta)
        {

            //目标丢失则保持加速度速度继续运动
            if (target == null || !target.Spawned)
            {
                missile.currentState = new OrbitState_Line(this, velocity, velocity.magnitude);
                return;
            }

            //加速度指向目标运动
            if (acceleration.magnitude > 1e-5f)
                acceleration = (target.Position.ToVector3() - ExactPosition).normalized * acceleration.magnitude;
            base.UpdateInterval(delta);

            //吸附到目标格，防止过速导致的抖动
            if ((target.Position.ToVector3() - position).sqrMagnitude < 0.3f * 0.3f)
            {
                position = target.Position.ToVector3();
                velocity = Vector3.zero;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref target, "target");
        }
    }

    public class TrackingState_WithRange : TrackingState
    {
        private float radius;

        public TrackingState_WithRange(Projectile_MagicMissileBase missile, Thing target, float radius, float a = 0.1f) : base(missile, target, a)
        {
            this.radius = radius;
            maxA = a;
        }

        public TrackingState_WithRange(BaseMagicMissileState other, Thing target, float radius, float a = 0.1f) : base(other, target, a)
        {
            this.radius = radius;
            maxA = a;
        }

        public override void UpdateInterval(int delta)
        {
            //目标丢失则保持加速度速度继续运动
            if (target == null || !target.Spawned)
            {
                missile.currentState = new OrbitState_Line(this, velocity, velocity.magnitude);
                return;
            }

            //目标在范围内，不再加速
            if ((target.Position.ToVector3() - position).sqrMagnitude <= radius * radius)
                acceleration = Vector3.zero;
            else if (maxA.HasValue)
                acceleration = Vector3.forward * maxA.Value;
            else
            {
                Log.Warning($"[FamiliaMyth] Magic Missile Named {missile.Label} Lauched By {Caster} Can't Get MaxA In TrackingState_WithRange. It's Now Stopped Accelerate.");
                maxA = 0;
            }
            base.UpdateInterval(delta);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref radius, "radius");
        }
    }

    /// <summary>
    /// 定轨_直线；沿给定直线按固定速度运动
    /// </summary>
    public class OrbitState_Line : BaseMagicMissileState
    {
        Vector3 direction;

        public OrbitState_Line(Projectile_MagicMissileBase missile, Vector3 direction, float speed = 1f) : base(missile)
        {
            this.direction = direction.normalized;
            if (direction.magnitude < 1e-5f)
                this.direction = Vector3.forward;
            maxV = speed;
        }
        public OrbitState_Line(Projectile_MagicMissileBase missile, Vector3 start, Vector3 end, float speed = 1f) : this(missile, (end - start).normalized, speed){ }
        public OrbitState_Line(Projectile_MagicMissileBase missile, IntVec3 start, IntVec3 end, float speed = 1f) : this(missile, start.ToVector3(), end.ToVector3(), speed) { }

        public OrbitState_Line(BaseMagicMissileState other, Vector3 direction, float speed = 1f) : base(other)
        {
            this.direction = direction.normalized;
            if (direction.magnitude < 1e-5f)
                this.direction = Vector3.forward;
            maxV = speed;
        }

        public OrbitState_Line(BaseMagicMissileState other, Vector3 start, Vector3 end, float speed = 1f) : this(other, (end - start).normalized, speed) { }
        public OrbitState_Line(BaseMagicMissileState other, IntVec3 start, IntVec3 end, float speed = 1f) : this(other, start.ToVector3(), end.ToVector3(), speed) { }

        public override void UpdateInterval(int delta)
        {
            acceleration = Vector3.zero;
            if (maxV.HasValue)
                velocity = direction * maxV.Value;
            else
            {
                Log.Warning($"[FamiliaMyth] Magic Missile Named {missile.Label} Lauched By {Caster} Can't Get MaxA In TrackingState_WithRange. It's Now Stopped Moving.");
                maxV = 0f;
            }
            base.UpdateInterval(delta);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref direction, "direction");
        }
    }
}