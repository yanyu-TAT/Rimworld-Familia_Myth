using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_ZeoGullveig : Mote
    {
        private const int FadeInTicks = 120;
        private const int FlashTicks = 60;
        private const float FlashScaleBoost = 0.05f;
        private const float RotationRate = 0.1f;

        private static readonly Color Gold = new(1f, 0.83f, 0.25f, 0.5f);
        private static float Radius => Spell_ZeoGullveig.Radius;

        private int fadeInTimer = -1;
        private int flashTimer = -1;

        public void Flash()
        {
            flashTimer = FlashTicks;
        }

        public void Init()
        {
            fadeInTimer = FadeInTicks;
        }

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (fadeInTimer > 0)
                fadeInTimer -= delta;
            if (flashTimer > 0)
                flashTimer -= delta;
        }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            float t = Mathf.Clamp01(1f - (float)fadeInTimer / FadeInTicks);
            float d = Radius * 2f * t * t;
            linearScale = new(d, 1f, d);

            float k = Mathf.Clamp01((float)flashTimer / FlashTicks);
            linearScale *= 1f + FlashScaleBoost * k;
            instanceColor = LerpColor(Gold, Color.white, k);

            exactRotation += RotationRate * deltaTime;
        }

        private static Color LerpColor(Color a, Color b, float t)
        {
            return new(
                Mathf.Lerp(a.r, b.r, t),
                Mathf.Lerp(a.g, b.g, t),
                Mathf.Lerp(a.b, b.b, t),
                Mathf.Lerp(a.a, b.a, t));
        }
    }
}
