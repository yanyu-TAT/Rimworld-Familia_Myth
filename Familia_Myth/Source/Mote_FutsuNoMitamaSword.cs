using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 布都御魂法阵中心的巨剑：自屏幕上方下降并淡入，随后整把剑向下沉入地面，截断线固定在法阵中心。
    /// </summary>
    public class Mote_FutsuNoMitamaSword : Mote
    {
        //竖直分段数（越大截断越平滑）
        private const int SegmentCount = 32;
        //入场：从上方下降的距离（世界 Z，+Z 为屏幕上方）与时长
        private const float EntryHeight = 3f;
        private const float EntrySeconds = 0.6f;
        //完全沉入前最后这段进度内淡出
        private const float SinkFadeFrac = 0.1f;

        private static Mesh[] segments;
        private static readonly MaterialPropertyBlock mpb = new();

        private Material mat;
        private float progress;

        /// <summary>
        /// 0 = 刚展开，1 = 完全沉入地面（由 Spell 按剩余持续时间写入）。
        /// </summary>
        public float Progress
        {
            get => progress;
            set => progress = Mathf.Clamp01(value);
        }

        public void Init(Color color) => instanceColor = color;

        private float EntryT => EntrySeconds <= 0f ? 1f : Mathf.Clamp01(AgeSecs / EntrySeconds);

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            EnsureSegments();

            float entryT = EntryT;
            float alpha = entryT * Mathf.Clamp01(Alpha);
            if (SinkFadeFrac > 0f)
                alpha *= Mathf.Clamp01((1f - progress) / SinkFadeFrac);
            if (alpha <= 0.001f) return;

            Vector2 size = def.graphicData.drawSize;
            float segH = size.y / SegmentCount;
            float entryZ = EntryHeight * (1f - entryT);
            //剑底初始位于法阵中心，下沉时整把剑向下（-Z）移动；截断线固定为法阵中心
            Vector3 drawPos = drawLoc + new Vector3(0.5f, 0, 0.5f);
            float bottomZ = drawPos.z - progress * size.y + entryZ;
            float clipZ = drawLoc.z;

            mat ??= Graphic.MatSingle;
            mpb.SetColor(ShaderPropertyIDs.Color, instanceColor.WithAlpha(alpha));

            for (int i = 0; i < SegmentCount; i++)
            {
                float segBottom = bottomZ + i * segH;
                float segTop = segBottom + segH;
                if (segTop <= clipZ) continue;

                float scaleZ = 1f;
                float centerZ;
                if (segBottom < clipZ)
                {
                    //跨界段：只保留截断线以上的部分，按剩余高度压缩
                    float visible = segTop - clipZ;
                    scaleZ = visible / segH;
                    centerZ = clipZ + visible * 0.5f;
                }
                else
                {
                    centerZ = segBottom + segH * 0.5f;
                }

                var pos = new Vector3(drawLoc.x, drawLoc.y, centerZ);
                var matrix = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(1f, 1f, scaleZ));
                Graphics.DrawMesh(segments[i], matrix, mat, 0, null, 0, mpb, false, false, false);
            }
        }

        private static readonly int[] Triangles = new[] { 0, 1, 2, 0, 2, 3 };
        private void EnsureSegments()
        {
            if (segments != null) return;

            Vector2 size = def.graphicData.drawSize;
            float halfW = size.x * 0.5f;
            float segH = size.y / SegmentCount;
            segments = new Mesh[SegmentCount];

            for (int i = 0; i < SegmentCount; i++)
            {
                float v0 = (float)i / SegmentCount;
                float v1 = (float)(i + 1) / SegmentCount;
                var mesh = new Mesh
                {
                    name = $"FutsuNoMitamaSwordSegment{i}",
                    vertices = new[]
                    {
                        new Vector3(-halfW, 0f, -segH * 0.5f),
                        new Vector3(-halfW, 0f, segH * 0.5f),
                        new Vector3(halfW, 0f, segH * 0.5f),
                        new Vector3(halfW, 0f, -segH * 0.5f)
                    },
                    uv = new[]
                    {
                        new Vector2(0f, v0),
                        new Vector2(0f, v1),
                        new Vector2(1f, v1),
                        new Vector2(1f, v0)
                    }
                };
                mesh.SetTriangles(Triangles, 0);
                mesh.RecalculateBounds();
                segments[i] = mesh;
            }
        }
    }
}
