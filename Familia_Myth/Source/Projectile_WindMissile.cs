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
                currentState = new TrackingState(currentState, target, stiffness: 2f, damping: 1.5f);
                //从施法者附近散开的初始速度
                currentState.SetV((ExactPosition.WithY(0) - caster.Position.ToVector3Shifted()).normalized * Rand.Range(0.2f, 0.4f));
            } 
            else
            {
                currentState = new OrbitState_Line(currentState, caster.Position, Position, 0.2f);
            }
        }

        protected override void ExtraImpact()
        {
            //过滤友军
            var ignored = new List<Thing>();
            foreach (var cell in GenRadial.RadialCellsAround(Position, 5f, true))
                foreach (var thing in cell.GetThingList(launcher.Map))
                    if (thing is Pawn p && p.Faction == launcher.Faction)
                        ignored.Add(p);

            //爆炸
            GenExplosion.DoExplosion(
                Position, Map,
                radius: 2.5f,
                DamageDefOf.Bomb, Launcher,
                damAmount: (int)(DamageAmount * 1.5),
                armorPenetration: ArmorPenetration,
                ignoredThings: ignored);

            //更大的爆炸特效
            MoteThrown mote = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("MagicMissileBurst"));
            mote.Scale = 4f;
            mote.instanceColor = EdgeColor;
            GenSpawn.Spawn(mote, Position, Map);

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

            //爆炸特效
            MoteThrown mote = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("MagicMissileBurst"));
            mote.instanceColor = EdgeColor;
            GenSpawn.Spawn(mote, Position, Map);

            base.Impact(hitThing, blockedByShield);
        }
    }
}
