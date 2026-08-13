using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public abstract class BaseMagicMissileState : IExposable
    {
        protected Projectile_MagicMissileBase missile;

        //阻力系数
        protected const float k = -0.1f;

        //速度&加速度&位置
        protected Vector3 velocity;
        protected Vector3 acceleration;
        protected Vector3 position;

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

        public BaseMagicMissileState() { }

        public BaseMagicMissileState(Projectile_MagicMissileBase missile)
        {
            this.missile = missile;
            acceleration = velocity = Vector3.zero;
            position = missile.Position.ToVector3();
        }

        public BaseMagicMissileState(BaseMagicMissileState other)
        {
            velocity = other.velocity;
            acceleration = other.acceleration;
            position = other.position;
            missile = other.missile;
        }

        public virtual void UpdateInterval(int delta)
        {
            if (missile != null && missile.Spawned)
            {
                var a = acceleration + velocity * k;
                if (a.magnitude < 1e-5f)
                    a = Vector3.zero;

                velocity += a * delta;
                if (velocity.magnitude < 1e-5f)
                    velocity = Vector3.zero;

                position += velocity * delta;
            }
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref position, "position");
            Scribe_Values.Look(ref velocity, "velocity");
            Scribe_Values.Look(ref acceleration, "acceleration");
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

        public HoveringState_Offset() { }

        public override void UpdateInterval(int delta)
        {
            //模拟速度/加速度
            var prevP = position;
            var prevV = velocity;
            position = offset + Caster?.Position.ToVector3() ?? Vector3.zero;
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
    /// 追踪：锁定给定的目标移动
    /// </summary>
    public class TrackingState : BaseMagicMissileState
    {
        private Thing target;

        public TrackingState(Projectile_MagicMissileBase missile, Thing target, float a = 0.1f) : base(missile)
        {
            this.target = target;
            acceleration = new Vector3(a, 0, 0);
        }

        public TrackingState(BaseMagicMissileState other, Thing target, float a = 0.1f) : base(other)
        {
            this.target = target;
            acceleration = new Vector3(a, 0, 0);
        }

        public TrackingState() { }

        public override void UpdateInterval(int delta)
        {
            //目标丢失则保持速度直线运动
            if (target == null || !target.Spawned)
            {
                missile.currentState = new OrbitState_Line(this, velocity, velocity.magnitude);
                return;
            }

            //加速度指向目标运动
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

    /// <summary>
    /// 定轨_直线；沿给定直线按固定速度运动
    /// </summary>
    public class OrbitState_Line : BaseMagicMissileState
    {
        Vector3 direction;
        float speed;

        public OrbitState_Line(Projectile_MagicMissileBase missile, Vector3 direction, float speed = 1f) : base(missile)
        {
            this.direction = direction.normalized;
            if (direction.magnitude < 1e-5f)
                this.direction = Vector3.forward;
            this.speed = speed;
        }
        public OrbitState_Line(Projectile_MagicMissileBase missile, Vector3 start, Vector3 end, float speed = 1f) : this(missile, (end - start).normalized, speed){ }
        public OrbitState_Line(Projectile_MagicMissileBase missile, IntVec3 start, IntVec3 end, float speed = 1f) : this(missile, start.ToVector3(), end.ToVector3(), speed) { }

        public OrbitState_Line(BaseMagicMissileState other, Vector3 direction, float speed = 1f) : base(other)
        {
            this.direction = direction.normalized;
            if (direction.magnitude < 1e-5f)
                this.direction = Vector3.forward;
            this.speed = speed;
        }

        public OrbitState_Line(BaseMagicMissileState other, Vector3 start, Vector3 end, float speed = 1f) : this(other, (end - start).normalized, speed) { }
        public OrbitState_Line(BaseMagicMissileState other, IntVec3 start, IntVec3 end, float speed = 1f) : this(other, start.ToVector3(), end.ToVector3(), speed) { }

        public OrbitState_Line() { }

        public override void UpdateInterval(int delta)
        {
            acceleration = Vector3.zero;
            velocity = direction * speed;
            base.UpdateInterval(delta);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref speed, "speed");
            Scribe_Values.Look(ref direction, "direction");
        }
    }
}