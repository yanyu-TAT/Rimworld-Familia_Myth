using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_Incantation : MoteAttached
    {
        private const float fadeInOut = 0.6f;
        public const int TextHeight = 1024;
        private const int MeshHeight = 8;

        public Texture2D tex;
        public string text;
        public float duration = 1f;

        private readonly MaterialPropertyBlock mpb = new();
        private static readonly int MainTexID = Shader.PropertyToID("_MainTex");

        private Mesh mesh = null;
        private int retryCnt = 0;
        private Vector4 _TexRect = new(0f, 0f, 1f, 1f);   // 文字区在 quad 中的 UV 矩形
        private float quadAspect = 1f;                     // quad 宽高比（含透明边距）

        public void Init(string s, Color color, float duration)
        {
            text = s;
            tex = IncantationTextureCache.GetTexture(s);
            instanceColor = color;
            this.duration = duration;
            //linearScale = new(10f, 1f, 10f);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            // 兜底：若构造时未能拿到纹理，用记录的原文再取一次
            tex ??= IncantationTextureCache.GetTexture(text ?? "");
        }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            if (AgeSecs >= duration)
                Destroy();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            // 若当前仍是占位/空纹理，仍从缓存重取一次，防止 Init 时未就绪导致永久不可见
            if (tex == null || tex == IncantationTextureCache.PlaceholderTex)
            {
                retryCnt++;
                tex = IncantationTextureCache.GetTexture(text ?? "");
                if (tex == null || tex == IncantationTextureCache.PlaceholderTex)
                {
                    if (retryCnt >= 10)
                    {
                        Log.Warning($"[FamiliaMyth] Incantation For \"{text}\" Tried To Generate Texture For Over 10 Times. Now Destroyed.");
                        Destroy();
                    }
                    return; // 仍未就绪，本帧跳过
                }
            }

            //中断时固定到消散起点（不回退）；否则按时间线性推进
            float progress = Mathf.Clamp01(AgeSecs / duration);
            float burst = Mathf.Clamp01((progress - 0.75f) / 0.25f);
            float a = Mathf.Min(Mathf.Clamp01(AgeSecs / fadeInOut), Mathf.Clamp01((duration - AgeSecs) / fadeInOut));

            mpb.Clear();
            mpb.SetTexture(MainTexID, tex);
            mpb.SetColor(ShaderPropertyIDs.Color, instanceColor.WithAlpha(a));
            mpb.SetFloat("_Progress", progress);

            Vector3 pos = drawLoc + Vector3.up * 0.1f;

            if (mesh == null)
            {
                // 显示 quad：在紧凑文字纹理基础上，两侧各"拼接"一段透明区作粒子施展空间。
                // 文字世界宽度保持在 textPxW/TextHeight × MeshHeight（与烘焙一致 → 清晰度不变）。
                float textAspect = (float)tex.width / tex.height;          // 紧凑纹理宽高比（=文字）
                float marginRatio = 1.4f;                                  // 两侧透明边距 = 文字宽 × 此比例
                float textU = 1.0f / (1.0f + marginRatio);                 // 文字在 quad 中占的 UV 宽
                quadAspect = textAspect / textU;                           // quad 宽高比（含边距）
                _TexRect = new Vector4(0.5f - textU * 0.5f, 0f, textU, 1f); // 文字区在 quad 中的 UV 矩形
                mesh = MeshPool.GridPlane(new(quadAspect * MeshHeight, MeshHeight));
            }
            // MPB 每帧被 Clear，必须在每帧重新设置这些 uniform
            mpb.SetVector("_TexRect", _TexRect);
            mpb.SetFloat("_QuadAspect", quadAspect);

            var matrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
            Graphics.DrawMesh(mesh, matrix,IncantationTextureCache.WhiteMat, 0, null, 0, mpb, false, false, false);

            //Log.Message($"Using Shader{IncantationTextureCache.WhiteMat.shader.name}");
        }
    }

    [StaticConstructorOnStartup]
    public static class IncantationTextureCache
    {
        const int TextHeight = Mote_Incantation.TextHeight;

        private readonly static Dictionary<string, Texture2D> textures = new();

        /// <summary>缓存未就绪时的占位（纯透明 1x1），调用方据此判断“尚未生成”。</summary>
        public static readonly Texture2D PlaceholderTex = BuildPlaceholder();

        /// <summary>CPU 栅格化复用的动态字体（含中文字形），全局单例。</summary>
        private static Font bakedFont;

        private static Material whiteMat;

        public static Material WhiteMat
        {
            get
            {
                if (whiteMat == null)
                {
                    var shader = DefDatabase<ShaderTypeDef>.GetNamed("IncantationText").Shader;
                    whiteMat = new Material(shader)
                    {
                        name = "Mote_Incantation_SharedWhite",
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    whiteMat.SetColor("_Color", Color.white);
                    whiteMat.SetTexture("_NoiseTex", ContentFinder<Texture2D>.Get("Familia_Myth/Noise"));
                }

                return whiteMat;
            }
        }

        private static Texture2D BuildPlaceholder()
        {
            var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false)
            {
                name = "Incantation_Placeholder",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixel(0, 0, new Color(0, 0, 0, 0));
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 返回词条 s 的纹理缓存。
        /// 命中直接返回；未命中则同步生成并缓存，生成失败时返回 <see cref="PlaceholderTex"/>（调用方据此重试）。
        /// </summary>
        public static Texture2D GetTexture(string s)
        {
            if (textures.TryGetValue(s, out var t))
                return t;

            try
            {
                var tex = BuildTexture(s);
                if (tex != null)
                    return tex;
            }
            catch (Exception e)
            {
                Log.Error($"生成咏唱文字纹理失败（\"{s}\"）：{e}");
            }
            return PlaceholderTex;
        }


        private static Texture2D BuildTexture(string s)
        {
            // 用 TextMesh + 正交相机把文本光栅化到 RT（GPU 真正渲染字形）。
            // 避开两个不可靠点：IMGUI 会渲染到屏幕而非 RT、动态字库图集(Alpha8)的 CPU 读取不同步。
            Camera bakeCam;
            GameObject bakeGo = null;
            GameObject camGo = null;
            Material mat = null;
            RenderTexture rt = null;
            try
            {
                const int fontSize = 96;
                // 默认楷体，找不到时回退到微软雅黑，再回退宋体/Arial
                bakedFont ??= Font.CreateDynamicFontFromOSFont(
                    new[] { "楷体", "KaiTi", "微软雅黑", "Microsoft YaHei", "SimSun", "Arial" }, fontSize);

                // 1) 建 TextMesh（白色，左下对齐到原点），根据实际 bounds 确定世界尺寸
                // TextMesh 组件自带 MeshRenderer，无需也不能重复添加
                bakeGo = new GameObject("IncantBake", typeof(TextMesh))
                {
                    transform = { position = Vector3.zero }
                };
                var tm = bakeGo.GetComponent<TextMesh>();
                tm.font = bakedFont;
                tm.text = s;
                tm.fontSize = fontSize;
                tm.characterSize = 0.1f;
                tm.anchor = TextAnchor.LowerLeft;
                tm.alignment = TextAlignment.Left;
                tm.fontStyle = FontStyle.Bold;   // 加粗
                tm.color = Color.white;

                // 取 TextMesh 生成的网格真实包围盒作为世界尺寸（反映顶点范围，不受 advance 单位混淆）
                var mr = bakeGo.GetComponent<MeshRenderer>();
                mat = new Material(bakedFont.material);   // 临时材质，finally 中释放
                mr.material = mat;
                var b = mr.bounds;
                float lineHeight = tm.fontSize * tm.characterSize;           // 字形世界高（作为下限）

                // 文字真实世界尺寸（不含边距）
                float textW = Mathf.Max(b.size.x, 0.1f);
                float textH = Mathf.Max(b.size.y, lineHeight, 0.1f); // 高度取网格真实高（汉字易溢出）

                // 把 TextMesh 中心移到世界原点，相机正对原点即可完整覆盖
                bakeGo.transform.position = new Vector3(-b.center.x, -b.center.y, 0f);

                // 2) 纹理布局：只存最小的文字紧贴区域（textPxW × TextHeight），不含边距。
                //    透明边距在渲染时由 DrawAt 通过 UV 重映射"拼接"到 quad 周围（节省显存，清晰度不变）。
                float textPxH = TextHeight;
                float textPxW = Mathf.CeilToInt(textPxH * (textW / textH)); // 文字目标像素宽（保持宽高比）
                int totalH = TextHeight;
                int totalW = (int)textPxW;                                  // 紧凑：只有文字

                // 相机可视世界范围 = 文字世界尺寸（文字铺满纹理，纹理即文字）
                float worldH_total = textH * (totalH / textPxH);
                float worldW_total = worldH_total * (totalW / (float)totalH);

                rt = new RenderTexture(totalW, totalH, 0, RenderTextureFormat.ARGB32);
                camGo = new GameObject("IncantBakeCam");
                bakeCam = camGo.AddComponent<Camera>();
                bakeCam.enabled = false;
                bakeCam.orthographic = true;
                bakeCam.clearFlags = CameraClearFlags.SolidColor;
                bakeCam.backgroundColor = new Color(0f, 0f, 0f, 0f);
                bakeCam.orthographicSize = worldH_total / 2f;
                bakeCam.aspect = worldW_total / worldH_total; // = totalW/totalH，用世界比例确保两端可见
                bakeCam.transform.position = new Vector3(0f, 0f, -10f);
                bakeCam.transform.rotation = Quaternion.identity;
                bakeCam.nearClipPlane = 0.1f;
                bakeCam.farClipPlane = 100f;
                bakeCam.targetTexture = rt;

                // 3) 渲染并从 RT 读回纹理
                bakeCam.Render();
                var tex = new Texture2D(totalW, totalH, TextureFormat.ARGB32, false)
                {
                    name = "Incantation_" + s,
                    hideFlags = HideFlags.HideAndDontSave,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, totalW, totalH), 0, 0);
                tex.Apply();

                textures[s] = tex;

                // 临时诊断：落盘确认字形已光栅化
                //var png = tex.EncodeToPNG();
                //File.WriteAllBytes($@"D:\mote_incant_debug_{s}.png", png);
                return tex;
            }
            finally
            {
                // 完整释放所有临时资源，防止泄漏；并恢复活动 RT，防止异常残留
                RenderTexture.active = null;
                if (rt != null) { rt.Release(); UnityEngine.Object.Destroy(rt); }
                if (camGo != null) UnityEngine.Object.Destroy(camGo);
                if (mat != null) UnityEngine.Object.Destroy(mat);
                if (bakeGo != null) UnityEngine.Object.Destroy(bakeGo);
            }
        }
    }
}
