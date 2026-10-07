using FamiliaMyth.Feat;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Projectile_LightOrb : Projectile_MagicMissileBase
    {
        //半径
        private const float BasicRadius = 0.75f;
        //冷却
        private const int BasicCooldownTicks = 60;
        //击晕范围
        private const float StunRadius = 5f;
        //特效缩放
        private const float BurstScale = 2f;
        //眩晕时间乘数
        private const float StunTicksMultiplier = 0.5f;

        public override bool CanShieldBlock => false;
        protected override bool CanHitAt(Thing thing) => false;
        protected override bool NeedTrail => false;

        private int cooldownTicks = 0;

        public override Action NotifyBy(string message)
        {
            return message switch
            {
                "SpellKey" => ExtraImpact,
                _ => null
            };
        }

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (cooldownTicks > 0)
                cooldownTicks -= delta;
            else if (TryGetEnemy(out var enemy))
            {
                DamageInfo dinfo = new(
                    DamageDef, DamageAmount, ArmorPenetration,
                    ExactRotation.eulerAngles.y, launcher, null, null,
                    DamageInfo.SourceCategory.ThingOrUnknown);
                enemy.TakeDamage(dinfo);
                cooldownTicks = BasicCooldownTicks;
            }
            else if (TryAbsorbProjectile())
            {
                cooldownTicks = BasicCooldownTicks / 2;
            }
        }

        private bool TryAbsorbProjectile()
        {
            if (Map == null || launcher?.Faction == null)
                return false;

            var list = Map.listerThings.ThingsInGroup(ThingRequestGroup.Projectile).ToList();
            foreach (var thing in list)
            {
                if (thing == this || !thing.Spawned) continue;
                if (thing is not Projectile proj) continue;

                if (proj is Projectile_MagicMissileBase missile && !missile.CanShieldBlock) 
                    continue;

                if (proj.Launcher == null || !proj.Launcher.HostileTo(launcher)) 
                    continue;

                if ((proj.ExactPosition - ExactPosition).sqrMagnitude > BasicRadius * BasicRadius)
                    continue;

                proj.Destroy();
                FleckMaker.Static(proj.ExactPosition, Map, FleckDefOf.FireGlow);
                return true;
            }

            return false;
        }

        private bool TryGetEnemy(out Pawn enemy)
        {
            enemy = null;
            if (Map == null || launcher == null) return false;
            foreach (var cell in GenRadial.RadialCellsAround(Position, BasicRadius, true))
            {
                if (!cell.InBounds(Map)) continue;
                foreach(var thing in cell.GetThingList(Map).ToList())
                {
                    if (thing is Pawn pawn && pawn.HostileTo(launcher.Faction))
                    {
                        enemy = pawn;
                        return true;
                    }
                }
            }
            return false;
        }

        public override Color DrawColor
        {
            get
            {
                var color = base.DrawColor;
                float t = 1f - Mathf.Clamp01((float)cooldownTicks / BasicCooldownTicks) * 0.75f;
                color.a *= t;
                return color;
            }
        }

        protected override void ExtraImpact()
        {
            int stunTicks = (int)(((Spell_LightGuard)spell).RemainingTicks * StunTicksMultiplier);
            if (stunTicks <= 0 || Map == null || launcher == null) return;

            int stunned = 0;
            foreach (var cell in GenRadial.RadialCellsAround(Position, StunRadius, true))
            {
                if (!cell.InBounds(Map)) continue;
                foreach (var thing in cell.GetThingList(Map).ToList())
                {
                    if (thing is Pawn pawn && pawn.HostileTo(launcher.Faction))
                    {
                        pawn.stances.stunner.StunFor(stunTicks, launcher);
                        stunned++;
                    }
                }
            }

            //眩晕无 Hediff 载体，按命中人数与眩晕时长计入支援贡献
            if (stunned > 0 && spell?.pawn != null)
                GameComponent_FeatTracker.Instance?.RecordSupport(
                    spell.pawn, stunned * (stunTicks / 60f) * FeatUtility.ControlPerSecond);

            //特效
            MoteThrown mote = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("MagicMissileBurst"));
            mote.Scale = BurstScale;
            mote.instanceColor = EdgeColor;
            GenSpawn.Spawn(mote, Position, Map);

            Destroy();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cooldownTicks, "cooldownTicks");
        }
    }
}
