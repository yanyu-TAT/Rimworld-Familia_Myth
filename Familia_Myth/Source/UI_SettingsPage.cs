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

                DrawTutorialToggle(cursor.Row(RowHeight));
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

        //折叠分组头：视觉与交互统一由 UI_FoldHeader 提供
        private static bool DrawFoldHeader(Rect rect, string label, string tooltip, bool expanded, float rightReserve = 0f)
            => UI_FoldHeader.Draw(rect, label, tooltip, expanded, rightReserve);

        private static void DrawTutorialToggle(Rect row)
        {
            FamiliaMythSettingsData data = FamiliaMythSettings.Data;
            if (data is null) return;

            bool value = data.tutorialLetters;
            Widgets.CheckboxLabeled(row, "FamiliaMyth.Settings.TutorialLettersLabel".Translate(), ref value);
            data.tutorialLetters = value;
            TooltipHandler.TipRegion(row, "FamiliaMyth.Settings.TutorialLettersDesc".Translate());
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
