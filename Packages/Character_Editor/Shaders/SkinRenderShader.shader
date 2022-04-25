Shader "CharacterEditor/SkinRenderShader"
{
	Properties
	{
		_SkinTex ("Texture Skin", 2D) = "white" {}
		_ScarTex("Texture Scar", 2D) = "white" {}
		_BeardTex("Texture Beard", 2D) = "white" {}
		_FaceFeatureTex("Texture FaceFeature", 2D) = "white" {}
		_HairTex("Texture Hair", 2D) = "white" {}
		_EyeTex("Texture Eye", 2D) = "white" {}
		_EyebrowTex("Texture Eyebrow", 2D) = "white" {}
		_TorsoTex("Texture Torso", 2D) = "white" {}
		_PantsTex("Texture Pants", 2D) = "white" {}

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100


		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv_0 : TEXCOORD0;
				float2 uv_1 : TEXCOORD1;
				float2 uv_2 : TEXCOORD2;
				float2 uv_3 : TEXCOORD3;
				float2 uv_4 : TEXCOORD4;
				float2 uv_5 : TEXCOORD5;
				float2 uv_6 : TEXCOORD6;
				float2 uv_7 : TEXCOORD7;
				float2 uv_8 : TEXCOORD8;
				float4 vertex : SV_POSITION;
			};

			sampler2D _SkinTex;
			sampler2D _ScarTex;
			sampler2D _BeardTex;
			sampler2D _FaceFeatureTex;
			sampler2D _HairTex;
			sampler2D _EyeTex;
			sampler2D _EyebrowTex;
			sampler2D _TorsoTex;
			sampler2D _PantsTex;

			float4 _SkinTex_ST;
			float4 _ScarTex_ST;
			float4 _BeardTex_ST;
			float4 _FaceFeatureTex_ST;
			float4 _HairTex_ST;
			float4 _EyeTex_ST;
			float4 _EyebrowTex_ST;
			float4 _TorsoTex_ST;
			float4 _PantsTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv_0 = TRANSFORM_TEX(v.uv, _SkinTex);

				o.uv_1 = v.uv.xy * _ScarTex_ST.xy - _ScarTex_ST.zw;
				o.uv_2 = v.uv.xy * _BeardTex_ST.xy - _BeardTex_ST.zw;
				o.uv_3 = v.uv.xy * _FaceFeatureTex_ST.xy - _FaceFeatureTex_ST.zw;
				o.uv_4 = v.uv.xy * _HairTex_ST.xy - _HairTex_ST.zw;
				o.uv_5 = v.uv.xy * _EyeTex_ST.xy - _EyeTex_ST.zw;
				o.uv_6 = v.uv.xy * _EyebrowTex_ST.xy - _EyebrowTex_ST.zw;
				o.uv_7 = v.uv.xy * _TorsoTex_ST.xy - _TorsoTex_ST.zw;
				o.uv_8 = v.uv.xy * _PantsTex_ST.xy - _PantsTex_ST.zw;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 skinT = tex2D(_SkinTex, i.uv_0);
				fixed4 scarT = tex2D(_ScarTex, i.uv_1);
				fixed4 beardT = tex2D(_BeardTex, i.uv_2);
				fixed4 faceFT = tex2D(_FaceFeatureTex, i.uv_3);
				fixed4 hairT = tex2D(_HairTex, i.uv_4);
				fixed4 eyeT = tex2D(_EyeTex, i.uv_5);
				fixed4 eyebrowT = tex2D(_EyebrowTex, i.uv_6);
				fixed4 torsoT = tex2D(_TorsoTex, i.uv_7);
				fixed4 pantsT = tex2D(_PantsTex, i.uv_8);

				fixed a = scarT.a;
				a *= step(0, i.uv_1.x);
				a *= step(-1, -i.uv_1.x);
				a *= step(0, i.uv_1.y);
				a *= step(-1, -i.uv_1.y);
				fixed4 col = lerp(skinT, scarT, a);

				a = beardT.a;
				a *= step(0, i.uv_2.x);
				a *= step(-1, -i.uv_2.x);
				a *= step(0, i.uv_2.y);
				a *= step(-1, -i.uv_2.y);
				col = lerp(col, beardT, a);

				a = faceFT.a;
				a *= step(0, i.uv_3.x);
				a *= step(-1, -i.uv_3.x);
				a *= step(0, i.uv_3.y);
				a *= step(-1, -i.uv_3.y);
				col = lerp(col, faceFT, a);

				a = hairT.a;
				a *= step(0, i.uv_4.x);
				a *= step(-1, -i.uv_4.x);
				a *= step(0, i.uv_4.y);
				a *= step(-1, -i.uv_4.y);
				col = lerp(col, hairT, a);

				a = eyeT.a;
				a *= step(0, i.uv_5.x);
				a *= step(-1, -i.uv_5.x);
				a *= step(0, i.uv_5.y);
				a *= step(-1, -i.uv_5.y);
				col = lerp(col, eyeT, a);

				a = eyebrowT.a;
				a *= step(0, i.uv_6.x);
				a *= step(-1, -i.uv_6.x);
				a *= step(0, i.uv_6.y);
				a *= step(-1, -i.uv_6.y);
				col = lerp(col, eyebrowT, a);

				a = torsoT.a;
				a *= step(0, i.uv_7.x);
				a *= step(-1, -i.uv_7.x);
				a *= step(0, i.uv_7.y);
				a *= step(-1, -i.uv_7.y);
				col = lerp(col, torsoT, a);

				a = pantsT.a;
				a *= step(0, i.uv_8.x);
				a *= step(-1, -i.uv_8.x);
				a *= step(0, i.uv_8.y);
				a *= step(-1, -i.uv_8.y);
				col = lerp(col, pantsT, a);
	
				return col;
			}
			ENDCG
		}
	}
}
