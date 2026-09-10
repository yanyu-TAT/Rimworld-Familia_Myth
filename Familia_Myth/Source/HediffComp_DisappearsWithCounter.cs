using Verse;

namespace FamiliaMyth.Abilities
{
    public class HediffComp_DisappearsWithCounter : HediffComp
    {
        private HediffCompProperties_DisappearsWithCounter Props => props as HediffCompProperties_DisappearsWithCounter;

        private int cnt = 999;

        public override bool CompShouldRemove => base.CompShouldRemove || cnt <= 0;

        public override void CompPostMake()
        {
            base.CompPostMake();
            cnt = Props.maxCount;
        }

        public bool TryConsume()
        {
            if (cnt <= 0) return false;
            cnt--;
            return true;
        }

        public bool TryAdd()
        {
            if (cnt >= Props.maxCount) return false;
            cnt++;
            return true;
        }

        public override string CompLabelInBracketsExtra => "FamiliaMyth.Ability.RemainingCount".Translate(cnt);

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref cnt, "cnt");
        }
    }

    public class HediffCompProperties_DisappearsWithCounter : HediffCompProperties
    {
        public int maxCount;

        public HediffCompProperties_DisappearsWithCounter()
        {
            compClass = typeof(HediffComp_DisappearsWithCounter);
        }
    }
}
