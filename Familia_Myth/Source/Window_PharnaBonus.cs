using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    /// <summary>
    /// 恩惠加成统计窗口：按原版分类与属性两级折叠，列出恩惠来源的加成说明。
    /// </summary>
    public class Window_PharnaBonus : Window_FamiliaBase
    {
        private const float WindowWidth = 450f;
        private const float MaxWindowHeight = 660f;
        private const float HeightScreenFraction = 0.72f;

        private const float CategoryHeaderH = 30f;
        private const float StatHeaderH = 26f;
        private const float LineMinH = 18f;
        private const float IndentStep = 14f;
        private const float LineIndent = IndentStep * 2f;
        private const float ValueWidth = 96f;
        private const float ColumnGap = 3f;
        private const float GroupGap = 6f;
        private const float StatGap = 6f;
        private const float BottomPad = 6f;
        private const float ScrollBarW = 16f;

        private readonly Pawn pawn;
        private readonly HashSet<StatCategoryDef> expandedCategories = new();
        private readonly HashSet<StatDef> expandedStats = new();
        private Vector2 scroll;

        //同帧缓存：OnGUI 每帧会多次调用，避免重复收集
        private int cachedFrame = -1;
        private List<PharnaBonusStats.CategoryEntry> cachedCategories;

        public Window_PharnaBonus(Pawn pawn)
        {
            this.pawn = pawn;
            forcePause = false;
            doCloseX = true;
            closeOnCancel = true;
            draggable = true;
            resizeable = false;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
        }

        public override Vector2 InitialSize
        {
            get
            {
                float height = Mathf.Min(Screen.height * HeightScreenFraction / Prefs.UIScale, MaxWindowHeight);
                return new Vector2(WindowWidth, height);
            }
        }

        protected override void DrawBackLayer(Rect inRect)
        {
            Widgets.DrawBoxSolid(inRect, UI_Palette.ContentBg);
            Widgets.DrawBoxSolid(new Rect(inRect.x, inRect.y, inRect.width, HeaderHeight), UI_Palette.TitleBarBg);
            Widgets.DrawBoxSolid(new Rect(inRect.x, inRect.y + HeaderHeight, inRect.width, 1f), UI_Palette.SectionBorder);
        }

        //标题带：「综合加成」+ 角色全名（小一号、染角色偏好色）
        protected override void DrawHeader(Rect inRect)
        {
            string title = (string)"FamiliaMyth.Bonus.Title".Translate();
            Rect titleRect = new(inRect.x + BorderGap + 2f, inRect.y + 9f, inRect.width - BorderGap * 2f - 30f, 28f);

            using (UI_Scope.Text(GameFont.Medium, TextAnchor.MiddleLeft))
            using (UI_Scope.Color(UI_Palette.Accent))
                Widgets.Label(titleRect, title);

            string name = pawn?.Name?.ToStringFull;
            if (name.NullOrEmpty()) return;

            float nameX = titleRect.x + UI_Text.Width(title, GameFont.Medium) + 8f;
            Rect nameRect = new(nameX, inRect.y + 12f, inRect.xMax - nameX - BorderGap - 28f, 22f);
            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            using (UI_Scope.Color(FavoriteColor()))
                Widgets.Label(nameRect, name);
        }

        protected override void DrawContent(Rect contentRect)
        {
            if (!TryGetPharna(out Hediff_Pharna pharna))
            {
                Close();
                return;
            }

            List<PharnaBonusStats.CategoryEntry> categories = Categories(pharna);

            if (categories.Count == 0)
            {
                using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleCenter))
                using (UI_Scope.Color(UI_Palette.TextNormal.WithAlpha(0.6f)))
                    Widgets.Label(contentRect, "FamiliaMyth.Bonus.Empty".Translate());
                return;
            }

            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            {
                float viewWidth = contentRect.width - ScrollBarW;
                float viewHeight = Mathf.Max(Measure(categories, viewWidth), contentRect.height);
                Rect view = new(0f, 0f, viewWidth, viewHeight);

                Widgets.BeginScrollView(contentRect, ref scroll, view);
                UI_Cursor cursor = new(0f, 0f, view.width);
                DrawCategories(categories, ref cursor);
                Widgets.EndScrollView();
            }
        }

        //==================== 收集 ====================

        private bool TryGetPharna(out Hediff_Pharna pharna)
        {
            pharna = pawn?.GetPharna();
            return pawn is not null && !pawn.Destroyed && !pawn.Dead && pharna is not null;
        }

        private List<PharnaBonusStats.CategoryEntry> Categories(Hediff_Pharna pharna)
        {
            if (cachedCategories == null || cachedFrame != Time.frameCount)
            {
                cachedFrame = Time.frameCount;
                cachedCategories = PharnaBonusStats.Collect(pharna);
            }
            return cachedCategories;
        }

        //==================== 绘制 ====================

        private void DrawCategories(List<PharnaBonusStats.CategoryEntry> categories, ref UI_Cursor cursor)
        {
            foreach (PharnaBonusStats.CategoryEntry category in categories)
            {
                cursor.Gap(GroupGap);
                bool categoryExpanded = expandedCategories.Contains(category.category);
                if (UI_FoldHeader.Draw(cursor.Row(CategoryHeaderH), CategoryLabel(category.category), null, categoryExpanded))
                {
                    categoryExpanded = !categoryExpanded;
                    if (categoryExpanded)
                    {
                        expandedCategories.Add(category.category);
                    }
                    else
                    {
                        //收起分类时一并复位其下属性的展开状态
                        expandedCategories.Remove(category.category);
                        foreach (PharnaBonusStats.StatEntry child in category.stats)
                            expandedStats.Remove(child.stat);
                    }
                }

                if (!categoryExpanded) continue;

                foreach (PharnaBonusStats.StatEntry statEntry in category.stats)
                {
                    cursor.Gap(StatGap);
                    bool statExpanded = expandedStats.Contains(statEntry.stat);
                    Rect statHeader = cursor.Row(IndentStep, cursor.Width - IndentStep, StatHeaderH);
                    if (UI_FoldHeader.Draw(statHeader, statEntry.stat.label.CapitalizeFirst(), null, statExpanded))
                    {
                        statExpanded = !statExpanded;
                        if (statExpanded) expandedStats.Add(statEntry.stat);
                        else expandedStats.Remove(statEntry.stat);
                    }

                    if (!statExpanded) continue;

                    cursor.Gap(StatGap);
                    for (int i = 0; i < statEntry.lines.Count; i++)
                        DrawLine(statEntry.lines[i], i, cursor.Width, ref cursor);
                }
            }
            cursor.Gap(BottomPad);
        }

        //单条说明：左栏信息与右栏数值各自铺底，中间留列间距形成两块
        private static void DrawLine(PharnaBonusStats.Line line, int index, float width, ref UI_Cursor cursor)
        {
            float descWidth = width - LineIndent - ValueWidth - ColumnGap;
            string desc = " · " + line.desc;
            float height = Mathf.Max(LineMinH, CalcHeight(desc, descWidth));
            Rect row = cursor.Row(LineIndent, width - LineIndent, height);

            Rect descRect = new(row.x, row.y, descWidth, height);
            Rect valueRect = new(row.xMax - ValueWidth, row.y, ValueWidth, height);
            DrawLineFill(descRect, index);
            DrawLineFill(valueRect, index);

            using (UI_Scope.Color(UI_Palette.TextNormal))
                Widgets.Label(descRect, desc);

            if (line.value.Length > 0)
            {
                using (UI_Scope.Text(GameFont.Small, TextAnchor.UpperRight))
                using (UI_Scope.Color(ValueColor(line.sign)))
                    Widgets.Label(valueRect, line.value);
            }
        }

        //斑马纹：偶数行叠一层淡白
        private static void DrawLineFill(Rect rect, int index)
        {
            Widgets.DrawBoxSolid(rect, UI_Palette.CardBg);
            if ((index & 1) == 0)
                Widgets.DrawBoxSolid(rect, UI_Palette.RowStripe);
        }

        //==================== 度量 ====================

        private float Measure(List<PharnaBonusStats.CategoryEntry> categories, float width)
        {
            float height = 0f;
            float descWidth = width - LineIndent - ValueWidth - ColumnGap;

            foreach (PharnaBonusStats.CategoryEntry category in categories)
            {
                height += GroupGap + CategoryHeaderH;
                if (!expandedCategories.Contains(category.category)) continue;

                foreach (PharnaBonusStats.StatEntry statEntry in category.stats)
                {
                    height += StatGap + StatHeaderH;
                    if (!expandedStats.Contains(statEntry.stat)) continue;

                    height += StatGap;
                    foreach (PharnaBonusStats.Line line in statEntry.lines)
                        height += Mathf.Max(LineMinH, CalcHeight("· " + line.desc, descWidth));
                }
            }
            return height + BottomPad;
        }

        private static float CalcHeight(string text, float width)
        {
            using (UI_Scope.Text(GameFont.Small, TextAnchor.UpperLeft))
                return Text.CalcHeight(text, width);
        }

        //==================== 辅助 ====================

        private static string CategoryLabel(StatCategoryDef category)
            => category != null ? category.label.CapitalizeFirst() : string.Empty;

        private Color FavoriteColor()
        {
            ColorDef favorite = pawn?.story?.favoriteColor;
            return favorite?.color ?? Color.white;
        }

        private static Color ValueColor(int sign)
            => sign > 0 ? UI_Palette.BonusPositive
             : sign < 0 ? UI_Palette.BonusNegative
             : UI_Palette.TextValue;
    }
}
