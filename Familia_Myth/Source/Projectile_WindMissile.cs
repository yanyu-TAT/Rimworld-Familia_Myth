using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Projectile_WindMissile : Projectile_MagicMissileBase
    {
        public static HashSet<Thing> TrackedThings = new();

        public override Action NotifyBy(string message)
        {
            return message switch
            {
                "Activate" => Activate,//激活：锁定敌人，无目标则向施法者反方向直线移动
                "SpellKey" => ExtraImpact,//引爆咒：原地爆炸
                _ => base.NotifyBy(message),
            };
        }

        private void Activate()
        {
            var caster = spell?.pawn;
            if (caster?.Map == null || caster.Faction == null) return;
            //寻找最近敌人
            var target = caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => p.Faction?.HostileTo(caster.Faction) ?? false)
                .OrderBy(p => p.Position.DistanceTo(Position) * (TrackedThings.Contains(p) ? 2 : 1))
                .FirstOrDefault();

            if (target != null)
            {
                TrackedThings.Add(target);
                currentState = new TrackingState(currentState, target, 0.01f);
            } 
            else
            {
                currentState = new OrbitState_Line(this, caster.Position, Position, 0.2f);
            }
        }

        protected override void ExtraImpact()
        {
            //爆炸
            GenExplosion.DoExplosion(
                Position, Map,
                radius: 2.5f,
                DamageDefOf.Bomb, Launcher,
                damAmount: (int)(DamageAmount * 1.5),
                armorPenetration: ArmorPenetration);

            //销毁
            Destroy();
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (!blockedByShield && hitThing !=  null)
            {
                //造成伤害
                BattleLogEntry_RangedImpact log = new(Launcher, hitThing, null, null, def, null);
                Find.BattleLog.Add(log);

                DamageInfo dinfo = new(
                    DamageDef, DamageAmount, ArmorPenetration,
                    ExactRotation.eulerAngles.y, Launcher, null,
                    null, DamageInfo.SourceCategory.ThingOrUnknown);
                hitThing.TakeDamage(dinfo).AssociateWithLog(log);
            }
            else
            {
                // 地面特效
                FleckMaker.Static(ExactPosition, Map, FleckDefOf.ShotHit_Dirt);
            }
            base.Impact(hitThing, blockedByShield);
        }
    }
}
