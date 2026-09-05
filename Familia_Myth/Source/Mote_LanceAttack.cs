using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_LanceAttack : MoteAttached
    {
        private Vector3 chargeDir = Vector3.forward;

        public void Init(Vector3 dir, Pawn pawn)
        {
            chargeDir = dir.sqrMagnitude < 1e-6f ? Vector3.forward : dir.normalized;
            Attach(pawn);
            instanceColor = new Color(0.85f, 0.92f, 1f, 0.5f);
        }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime); 
            if (link1.Linked)
            {
                exactRotation = chargeDir.ToAngleFlat() + 90f;
                exactPosition += chargeDir * 0.6f;
            }
        }
    }
}
