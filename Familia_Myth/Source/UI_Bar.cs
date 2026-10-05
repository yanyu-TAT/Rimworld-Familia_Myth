using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    //进度条：底槽 + 纯色填充 + 顶部高光，可选门槛刻度线
    public static class UI_Bar
    {
        private const float HighlightHeight = 1f;
        private const float MinFillWidth = 0.5f;
        private static readonly Dictionary<Color, Texture2D> fillCache = new();

        public static void Draw(Rect r, float frac, Color fill)
        {
            Widgets.DrawBoxSolid(r, UI_Palette.BarTrack);

            float w = r.width * Mathf.Clamp01(frac);
            if (w < MinFillWidth)
                return;

            Rect filled = new(r.x, r.y, w, r.height);
            GUI.DrawTexture(filled, FillTexture(fill));
            Widgets.DrawBoxSolid(new Rect(filled.x, filled.y, filled.width, HighlightHeight), new Color(1f, 1f, 1f, 0.12f));
        }

        //thresholdFrac 为归一化门槛位置（0~1）
        public static void DrawWithThreshold(Rect r, float frac, Color fill, float thresholdFrac, Color thresholdColor)
        {
            Draw(r, frac, fill);

            float x = r.x + r.width * Mathf.Clamp01(thresholdFrac);
            if (x <= r.x + 1f || x >= r.xMax - 1f)
                return;

            Widgets.DrawBoxSolid(new Rect(x, r.y - 1f, 1f, r.height + 2f), thresholdColor);
        }

        private static Texture2D FillTexture(Color color)
        {
            if (fillCache.TryGetValue(color, out Texture2D tex))
                return tex;

            tex = SolidColorMaterials.NewSolidColorTexture(color);
            fillCache[color] = tex;
            return tex;
        }
    }
}
