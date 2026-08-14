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
        private const float PathDamageRadius = 1.5f;

        public float absorbedDamage;

        private Vector3 dashDir;

        private bool isSneathing = false;
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

        public override bool CanCast(out string failReason)
        {
            if (!base.CanCast(out failReason)) return false;

            if (Owner.equipment == null || !Owner.equipment.AllEquipmentVerbs.Any(
                v => v.verbProps.meleeDamageDef?.armorCategory == DamageArmorCategoryDefOf.Sharp))
            {
                failReason = "未持有太刀（锐伤近战武器）";
                return false;
            }
            return true;
        }

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            if (Owner?.Map == null || !target.IsValid) return false;
            if (target.Cell == Owner.Position) return false;

            //记录居合方向
            dashDir = (target.Cell - Owner.Position).ToVector3().normalized;
            if (dashDir.magnitude < 1e-5f) return false;

            absorbedDamage = 0f;
            isSneathing = true;

            //击晕（进入纳刀）
            Owner.stances.stunner.StunFor(SneatheTicks, Owner, false, false, true);

            //生成纳刀特效
            moteSneathe = ThingMaker.MakeThing(MoteSneathDef) as Mote_Sneathe;
            if (moteSneathe != null)
            {
                moteSneathe.Attach(Owner);
                moteSneathe.instanceColor = new Color(1f, 0.25f, 0.25f, 0.75f);
                GenSpawn.Spawn(moteSneathe, Owner.Position, Owner.Map);
            }

            //计时后拔刀
            GameComponent_FamiliaMyth.Schedule(SneatheTicks, Dash);
            return true;
        }

        public override void Tick()
        {
            if (isSneathing)
                moteSneathe?.Maintain();
        }

        private void Dash()
        {
            //结束纳刀
            isSneathing = false;

            if (Owner?.Map == null || !Owner.Spawned || Owner.DeadOrDowned) return;

            var start = Owner.Position;
            var dest = GetDest();
            if (dest == start) return;

            Owner.Rotation = Rot4.FromAngleFlat((dest - start).ToVector3().AngleFlat());
            Owner.Position = dest;
            Owner.Notify_Teleported();

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
            var verb = Owner.equipment?.AllEquipmentVerbs?
                .Where(v => v.verbProps.IsMeleeAttack && v.verbProps.meleeDamageDef?.armorCategory == DamageArmorCategoryDefOf.Sharp)
                .OrderByDescending(v => v.verbProps.AdjustedMeleeDamageAmount(v, Owner))
                .FirstOrDefault();
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

            foreach (var enemy in GetEnemiesOnPath(start, end).ToList())
                enemy.TakeDamage(dinfo);
        }

        private IEnumerable<Pawn> GetEnemiesOnPath(IntVec3 start, IntVec3 end)
        {
            var hit = new HashSet<Pawn>();
            foreach (var cell in GetHitCells(start, end).ToList())
            {
                foreach(var thing in cell.GetThingList(Owner.Map).ToList())
                {
                    if (thing is Pawn pawn && pawn != Owner && pawn.Spawned && !pawn.DeadOrDowned &&
                        (pawn.Faction?.HostileTo(Owner.Faction) ?? false))
                        hit.Add(pawn);
                }
            }

            //Log.Message($"Hit Pawn:{hit.Select(p => p.Name.ToStringShort).ToList()}");
            return hit;
        }

        private IEnumerable<IntVec3> GetHitCells(IntVec3 start, IntVec3 end)
        {
            var cells = new HashSet<IntVec3>();
            var d = end - start;
            int steps = Mathf.Max(Mathf.Abs(d.x), Mathf.Abs(d.z));
            for (int i = 0; i <= steps; i++)
            {
                var celll =  start + new IntVec3(d.x * i / steps, 0, d.z * i / steps);
                foreach (var cell in GenRadial.RadialCellsAround(celll, PathDamageRadius, true))
                {
                    if (!cell.InBounds(Owner.Map)) continue;
                    cells.Add(cell);
                }
            }
            return cells;
        }

        private IntVec3 GetDest()
        {
            HashSet<IntVec3> cells = new();
            var start = Owner.Position.ToVector3();
            var end = Owner.Position;
            for (float i = 0; i <= DashDistance; i += 0.5f)
            {
                var cell = (start + i * dashDir).ToIntVec3();
                if (!cells.Contains(cell))
                    cells.Add(cell);
                else
                    continue;
                if (!cell.InBounds(Owner.Map) || !cell.Walkable(Owner.Map))
                    break;
                end = cell;
            }
            return end;
        }

        protected override void OnTargeting(LocalTargetInfo targetInfo)
        {
            if (!targetInfo.IsValid || !targetInfo.Cell.InBounds(Owner.Map))
                return;
            var start = Owner.Position;
            var end = start + ((targetInfo.Cell - start).ToVector3().normalized * DashDistance).ToIntVec3();
            if (end == start) return;
            //绘制路径范围
            GenDraw.DrawFieldEdges(
                GetHitCells(start, end)
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
            Scribe_Values.Look(ref isSneathing, "isSneathing");
        }

        public static Skill_IaiSpiritSlash GetActiveSkill(Pawn pawn)
        {
            var pharna = Hediff_Pharna.GetPharna(pawn);
            var skill = pharna?.skills?.FirstOrDefault(s => s is Skill_IaiSpiritSlash);
            if (skill is Skill_IaiSpiritSlash iai && iai.isSneathing)
                return iai;

            return null;
        }
    }
}
