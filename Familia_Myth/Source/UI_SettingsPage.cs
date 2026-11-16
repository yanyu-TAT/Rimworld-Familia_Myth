using FamiliaMyth.Core;
using FamiliaMyth.Pharna;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    /// <summary>
    /// 模组设置页：「实验性内容」「数值配置」「成长衰减设置」三个可折叠分组
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
        private const float PresetLabelWidth = 60f;
        private const float PresetButtonWidth = 96f;
        private const float PresetButtonGap = 6f;
        private const float DecayLabelWidth = 160f;
        private const float DecayFieldWidth = 104f;

        private static readonly PharnaStatusType[] StatusTypes =
        {
            PharnaStatusType.Strength,
            PharnaStatusType.Endurance,
            PharnaStatusType.Dexterity,
            PharnaStatusType.Agility,
            PharnaStatusType.Mana,
            PharnaStatusType.Will
        };

        private static readonly GrowthDecayPreset[] DecayPresets =
        {
            GrowthDecayPreset.Default,
            GrowthDecayPreset.Low,
            GrowthDecayPreset.Custom
        };

        private static bool experimentalExpanded;
        private static bool numericExpanded;
        private static bool curveGroupExpanded = true;
        private static int expandedCurve = -1;
        private static bool decayExpanded;
        private static bool statDecayExpanded;
        private static bool totalDecayExpanded;
        private static string statThresholdBuffer;
        private static string statPeakRateBuffer;
        private static string totalThresholdBuffer;
        private static string totalPeakRateBuffer;
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

                cursor.Gap(SectionGap);

                Rect decayHeader = cursor.Row(HeaderHeight);
                //取 RawText 以免字符串转换剥掉 tooltip 里的颜色标签
                string decayTooltip = "FamiliaMyth.Settings.DecayGroupDesc".Translate().RawText;
                if (DrawFoldHeader(decayHeader, "FamiliaMyth.Settings.DecayGroup".Translate(), decayTooltip, decayExpanded))
                {
                    decayExpanded = !decayExpanded;
                }
                if (decayExpanded)
                {
                    cursor.Gap(SectionGap);
                    DrawDecaySection(ref cursor);
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

            if (DrawFoldHeader(subHeader, "FamiliaMyth.Settings.CurveSubGroup".Translate(), "FamiliaMyth.Settings.CurveHint".Translate().RawText, curveGroupExpanded, ResetAllWidth + 6f))
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

        //成长衰减 → 预设按钮行与两个二级条目同级，参数行再缩进一级
        private static void DrawDecaySection(ref UI_Cursor cursor)
        {
            FamiliaMythSettingsData data = FamiliaMythSettings.Data;
            if (data is null)
            {
                return;
            }

            DrawDecayPresetRow(cursor.Row(IndentStep, cursor.Width - IndentStep, RowHeight), data);
            cursor.Gap(SectionGap);

            Rect statHeader = cursor.Row(IndentStep, cursor.Width - IndentStep, HeaderHeight);
            if (DrawFoldHeader(statHeader, "FamiliaMyth.Settings.DecayStatGroup".Translate(), null, statDecayExpanded))
            {
                statDecayExpanded = !statDecayExpanded;
            }
            if (statDecayExpanded)
            {
                cursor.Gap(SectionGap);
                DrawStatDecayParams(ref cursor, data);
                cursor.Gap(SectionGap);
            }

            Rect totalHeader = cursor.Row(IndentStep, cursor.Width - IndentStep, HeaderHeight);
            if (DrawFoldHeader(totalHeader, "FamiliaMyth.Settings.DecayTotalGroup".Translate(), null, totalDecayExpanded))
            {
                totalDecayExpanded = !totalDecayExpanded;
            }
            if (totalDecayExpanded)
            {
                cursor.Gap(SectionGap);
                DrawTotalDecayParams(ref cursor, data);
                cursor.Gap(SectionGap);
            }
        }

        private static void DrawDecayPresetRow(Rect row, FamiliaMythSettingsData data)
        {
            Rect labelRect = new(row.x, row.y, PresetLabelWidth, row.height);
            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            {
                Widgets.Label(labelRect, "FamiliaMyth.Settings.DecayPreset".Translate());
            }

            float x = labelRect.xMax + PresetButtonGap;
            for (int i = 0; i < DecayPresets.Length; i++)
            {
                GrowthDecayPreset preset = DecayPresets[i];
                Rect button = new(x, row.y + 1f, PresetButtonWidth, row.height - 2f);
                Widgets.DrawBoxSolid(new Rect(button.x, button.y, 3f, button.height),
                    data.decayPreset == preset ? UI_Palette.Accent : UI_Palette.Disabled);
                if (Widgets.ButtonText(new Rect(button.x + 5f, button.y, button.width - 5f, button.height), DecayPresetLabelKey(preset).Translate()))
                {
                    FamiliaMythSettings.ApplyDecayPreset(preset);
                    ResetDecayBuffers();
                }
                x += PresetButtonWidth + PresetButtonGap;
            }
        }

        private static string DecayPresetLabelKey(GrowthDecayPreset preset) => preset switch
        {
            GrowthDecayPreset.Default => "FamiliaMyth.Settings.DecayPresetDefault",
            GrowthDecayPreset.Low => "FamiliaMyth.Settings.DecayPresetLow",
            _ => "FamiliaMyth.Settings.DecayPresetCustom"
        };

        //预设切换后参数整体换值，输入框缓存需一并清空以显示新值
        private static void ResetDecayBuffers()
        {
            statThresholdBuffer = null;
            statPeakRateBuffer = null;
            totalThresholdBuffer = null;
            totalPeakRateBuffer = null;
        }

        private static void DrawStatDecayParams(ref UI_Cursor cursor, FamiliaMythSettingsData data)
        {
            float indent = IndentStep * 2f;
            bool changed = DrawDecayThresholdRow(cursor.Row(indent, cursor.Width - indent, RowHeight),
                ref data.statDecayThreshold, ref statThresholdBuffer, GrowthDecay.StatusCap,
                "FamiliaMyth.Settings.DecayThreshold".Translate(),
                "FamiliaMyth.Settings.DecayStatThresholdDesc".Translate().RawText);
            changed |= DrawDecayPeakRateRow(cursor.Row(indent, cursor.Width - indent, RowHeight),
                ref data.statDecayPeakRate, ref statPeakRateBuffer, "FamiliaMyth.Settings.DecayPeakRate".Translate(),
                "FamiliaMyth.Settings.DecayStatPeakRateDesc".Translate().RawText);
            if (changed)
            {
                data.decayPreset = GrowthDecayPreset.Custom;
            }
        }

        private static void DrawTotalDecayParams(ref UI_Cursor cursor, FamiliaMythSettingsData data)
        {
            float indent = IndentStep * 2f;
            float cap = GrowthDecay.TotalCap(GrowthDecay.StatusCap);
            bool changed = DrawDecayThresholdRow(cursor.Row(indent, cursor.Width - indent, RowHeight),
                ref data.totalDecayThreshold, ref totalThresholdBuffer, cap,
                "FamiliaMyth.Settings.DecayThreshold".Translate(),
                "FamiliaMyth.Settings.DecayTotalThresholdDesc".Translate().RawText);
            changed |= DrawDecayPeakRateRow(cursor.Row(indent, cursor.Width - indent, RowHeight),
                ref data.totalDecayPeakRate, ref totalPeakRateBuffer, "FamiliaMyth.Settings.DecayPeakRate".Translate(),
                "FamiliaMyth.Settings.DecayTotalPeakRateDesc".Translate().RawText);
            if (changed)
            {
                data.decayPreset = GrowthDecayPreset.Custom;
            }
        }

        //衰减起始阈值行：右侧数字输入框，范围 0 到该项的单级上限
        private static bool DrawDecayThresholdRow(Rect row, ref float value, ref string buffer, float max, string label, string tooltip)
        {
            Rect labelRect = new(row.x, row.y, DecayLabelWidth, row.height);
            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            {
                Widgets.Label(labelRect, label);
            }

            Rect field = new(row.xMax - DecayFieldWidth, row.y + 2f, DecayFieldWidth, row.height - 4f);
            float before = value;
            Widgets.TextFieldNumeric(field, ref value, ref buffer, 0f, Mathf.Max(max, value));
            TooltipHandler.TipRegion(row, tooltip);
            return !Mathf.Approximately(before, value);
        }

        //最高衰减率行：滑块与百分比输入框双向同步，0% 即该项不衰减
        private static bool DrawDecayPeakRateRow(Rect row, ref float value, ref string buffer, string label, string tooltip)
        {
            Rect labelRect = new(row.x, row.y, DecayLabelWidth, row.height);
            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            {
                Widgets.Label(labelRect, label);
            }

            Rect field = new(row.xMax - DecayFieldWidth, row.y + 2f, DecayFieldWidth, row.height - 4f);
            Rect slider = new(labelRect.xMax + 8f, row.y + 2f, Mathf.Max(field.x - labelRect.xMax - 16f, 1f), row.height - 4f);

            float before = value;
            value = Widgets.HorizontalSlider(slider, value, 0f, 1f, middleAlignment: true, roundTo: -1f);
            if (!Mathf.Approximately(before, value) || buffer is null)
            {
                buffer = (value * 100f).ToString("0.#");
            }
            Widgets.TextFieldPercent(field, ref value, ref buffer, 0f, 1f);
            TooltipHandler.TipRegion(row, tooltip);
            return !Mathf.Approximately(before, value);
        }
    }
}
