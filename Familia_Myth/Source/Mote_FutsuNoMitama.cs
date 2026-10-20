using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 布都御魂的领域法阵：单层、按目标半径展开、缓慢旋转并呼吸，打断时小幅收缩。
    /// </summary>
    public class Mote_FutsuNoMitama : Mote
    {
        //转速（度/秒）
        private const float RotationRate = 8f;
        //呼吸幅度与频率
        private const float BreathAmplitude = 0.02f;
        private const float BreathSpeed = 1.6f;
        //打断收缩：压到这个比例后回弹
        private const float PulseMinScale = 0.98f;
        private const int PulseTicks = 30;
        //展开动画时长
        private const int GrowTicks = 36;

        private float targetSize = 1f;
        private int growTicksLeft = -1;
        private int pulseTicksLeft = -1;

        /// <param name="radius">法阵半径（世界单位）</param>
        /// <param name="color">法阵染色</param>
        public void Init(float radius, Color color)
        {
            targetSize = radius * 2f;
            instanceColor = color;
            linearScale = new Vector3(0.001f, 1f, 0.001f);
            growTicksLeft = GrowTicks;
        }

        public void Pulse() => pulseTicksLeft = PulseTicks;

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (growTicksLeft > 0) growTicksLeft -= delta;
            if (pulseTicksLeft > 0) pulseTicksLeft -= delta;
        }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);

            float grow = growTicksLeft <= 0 ? 1f : 1f - (float)growTicksLeft / GrowTicks;
            float breath = 1f + BreathAmplitude * Mathf.Sin(AgeSecs * BreathSpeed);
            float pulse = pulseTicksLeft <= 0 ? 1f : Mathf.Lerp(1f, PulseMinScale, (float)pulseTicksLeft / PulseTicks);
            float size = targetSize * Mathf.SmoothStep(0f, 1f, grow) * breath * pulse;

            linearScale = new Vector3(size, 1f, size);
            exactRotation += RotationRate * deltaTime;
        }
    }
}
