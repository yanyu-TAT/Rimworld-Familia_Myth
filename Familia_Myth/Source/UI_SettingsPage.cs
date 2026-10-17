using FamiliaMyth.Core;
using FamiliaMyth.Pharna;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    /// <summary>
    /// 模组设置页：「实验性内容」与「数值配置」两个可折叠分组
    /// </summary>
    public static class UI_SettingsPage
    {
        private const float HeaderHeight = 30f;
        private const float RowHeight = 28f;
        private const float PlotSide = 300f;
        private const float IndentStep = 14f;
        private const float SectionGap = 6f;
        private const float IconSize = 18f;
        private const float ResetAllWidth = 90f;
        private const float ScrollBarWidth = 20f;

        private static readonly PharnaStatusType[] StatusTypes =
        {
            PharnaStatusType.Strength,
            PharnaStatusType.Endurance,
            PharnaStatusType.Dexterity,
            PharnaStatusType.Agility,
            PharnaStatusType.Mana,
            PharnaStatusType.Will
        };

        private static bool experimentalExpanded;
        private static bool numericExpanded;
        private static bool curveGroupExpanded = true;
        private static int expandedCurve = -1;
        private static Vector2 scrollPosition;
        private static float contentHeight;

        /// <summary>
        /// 绘制设置页内容；inRect 为「选项 → 模组选项」给出的内容区
        /// </summary>
        public static void Draw(Rect inRect)
        {
            float viewHeight = Mathf.Max(contentHeight, inRect.height);
            Rect viewRect = new(0f, 0f, inRect.width - ScrollBarWidth, viewHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect, true);
            try
            {
                UI_Cursor cursor = new(new Rect(0f, 0f, viewRect.width, viewRect.height));
                cursor.Gap(SectionGap);

                Rect experimentalHeader = cursor.Row(HeaderHeight);
                if (DrawFoldHeader(experimentalHeader, "FamiliaMyth.Settings.ExperimentalGroup".Translate(), null, experimentalExpanded))
                {
                    experimentalExpanded = !experimentalExpanded;
                }
                if (experimentalExpanded)
                {
                    cursor.Gap(SectionGap);
                    DrawGuildToggle(cursor.Row(IndentStep, cursor.Width - IndentStep, RowHeight));
                    cursor.Gap(SectionGap);
                }

                cursor.Gap(SectionGap);

                Rect numericHeader = cursor.Row(HeaderHeight);
                if (DrawFoldHeader(numericHeader, "FamiliaMyth.Settings.NumericGroup".Translate(), "FamiliaMyth.Settings.NumericGroupDesc".Translate(), numericExpanded))
                {
                    numericExpanded = !numericExpanded;
                }
                if (numericExpanded)
                {
                    cursor.Gap(SectionGap);
                    DrawCurveSection(ref cursor);
                    cursor.Gap(SectionGap);
                }

                contentHeight = cursor.Y + SectionGap;
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }

        //折叠分组头：rect 已按层级缩进，底色铺满 rect（右端到行尾），右侧 rightReserve 留给按钮
        private static bool DrawFoldHeader(Rect rect, string label, string tooltip, bool expanded, float rightReserve = 0f)
        {
            Widgets.DrawBoxSolid(rect, UI_Palette.TitleBarBg);
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, 3f, rect.height), UI_Palette.Accent);

            Rect clickRect = new(rect.x, rect.y, Mathf.Max(rect.width - rightReserve, 1f), rect.height);
            Widgets.DrawHighlightIfMouseover(clickRect);

            Rect iconRect = new(rect.x + 9f, rect.y + (rect.height - IconSize) * 0.5f, IconSize, IconSize);
            Texture2D icon = ContentFinder<Texture2D>.Get(expanded ? "UI/Buttons/Minus" : "UI/Buttons/Plus", false);
            if (icon is not null)
            {
                GUI.DrawTexture(iconRect, icon);
            }
            else
            {
                using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleCenter))
                using (UI_Scope.Color(Color.white))
                    Widgets.Label(iconRect, expanded ? "-" : "+");
            }

            Rect labelRect = new(iconRect.xMax + 7f, rect.y, Mathf.Max(clickRect.xMax - iconRect.xMax - 7f, 1f), rect.height);
            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            using (UI_Scope.Color(UI_Palette.TextNormal))
                Widgets.Label(labelRect, label);

            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(clickRect, tooltip);
            }

            return Widgets.ButtonInvisible(clickRect);
        }

        private static void DrawGuildToggle(Rect row)
        {
            FamiliaMythSettingsData data = FamiliaMythSettings.Data;
            if (data is null)
            {
                return;
            }
            bool value = data.guildContent;
            Widgets.CheckboxLabeled(row, "FamiliaMyth.Settings.GuildContentLabel".Translate(), ref value);
            data.guildContent = value;
            TooltipHandler.TipRegion(row, "FamiliaMyth.Settings.GuildContentDesc".Translate());
        }

        //数值配置 → 子标签「映射曲线」→ 六条曲线（手风琴，同时只展开一条）
        private static void DrawCurveSection(ref UI_Cursor cursor)
        {
            Rect subHeader = cursor.Row(IndentStep, cursor.Width - IndentStep, HeaderHeight);
            Rect resetAllRect = new(subHeader.xMax - ResetAllWidth - 4f, subHeader.y + 3f, ResetAllWidth, subHeader.height - 6f);

            if (DrawFoldHeader(subHeader, "FamiliaMyth.Settings.CurveSubGroup".Translate(), "FamiliaMyth.Settings.CurveHint".Translate(), curveGroupExpanded, ResetAllWidth + 6f))
            {
                curveGroupExpanded = !curveGroupExpanded;
            }
            if (Widgets.ButtonText(resetAllRect, "FamiliaMyth.Settings.CurveResetAll".Translate()))
            {
                FamiliaMythSettings.ResetAllCurves();
            }

            if (!curveGroupExpanded)
            {
                return;
            }

            cursor.Gap(SectionGap);
            for (int i = 0; i < StatusTypes.Length; i++)
            {
                PharnaStatusType type = StatusTypes[i];
                SimpleCurve curve = FamiliaMythSettings.Curve(type);
                Rect row = cursor.Row(IndentStep * 2f, cursor.Width - IndentStep * 2f, RowHeight);
                bool expanded = expandedCurve == i;
                if (UI_CurveEditor.DrawRow(row, curve, type, expanded))
                {
                    expandedCurve = expanded ? -1 : i;
                    expanded = !expanded;
                }
                cursor.Gap(UI_CurveEditor.RowSpacing);

                if (!expanded)
                {
                    continue;
                }
                float indent = IndentStep * 2f;
                float side = Mathf.Min(cursor.Width - indent, PlotSide);
                Rect plot = cursor.Row(indent, side, side);
                UI_CurveEditor.DrawPlot(plot, curve);
                cursor.Gap(SectionGap);
            }
        }
    }
}
