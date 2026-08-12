using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Projectile_ArcsRay : Projectile_MagicMissileBase
    {
        private const float TrackingAcceleration = 0.12f;  // 追踪加速度（必中锁定）
        private const float LaunchSpeed = 0.8f;            // 无有效目标时的直线速度（格/帧）
        private const float DetonationRadius = 5f;       // 引爆范围
        private const int StunTicks = 120;                 // 引爆眩晕时长（2秒）
        private const float ExplosionFactor = 0.25f;    //引爆的伤害系数
        private const float BuildingFactor = 5f;        //穿透建筑的伤害系数

        private readonly HashSet<Thing> hittenThings = new();
        private int? damageAmount = null;
        private Pawn curTarget;

        public override int DamageAmount => damageAmount ?? base.DamageAmount;

        //不可被护盾拦截
        public override bool CanShieldBlock => false;

        public override Quaternion ExactRotation
        {
            get
            {
                if (currentState is HoveringState_Offset)
                {
                    var dir = curTarget.Position - launcher.Position;
                    if (dir.Magnitude < 1e-5f) return Quaternion.identity;
                    return Quaternion.LookRotation(dir.ToVector3().normalized);
                }
                return base.ExactRotation;
            }
        }

        protected override bool CanHitAt(Thing thing)
        {
            //目标丢失则停止穿透
            if (curTarget == null || !curTarget.Spawned || curTarget.DeadOrDowned) 
                return base.CanHitAt(thing);

            if (base.CanHitAt(thing))
            {
                //造成伤害
                TryDamage(thing);
                //命中目标则停止
                if (thing is Pawn pawn && pawn == curTarget)
                    return true;
            }
            //未命中，穿透
            return false;
        }

        private void TryDamage(Thing thing)
        {
            if (hittenThings.Contains(thing)) return;
            //非敌方目标不伤害
            if (thing is Pawn pawn && pawn.Faction != null && !pawn.Faction.HostileTo(launcher.Faction))
            {
                hittenThings.Add(thing);
                return;
            }

            BattleLogEntry_RangedImpact log = new(Launcher, thing, null, null, def, null);
            Find.BattleLog.Add(log);

            DamageInfo dinfo = new(
                DamageDef, 
                DamageAmount * ((thing is Building) ? BuildingFactor : 1f), 
                ArmorPenetration,
                ExactRotation.eulerAngles.y, Launcher, null,
                null, DamageInfo.SourceCategory.ThingOrUnknown);
            dinfo.SetIgnoreArmor(true); //无视护甲，无视承伤系数由DamageWorker处理

            thing.TakeDamage(dinfo).AssociateWithLog(log);
            hittenThings.Add(thing);
        }

        public override void Init(Spell spell)
        {
            base.Init(spell);
            damageAmount = (int)(base.DamageAmount * spell.GetCurrentSpellPower());
            if (spell is Spell_ArcsRay spell_ArcsRay)
            {
                curTarget = spell_ArcsRay.mainTarget;
            }
            else
            {
                Log.Warning("[FamiliaMyth] Arrow Can't Initialize When Not Created By Spell Arcs Ray.");
            }
        }

        public override Action NotifyBy(string message)
        {
            return message switch
            {
                "Shoot" => Activate,
                "SpellKey" => ExtraImpact,
                _ => null
            };
        }

        private void Activate()
        {
            if (launcher?.Map == null || launcher.Faction == null) return;

            //目标丢失或倒地则重新锁定
            if (curTarget == null || !curTarget.Spawned || curTarget.DeadOrDowned)
            {
                curTarget = launcher.Map.mapPawns.AllPawnsSpawned
                .Where(p => p.Faction?.HostileTo(launcher.Faction) ?? false)
                .OrderBy(p => p.Position.DistanceTo(Position) * (hittenThings.Contains(p) ? 2 : 1))
                .FirstOrDefault();
            }

            if (curTarget != null)
                currentState = new TrackingState(currentState, curTarget, TrackingAcceleration);
            else
            {
                //无目标则直线飞行
                var start = launcher.Position.ToVector3();
                var end = ExactPosition;
                currentState = new OrbitState_Line(currentState, start, end, LaunchSpeed);
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
            //爆
            GenExplosion.DoExplosion(
                Position, Map,
                radius: DetonationRadius,
                DamageDefOf.Bomb, launcher,
                damAmount: (int)(damageAmount * ExplosionFactor),
                armorPenetration: ArmorPenetration,
                ignoredThings: ignored);

            //范围内眩晕
            foreach (var cell in GenRadial.RadialCellsAround(Position, DetonationRadius, true))
            {
                foreach (var thing in cell.GetThingList(Map))
                {
                    if (thing is Pawn p && p.Spawned && !p.Dead &&
                        launcher?.Faction != null && p.Faction != null &&
                        launcher.Faction.HostileTo(p.Faction))
                    {
                        p.stances.stunner.StunFor(StunTicks, Launcher, addBattleLog: true, showMote: true);
                    }
                }
            }

            Destroy();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref curTarget, "curTarget");
        }
    }
}
