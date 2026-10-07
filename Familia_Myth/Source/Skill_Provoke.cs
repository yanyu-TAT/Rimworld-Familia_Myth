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

        public override AcceptanceReport CanCast
        {
            get
            {
                var canCast = base.CanCast;
                if (!canCast) return canCast;

                if (!Owner.Drafted)
                    return "FamiliaMyth.Ability.ProvokeNotDrafted".Translate(Owner.Name.ToStringShort);

                return true;
            }
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
            //效果挂在自己身上、作用于敌人，贡献由组件按实际生效时间累计
            hediff.TryGetComp<HediffComp_FeatSupport>()?.SetCaster(Owner);
            return true;
        }

        public override void NotifyEnffectStart()
        {
            base.NotifyEnffectStart();
            Provokers.Add(Owner);
        }

        public override void NotifyEffectEnd()
        {
            base.NotifyEffectEnd();
            Provokers.Remove(Owner);
        }

        public override void OnHovering()
        {
            GenDraw.DrawRadiusRing(Owner.Position, Radius, new(1f, 0.2f, 0.2f, 0.8f));
        }

        public override void Update()
        {
            if (Owner == null || !Owner.Spawned) return;
            if (active)
                GenDraw.DrawRadiusRing(Owner.Position, Radius, new(1f, 0.2f, 0.2f, 0.6f));
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.PostLoadInit && active)
                Provokers.Add(Owner);
        }
    }
}
