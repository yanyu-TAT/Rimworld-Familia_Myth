using FamiliaMyth.Feat;
using FamiliaMyth.Pharna;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    public class ITab_Pawn_Feats : ITab
    {
        private const float RowHeight = 64f;
        private const float TabWidth = 630f;
        private const float TabHeight = 510f;

        private Vector2 scrollPosition;

        public ITab_Pawn_Feats()
        {
            size = new Vector2(TabWidth, TabHeight);
            labelKey = "FamiliaMyth.TabFeats";
        }

        //仅持有恩惠者显示本页签
        public override bool IsVisible => SelPawn?.GetPharna() != null;

        protected override void FillTab()
        {
            List<FeatRecord> records = SelPawn?.GetPharna()?.feats;
            Rect rect = new Rect(0f, 0f, TabWidth, TabHeight).ContractedBy(10f);

            if (records.NullOrEmpty())
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.grey;
                Widgets.Label(rect, "FamiliaMyth.NoFeat".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
                return;
            }

            List<FeatRecord> sorted = records
                .Where(r => r != null)
                .OrderByDescending(r => r.tier)
                .ThenByDescending(r => r.tick)
                .ToList();

            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, sorted.Count * RowHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            float y = 0f;
            for (int i = 0; i < sorted.Count; i++)
            {
                DrawRow(new Rect(0f, y, viewRect.width, RowHeight - 4f), sorted[i]);
                y += RowHeight;
            }
            Widgets.EndScrollView();
        }

        private static void DrawRow(Rect rect, FeatRecord record)
        {
            Widgets.DrawHighlightIfMouseover(rect);

            string traits = record.featTraits.NullOrEmpty()
                ? "-"
                : string.Join(" × ", record.featTraits.Where(t => t != null).Select(t => t.label));
            string used = record.usedForLevel >= 0
                ? "FamiliaMyth.FeatUsed".Translate(record.usedForLevel)
                : "FamiliaMyth.FeatUnused".Translate();

            //数值格式在参数侧完成（语言文件中不得出现 P0 / F1 等格式说明符）
            TaggedString text = "FamiliaMyth.FeatRow".Translate(
                record.tier,
                traits,
                record.battleMagnitude.ToString("F0"),
                record.share.ToStringPercent(),
                GenDate.DateFullStringAt(record.tick, Vector2.zero),
                DescribeTile(record.tileId),
                used);
            Widgets.Label(rect, text);
        }

        //地点：取该地块的主要生物群系名；无效或越界地块返回占位符
        private static string DescribeTile(int tileId)
        {
            if (tileId < 0 || Find.WorldGrid == null)
                return "FamiliaMyth.FeatUnknownPlace".Translate();

            try
            {
                Tile tile = Find.WorldGrid[new PlanetTile(tileId)];
                return tile?.PrimaryBiome?.LabelCap ?? "FamiliaMyth.FeatUnknownPlace".Translate();
            }
            catch
            {
                return "FamiliaMyth.FeatUnknownPlace".Translate();
            }
        }
    }
}
