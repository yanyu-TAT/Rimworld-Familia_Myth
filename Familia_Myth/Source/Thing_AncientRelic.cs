using Verse;

namespace FamiliaMyth.Quest
{
    //任务物品「远古圣物匣」：自建 ThingDef 的实例类。
    //label/description 在生成时按「随机异教文化的圣物命名规则」现造，实例级存储（跨存档 Scribe）。
    public class Thing_AncientRelic : ThingWithComps
    {
        //由随机异教文化(meme)词条 + 原版圣物命名规则生成的实例名（无则用 def label 兜底）
        public string relicName;

        public bool HasCustomName => !relicName.NullOrEmpty();

        public override string Label => HasCustomName ? relicName : base.Label;

        public override string LabelNoCount => HasCustomName ? relicName : base.LabelNoCount;

        public override string DescriptionFlavor =>
            HasCustomName
                ? "FM_AncientRelic_Description".Translate(relicName)
                : base.DescriptionFlavor;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref relicName, "relicName");
        }
    }
}
