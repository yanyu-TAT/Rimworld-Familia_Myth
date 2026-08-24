using Verse;

namespace FamiliaMyth.Abilities
{
    public class Stance_Warmup_ConcurrentCasting : Stance_Warmup
    {
        public Stance_Warmup_ConcurrentCasting() { }

        public Stance_Warmup_ConcurrentCasting(int ticks, LocalTargetInfo focusTarg, Verb verb)
            : base(ticks, focusTarg, verb) { }

        public override bool StanceBusy => false;
    }
}
