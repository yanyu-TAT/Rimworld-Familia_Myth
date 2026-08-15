using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_Sneathe : MoteAttached
    {
        public void Hit(float damage)
        {
            float s = Mathf.Clamp01(linearScale.x - 0.005f * damage);
            linearScale = new(s, 1, s);
        }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            float s = Mathf.Lerp(linearScale.x, 1f, 0.4f);
            linearScale = new(s, 1f, s);
        }
    }
}
