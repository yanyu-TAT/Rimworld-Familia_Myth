using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    //折叠头：标题栏底色、左侧强调竖条、行首 +/- 图标与整行点击切换
    public static class UI_FoldHeader
    {
        public const float IconSize = 18f;
        private const float IconOffsetX = 9f;
        private const float LabelGap = 7f;
        private const float AccentBarW = 3f;

        //返回是否被点击；rightReserve 为右侧留给按钮的宽度
        public static bool Draw(Rect rect, string label, string tooltip, bool expanded, float rightReserve = 0f)
        {
            Widgets.DrawBoxSolid(rect, UI_Palette.TitleBarBg);
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, AccentBarW, rect.height), UI_Palette.Accent);

            Rect clickRect = new(rect.x, rect.y, Mathf.Max(rect.width - rightReserve, 1f), rect.height);
            Widgets.DrawHighlightIfMouseover(clickRect);

            Rect iconRect = new(rect.x + IconOffsetX, rect.y + (rect.height - IconSize) * 0.5f, IconSize, IconSize);
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

            Rect labelRect = new(iconRect.xMax + LabelGap, rect.y,
                Mathf.Max(clickRect.xMax - iconRect.xMax - LabelGap, 1f), rect.height);
            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            using (UI_Scope.Color(UI_Palette.TextNormal))
                Widgets.Label(labelRect, label);

            if (!tooltip.NullOrEmpty())
                TooltipHandler.TipRegion(clickRect, tooltip);

            return Widgets.ButtonInvisible(clickRect);
        }
    }
}
