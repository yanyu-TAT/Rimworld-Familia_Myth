using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_ExplosionCircle : Mote
    {
        private static readonly MaterialPropertyBlock Mpb = new();

        //放大渐隐
        private const int FadeOutTicks = 120;
        //放大倍率
        private const float ScaleOutFactor = 1f;
        //法阵最高高度
        private const float RingMaxHeight = 20f;
        //法阵基础大小
        private const float RingBasicScale = 15f;
        //透视参数（相对地面高度 0~RingMaxHeight 映射；相机近垂直俯视，屏幕上下方向=世界 Z）
        private const float RingHeightToZOffset = 0.8f;   
        private const float PerspectiveZScale = 0.45f;     //世界 Z 压缩系数：正圆→透视椭圆（越小越扁）
        private const float HeightSizeFalloff = 0.012f;

        private static Material ringMat;
        private static Material RingMat => ringMat ??= MaterialPool.MatFrom("Familia_Myth/Things/Mote/ExplosionMagicCircle", ShaderDatabase.MoteGlow);

        class Ring
        {
            public float yOffset;
            public float angle;
            public float spinRate;
            public Color color;
            public float scale;

            public Ring(float y)
            {
                yOffset = y;
                angle = Rand.Range(0f, 360f);
                spinRate = Rand.Range(5f, 5f);
                color = new(
                    Rand.Range(0.65f, 1f),
                    Rand.Range(0.2f, 0.45f),
                    Rand.Range(0.2f, 0.9f),
                    1f);
                scale = Rand.Range(0.5f, 1.5f) * RingBasicScale;
            }
        }
        private List<Ring> rings= new();

        private static int CurrentTick => Find.TickManager.TicksGame;

        private int fadeOutStartTick = -1;
        private bool IsFadingOut => fadeOutStartTick > 0;

        private int warmupTicks;
        private int warmupTimer = 0;

        public void Init(int warmupTicks)
        {
            this.warmupTicks = warmupTicks;

            int cnt = Rand.Range(2, 10);
            for (int i = 0; i < cnt; i++)
            {
                float y = Rand.Range(0f, RingMaxHeight) + def.altitudeLayer.AltitudeFor();
                rings.Add(new(y));
            }

            rings = rings.OrderBy(r => r.yOffset).ToList();
        }

        public void FadeOut()
        {
            if (IsFadingOut) return;
            fadeOutStartTick = CurrentTick;
        }

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            warmupTimer += delta;

            if (IsFadingOut && CurrentTick - fadeOutStartTick >= FadeOutTicks)
                Destroy();
        }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            foreach (var ring in rings)
                ring.angle += ring.spinRate * deltaTime;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            float fading = 1f;
            float scaleEx = 1f;
            if (IsFadingOut)
            {
                float t = (float)(CurrentTick - fadeOutStartTick) / FadeOutTicks;
                fading = 1f - t;
                scaleEx = 1f + t * ScaleOutFactor;
            }

            float p = Mathf.Clamp01((float)warmupTimer / warmupTicks);
            float curH = (RingMaxHeight + def.altitudeLayer.AltitudeFor()) * p;
            for (int i = 0; i < rings.Count; i++)
            {
                var ring = rings[i];
                float start = ring.yOffset + 1f;
                if (start > curH) continue;
                float progress = Mathf.Clamp01((curH - start) / 2f); 
                float rel = ring.yOffset - def.altitudeLayer.AltitudeFor(); // 相对地面高度 0~RingMaxHeight
                float heightFactor = Mathf.Clamp01(1f - rel * HeightSizeFalloff);
                DrawRing(drawLoc, progress * scaleEx * ring.scale * heightFactor, fading * progress, ring);
            }
        }

        private void DrawRing(Vector3 drawLoc, float scale, float a, Ring ring)
        {
            Mpb.SetColor(ShaderPropertyIDs.Color, ring.color.WithAlpha(a));
            // 屏幕上下方向=世界 Z：按相对高度映射 z 偏移，把 y 层次转成可见叠层（近垂直俯视下 y 位移≈0）
            float rel = ring.yOffset - def.altitudeLayer.AltitudeFor();
            var pos = new Vector3(drawLoc.x, ring.yOffset, drawLoc.z + rel * RingHeightToZOffset);
            var spin = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(ring.angle, Vector3.up), new Vector3(scale, 1f, scale));
            // 世界 Z 压缩（透视椭圆）必须施加在环自转之后，否则椭圆长轴会跟着自转一起倾斜
            var squash = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, 1f, PerspectiveZScale));
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one) * squash * spin,
                RingMat, 0, null, 0, Mpb);
        }
    }
}
