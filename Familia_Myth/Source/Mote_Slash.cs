using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_Slash : Mote
    {
        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            var t = Mathf.Clamp01((AgeSecs - def.mote.fadeInTime - def.mote.solidTime) / def.mote.fadeOutTime);
            //var s = Mathf.Lerp(1f, 0f, t);
            linearScale.z = 1f - t;
        }
    }
}
