using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 魔法弹行为状态基类（纯运动学积分器）
    /// - 提供位置、速度、加速度及最大限幅
    /// - 默认 UpdateInterval 执行标准欧拉积分（无阻力）
    /// - 子类应重写 UpdateInterval 来实现弹簧等自定义加速度
    /// </summary>
    public class BaseMagicMissileState : IExposable
    {
        protected Projectile_MagicMissileBase missile;

        // 运动学参数
        protected Vector3 velocity;
        protected Vector3 acceleration;
        protected Vector3 position;

        // 最大限幅（可选）
        public float? maxA;
        public float? maxV;

        public virtual bool CanHit => true;

        public Vector3 Velocity => velocity;
        public Vector3 Acceleration => acceleration;
        public Vector3 ExactPosition => position;
        public Pawn Caster => missile?.Launcher as Pawn;

        // 构造函数
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
            maxA = other.maxA;   // 补拷贝限幅，避免状态转换时丢失
            maxV = other.maxV;
        }

        /// <summary>
        /// 默认物理更新：纯欧拉积分（无阻力），子类可重写并选择是否调用 base
        /// </summary>
        public virtual void UpdateInterval(int delta)
        {
            // 加速度限幅
            if (maxA.HasValue)
                acceleration = ClampVector(acceleration, maxA.Value);

            velocity += acceleration * delta;

            if (maxV.HasValue)
                velocity = ClampVector(velocity, maxV.Value);

            position += velocity * delta;
        }

        // 向量钳制（限制长度）
        protected Vector3 ClampVector(Vector3 vec, float length)
        {
            if (vec.sqrMagnitude <= length * length)
                return vec;
            return vec.normalized * length;
        }

        // 基础属性设置（外部调用）
        public void SetV(Vector3 v) => velocity = v;
        public void SetA(Vector3 v) => acceleration = v;
        public void SetMaxV(float f) => maxV = f;
        public void SetMaxA(float f) => maxA = f;

        // 存档接口
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref position, "position");
            Scribe_Values.Look(ref velocity, "velocity");
            Scribe_Values.Look(ref acceleration, "acceleration");
            Scribe_Values.Look(ref maxA, "maxA");
            Scribe_Values.Look(ref maxV, "maxV");
            Scribe_References.Look(ref missile, "missile");
        }
    }

    /// <summary>
    /// 弹簧-阻尼状态基类：向目标点施加弹簧力与阻尼并积分
    /// stiffness/damping 为每秒（连续时间）物理值，SpringTo 内按本次实际步长换算
    /// 子类只需计算目标点并调用 SpringTo
    /// </summary>
    public abstract class SpringStateBase : BaseMagicMissileState
    {
        protected float stiffness;
        protected float damping;

        public SpringStateBase(Projectile_MagicMissileBase missile, float stiffness, float damping) : base(missile)
        {
            this.stiffness = stiffness;
            this.damping = damping;
        }

        public SpringStateBase(BaseMagicMissileState other, float stiffness, float damping) : base(other)
        {
            this.stiffness = stiffness;
            this.damping = damping;
        }

        /// <summary>
        /// 向目标点施加弹簧-阻尼加速度并执行限幅与积分
        /// </summary>
        protected void SpringTo(int delta, Vector3 target, Vector3 feedforward = default)
        {
            // 将连续时间物理值换算到本次积分步长
            float dt = delta / 60f;  // RimWorld 标准：1 tick = 1/60 秒

            Vector3 toTarget = target - position;
            float dist = toTarget.magnitude;
            if (dist < 0.001f) dist = 0.001f;
            Vector3 dir = toTarget / dist;

            //径向速度
            float radialVel = Vector3.Dot(velocity, dir);

            // 弹簧项（胡克：力 ∝ 位移）× dt²、阻尼项 × dt（速度 格/tick、加速度 格/tick²）+ 外部前馈（如向心/切向）
            acceleration = stiffness * toTarget * (dt * dt)
                         - damping * radialVel * dir * dt
                         + feedforward;

            //限幅与积分
            base.UpdateInterval(delta);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref stiffness, "stiffness");
            Scribe_Values.Look(ref damping, "damping");
        }
    }

    /// <summary>
    /// 偏移悬浮：弹簧拉向相对施法者的位置
    /// </summary>
    public class HoveringState_Offset : SpringStateBase
    {
        private Vector3 offset;

        public override bool CanHit => false;

        public HoveringState_Offset(Projectile_MagicMissileBase missile, Vector3 offset,
            float stiffness = 20f, float damping = 10f) : base(missile, stiffness, damping)
        {
            this.offset = offset;
            position = Caster.Position.ToVector3Shifted() + offset;
            velocity = Vector3.zero;
        }

        public HoveringState_Offset(BaseMagicMissileState other, Vector3 offset,
            float stiffness = 20f, float damping = 10f) : base(other, stiffness, damping)
        {
            this.offset = offset;
        }

        public override void UpdateInterval(int delta)
        {
            if (Caster == null || !Caster.Spawned) return;
            SpringTo(delta, Caster.Position.ToVector3Shifted() + offset);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref offset, "offset");
        }
    }

    /// <summary>
    /// 环绕悬浮：以固定半径绕施法者做圆周运动
    /// 位置由角度解析确定（angle 以度存储，计算位置时换算弧度），弹簧拉向轨道点
    /// </summary>
    public class HoveringState_Orbit : SpringStateBase
    {
        private float w;        // 角速度 (度/tick)
        private float r;        // 半径
        public float angle;     // 当前角度（度）

        public override bool CanHit => false;

        public HoveringState_Orbit(Projectile_MagicMissileBase missile, float angularSpeed, float radius,
            float? angle = null, float stiffness = 25f, float damping = 10f) : base(missile, stiffness, damping)
        {
            w = angularSpeed;
            r = radius;
            this.angle = angle ?? Rand.Range(0f, 360f);
        }

        public HoveringState_Orbit(BaseMagicMissileState other, float angularSpeed, float radius,
            float? angle = null, float stiffness = 25f, float damping = 10f) : base(other, stiffness, damping)
        {
            w = angularSpeed;
            r = radius;
            this.angle = angle ?? Rand.Range(0f, 360f);
        }

        // 重设轨道角度（外部调用，例如槽位重排）
        public void SetAngle(float newAngle)
        {
            angle = newAngle;
        }

        public override void UpdateInterval(int delta)
        {
            if (Caster == null || !Caster.Spawned) return;

            // 角度累计（度），w 为度/tick
            angle += w * delta;

            // 目标轨道点：换算弧度后计算位置
            float rad = angle * Mathf.Deg2Rad;
            Vector3 center = Caster.Position.ToVector3Shifted();
            Vector3 target = center + r * new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));

            float dt = delta / 60f;     // 1 tick = 1/60 秒
            float omega = w * Mathf.Deg2Rad;    // rad/tick

            // 当前实时半径（水平面内到圆心的距离）
            Vector3 radialVec = position - center;
            radialVec.y = 0;
            float currentRadius = radialVec.magnitude;

            Vector3 extra = Vector3.zero;

            // 前馈均基于圆心→光球的径向，与半径/角度解耦，避免交叉耦合振荡
            if (currentRadius > 1e-5f)
            {
                Vector3 rhat = radialVec / currentRadius;

                // 随动半径向心前馈：按当前位置提供匹配的旋转惯性，弹簧只纠半径/相位误差
                extra -= rhat * (omega * omega * currentRadius);

                // 真实轨道切向（逆时针）：只沿切向阻尼，不注入径向力
                // 目标切向速度按目标半径 r 匹配（而非当前半径），避免滞后导致半径收缩
                Vector3 shat = new(-rhat.z, 0, rhat.x);
                float vTan = Vector3.Dot(velocity, shat);
                float relTangVel = vTan - omega * r;
                extra -= damping * relTangVel * shat * dt;
            }

            SpringTo(delta, target, extra);
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
    /// 追踪：弹簧拉向目标
    /// </summary>
    public class TrackingState : SpringStateBase
    {
        protected Thing target;

        public TrackingState(Projectile_MagicMissileBase missile, Thing target,
            float stiffness = 12f, float damping = 6.5f) : base(missile, stiffness, damping)
        {
            this.target = target;
        }

        public TrackingState(BaseMagicMissileState other, Thing target,
            float stiffness = 12f, float damping = 6.5f) : base(other, stiffness, damping)
        {
            this.target = target;
        }

        public override void UpdateInterval(int delta)
        {
            // 目标丢失则切换为直线匀速（保持当前速度方向）
            if (target == null || !target.Spawned)
            {
                missile.currentState = new OrbitState_Line(this, velocity, velocity.magnitude);
                return;
            }

            Vector3 targetPos = target.Position.ToVector3Shifted();
            float dist = (targetPos - position).magnitude;

            // 极近距离：用强阻尼自然停下，避免硬吸附
            if (dist < 0.5f)
            {
                velocity *= 0.9f;
                position = Vector3.Lerp(position, targetPos, 0.5f);
                acceleration = Vector3.zero;
                return;
            }

            //相对目标的切向阻尼（目标速度0），消除继承的环绕切向速度，避免绕目标转圈
            Vector3 toTarget = targetPos - position;
            toTarget.y = 0;
            Vector3 tangDamp = Vector3.zero;
            if (toTarget.sqrMagnitude > 1e-6f)
            {
                var tdir = new Vector3(-toTarget.z, 0f, toTarget.x).normalized;
                float vTan = Vector3.Dot(velocity, tdir);
                tangDamp = -damping * vTan * tdir * (delta / 60f);
            }

            SpringTo(delta, targetPos, tangDamp);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref target, "target");
        }
    }

    /// <summary>
    /// 带范围减速的追踪：范围外弹簧追踪，范围内刹车停下
    /// </summary>
    public class TrackingState_WithRange : TrackingState
    {
        private float radius;

        public TrackingState_WithRange(Projectile_MagicMissileBase missile, Thing target, float radius,
            float stiffness = 12f, float damping = 6.5f) : base(missile, target, stiffness, damping)
        {
            this.radius = radius;
        }

        public TrackingState_WithRange(BaseMagicMissileState other, Thing target, float radius,
            float stiffness = 12f, float damping = 6.5f) : base(other, target, stiffness, damping)
        {
            this.radius = radius;
        }

        public override void UpdateInterval(int delta)
        {
            // 目标丢失则切换为直线匀速（保持当前速度方向）
            if (target == null || !target.Spawned)
            {
                missile.currentState = new OrbitState_Line(this, velocity, velocity.magnitude);
                return;
            }

            float dist = (target.Position.ToVector3Shifted() - position).magnitude;

            if (dist > radius)
            {
                // 范围外：正常追踪（调用父类弹簧逻辑）
                base.UpdateInterval(delta);
            }
            else
            {
                // 范围内：刹车阻尼，平滑停下
                acceleration = -0.5f * velocity;
                if (maxA.HasValue)
                    acceleration = ClampVector(acceleration, maxA.Value);
                velocity += acceleration * delta;
                if (maxV.HasValue)
                    velocity = ClampVector(velocity, maxV.Value);
                position += velocity * delta;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref radius, "radius");
        }
    }

    /// <summary>
    /// 定轨_直线：沿给定方向按固定速度匀速运动（用于丢失目标后的惯性）
    /// </summary>
    public class OrbitState_Line : BaseMagicMissileState
    {
        private Vector3 direction;

        public OrbitState_Line(Projectile_MagicMissileBase missile, Vector3 direction, float speed = 1f) : base(missile)
        {
            this.direction = direction.normalized;
            if (direction.magnitude < 1e-5f) this.direction = Vector3.forward;
            maxV = speed;
        }

        public OrbitState_Line(Projectile_MagicMissileBase missile, Vector3 start, Vector3 end, float speed = 1f)
            : this(missile, (end - start).normalized, speed) { }

        public OrbitState_Line(Projectile_MagicMissileBase missile, IntVec3 start, IntVec3 end, float speed = 1f)
            : this(missile, start.ToVector3(), end.ToVector3(), speed) { }

        public OrbitState_Line(BaseMagicMissileState other, Vector3 direction, float speed = 1f) : base(other)
        {
            this.direction = direction.normalized;
            if (direction.magnitude < 1e-5f) this.direction = Vector3.forward;
            maxV = speed;
        }

        public OrbitState_Line(BaseMagicMissileState other, Vector3 start, Vector3 end, float speed = 1f)
            : this(other, (end - start).normalized, speed) { }

        public OrbitState_Line(BaseMagicMissileState other, IntVec3 start, IntVec3 end, float speed = 1f)
            : this(other, start.ToVector3(), end.ToVector3(), speed) { }

        public override void UpdateInterval(int delta)
        {
            // 保持速度大小和方向不变
            acceleration = Vector3.zero;
            if (maxV.HasValue)
                velocity = direction * maxV.Value;
            else
            {
                Log.Warning($"[FamiliaMyth] {missile.Label} has no maxV in OrbitState_Line, using default 1.");
                velocity = direction * 1f;
            }
            position += velocity * delta;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref direction, "direction");
        }
    }
}
