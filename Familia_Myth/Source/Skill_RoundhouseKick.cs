using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_RoundhouseKick : Skill
    {
        public const float KnockbackDist = 5f;
        public const float BlockBonus = 2f;
        public const int KnckbackTicks = 30;

        public override TargetingParameters TargetingParms => new()
        {
            canTargetHumans = true,
            canTargetMechs = true,
            canTargetAnimals = true,
            canTargetBuildings = false,
            validator = t => t.IsValid
                && t.Thing is Pawn p && p != Owner && p.Spawned && !p.DeadOrDowned
                && p.Faction != null && Owner.Faction.HostileTo(p.Faction)
                && p.Position.AdjacentTo8Way(Owner.Position)
        };

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            if (Owner?.Map == null || target.Thing is not Pawn enemy)
                return false;

            var verb = Owner.GetUnarmedMeleeVerb();
            if (verb == null) return false;

            float dmg = verb.verbProps.AdjustedMeleeDamageAmount(verb, Owner);
            float apt = verb.verbProps.AdjustedArmorPenetration(verb, Owner);
            var dir = (enemy.Position - Owner.Position).ToVector3();
            var dinfo = new DamageInfo(verb.GetDamageDef(), dmg, apt, (-dir).ToAngleFlat(), Owner);
            dinfo.SetTool(verb.tool);
            var log = CombatLogHelper.MakeHitLog(Owner, enemy, def);
            enemy.TakeDamage(dinfo).AssociateWithLog(log);

            GameComponent_RoundhouseKick.Kick(enemy, verb, dir, def);

            return true;
        }

        public override void OnHovering()
        {
            if (Owner?.Spawned == true && IsReady)
                GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(Owner.Position, 1f, false).ToList());
        }
    }
}
