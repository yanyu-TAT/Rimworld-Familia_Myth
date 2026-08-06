using UnityEngine;
using Verse;

namespace FamiliaMyth.Pharna
{
    public class Mote_Floating : MoteAttached
    {
        private float floatOffset;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            floatOffset = Rand.Range(-3f, 3f);
        }

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            //浮动
            exactPosition.z += Mathf.Sin((AgeSecs + floatOffset) * 1.2f) * 0.2f;
        }
    }
}
