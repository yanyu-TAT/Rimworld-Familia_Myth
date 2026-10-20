using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_SolLight : Mote
    {
        private int elapsed;
        private float radius = 8f;
        private Color themeColor = Color.white;

        public void SetRadius(float radius) => this.radius = radius;

        public void SetThemeColor(Color color) => themeColor = color;

        public void SetElapsed(int ticks) => elapsed = ticks;

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);

            float scale;
            float alpha;
            Color color = themeColor;

            if (elapsed < 120)
            {
                //展开 2s：ease-out 放大渐入
                float k = Mathf.Clamp01(elapsed / 120f);
                float ease = 1f - (1f - k) * (1f - k);
                scale = radius * 2f * ease;
                alpha = ease;
            }
            else if (elapsed < 180)
            {
                //等待 1s
                scale = radius * 2f;
                alpha = 1f;
            }
            else
            {
                //治疗时闪烁（不放大），随后 5s 原地淡出
                float k = Mathf.Clamp01((elapsed - 180) / 300f);
                float flash = Mathf.Clamp01(1f - (elapsed - 180) / 10f);
                scale = radius * 2f;
                alpha = 1f - k;
                color = LerpColor(themeColor, Color.white, flash);
            }

            linearScale = new Vector3(scale, 1f, scale);
            instanceColor = color.WithAlpha(color.a * alpha);
            exactRotation += 0.15f * deltaTime;
        }

        private static Color LerpColor(Color a, Color b, float t)
            => new(
                Mathf.Lerp(a.r, b.r, t),
                Mathf.Lerp(a.g, b.g, t),
                Mathf.Lerp(a.b, b.b, t),
                Mathf.Lerp(a.a, b.a, t));
    }
}
