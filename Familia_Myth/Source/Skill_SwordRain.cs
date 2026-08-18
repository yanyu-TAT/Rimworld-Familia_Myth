using FamiliaMyth.Core;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_SwordRain : Skill
    {
        private const float Radius = 4f;
        private const int HitIntervalTicks = 20;

        private int resetCount = 0;
        public int swordCount;
        public bool kill;
        private bool finished = true;

        private static ThingDef swordDef;

        private const int ResetCountMax = 2;

        private static ThingDef SwordDef =>
            swordDef ??= DefDatabase<ThingDef>.GetNamed("SwordRain_FallingSword");

        public override bool CanCast(out string failReason)
        {
            if(!base.CanCast(out failReason)) return false;
            if(Owner.equipment == null || !Owner.equipment.AllEquipmentVerbs.Any(
                v => v.verbProps.meleeDamageDef?.armorCategory == DamageArmorCategoryDefOf.Sharp))
            {
                failReason = "未持有剑（锐伤近战武器）";
                return false;
            }
            return true;
        }

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            if (Owner?.Map == null) return false;
            var verb = Owner.equipment?.AllEquipmentVerbs?
                .Where(v => v.verbProps.IsMeleeAttack && v.verbProps.meleeDamageDef?.armorCategory == DamageArmorCategoryDefOf.Sharp)
                .OrderByDescending(v => v.verbProps.AdjustedMeleeDamageAmount(v, Owner))
                .FirstOrDefault();
            if (verb == null)
            {
                Log.Error($"[FamiliaMyth] Pawn {Owner.Name.ToStringShort} Can't Use Sword Rain With No Sword");
                return false;
            }

            if (GetEnemiesInRange().ToList().NullOrEmpty())
            {
                Messages.Message("范围内没有敌人", MessageTypeDefOf.RejectInput);
                return false;
            }

            kill = false;
            swordCount = 0;
            finished = false;

            Hit(verb);
            GameComponent_FamiliaMyth.Schedule(HitIntervalTicks, () =>
            {
                if (Owner.Dead || Owner.Downed || Owner.Map == null) return;
                Hit(verb);
            }, Owner);

            return true;
        }

        public void OnCastFinish()
        {
            //避免重复判定
            if (finished) return;
            finished = true;
            //未造成倒地/击杀
            if (!kill)
            {
                //重置计数清零
                resetCount = 0;
                return;
            }

            //重置计数未达上限
            if (resetCount < ResetCountMax)
            {
                //重置
                resetCount++;
                lastCastTick = CurrentTick - CooldownTicks;
            }
            else
            {
                //清零
                resetCount = 0;
            }
        }

        private void Hit(Verb verb)
        {
            if (verb == null) return;
            float dmg = verb.verbProps.AdjustedMeleeDamageAmount(verb, Owner);
            float apt = verb.verbProps.AdjustedArmorPenetration(verb, Owner);
            var damageDef = verb.GetDamageDef();
            var tool = verb.tool;
            var weapon = verb.EquipmentSource?.def;
            bool guilty = !Owner.Drafted;

            var dinfo = new DamageInfo(damageDef, dmg, apt, 0, Owner, null, weapon,
            DamageInfo.SourceCategory.ThingOrUnknown, null, guilty);
            dinfo.SetTool(tool);

            foreach (var enemy in GetEnemiesInRange())
            {
                //剑雨特效
                var cell = enemy.Position;
                if (ThingMaker.MakeThing(SwordDef) is not Skyfaller_Sword sword) continue;

                //设定目标和伤害
                sword.target = enemy;
                sword.dinfo = dinfo;
                sword.skill = this;
                swordCount++;

                GenSpawn.Spawn(sword, cell, Owner.Map);
            }
        }

        public override void Update()
        {
            if (Owner == null || !Owner.Spawned) return;
            if (!finished)
                GenDraw.DrawRadiusRing(Owner.Position, Radius, new(0.4f, 0.5f, 0.9f, 1f));
        }

        public override void OnHovering()
        {
            if (Owner == null || !Owner.Spawned || !CanCast(out _)) return;
            GenDraw.DrawRadiusRing(Owner.Position, Radius, new(0.4f, 0.4f, 0.8f, 0.8f));
        }

        private IEnumerable<Pawn> GetEnemiesInRange()
        {
            foreach (var cell in GenRadial.RadialCellsAround(Owner.Position, Radius, true).ToList())
                foreach (var thing in cell.GetThingList(Owner.Map).ToList())
                    if (thing is Pawn pawn && pawn != Owner && pawn.Spawned && !pawn.Downed && !pawn.Dead
                        && pawn.Faction != null && Owner.Faction.HostileTo(pawn.Faction))
                        yield return pawn;
        }
    }
}
