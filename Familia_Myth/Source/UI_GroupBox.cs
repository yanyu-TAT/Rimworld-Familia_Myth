using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    public static class UI_GroupBox
    {
        public static readonly Color ColorContent = new(47f / 255f, 48f / 255f, 55f / 255f, 0.98f); //内容区背景色
        public static readonly Color ColorTitleBar = new(14f / 255f, 16f / 255f, 39f / 255f, 1f);   //标题区背景色
        public const float TitleHeight = 20f;
        public const float BasicMargin = 4f;

        //带标题栏的卡片
        public static void DrawCardBg(Rect r, string title, Color color)
        {
            Widgets.DrawBoxSolid(r, ColorContent);
            if (!string.IsNullOrEmpty(title))
            {
                Rect titleR = new(r.x, r.y, r.width, TitleHeight);
                Widgets.DrawBoxSolid(titleR, ColorTitleBar);
                Text.Font = GameFont.Small;
                GUI.color = color;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(titleR, title);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
            Rect highlight = new(r.x, r.y, r.width, 6f);
            VGradientHighlight(highlight, 1f, color);
        }

        //四边描边（空心卡与分区卡共用）
        public static void DrawOutlinedBox(Rect r, Color borderColor, float thickness = 3f)
        {
            Widgets.DrawBoxSolid(new Rect(r.x, r.y, r.width, thickness), borderColor);
            Widgets.DrawBoxSolid(new Rect(r.x, r.yMax - thickness, r.width, thickness), borderColor);
            Widgets.DrawBoxSolid(new Rect(r.x, r.y, thickness, r.height), borderColor);
            Widgets.DrawBoxSolid(new Rect(r.xMax - thickness, r.y, thickness, r.height), borderColor);
        }

        //带描边与标题栏的卡片：内容区与标题栏沿用既有填充色，强调色仅用于描边、标题文字与顶部高光
        public static void DrawOutlinedCardBg(Rect r, string title, Color borderColor)
        {
            Widgets.DrawBoxSolid(r, ColorContent);
            if (!string.IsNullOrEmpty(title))
            {
                Rect titleR = new(r.x, r.y, r.width, TitleHeight);
                Widgets.DrawBoxSolid(titleR, ColorTitleBar);
                Text.Font = GameFont.Small;
                GUI.color = borderColor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(titleR, title);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
            DrawOutlinedBox(r, borderColor);
            Rect highlight = new(r.x + 3f, r.y + 3f, r.width - 6f, 4f);
            VGradientHighlight(highlight, 0.6f, borderColor);
        }

        //纯色背景 + 横向渐变高光 + 左侧色条 + 浅色细边框
        public static void DrawPlainBg(Rect r, Color? bgColor = null, Color? accentColor = null, float highlightIntensity = 1f)
        {
            Widgets.DrawBoxSolid(r, bgColor ?? ColorContent);
            //浅色细边框
            DrawOutlinedBox(r, Color.gray.WithAlpha(0.5f));
            //顶部横向渐变高光
            Rect highlight = new(r.x, r.y, r.width, 4f);
            VGradientHighlight(highlight, highlightIntensity, accentColor ?? Color.white);
            //左侧色条
            if (accentColor.HasValue)
                Widgets.DrawBoxSolid(new Rect(r.x, r.y, 4f, r.height), accentColor.Value);
        }

        //返回卡片的内容区
        public static Rect ContentRect(Rect r, bool hasTitle = true)
        {
            float t = hasTitle ? TitleHeight : 0f;
            return new Rect(r.x + BasicMargin, r.y + t + BasicMargin,
                r.width - BasicMargin * 2f,
                r.height - t - BasicMargin * 2f);
        }

        private static Texture2D hgradCache;
        public static Color ColorAccent = Color.white;

        private static void VGradientHighlight(Rect r, float intensity, Color color)
        {
            if (hgradCache == null)
            {
                hgradCache = new Texture2D(8, 1, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    wrapMode = TextureWrapMode.Clamp
                };
                for (int x = 0; x < 8; x++)
                {
                    float a = Mathf.Lerp(intensity, 0f, x / 7f);
                    hgradCache.SetPixel(x, 0, new Color(1f, 1f, 1f, a));
                }
                hgradCache.Apply();
            }
            var prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(r, hgradCache);
            GUI.color = prev;
        }
    }
}