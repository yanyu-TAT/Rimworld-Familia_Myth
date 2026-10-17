using FamiliaMyth.Core;
using FamiliaMyth.Pharna;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    /// <summary>
    /// 六维加成曲线的行内编辑器：条目头（名称 + 重置）与绘图区（吸附拖点 / 加点 / 删点 / 悬停指示）
    /// </summary>
    public static class UI_CurveEditor
    {
        private const float RowGap = 4f;
        private const float ResetWidth = 52f;
        private const float IconSize = 18f;
        private const float HitRadius = 9f;
        private const float SnapUnit = 100f;
        private const float HoverSize = 12f;
        private const float MinGap = SnapUnit;
        private const int GridCount = 10;

        private static readonly Color HoverAddColor = new(1f, 1f, 1f, 0.35f);
        private static readonly Color HoverDelColor = new(1f, 0.3f, 0.25f, 0.5f);
        private static readonly Color PlotBg = new(0.07f, 0.08f, 0.11f);
        private static readonly Color GridLineColor = new(1f, 1f, 1f, 0.16f);
        private static readonly Color PlotBorderColor = new(1f, 1f, 1f, 0.4f);
        private static readonly Rect ViewRect = new(0f, 0f, PharnaCurve.CurveMax, PharnaCurve.CurveMax);

        private static readonly SimpleCurveDrawerStyle Style = new()
        {
            DrawBackground = false,
            DrawBackgroundLines = false,
            DrawPoints = true,
            DrawCurveMousePoint = false,
            DrawMeasures = false,
            UseFixedSection = true,
            FixedSection = new FloatRange(0f, PharnaCurve.CurveMax),
            UseFixedScale = true,
            FixedScale = new Vector2(0f, PharnaCurve.CurveMax),
        };

        private static int dragIndex = -1;

        /// <summary>条目头间距，供设置页排布曲线行</summary>
        public static float RowSpacing => RowGap;

        /// <summary>绘制条目头（row 已按层级缩进，底色铺满 row）；返回是否应切换展开状态</summary>
        public static bool DrawRow(Rect row, SimpleCurve curve, PharnaStatusType statusType, bool expanded)
        {
            Widgets.DrawBoxSolid(row, UI_Palette.CardBg);
            Widgets.DrawBoxSolid(new Rect(row.x, row.y, 3f, row.height), expanded ? UI_Palette.Accent : UI_Palette.Disabled);

            Rect resetRect = new(row.xMax - ResetWidth - 4f, row.y + 2f, ResetWidth, row.height - 4f);
            Rect rowClick = new(row.x, row.y, Mathf.Max(resetRect.x - row.x - 2f, 1f), row.height);
            Widgets.DrawHighlightIfMouseover(rowClick);
            bool clicked = Widgets.ButtonInvisible(rowClick);

            Rect iconRect = new(row.x + 8f, row.y + (row.height - IconSize) * 0.5f, IconSize, IconSize);
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

            Rect labelRect = new(iconRect.xMax + 6f, row.y, Mathf.Max(resetRect.x - iconRect.xMax - 10f, 1f), row.height);
            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
                Widgets.Label(labelRect, curve is null ? statusType.ToString() : Hediff_Pharna.GetStatusName(statusType));

            if (Widgets.ButtonText(resetRect, "FamiliaMyth.Settings.CurveReset".Translate()))
            {
                FamiliaMythSettings.ResetCurve(statusType);
            }

            return clicked;
        }

        /// <summary>绘制绘图区：先交互，再绘制曲线，最后画悬停指示；绘图区裁为正方形以保证 x/y 等比</summary>
        public static void DrawPlot(Rect plot, SimpleCurve curve)
        {
            if (curve is null || curve.PointsCount < 2)
            {
                return;
            }
            float side = Mathf.Min(plot.width, plot.height);
            Rect square = new(plot.x, plot.y, side, side);
            HandleInput(square, curve);
            DrawPlotBackground(square);
            SimpleCurveDrawer.DrawCurve(square, curve, Style);
            DrawHoverIndicator(square, curve);
        }

        //深色底 + 浅色网格，两个方向各十等分
        private static void DrawPlotBackground(Rect plot)
        {
            Widgets.DrawBoxSolid(plot, PlotBg);
            for (int i = 1; i < GridCount; i++)
            {
                float t = (float)i / GridCount;
                Widgets.DrawBoxSolid(new Rect(plot.x + plot.width * t, plot.y, 1f, plot.height), GridLineColor);
                Widgets.DrawBoxSolid(new Rect(plot.x, plot.y + plot.height * t, plot.width, 1f), GridLineColor);
            }
            Widgets.DrawBoxSolid(new Rect(plot.x, plot.y, plot.width, 1f), PlotBorderColor);
            Widgets.DrawBoxSolid(new Rect(plot.x, plot.yMax - 1f, plot.width, 1f), PlotBorderColor);
            Widgets.DrawBoxSolid(new Rect(plot.x, plot.y, 1f, plot.height), PlotBorderColor);
            Widgets.DrawBoxSolid(new Rect(plot.xMax - 1f, plot.y, 1f, plot.height), PlotBorderColor);
        }

        //按 100 为最低分度吸附
        private static float Snap(float value) => Mathf.Round(value / SnapUnit) * SnapUnit;

        private static void HandleInput(Rect plot, SimpleCurve curve)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseUp)
            {
                dragIndex = -1;
                return;
            }
            if (!Mouse.IsOver(plot))
            {
                return;
            }

            Vector2 local = e.mousePosition - plot.position;
            Vector2 raw = SimpleCurveDrawer.ScreenToCurveCoords(plot, ViewRect, local);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                int hit = HitTest(plot, curve, local);
                if (hit >= 0)
                {
                    dragIndex = hit;
                    e.Use();
                    return;
                }
                if (e.clickCount == 2 && TryGetAddPosition(curve, raw.x, raw.y, out CurvePoint add))
                {
                    curve.Add(add);
                    dragIndex = curve.Points.IndexOf(add);
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag && dragIndex >= 0)
            {
                MovePoint(curve, dragIndex, raw);
                e.Use();
            }
            else if (e.type == EventType.MouseDown && e.button == 1)
            {
                int hit = HitTest(plot, curve, local);
                if (hit > 0 && hit < curve.PointsCount - 1)
                {
                    curve.Points.RemoveAt(hit);
                    dragIndex = -1;
                    e.Use();
                }
            }
        }

        private static void MovePoint(SimpleCurve curve, int index, Vector2 raw)
        {
            int last = curve.PointsCount - 1;
            float x;
            if (index <= 0)
            {
                x = 0f;
            }
            else if (index >= last)
            {
                x = PharnaCurve.CurveMax;
            }
            else
            {
                float lo = curve.Points[index - 1].x + MinGap;
                float hi = curve.Points[index + 1].x - MinGap;
                x = lo > hi ? curve.Points[index].x : Mathf.Clamp(Snap(raw.x), lo, hi);
            }
            float y = Mathf.Clamp(Snap(raw.y), 0f, PharnaCurve.CurveMax);
            curve[index] = new CurvePoint(x, y);
            curve.SortPoints();
        }

        //命中测试：返回最近点的索引，未命中为 -1（像素半径判定，与绘制同基准）
        private static int HitTest(Rect plot, SimpleCurve curve, Vector2 local)
        {
            float best = HitRadius * HitRadius;
            int hit = -1;
            for (int i = 0; i < curve.PointsCount; i++)
            {
                Vector2 point = SimpleCurveDrawer.CurveToScreenCoordsInsideScreenRect(plot, ViewRect, curve.Points[i].Loc);
                float sqr = (point - local).sqrMagnitude;
                if (sqr <= best)
                {
                    best = sqr;
                    hit = i;
                }
            }
            return hit;
        }

        //加点位置：吸附后须落在相邻两点之间且与两侧各留一个分度
        private static bool TryGetAddPosition(SimpleCurve curve, float rawX, float rawY, out CurvePoint snapped)
        {
            snapped = default;
            if (curve.PointsCount < 2)
            {
                return false;
            }
            float x = Mathf.Clamp(Snap(rawX), MinGap, PharnaCurve.CurveMax - MinGap);
            for (int i = 0; i < curve.PointsCount - 1; i++)
            {
                float left = curve.Points[i].x;
                float right = curve.Points[i + 1].x;
                if (x <= left || x >= right)
                {
                    continue;
                }
                if (x - left < MinGap || right - x < MinGap)
                {
                    return false;
                }
                snapped = new CurvePoint(x, Mathf.Clamp(Snap(rawY), 0f, PharnaCurve.CurveMax));
                return true;
            }
            return false;
        }

        //悬停指示：可删中间点画红点，可加点画白点（落在吸附后的位置），其余不画
        private static void DrawHoverIndicator(Rect plot, SimpleCurve curve)
        {
            if (Event.current.type != EventType.Repaint || dragIndex >= 0)
            {
                return;
            }
            Vector2 local = Event.current.mousePosition - plot.position;
            if (local.x < 0f || local.y < 0f || local.x > plot.width || local.y > plot.height)
            {
                return;
            }

            Vector2 raw = SimpleCurveDrawer.ScreenToCurveCoords(plot, ViewRect, local);
            int hit = HitTest(plot, curve, local);
            Color color;
            Vector2 target;
            if (hit > 0 && hit < curve.PointsCount - 1)
            {
                color = HoverDelColor;
                target = SimpleCurveDrawer.CurveToScreenCoordsInsideScreenRect(plot, ViewRect, curve.Points[hit].Loc);
            }
            else if (hit < 0 && TryGetAddPosition(curve, raw.x, raw.y, out CurvePoint add))
            {
                color = HoverAddColor;
                target = SimpleCurveDrawer.CurveToScreenCoordsInsideScreenRect(plot, ViewRect, add.Loc);
            }
            else
            {
                return;
            }

            Rect dot = new(plot.x + target.x - HoverSize * 0.5f, plot.y + target.y - HoverSize * 0.5f, HoverSize, HoverSize);
            Texture2D pointTex = ContentFinder<Texture2D>.Get("UI/Widgets/Dev/CurvePoint", false);
            if (pointTex is null)
            {
                Widgets.DrawBoxSolid(dot, color);
            }
            else
            {
                using (UI_Scope.Color(color))
                    GUI.DrawTexture(dot, pointTex);
            }
        }
    }
}
