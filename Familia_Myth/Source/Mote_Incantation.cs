using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 咏唱词的烘焙和渲染，可能导致较高的显存占用
    /// </summary>
    public class Mote_Incantation : MoteAttached
    {
        private const float fadeInOut = 0.6f;
        public const int TextHeight = 512;
        /// <summary>咏唱词 quad 的世界高度：整体尺度，同时决定碎屑飞溅的最大范围。</summary>
        private const float MeshHeight = 2f;

        /// <summary>文字在 quad 高度中占的比例：调小即字变小，上下留白变多（碎屑飞溅空间）。</summary>
        private const float TextHeightRatio = 0.72f;

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
                float textV = TextHeightRatio;                             // 文字在 quad 中占的 UV 高，其余为上下留白
                quadAspect = textAspect * textV / textU;                   // quad 宽高比（含边距），保证文字不变形
                _TexRect = new Vector4(0.5f - textU * 0.5f, (1f - textV) * 0.5f, textU, textV);
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

        /// <summary>开启后把烘焙结果落盘到存档目录，便于排查字形渲染。</summary>
        public static bool DumpBakedTexture;

        public static Material WhiteMat
        {
            get
            {
                if (whiteMat == null)
                {
                    Shader shader = FamiliaMythShader.Resolve("IncantationText");
                    if (shader == null)
                    {
                        Log.Error("[FamiliaMyth] IncantationText shader unavailable, falling back to MoteGlow.");
                        shader = ShaderDatabase.MoteGlow;
                    }
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

        /// <summary>离线预渲染纹理所在的资源目录（相对 mod 根目录）。</summary>
        private const string OfflineTextureDir = "Familia_Myth/Incantation";

        /// <summary>已确认没有离线资源的词条，避免反复查盘与刷屏。</summary>
        private static readonly HashSet<string> missingOfflineTextures = new();

        /// <summary>词条的稳定短名（FNV-1a 32 位），同时用作离线纹理文件名。</summary>
        public static string TextureKeyOf(string s)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in s)
                {
                    hash ^= c;
                    hash *= 16777619u;
                }
                return hash.ToString("x8");
            }
        }

        /// <summary>
        /// 返回词条 s 的纹理缓存：只读离线预渲染资源，运行时不再烘焙，两版取值路径一致。
        /// 缺失时返回 <see cref="PlaceholderTex"/> 并报一次错（调用方据此重试）。
        /// </summary>
        public static Texture2D GetTexture(string s)
        {
            if (textures.TryGetValue(s, out var t))
                return t;

            if (missingOfflineTextures.Contains(s))
                return PlaceholderTex;

            var offline = ContentFinder<Texture2D>.Get($"{OfflineTextureDir}/{TextureKeyOf(s)}", reportFailure: false);
            if (offline != null)
            {
                offline.wrapMode = TextureWrapMode.Clamp;
                offline.filterMode = FilterMode.Bilinear;
                textures[s] = offline;
                return offline;
            }

            missingOfflineTextures.Add(s);
            Log.Error($"[FamiliaMyth] 缺少咏唱词纹理（\"{s}\"，键 {TextureKeyOf(s)}）：请在 1.6 下用调试指令「导出咏唱词纹理」导出后重进游戏。");
            return PlaceholderTex;
        }

        /// <summary>
        /// 把全部咏唱词烘焙成 PNG 落到 mod 的 Textures/Incantation/ 下，供 1.7 直接加载。
        /// 依赖 OS 动态字体，仅在 1.6 下可用。
        /// </summary>
        public static void DumpAllIncantationTextures()
        {
            // mod 根目录：本程序集位于 <mod 根>/<版本>/Assemblies/，上溯两级即根目录
            string asmPath = typeof(IncantationTextureCache).Assembly.Location;
            string root = asmPath.NullOrEmpty()
                ? null
                : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(asmPath), "..", ".."));
            if (root.NullOrEmpty())
            {
                Log.Error("[FamiliaMyth] 导出咏唱词纹理失败： mod 根目录获取失败。");
                return;
            }

            string dir = Path.Combine(root, "Textures", OfflineTextureDir.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(dir);

            var lines = new HashSet<string>();
            foreach (SpellDef def in DefDatabase<SpellDef>.AllDefs)
            {
                if (!def.spellKey.NullOrEmpty())
                    lines.Add(def.spellKey);

                if (def.incantationLines == null)
                    continue;
                foreach (string line in def.incantationLines)
                {
                    if (!line.NullOrEmpty())
                        lines.Add(line);
                }
            }

            int ok = 0;
            int fail = 0;
            foreach (string line in lines)
            {
                // 每句用全新字体：动态字体的图集跨句累积与重建会让先前取出的字形数据失效
                if (bakedFont != null)
                {
                    UnityEngine.Object.DestroyImmediate(bakedFont);
                    bakedFont = null;
                }

                Texture2D tex = BuildTexture(line, cache: false);
                if (tex == null)
                {
                    fail++;
                    Log.Warning($"[FamiliaMyth] 咏唱词纹理导出失败：{line}");
                    continue;
                }

                try
                {
                    File.WriteAllBytes(Path.Combine(dir, TextureKeyOf(line) + ".png"), tex.EncodeToPNG());
                    ok++;
                }
                catch (Exception e)
                {
                    fail++;
                    Log.Error($"[FamiliaMyth] 咏唱词纹理写盘失败（{line}）：{e}");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }

            Log.Message($"[FamiliaMyth] 咏唱词纹理导出完成：成功 {ok}，失败 {fail}，目录 {dir}");
        }


        /// <summary>游戏字体后端可用的最大字体索引（1.7）。</summary>
        private const int MaxFontIndex = 3;

        /// <summary>探测游戏字体时附加的探针字符，保证能筛出含中文字形的字体索引。</summary>
        private const string FontProbeExtra = "魔法技能";

        /// <summary>单个字形的排版结果：包围盒、前进量与四角 UV。</summary>
        private struct GlyphQuad
        {
            public float X0;
            public float X1;
            public float Y0;
            public float Y1;
            public float Advance;
            public Vector2 Bl;
            public Vector2 Br;
            public Vector2 Tr;
            public Vector2 Tl;
        }

        /// <summary>
        /// 取出字形的包围盒、前进量与四角 UV。
        /// 1.6 填 minX/maxX/minY/maxY + advance + 四角 UV；1.7 只填 vert + width + uv 矩形，其余字段为默认 0。
        /// </summary>
        private static bool TryGetGlyphQuad(in CharacterInfo ci, out GlyphQuad quad)
        {
            quad = default;
#if RW17
            var uv = ci.uv;
            if (ci.vert.width <= 0f || ci.vert.height <= 0f || uv.width <= 0f || uv.height <= 0f || ci.width <= 0f)
                return false;

            quad.X0 = ci.vert.xMin;
            quad.X1 = ci.vert.xMax;
            quad.Y0 = ci.vert.yMin;
            quad.Y1 = ci.vert.yMax;
            quad.Advance = ci.width;
            quad.Bl = new Vector2(uv.xMin, uv.yMin);
            quad.Br = new Vector2(uv.xMax, uv.yMin);
            quad.Tr = new Vector2(uv.xMax, uv.yMax);
            quad.Tl = new Vector2(uv.xMin, uv.yMax);
            return true;
#else
            if (ci.maxX <= ci.minX || ci.maxY <= ci.minY || ci.advance <= 0)
                return false;

            quad.X0 = ci.minX;
            quad.X1 = ci.maxX;
            quad.Y0 = ci.minY;
            quad.Y1 = ci.maxY;
            quad.Advance = ci.advance;
            quad.Bl = ci.uvBottomLeft;
            quad.Br = ci.uvBottomRight;
            quad.Tr = ci.uvTopRight;
            quad.Tl = ci.uvTopLeft;
            return true;
#endif
        }

#if RW17
        /// <summary>
        /// 1.7 的字形由游戏字体后端提供，逐个字体索引比较字形覆盖度，取覆盖最多的那个。
        /// </summary>
        private static Font ResolveGameFont(string sample)
        {
            var font = new Font();
            int bestIndex = font.GameFontIndex;
            int bestHit = -1;
            for (int idx = 0; idx <= MaxFontIndex; idx++)
            {
                font.GameFontIndex = idx;
                int hit = 0;
                foreach (char c in sample + FontProbeExtra)
                {
                    if (font.HasCharacter(c))
                        hit++;
                }
                if (hit > bestHit)
                {
                    bestHit = hit;
                    bestIndex = idx;
                }
            }
            font.GameFontIndex = bestIndex;
            return font;
        }
#endif

        private static Texture2D BuildTexture(string s, bool cache = true)
        {
            // 用 TextMesh + 正交相机把文本光栅化到 RT（GPU 真正渲染字形）。
            // 避开两个不可靠点：IMGUI 会渲染到屏幕而非 RT、动态字库图集(Alpha8)的 CPU 读取不同步。
            Camera bakeCam;
            GameObject bakeGo = null;
            GameObject camGo = null;
            Material mat = null;
            RenderTexture rt = null;
            Mesh bakeMesh = null;
            try
            {
                // 字形像素尺寸：请求与查询必须用同一个值，否则动态字体的图集状态错乱、字形全叠在起点
                const int bakePixelSize = 512;
                // 字形世界高固定 9.6，沿用原 TextMesh 的 fontSize(96) × characterSize(0.1)
                const float worldPerPixel = 9.6f / bakePixelSize;
#if RW17
                // 1.7 的字形来自游戏字体后端，不加载 OS 字体
                bakedFont ??= ResolveGameFont(s);
#else
                // 默认楷体，找不到时回退到微软雅黑，再回退宋体/Arial
                bakedFont ??= Font.CreateDynamicFontFromOSFont(
                    new[] { "楷体", "KaiTi", "微软雅黑", "Microsoft YaHei", "SimSun", "Arial" }, bakePixelSize);
#endif
                if (bakedFont == null)
                {
                    Log.Error($"生成咏唱文字纹理失败（\"{s}\"）：未找到可用字体。");
                    return null;
                }

                // 1) 请求字形并逐字排版，同时记录像素级包围盒
                bakedFont.RequestCharactersInTexture(s, bakePixelSize, FontStyle.Bold);
                var glyphs = new List<GlyphQuad>();
                float penX = 0f;
                float pxMinX = float.MaxValue, pxMaxX = float.MinValue;
                float pxMinY = float.MaxValue, pxMaxY = float.MinValue;
                foreach (char c in s)
                {
                    if (!bakedFont.GetCharacterInfo(c, out var ci, bakePixelSize, FontStyle.Bold))
                        continue;
                    if (!TryGetGlyphQuad(ci, out var quad))
                        continue;

                    quad.X0 += penX;
                    quad.X1 += penX;
                    glyphs.Add(quad);
                    pxMinX = Mathf.Min(pxMinX, quad.X0);
                    pxMaxX = Mathf.Max(pxMaxX, quad.X1);
                    pxMinY = Mathf.Min(pxMinY, quad.Y0);
                    pxMaxY = Mathf.Max(pxMaxY, quad.Y1);
                    penX += quad.Advance;
                }
                if (glyphs.Count == 0)
                {
                    Log.Error($"生成咏唱文字纹理失败（\"{s}\"）：字体未提供任何字形。");
                    return null;
                }

                // 文字真实世界尺寸（不含边距）；以字体行高为下限，避免标点等矮字形压扁纹理
                float textW = Mathf.Max((pxMaxX - pxMinX) * worldPerPixel, 0.1f);
                float textH = Mathf.Max((pxMaxY - pxMinY) * worldPerPixel, bakedFont.lineHeight * worldPerPixel, 0.1f);
                float centerX = (pxMinX + pxMaxX) * 0.5f;
                float centerY = (pxMinY + pxMaxY) * 0.5f;

                // 逐字形构建网格：每字形 4 顶点 + 2 三角形，顶点绕序令正面朝向位于 -Z 的相机
                var verts = new List<Vector3>(glyphs.Count * 4);
                var uvs = new List<Vector2>(glyphs.Count * 4);
                var tris = new List<int>(glyphs.Count * 6);
                foreach (var quad in glyphs)
                {
                    int v = verts.Count;
                    float x0 = (quad.X0 - centerX) * worldPerPixel;
                    float x1 = (quad.X1 - centerX) * worldPerPixel;
                    float y0 = (quad.Y0 - centerY) * worldPerPixel;
                    float y1 = (quad.Y1 - centerY) * worldPerPixel;
                    verts.Add(new Vector3(x0, y0, 0f)); uvs.Add(quad.Bl);
                    verts.Add(new Vector3(x1, y0, 0f)); uvs.Add(quad.Br);
                    verts.Add(new Vector3(x1, y1, 0f)); uvs.Add(quad.Tr);
                    verts.Add(new Vector3(x0, y1, 0f)); uvs.Add(quad.Tl);
                    tris.Add(v); tris.Add(v + 2); tris.Add(v + 1);
                    tris.Add(v); tris.Add(v + 3); tris.Add(v + 2);
                }
                if (tris.Count == 0)
                {
                    Log.Error($"生成咏唱文字纹理失败（\"{s}\"）：字形缺少 UV 数据。");
                    return null;
                }

                bakeMesh = new Mesh { name = "IncantBake_" + s };
                bakeMesh.SetVertices(verts);
                bakeMesh.SetUVs(0, uvs, 0, uvs.Count);
                bakeMesh.SetTriangles(tris, 0);
                bakeMesh.RecalculateBounds();

                bakeGo = new GameObject("IncantBake", typeof(MeshFilter), typeof(MeshRenderer))
                {
                    transform = { position = Vector3.zero }
                };
                bakeGo.GetComponent<MeshFilter>().sharedMesh = bakeMesh;
                mat = new Material(bakedFont.material);   // 临时材质，finally 中释放
                bakeGo.GetComponent<MeshRenderer>().sharedMaterial = mat;

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

                if (cache)
                    textures[s] = tex;

                if (DumpBakedTexture)
                {
                    var png = tex.EncodeToPNG();
                    File.WriteAllBytes(Path.Combine(GenFilePaths.SaveDataFolderPath, $"incant_{s}.png"), png);
                }
                return tex;
            }
            finally
            {
                // 完整释放所有临时资源，防止泄漏；并恢复活动 RT，防止异常残留。
                // 必须用 DestroyImmediate：Destroy 延迟到帧末，同帧连续烘焙时上一句的网格仍在相机视野里叠加
                RenderTexture.active = null;
                if (rt != null) { rt.Release(); UnityEngine.Object.DestroyImmediate(rt); }
                if (camGo != null) UnityEngine.Object.DestroyImmediate(camGo);
                if (mat != null) UnityEngine.Object.DestroyImmediate(mat);
                if (bakeMesh != null) UnityEngine.Object.DestroyImmediate(bakeMesh);
                if (bakeGo != null) UnityEngine.Object.DestroyImmediate(bakeGo);
            }
        }
    }
}
