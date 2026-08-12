using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_Provoke : Skill
    {
        public const float Radius = 20f;

        public static readonly HashSet<Pawn> Provokers = new();

        private static HediffDef effectDef;
        private static HediffDef EffectDef =>
            effectDef ??= DefDatabase<HediffDef>.GetNamed("ProvokeInEffect");

        private BodyPartRecord middleFinger;
        private BodyPartRecord MiddleFinger =>
            middleFinger ??= Owner?.RaceProps?.body?
            .GetPartsWithDef(DefDatabase<BodyPartDef>.GetNamed("Finger"))
            .FirstOrDefault(p => p.IsInGroup(DefDatabase<BodyPartGroupDef>.GetNamed("MiddleFingers")));

        public override bool CanCast(out string failReason)
        {
            if (!base.CanCast(out failReason)) return false;
            if (!Owner.Drafted)
            {
                failReason = $"{Owner.Name.ToStringShort}尚未被征召";
                return false;
            }
            return true;
        }

        public override void Tick()
        {
            //效果结束前不进入冷却
            if (Provokers.Contains(Owner))
                lastCastTick = CurrentTick;
        }

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            if (Owner?.Map == null) return false;

            var hediff = HediffMaker.MakeHediff(EffectDef, Owner, MiddleFinger);
            var comp = hediff.TryGetComp<HediffComp_Disappears>();
            if (comp != null)
                comp.ticksToDisappear = (int)(def.duration * 60f);
            else
                return false;
            Owner.health.AddHediff(hediff);

            Provokers.Add(Owner);
            return true;
        }

        public static bool IsProvokeActive(Pawn pawn)
        {
            return pawn != null &&
                pawn.Spawned && !pawn.DeadOrDowned &&
                pawn.health?.hediffSet?.GetFirstHediffOfDef(EffectDef) != null;
        }

        public override void OnHovering()
        {
            GenDraw.DrawRadiusRing(Owner.Position, Radius, new(1f, 0.2f, 0.2f, 0.8f));
        }

        public override void Update()
        {
            if (Owner == null || !Owner.Spawned) return;
            if (Provokers.Contains(Owner))
                GenDraw.DrawRadiusRing(Owner.Position, Radius, new(1f, 0.2f, 0.2f, 0.6f));
        }
    }
}
