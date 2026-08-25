//   君王之剑着色器（三 Pass）：
//   Pass 0「本体」：普通混合 SrcAlpha OneMinusSrcAlpha，直出贴图本色 + alpha 裁剪剑形，
//     剑身本体不透背景（可遮挡），_Highlight 整体提亮（类原版 TransparentPostLight）。
//   Pass 1「辉光」：additive 叠加，边缘辉光 + 羽化，软边光晕（叠加在剑身与背景之上）。
//   Pass 2「纯色剪影」：_Flat=1 时启用，纯色填充 + 贴图 alpha 裁剪（用于残影）。
// 注意：本 mod 采用自定义着色器，需将本 .shader 打包进 asset bundle，
//   bundle 内路径约定为 Assets/Data/<ModFolderName>/Materials/KingsSword.shader，
//   对应 ShaderTypeDef.shaderPath = "KingsSword"。
Shader "Familia_Myth/KingsSword"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_GlowColor ("Glow Color", Color) = (0.55, 0.8, 1, 1)
		_GlowStrength ("Glow Strength", Range(0, 4)) = 0.5
		_GlowWidth ("Glow Feather Width", Range(0, 16)) = 2.5   // 羽化带总宽度（纹素）
		_Highlight ("Highlight", Range(0, 4)) = 0              // 整体亮度提升接口：0=无高亮，>0 整体提亮
		_Color ("Flat Color", Color) = (1,1,1,1)               // 纯色模式填充色（残影注入 EdgeColor）
		_Flat ("Flat Mode", Range(0, 1)) = 0                   // 0=剑身+辉光，1=纯色剪影(残影)
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
		LOD 100
		Cull Off
		ZWrite Off

		// ---------- Pass 0：本体（普通混合，不透背景） ----------
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Highlight;
			float _Flat;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};
			struct v2f
			{
				float2 uv  : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// 纯色模式：本 Pass 不贡献，交给 Pass 2 绘制
				if (_Flat > 0.5)
					return fixed4(0, 0, 0, 0);

				// 本体与给定贴图一致（类原版 TransparentPostLight）：直出贴图本色 + alpha 裁剪剑形。
				fixed4 tex = tex2D(_MainTex, i.uv);

				// 高亮最先应用（羽化/辉光之前）：整体提亮贴图亮度与 alpha。
				float bright = 1.0f + _Highlight;
				return fixed4(tex.rgb * bright, saturate(tex.a * bright));
			}
			ENDCG
		}

		// ---------- Pass 1：辉光（additive，叠加在剑身与背景之上） ----------
		Pass
		{
			Blend SrcAlpha One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			fixed4 _GlowColor;
			float _GlowStrength;
			float _GlowWidth;
			float _Highlight;
			float _Flat;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};
			struct v2f
			{
				float2 uv  : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			// 半径 off 处 8 邻域（十字+四角）max alpha：该"环"是否命中剑体
			float ringMax(float2 uv, float2 off)
			{
				float a =
					tex2D(_MainTex, uv + float2(0, off.y)).a;
				a = max(a, tex2D(_MainTex, uv - float2(0, off.y)).a);
				a = max(a, tex2D(_MainTex, uv + float2(off.x, 0)).a);
				a = max(a, tex2D(_MainTex, uv - float2(off.x, 0)).a);
				a = max(a, tex2D(_MainTex, uv + off).a);
				a = max(a, tex2D(_MainTex, uv - off).a);
				a = max(a, tex2D(_MainTex, uv + float2(off.x, -off.y)).a);
				a = max(a, tex2D(_MainTex, uv - float2(off.x, -off.y)).a);
				return a;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// 纯色模式：本 Pass 不贡献，交给 Pass 2 绘制
				if (_Flat > 0.5)
					return fixed4(0, 0, 0, 0);

				fixed4 tex = tex2D(_MainTex, i.uv);

				// 高亮最先应用（羽化/辉光之前）：
				float bright = 1.0f + _Highlight;
				tex.rgb *= bright;
				tex.a = saturate(tex.a * bright);

				fixed a0 = tex.a;
				float band = _MainTex_TexelSize.x * _GlowWidth;

				// ---- 羽化：多环同心采样 ----
				// 由内环到外环逐环判定"该环是否命中剑体"；靠内的环计入的光强更大，
				// 靠外的环光强更小，累加后光晕从剑体边缘向外平滑弥散（软边）。
				float ring1 = smoothstep(0.02, 0.40, ringMax(i.uv, float2(1,1) * band));
				float ring2 = smoothstep(0.02, 0.40, ringMax(i.uv, float2(2,2) * band));
				float ring3 = smoothstep(0.02, 0.40, ringMax(i.uv, float2(3,3) * band));
				float ring4 = smoothstep(0.02, 0.40, ringMax(i.uv, float2(4,4) * band));

				// 加权累加：内环权重大，越向外衰减越平缓
				float glow = (ring1*4.0 + ring2*3.0 + ring3*2.0 + ring4*1.0) / 10.0;
				glow = smoothstep(0.0, 1.0, glow);   // 再次软化过渡（羽化核心）

				// 剑身本体基础微光（避免本体发暗；加在辉光层上随 add 叠亮）
				float core = smoothstep(0.30, 0.85, a0);

				float3 lum = _GlowColor.rgb * (glow + core * 0.30f) * _GlowStrength;

				float alpha = clamp(glow * 0.8 + core * 0.10, 0.0, 1.0);

				return fixed4(lum, alpha);
			}
			ENDCG
		}

		// ---------- Pass 2：纯色剪影（残影，普通混合） ----------
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _Color;
			float _Flat;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};
			struct v2f
			{
				float2 uv  : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// 非纯色模式：本 Pass 不贡献，由 Pass 0/1 绘制
				if (_Flat <= 0.5)
					return fixed4(0, 0, 0, 0);

				// 纯色填充：颜色用 _Color（注入 EdgeColor），
				// alpha 用贴图 alpha 裁剪出与大剑本体一致的剑形轮廓。
				fixed a0 = tex2D(_MainTex, i.uv).a;
				return fixed4(_Color.rgb, _Color.a * a0);
			}
			ENDCG
		}
	}
	Fallback "Unlit/Transparent"
}