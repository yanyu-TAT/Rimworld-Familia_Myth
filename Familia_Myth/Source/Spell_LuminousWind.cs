using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_LuminousWind : Spell
    {
        private const int BasicMissileCount = 18;

        private const float SpawnRadius = 5f;

        private int MissileCount => (int)(BasicMissileCount * GetCurrentSpellPower());

        private int MissileSummonTickInterval => (int)(Def.WarmupTime / (MissileCount + 1) * 60);

        private int lastSummonTick;

        protected override void WarmUpStart()
        {
            //Log.Message("Warm Up Start");
            lastSummonTick = CurrentTick;
        }

        protected override void WarmUpFinish(bool castDone)
        {
            NotifyAll("Activate");
            Projectile_WindMissile.TrackedThings.Clear();
        }

        protected override void TickWarmUp()
        {
            base.TickWarmUp();
            if (CurrentTick - lastSummonTick >= MissileSummonTickInterval)
            {
                //Log.Message($"Try Summon Missile With Cnt:{MissileCount}, Interval:{MissileSummonTickInterval}");
                var missile = TrySummonWindMissile();
                if (missile != null)
                {
                    //Log.Message("Missile Spawned Successfully");
                    lastSummonTick = CurrentTick;
                    activeProjectiles.Add(missile);
                }
            }
        }

        private Projectile_WindMissile TrySummonWindMissile()
        {
            if (pawn?.Map == null) return null;

            var offset2d = UnityEngine.Random.insideUnitCircle * SpawnRadius;
            var offset = new Vector3(offset2d.x, 0, offset2d.y);

            var missile = (Projectile_WindMissile)GenSpawn.Spawn(
                ThingDef.Named("Projectile_WindMissile"),
                pawn.Position + offset.ToIntVec3(), pawn.Map);

            if (missile == null) return null;

            missile.Init(this);
            missile.currentState = new HoveringState_Offset(missile, offset);

            return missile;
        }
    }
}
