using System;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    //GUI 全局状态的作用域包装：离开 using 块即恢复进入前的值
    public static class UI_Scope
    {
        public static TextState Text(GameFont font, TextAnchor anchor) => new(font, anchor);

        public static ColorState Color(Color color) => new(color);

        public static GroupState Group(Rect rect) => new(rect);

        public readonly struct TextState : IDisposable
        {
            private readonly GameFont font;
            private readonly TextAnchor anchor;

            internal TextState(GameFont font, TextAnchor anchor)
            {
                this.font = Verse.Text.Font;
                this.anchor = Verse.Text.Anchor;
                Verse.Text.Font = font;
                Verse.Text.Anchor = anchor;
            }

            public void Dispose()
            {
                Verse.Text.Font = font;
                Verse.Text.Anchor = anchor;
            }
        }

        public readonly struct ColorState : IDisposable
        {
            private readonly Color color;

            internal ColorState(Color color)
            {
                this.color = GUI.color;
                GUI.color = color;
            }

            public void Dispose() => GUI.color = color;
        }

        public readonly struct GroupState : IDisposable
        {
            internal GroupState(Rect rect) => GUI.BeginGroup(rect);

            public void Dispose() => GUI.EndGroup();
        }
    }
}
