using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    //窗口基类：统一内容区划分、初始尺寸换算与文本状态保护
    public abstract class Window_FamiliaBase : Window
    {
        public const float BorderGap = 10f;
        public const float HeaderHeight = 46f;

        //按屏幕比例取初始尺寸，保持界面宽高比不超出屏幕
        protected static Vector2 FitInitialSize(float screenFraction)
        {
            const float Aspect = 1.9142857f;
            float maxWidth = Screen.width * screenFraction / Prefs.UIScale;
            float maxHeight = Screen.height * screenFraction / Prefs.UIScale;
            float height = maxHeight;
            float width = height * Aspect;
            if (width > maxWidth)
            {
                width = maxWidth;
                height = width / Aspect;
            }
            return new Vector2(width, height);
        }

        //是否保留顶部标题带；不保留时内容区即整个窗口
        protected virtual bool HasHeaderArea => true;

        protected virtual float ScreenFraction => 0.9f;

        public override Vector2 InitialSize => FitInitialSize(ScreenFraction);

        public override void DoWindowContents(Rect inRect)
        {
            using (UI_Scope.Text(GameFont.Small, TextAnchor.UpperLeft))
            {
                DrawBackLayer(inRect);

                Rect content = HasHeaderArea
                    ? new Rect(inRect.x + BorderGap, inRect.y + HeaderHeight + BorderGap,
                        inRect.width - BorderGap * 2f, inRect.height - HeaderHeight - BorderGap * 2f)
                    : inRect;

                if (HasHeaderArea)
                    DrawHeader(inRect);

                DrawContent(content);
            }
        }

        //内容之前的底层绘制（背景装饰、边框等）
        protected virtual void DrawBackLayer(Rect inRect)
        {
        }

        protected virtual void DrawHeader(Rect inRect)
        {
        }

        protected abstract void DrawContent(Rect contentRect);
    }
}
