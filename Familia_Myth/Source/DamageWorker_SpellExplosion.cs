using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class DamageWorker_SpellExplosion : DamageWorker_Vaporize
    {
        protected override void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell)
        {
            //与DamageWorker.ExplosionDamageThing一致，仅去掉 Vaporize 的 <= 2.9f 判断
            if (t.def.category == ThingCategory.Mote || t.def.category == ThingCategory.Ethereal || damagedThings.Contains(t))
                return;
            damagedThings.Add(t);
            if (ignoredThings != null && ignoredThings.Contains(t))
                return;
            if (def == DamageDefOf.Bomb && t.def == ThingDefOf.Fire && !t.Destroyed)
            {
                t.Destroy();
                return;
            }
            var dinfo = new DamageInfo(
                angle: (!(t.Position == explosion.Position)) ? (t.Position - explosion.Position).AngleFlat : (float)Rand.RangeInclusive(0, 359),
                def: def,
                amount: explosion.GetDamageAmountAt(cell),
                armorPenetration: explosion.GetArmorPenetrationAt(cell),
                instigator: explosion.instigator,
                hitPart: null,
                weapon: explosion.weapon,
                category: DamageInfo.SourceCategory.ThingOrUnknown,
                intendedTarget: explosion.intendedTarget);
            if (def.explosionAffectOutsidePartsOnly)
                dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            BattleLogEntry_ExplosionImpact battleLogEntry = null;
            Pawn pawn = t as Pawn;
            if (pawn != null)
            {
                battleLogEntry = new BattleLogEntry_ExplosionImpact(explosion.instigator, t, explosion.weapon, explosion.projectile, def);
                Find.BattleLog.Add(battleLogEntry);
            }
            DamageResult damageResult = t.TakeDamage(dinfo);
            damageResult.AssociateWithLog(battleLogEntry);
            if (pawn != null && damageResult.wounded && pawn.stances != null)
                pawn.stances.stagger.StaggerFor(95);
        }

        private static readonly (string defName, float drawSize, float rotationRate)[] HeatwaveFlecks =
        {
            ("Fleck_HeatDiffusion",       9f,  0f),
            ("Fleck_HeatWaveDistortion",  3f,  0f),
            ("Fleck_RadialSparks",        8f,  0f),
            ("Fleck_VaporizeGlow",        4.5f, 0f),
            ("Fleck_VaporizeRays_A",     14f,  30f),
            ("Fleck_VaporizeRays_B",     14f, -20f),
            ("Fleck_VaporizeCenterFlash", 7f,  0f),
        };

        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            base.ExplosionStart(explosion, cellsToAffect);   //保留推热、原版 ExplosionFlash 中心闪光、震屏
            SpawnScaledHeatwave(explosion);
        }

        private void SpawnScaledHeatwave(Explosion explosion)
        {
            float r = explosion.radius;
            Vector3 pos = explosion.Position.ToVector3Shifted();
            foreach ((string defName, float drawSize, float rotationRate) in HeatwaveFlecks)
            {
                FleckDef def = DefDatabase<FleckDef>.GetNamed(defName);
                if (def == null) continue;
                //视觉直径 = drawSize × scale = 2r → 视觉半径 = r
                float scale = 2f * r / drawSize;
                FleckCreationData data = FleckMaker.GetDataStatic(pos, explosion.Map, def, scale);
                data.rotationRate = rotationRate;
                explosion.Map.flecks.CreateFleck(data);
            }
        }
    }
}
