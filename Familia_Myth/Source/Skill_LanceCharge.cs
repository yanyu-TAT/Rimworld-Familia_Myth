using FamiliaMyth.Core;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_LanceCharge : Skill
    {
        private const float MaxRange = 10f;          //最大冲刺距离（格）
        private const float DamageFactor = 1.5f;     //伤害系数
        private const float PathDamageRadius = 1.5f;   //路径伤害半径（格）
        private const float KnockbackDist = 2f;      //击退距离（格）
        private const int KnockTicks = 20;           //击退动画时长
        private const int ChargeTicks = 40;          //施法者冲锋动画时长
        private const float SpeedLineBackRange = 1.6f;//覆盖范围：dir 反向最多到身后 1.6 格
        private const float SpeedLineWidth = 1.2f;    //散布宽度(垂直 dir)

        private static FleckDef trailDef;
        private static FleckDef TrailDef =>
            trailDef ??= DefDatabase<FleckDef>.GetNamed("FamLanceSpeedLine");

        private static ThingDef attackMoteDef;
        private static ThingDef AttackMoteDef =>
            attackMoteDef ??= DefDatabase<ThingDef>.GetNamed("Mote_LanceAttack");

        private Vector3 chargeDir = Vector3.forward;
        private Mote_LanceAttack mote;

        public override TargetingParameters TargetingParms => new()
        {
            canTargetLocations = true,
            canTargetBuildings = false,
            canTargetHumans = false,
            canTargetMechs = false,
            canTargetAnimals = false,
            onlyTargetColonists = false,
            validator = t => t.IsValid
                && t.Cell.InBounds(Owner.Map)
                && t.Cell.DistanceTo(Owner.Position) <= MaxRange
        };

        public override AcceptanceReport CanCast
        {
            get
            {
                var canCast = base.CanCast;
                if (!canCast) return canCast;

                if (Owner.GetMeleeSharpWeapon() == null)
                    return "未持有长矛（锐伤近战武器）";

                return true;
            }
        }

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            if (Owner?.Map == null || !target.IsValid) return false;
            if (target.Cell == Owner.Position) return false;

            var verb = Owner.GetMeleeSharpWeapon();
            if (verb == null) return false;

            var start = Owner.Position;
            chargeDir = (target.Cell - start).ToVector3().normalized;
            if (chargeDir.magnitude < 1e-5f) return false;

            var dashDest = target.Cell;
            if (dashDest == start) return false;

            Owner.Rotation = Rot4.FromAngleFlat((dashDest - start).ToVector3().AngleFlat());

            //冲锋过程中禁用其他主动效果与自身冷却
            stunning = true;
            GameComponent_FamiliaMyth.Schedule(ChargeTicks, () => stunning = false, Owner);

            //对路径附近敌人结算伤害并击退
            foreach (var enemy in RangeUtilities.GetEnemiesAroundStraightWay(start, dashDest, PathDamageRadius, Owner).ToList())
            {
                float dmg = verb.verbProps.AdjustedMeleeDamageAmount(verb, Owner) * DamageFactor;
                float apt = verb.verbProps.AdjustedArmorPenetration(verb, Owner);
                float angle = (dashDest - start).AngleFlat;

                var dinfo = new DamageInfo(verb.GetDamageDef(), dmg, apt, angle,
                    Owner, null, verb.EquipmentSource?.def,
                    DamageInfo.SourceCategory.ThingOrUnknown, null, false);
                dinfo.SetTool(verb.tool);
                enemy.TakeDamage(dinfo);

                //沿冲刺方向击退
                GameComponent_LanceCharge.StartMoving(enemy, chargeDir, KnockTicks, KnockbackDist);
            }

            //冲锋
            float dashDist = (dashDest - start).ToVector3().MagnitudeHorizontal();
            GameComponent_LanceCharge.StartMoving(Owner, chargeDir, ChargeTicks, dashDist);
            SpawnAttackMote();

            return true;
        }

        private void SpawnAttackMote()
        {
            if (Owner?.Map == null) return;
            mote = ThingMaker.MakeThing(AttackMoteDef) as Mote_LanceAttack;
            if (mote == null) return;
            mote.Init(chargeDir, Owner);
            GenSpawn.Spawn(mote, Owner.Position, Owner.Map);
            mote.Maintain();
        }

        public override void Tick()
        {
            base.Tick();
            if (!stunning) return;

            mote?.Maintain();
            if (Find.TickManager.TicksGame % 5 == 0)
                EmitSpeedLines();
        }

        //速度线：一次生成多条细长条，长轴与冲锋方向 dir 同向(贴图默认长向 +Z)
        private void EmitSpeedLines()
        {
            if (Owner?.Map == null || chargeDir.sqrMagnitude < 1e-6f) return;
            var perp = new Vector3(-chargeDir.z, 0f, chargeDir.x).normalized; //垂直 dir 的横向轴
            var back = -chargeDir;                                            //dir 反向(身后)

            //沿身后方向随机一段 + 垂直 dir 随机偏移
            float d = Rand.Range(0.2f, SpeedLineBackRange);
            float w = Rand.Range(-SpeedLineWidth, SpeedLineWidth) * 0.5f;
            var origin = Owner.DrawPos + back * d + perp * w;

            var data = FleckMaker.GetDataStatic(origin, Owner.Map, TrailDef, 1f);
            data.rotation = chargeDir.ToAngleFlat();
            data.exactScale = new Vector3(Rand.Range(0.1f, 1.5f), 1f, 1f);
            data.instanceColor = new Color(0.95f, 0.97f, 1f, 0.5f);
            data.velocity = chargeDir * Rand.Range(0f, 1f);
            Owner.Map.flecks.CreateFleck(data);
        }

        protected override void OnTargeting(LocalTargetInfo targetInfo)
        {
            GenDraw.DrawRadiusRing(Owner.Position, MaxRange);
            if (!targetInfo.IsValid || !targetInfo.Cell.InBounds(Owner.Map))
                return;
            var start = Owner.Position;
            var end = targetInfo.Cell;
            GenDraw.DrawFieldEdges(
                RangeUtilities.GetHitCellsAroundStraightWay(start, end, PathDamageRadius)
                .Where(c => c.InBounds(Owner.Map))
                .ToList(),
                Color.yellow);
        }

        public override void OnHovering()
        {
            if (Owner?.Spawned == true && CanCast)
                GenDraw.DrawRadiusRing(Owner.Position, MaxRange);
        }
    }
}
