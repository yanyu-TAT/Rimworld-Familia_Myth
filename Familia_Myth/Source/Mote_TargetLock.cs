using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_TargetLock : MoteAttached
    {
        private const float InitialScale = 3f;
        private const float FadeInDuration = 0.8f;

        private float pulseOffset;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            pulseOffset = Rand.Range(-3f, 3f);
        }

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            //缩小淡入
            var t = Mathf.Clamp01(AgeSecs / FadeInDuration);
                                    //缩小                            跳动
            var scale = Mathf.Lerp(InitialScale, 1f, t) * (1.2f + Mathf.Sin((AgeSecs + pulseOffset) * 2f) * 0.2f);
            linearScale = new(scale, 1, scale);

            //旋转
            rotationRate = AgeSecs < FadeInDuration ? 5f : 1f;
            exactRotation += rotationRate * delta;
        }
    }
}
