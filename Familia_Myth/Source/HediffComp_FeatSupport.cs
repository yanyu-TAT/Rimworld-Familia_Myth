using FamiliaMyth.Feat;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class HediffCompProperties_FeatSupport : HediffCompProperties
    {
        //作用于他人时每秒折算的贡献点数；0 或负值表示该效果不计贡献
        public float supportPerSecond = 1f;
        //允许施术者与被挂载者为同一人（用于挂在自己身上、却作用于敌人的效果，如嘲讽）
        public bool allowSelf = false;

        public HediffCompProperties_FeatSupport()
        {
            compClass = typeof(HediffComp_FeatSupport);
        }
    }

    //按实际生效时间累计伟业支援贡献：效果被移除即停止，刷新重叠无需修正
    public class HediffComp_FeatSupport : HediffComp
    {
        //节流：攒够 1 秒再上报一次，避免每 tick 都去查贡献表
        private const int ReportIntervalTicks = 60;

        private Pawn caster;
        private float pending;
        private int elapsed;

        private HediffCompProperties_FeatSupport Props => (HediffCompProperties_FeatSupport)props;

        //施加点在 AddHediff 之后调用
        public void SetCaster(Pawn pawn)
        {
            caster = pawn;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (caster == null || Props.supportPerSecond <= 0f) return;

            pending += Props.supportPerSecond / 60f;
            if (++elapsed < ReportIntervalTicks) return;
            Flush();
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            Flush();
        }

        private void Flush()
        {
            elapsed = 0;

            if (caster == null || pending <= 0f) return;
            if (Props.allowSelf || Pawn != caster)
                GameComponent_FeatTracker.Instance?.RecordSupport(caster, pending);

            pending = 0f;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref caster, "caster");
            Scribe_Values.Look(ref pending, "pending", 0f);
            Scribe_Values.Look(ref elapsed, "elapsed", 0);
        }
    }
}
