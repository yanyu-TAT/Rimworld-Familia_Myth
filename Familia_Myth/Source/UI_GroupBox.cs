using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    public static class UI_GroupBox
    {
        public static readonly Color ColorContent = UI_Palette.ContentBg;      //内容区背景色
        public static readonly Color ColorTitleBar = UI_Palette.TitleBarBg;    //带标题栏卡片的标题底色
        public const float TitleHeight = 30f;
        public const float BasicMargin = 4f;
        //分区卡描边宽度（条目卡不描边）
        public const float SectionBorderThickness = 2f;
        //分区标题距左边框的内缩，以及标题前后用于截断边框的留白
        public const float TitleIndent = 20f;
        public const float TitlePaddingX = 6f;
        //分区上边框所在的水平线：标题中线
        public const float TitleLineOffset = TitleHeight / 2f;

        //带标题栏的卡片
        public static void DrawCardBg(Rect r, string title, Color color)
        {
            Widgets.DrawBoxSolid(r, ColorContent);
            if (!string.IsNullOrEmpty(title))
            {
                Widgets.DrawBoxSolid(new Rect(r.x, r.y, r.width, TitleHeight), ColorTitleBar);
                Rect titleR = new(r.x, r.y, r.width, TitleHeight);
                using (UI_Scope.Text(GameFont.Medium, TextAnchor.MiddleCenter))
                using (UI_Scope.Color(Color.white))
                    Widgets.Label(titleR, title);
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

        //分区卡：填充与边框同域（上边框下移至标题中线），标题左内缩并截断该段边框后压在其上
        public static void DrawOutlinedCardBg(Rect r, string title, Color borderColor)
        {
            float lineY = r.y + TitleLineOffset;
            //填充只在边框围住的区域内
            Widgets.DrawBoxSolid(new Rect(r.x, lineY, r.width, r.height - TitleLineOffset), ColorContent);

            if (string.IsNullOrEmpty(title))
            {
                DrawOutlinedBox(r, borderColor, SectionBorderThickness);
                return;
            }

            DrawSectionFrame(r, borderColor);
            DrawSectionTitle(r, title);
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

        private static void DrawSectionFrame(Rect r, Color borderColor)
        {
            float t = SectionBorderThickness;
            float lineY = r.y + TitleLineOffset;
            Widgets.DrawBoxSolid(new Rect(r.x, lineY, r.width, t), borderColor);
            Widgets.DrawBoxSolid(new Rect(r.x, r.yMax - t, r.width, t), borderColor);
            Widgets.DrawBoxSolid(new Rect(r.x, lineY, t, r.height - TitleLineOffset), borderColor);
            Widgets.DrawBoxSolid(new Rect(r.xMax - t, lineY, t, r.height - TitleLineOffset), borderColor);
        }

        private static void DrawSectionTitle(Rect r, string title)
        {
            const float TextHeight = 26f;
            const float MaskHeight = 8f;
            float lineY = r.y + TitleLineOffset;
            float textW = UI_Text.Width(title, GameFont.Medium);
            float x = r.x + TitleIndent;
            float y = lineY - TextHeight * 0.5f;

            //仅截断边框线本身，不在框外留下底色块
            Widgets.DrawBoxSolid(new Rect(x - TitlePaddingX, lineY - MaskHeight * 0.5f, textW + TitlePaddingX * 2f, MaskHeight),
                ColorContent);

            Rect titleR = new(x, y, textW, TextHeight);
            using (UI_Scope.Text(GameFont.Medium, TextAnchor.MiddleLeft))
            using (UI_Scope.Color(Color.white))
                Widgets.Label(titleR, title);
        }

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
            using (UI_Scope.Color(color))
                GUI.DrawTexture(r, hgradCache);
        }
    }
}
