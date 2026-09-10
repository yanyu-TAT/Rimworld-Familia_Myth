using FamiliaMyth.Core;
using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_IaiSpiritSlash : Skill
    {
        private const int SneatheTicks = 180;
        private const float DashDistance = 10f;
        private const float PathDamageRadius = 2f;

        public float absorbedDamage;

        private Vector3 dashDir;

        public Mote_Sneathe moteSneathe;

        private static ThingDef moteSneatheDef;
        private static ThingDef MoteSneathDef =>
            moteSneatheDef ??= DefDatabase<ThingDef>.GetNamed("Mote_Sneathe");

        private static ThingDef moteSlashDef;
        private static ThingDef MoteSlashDef =>
            moteSlashDef ??= DefDatabase<ThingDef>.GetNamed("Mote_Slash");

        public override TargetingParameters TargetingParms => new()
        {
            canTargetLocations = true,
            canTargetBuildings = false,
            canTargetHumans = false,
            canTargetMechs = false,
            canTargetAnimals = false,
            onlyTargetColonists = false,
            validator = t => t.IsValid && t.Cell.InBounds(Owner.Map) && t.Cell.DistanceTo(Owner.Position) <= DashDistance
        };

        public override AcceptanceReport CanCast
        {
            get
            {
                var canCast = base.CanCast;
                if (!canCast) return canCast;

                if (Owner.GetMeleeSharpWeapon() == null)
                {
                    return "FamiliaMyth.Ability.NoKatana".Translate();
                }
                return true;
            }
        }

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            if (Owner?.Map == null || !target.IsValid) return false;
            if (target.Cell == Owner.Position) return false;

            //记录居合方向
            dashDir = (target.Cell - Owner.Position).ToVector3().normalized;
            if (dashDir.magnitude < 1e-5f) return false;

            absorbedDamage = 0f;
            active = true;      //纳刀期间视为效果生效中（沿用基类 active）

            //击晕（进入纳刀）
            Owner.stances.stunner.StunFor(SneatheTicks, Owner, false, false, true);
            stunning = true;

            //生成纳刀特效
            moteSneathe = ThingMaker.MakeThing(MoteSneathDef) as Mote_Sneathe;
            if (moteSneathe != null)
            {
                moteSneathe.Attach(Owner);
                moteSneathe.instanceColor = new Color(1f, 0.25f, 0.25f, 0.75f);
                GenSpawn.Spawn(moteSneathe, Owner.Position, Owner.Map);
            }

            //计时后拔刀
            GameComponent_FamiliaMyth.Schedule(SneatheTicks, Dash, Owner);
            return true;
        }

        public override void Tick()
        {
            base.Tick();
            if (active)
                moteSneathe?.Maintain();
        }

        private void Dash()
        {
            //结束纳刀
            active = false;
            stunning = false;

            if (Owner?.Map == null || !Owner.Spawned || Owner.DeadOrDowned) return;

            var start = Owner.Position;
            var dest = RangeUtilities.GetDest(start.ToVector3(), start.ToVector3() + dashDir * DashDistance, Owner.Map, c => c.WalkableBy(Owner.Map, Owner));
            if (dest == start) return;

            Owner.Rotation = Rot4.FromAngleFlat((dest - start).ToVector3().AngleFlat());
            Owner.Position = dest;
            Owner.Notify_Teleported(true, false);

            //剑光特效
            MakeMoteSlash(start, dest, 1f, new Color(0.6f, 0f, 0f, 1f));
            MakeMoteSlash(start, dest, 0.5f, Color.white.WithAlpha(0.75f));

            DealDamage(start, dest);
        }

        private void MakeMoteSlash(IntVec3 start, IntVec3 dest, float width, Color color)
        {
            var vector = (dest - start).ToVector3();
            float num = vector.MagnitudeHorizontal();
            if (MoteMaker.MakeStaticMote(start.ToVector3() + vector * 0.5f, Owner.Map, MoteSlashDef, 1f, false, 0f) is Mote_Slash mote)
            {
                mote.linearScale = new Vector3(num, 1f, width);
                mote.exactRotation = Mathf.Atan2(-vector.z, vector.x) * 57.29578f;
                mote.instanceColor = color;
            }
        }

        private void DealDamage(IntVec3 start, IntVec3 end)
        {
            var verb = Owner.GetMeleeSharpWeapon();
            if (verb == null) return;

            float weaponDmg = verb.verbProps.AdjustedMeleeDamageAmount(verb, Owner);
            float amount = weaponDmg + absorbedDamage * 0.5f;   //100%武器伤害 + 50%吸收伤害
            float angle = (end - start).AngleFlat;
            bool guilty = !Owner.Drafted;
            float pentration = verb.verbProps.AdjustedArmorPenetration(verb, Owner);

            var dinfo = new DamageInfo(verb.GetDamageDef(), amount,
                pentration, angle,
                Owner, null, verb.EquipmentSource?.def,
                DamageInfo.SourceCategory.ThingOrUnknown, null, guilty);
            dinfo.SetTool(verb.tool);

            foreach (var enemy in RangeUtilities.GetEnemiesAroundStraightWay(start, end, PathDamageRadius, Owner).ToList())
                enemy.TakeDamage(dinfo);
        }

        protected override void OnTargeting(LocalTargetInfo targetInfo)
        {
            if (!targetInfo.IsValid || !targetInfo.Cell.InBounds(Owner.Map))
                return;
            var start = Owner.Position;
            var end = start + ((targetInfo.Cell - start).ToVector3().normalized * DashDistance).ToIntVec3();
            //绘制路径范围
            GenDraw.DrawFieldEdges(
                RangeUtilities.GetHitCellsAroundStraightWay(start, end, PathDamageRadius)
                .Where(c => c.InBounds(Owner.Map))
                .ToList(),
                Color.red);

            GenDraw.DrawRadiusRing(start, DashDistance);
        }

        public override void OnHovering()
        {
            GenDraw.DrawRadiusRing(Owner.Position, DashDistance);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref absorbedDamage, "absorbedDamage");
            Scribe_Values.Look(ref dashDir, "dashDir");
            // active 由基类 Skill.ExposeData 序列化，无需在此重复
        }
    }
}
