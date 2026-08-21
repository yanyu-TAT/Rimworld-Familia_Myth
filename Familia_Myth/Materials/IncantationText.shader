//   咏唱词动画着色器（三阶段，刚体瓦片平移）
//   聚合阶段(0~0.2)：每块 8px 瓦片携带自身内容从整个材质(含透明边距)散落位先后平移回原位。
//   悬浮阶段(0.2~0.75)：位移为 0，完整连续显示。
//   爆散阶段(0.75~1.0)：瓦片携带自身内容从原位先后平移散开至整个材质(含透明边距)。
// 注意：本 mod 采用自定义着色器，需将本 .shader 打包进 asset bundle，
//   bundle 内路径约定为 Assets/Data/<ModFolderName>/Materials/IncantationText.shader，
//   对应 ShaderTypeDef.shaderPath = "IncantationText"。
Shader "Familia_Myth/IncantationText"
{
	Properties
	{
		_MainTex ("Text Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		// 0~1 总进度；C# 每帧从 Mote_Incantation.DrawAt 注入
		_Progress ("Progress", Float) = 0
		// 粒子散落范围（相对 quad 的 UV 比例，默认约半个字形）
		_ScatterRadius ("Scatter Radius", Range(0, 1)) = 0.35
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
		LOD 100
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv   : TEXCOORD0;
				float4 pos  : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			// _MainTex_TexelSize 需显式声明：xy = 单像素的 uv 大小，zw = 纹理像素尺寸
			float4 _MainTex_TexelSize;
			fixed4 _Color;
			float _Progress;
			float _ScatterRadius;
			float4 _TexRect;    // 文字区在 quad 中的 UV 矩形 (x,y,w,h)
			float _QuadAspect;  // quad 宽高比（含透明边距）

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			// 哈希：给每个粒子格生成 0..1 伪随机种子
			float2 hash2(float2 p)
			{
				p = float2(dot(p, float2(127.1, 311.7)),
				dot(p, float2(269.5, 183.3)));
				return frac(sin(p) * 43758.5453);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float p = saturate(_Progress);

				// ---- 阶段判定 ----
				float aggPhase   = 1.0f - step(0.2f, p);                  // 聚合期 1
				float burstPhase = step(0.75f, p);                       // 爆散期 1
				float isIdle     = step(0.2f, p) * (1.0f - burstPhase);  // 悬浮期 1
				// 聚合回位度：单调递增，聚合结束后保持 1，避免悬浮期回退造成突变
				float agg   = saturate(p / 0.2f);
				float burst = burstPhase * saturate((p - 0.75f) / 0.25f);// 爆散完成度 0..1

				// ---- 刚体瓦片网格：每块 ~8 像素，携带自身内容整体平移 ----
				float2 cellSize = 8.0f * _MainTex_TexelSize.xy;
				float2 cellIdx  = floor(i.uv / cellSize);
				float2 seed     = hash2(cellIdx);                        // 0..1

				// ---- 全程同步：所有瓦片随阶段进度同时启动/停止，无先后居中 ----
				// 缓动：smoothstep（3t²−2t³），两端慢、中间快
				float arrive = smoothstep(0.0f, 1.0f, agg);      // 0..1 聚合回位度
				float depart = smoothstep(0.0f, 1.0f, burst);    // 0..1 爆散散开度

				// 瓦片在 quad 内的原位（中心 UV）
				float2 orig = (cellIdx + 0.5f) * cellSize;
				// 散落：以字形为锚的相对偏移，方向随机、幅度由 _ScatterRadius 控制。
				// 位移在 quad 全域做，但最终会映射回紧凑纹理的文字区采样。
				// x 沿 quad aspect 放大：让左右与上下世界位移相当，避免横长 quad 使左右过散。
				float aspect = max(_QuadAspect, 1e-5f);
				float2 dirA  = (hash2(cellIdx * 3.1f + 7.7f) - 0.5f) * 2.0f * _ScatterRadius;
				float2 dirB  = (hash2(cellIdx * 5.3f + 11.1f) - 0.5f) * 2.0f * _ScatterRadius;
				float2 scatterA = orig + float2(dirA.x / aspect, dirA.y);  // 聚合起点
				float2 scatterB = orig + float2(dirB.x / aspect, dirB.y);  // 爆散终点

				// 位移 = 散落目标 − 原位；聚合目标→原位、爆散原位→目标
				float2 displ = lerp((scatterA - orig) * (1.0f - arrive),
				                    (scatterB - orig) * depart, burstPhase);
				// 当前像素的 quad UV + 位移（刚体平移的展示位置）
				float2 uvQ = i.uv + displ;

				// ---- 紧凑纹理重映射：quad 全域 → 文字区纹理采样 UV ----
				// 文字只存了紧贴区域，改存时把文字区铺到 quad 中央(_TexRect)，这里逆映射回纹理。
				float2 uv2 = (uvQ - _TexRect.xy) / _TexRect.zw;
				float regionMask = step(0.0f, uv2.x) * step(uv2.x, 1.0f)
				                 * step(0.0f, uv2.y) * step(uv2.y, 1.0f);

				// ---- 颜色：黑色填充 + 主题色描边 ----
				fixed4 texCol = tex2D(_MainTex, uv2);
				fixed4 col;
				col.rgb = float3(0, 0, 0);             // 文字本体：黑色填充
				col.a   = texCol.a;

				// ---- 描边：外描边用主题色(_Color)，内侧细黑边（粒子和成形文字均生效）----
				float2 texel2   = _MainTex_TexelSize.xy;
				float  outlineW = 14.0f;                                  // 外描边半径（纹素）
				float aC  = texCol.a;
				float aL  = tex2D(_MainTex, uv2 + float2(-texel2.x * outlineW, 0)).a;
				float aRr = tex2D(_MainTex, uv2 + float2( texel2.x * outlineW, 0)).a;
				float aDn = tex2D(_MainTex, uv2 + float2(0, -texel2.y * outlineW)).a;
				float aUp = tex2D(_MainTex, uv2 + float2(0,  texel2.y * outlineW)).a;
				float neighborMax = max(max(aL, aRr), max(aDn, aUp));
				float outlineMask = smoothstep(0.02f, 0.25f, neighborMax)   // 邻域有内容
				                  * (1.0f - smoothstep(0.02f, 0.25f, aC));  // 自身无内容（边缘）
				col.rgb = lerp(col.rgb, _Color.rgb, outlineMask);          // 主题色外描边
				col.a   = max(col.a, neighborMax * outlineMask);           // 保留描边可见
				// ---- 白色轮廓描边：真正贴文字轮廓外侧（无偏移、无重影）----
				// 采样 8 邻域最大 alpha：自身在文字外部(aC 低)且邻域有文字内容(aNear 高)
				// → 该像素位于文字轮廓外侧边缘，描白边的带宽 = 采样半径 cR（纹素）。
				float cR  = 3.0f;                            // 轮廓采样半径，控制白边粗细
				float2 cS = cR * texel2;
				float aCN  = tex2D(_MainTex, uv2 + float2( 0.0f,  cS.y)).a;
				float aCS  = tex2D(_MainTex, uv2 + float2( 0.0f, -cS.y)).a;
				float aCE  = tex2D(_MainTex, uv2 + float2( cS.x,  0.0f)).a;
				float aCW  = tex2D(_MainTex, uv2 + float2(-cS.x,  0.0f)).a;
				float aCNE = tex2D(_MainTex, uv2 + float2( cS.x,  cS.y)).a;
				float aCNW = tex2D(_MainTex, uv2 + float2(-cS.x,  cS.y)).a;
				float aCSE = tex2D(_MainTex, uv2 + float2( cS.x, -cS.y)).a;
				float aCSW = tex2D(_MainTex, uv2 + float2(-cS.x, -cS.y)).a;
				float aNear = max(max(max(aCN, aCS), max(aCE, aCW)),
				                  max(max(aCNE, aCNW), max(aCSE, aCSW)));
				float contour = (1.0f - smoothstep(0.45f, 0.75f, aC)) // 在文字外部
				              * smoothstep(0.25f, 0.45f, aNear);      // 邻域有文字内容
				col.rgb = lerp(col.rgb, float3(1, 1, 1), contour);   // 白色轮廓
				col.a   = max(col.a, contour * aNear);

				// 全局 alpha（C# 传入的 _Color.a：淡入淡出）统一应用到最终输出，
				// 使描边等所有像素与文字本体一起淡入淡出
				col.a *= _Color.a;
				// 紧凑纹理重映射的文字区外（透明边距）置为透明
				col.a *= regionMask;

				return col;
			}
			ENDCG
		}
	}
	Fallback "Unlit/Transparent"
}