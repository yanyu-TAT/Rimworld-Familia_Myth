using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_ClockHand : Mote
    {
        public float angle = 0f;
        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            exactRotation = Mathf.Lerp(exactRotation, angle, 0.75f);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref angle, "angle");
        }
    }
}
