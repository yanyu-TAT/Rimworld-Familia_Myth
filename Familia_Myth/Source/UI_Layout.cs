using UnityEngine;

namespace FamiliaMyth.UI
{
    //纵向游标：按行取矩形并自动推进，使高度测算与绘制共用同一套步进
    public struct UI_Cursor
    {
        public float X;
        public float Y;
        public float Width;

        public UI_Cursor(float x, float y, float width)
        {
            X = x;
            Y = y;
            Width = width;
        }

        public UI_Cursor(Rect rect)
        {
            X = rect.x;
            Y = rect.y;
            Width = rect.width;
        }

        public Rect Row(float height) => Row(X, Width, height);

        public Rect Row(float width, float height) => Row(X, width, height);

        public Rect Row(float x, float width, float height)
        {
            Rect r = new(x, Y, width, height);
            Y += height;
            return r;
        }

        public void Gap(float px) => Y += px;
    }
}
