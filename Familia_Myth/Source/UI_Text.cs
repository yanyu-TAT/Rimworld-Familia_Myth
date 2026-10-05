using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    //文本度量与截断
    public static class UI_Text
    {
        private const string Ellipsis = "…";

        //超出宽度的文本截断为「前缀…」；装得下则原样返回
        public static string Truncate(string text, float width, GameFont font = GameFont.Tiny)
        {
            if (string.IsNullOrEmpty(text) || width <= 0f)
                return text;

            using (UI_Scope.Text(font, TextAnchor.UpperLeft))
            {
                float full = Text.CalcSize(text).x;
                if (full <= width)
                    return text;

                int len = Mathf.Clamp(Mathf.FloorToInt(text.Length * width / full), 1, text.Length);
                while (len > 1 && Text.CalcSize(text.Substring(0, len) + Ellipsis).x > width)
                    len--;
                while (len < text.Length && Text.CalcSize(text.Substring(0, len + 1) + Ellipsis).x <= width)
                    len++;

                return text.Substring(0, len) + Ellipsis;
            }
        }

        //当前字体下文本宽度
        public static float Width(string text, GameFont font = GameFont.Tiny)
        {
            if (string.IsNullOrEmpty(text))
                return 0f;

            using (UI_Scope.Text(font, TextAnchor.UpperLeft))
                return Text.CalcSize(text).x;
        }
    }
}
