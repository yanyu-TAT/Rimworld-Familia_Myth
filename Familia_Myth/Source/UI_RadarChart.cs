using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    public static class UI_RadarChart
    {
        private static readonly Dictionary<string, Texture2D> cache = new();
        private static readonly List<string> lru = new();
        private const int MaxCached = 16;

        public static void Draw(Rect r, (string label, float value)[] list, float labelPadding = 0f)
        {
            var max = list.Max(l => l.value);
            if (max <= 0f) max = 1f;

            Vector2 center = new(r.x + r.width / 2, r.y + r.height / 2);
            float radius = Mathf.Min(r.width, r.height) / 2f - labelPadding;

            //填充
            Texture2D tex = GetCachedTexture((int)r.width, (int)r.height, list, max);
            GUI.DrawTexture(r, tex);

            //同心参考网格
            for (int k = 1; k <= 3; k++)
            {
                float rr = radius * k / 3f;
                Vector2 prev = OnPoint(center, rr, list.Length - 1, list.Length);
                for (int i = 0; i < list.Length; i++)
                {
                    Vector2 now = OnPoint(center, rr, i, list.Length);
                    Widgets.DrawLine(prev, now, new Color(0.35f, 0.35f, 0.4f, 0.45f), 1f);
                    prev = now;
                }
            }

            //轴线
            for (int i = 0; i < list.Length; i++)
            {
                var p = OnPoint(center, radius, i, list.Length);
                Widgets.DrawLine(center, p, new Color(0.4f, 0.4f, 0.45f, 0.8f), 2f);
            }

            //标签
            for (int i = 0; i < list.Length; i++)
            {
                var p = OnPoint(center, radius + 12f, i, list.Length);
                Widgets.Label(new Rect(p.x - 25f, p.y - 10f, 50f, 20f), list[i].label);
            }
        }

        private static Vector2 OnPoint(Vector2 center, float radius, int index, int cnt)
        {
            var a = (float)(Mathf.PI / 2 + index * (Mathf.PI * 2f / cnt));
            return center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }

        private static Texture2D GetCachedTexture(int w, int h, (string label, float value)[] list, float max)
        {
            string sig = string.Join(",", list.Select(l => l.value.ToString("0.##")));
            string key = $"{w}x{h}|{sig}";

            if (cache.TryGetValue(key, out Texture2D tex))
            {
                lru.Remove(key);
                lru.Insert(0, key);
                return tex;
            }

            tex = BuildPolygonTexture(w, h, list, max);
            cache[key] = tex;
            lru.Insert(0, key);

            while (cache.Count > MaxCached)
            {
                string oldest = lru[lru.Count - 1];
                lru.RemoveAt(lru.Count - 1);
                if (cache.TryGetValue(oldest, out Texture2D oldTex))
                {
                    cache.Remove(oldest);
                    UnityEngine.Object.DestroyImmediate(oldTex);
                }
            }
            return tex;
        }

        private static Texture2D BuildPolygonTexture(int w, int h, (string label, float value)[] list, float max)
        {
            Texture2D tex = new(w, h, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            Vector2 center = new(w / 2f, h / 2f);
            float rad = Mathf.Min(w, h) / 2f;

            Vector2[] verts = new Vector2[list.Length];
            for (int i = 0; i < list.Length; i++)
                verts[i] = OnPoint(center, rad * (list[i].value / max), i, list.Length);

            Color fill = new(0.3f, 0.8f, 0.9f, 0.25f);
            Color clear = new(0f, 0f, 0f, 0f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector2 p = new(x + 0.5f, y + 0.5f);
                    //垂直翻转运行时纹理
                    tex.SetPixel(x, h - 1 - y, PointInPolygon(p, verts) ? fill : clear);
                }
            }

            tex.Apply();
            return tex;
        }

        private static bool PointInPolygon(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            int n = poly.Length;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                Vector2 a = poly[i], b = poly[j];
                if ((a.y > p.y) != (b.y > p.y) &&
                    p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}