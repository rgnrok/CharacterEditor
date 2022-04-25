Shader "CharacterEditor/ClothRenderShader"
{
	Properties
	{
		_SkinTex ("Texture Skin", 2D) = "white" {}
		_TorsoTex("Texture Torso", 2D) = "white" {}
		_PantsTex("Texture Pants", 2D) = "white" {}
		_GloveTex("Texture Glove", 2D) = "white" {}
		_ShoeTex("Texture Shoe", 2D) = "white" {}
		_RobeTex("Texture Robe", 2D) = "white" {}
		_BeltTex("Texture Belt", 2D) = "white" {}
		_HeadTex("Texture Head", 2D) = "white" {}
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
				float4 vertex : SV_POSITION;
			};

			sampler2D _SkinTex;
			sampler2D _TorsoTex;
			sampler2D _PantsTex;
			sampler2D _GloveTex;
			sampler2D _ShoeTex;
			sampler2D _RobeTex;
			sampler2D _BeltTex;
			sampler2D _HeadTex;

			float4 _SkinTex_ST;
			float4 _TorsoTex_ST;
			float4 _PantsTex_ST;
			float4 _GloveTex_ST;
			float4 _ShoeTex_ST;
			float4 _RobeTex_ST;
			float4 _BeltTex_ST;
			float4 _HeadTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv_0 = TRANSFORM_TEX(v.uv, _SkinTex);

				o.uv_1 = v.uv.xy * _TorsoTex_ST.xy - _TorsoTex_ST.zw;
				o.uv_2 = v.uv.xy * _PantsTex_ST.xy - _PantsTex_ST.zw;
				o.uv_3 = v.uv.xy * _GloveTex_ST.xy - _GloveTex_ST.zw;
				o.uv_4 = v.uv.xy * _ShoeTex_ST.xy - _ShoeTex_ST.zw;
				o.uv_5 = v.uv.xy * _RobeTex_ST.xy - _RobeTex_ST.zw;
				o.uv_6 = v.uv.xy * _BeltTex_ST.xy - _BeltTex_ST.zw;
				o.uv_7 = v.uv.xy * _HeadTex_ST.xy - _HeadTex_ST.zw;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 skinT = tex2D(_SkinTex, i.uv_0);
				fixed4 torsoT = tex2D(_TorsoTex, i.uv_1);
				fixed4 pantsT = tex2D(_PantsTex, i.uv_2);
				fixed4 gloveT = tex2D(_GloveTex, i.uv_3);
				fixed4 shoeT = tex2D(_ShoeTex, i.uv_4);
				fixed4 robeT = tex2D(_RobeTex, i.uv_5);
				fixed4 beltT = tex2D(_BeltTex, i.uv_6);
				fixed4 headT = tex2D(_HeadTex, i.uv_7);

				fixed a = torsoT.a;
				a *= step(0, i.uv_1.x);
				a *= step(-1, -i.uv_1.x);
				a *= step(0, i.uv_1.y);
				a *= step(-1, -i.uv_1.y);
				fixed4 col = lerp(skinT, torsoT, a);

				a = pantsT.a;
				a *= step(0, i.uv_2.x);
				a *= step(-1, -i.uv_2.x);
				a *= step(0, i.uv_2.y);
				a *= step(-1, -i.uv_2.y);
				col = lerp(col, pantsT, a);							

				a = gloveT.a;
				a *= step(0, i.uv_3.x);
				a *= step(-1, -i.uv_3.x);
				a *= step(0, i.uv_3.y);
				a *= step(-1, -i.uv_3.y);
				col = lerp(col, gloveT, a);

				a = shoeT.a;
				a *= step(0, i.uv_4.x);
				a *= step(-1, -i.uv_4.x);
				a *= step(0, i.uv_4.y);
				a *= step(-1, -i.uv_4.y);
				col = lerp(col, shoeT, a);

				a = robeT.a;
				a *= step(0, i.uv_5.x);
				a *= step(-1, -i.uv_5.x);
				a *= step(0, i.uv_5.y);
				a *= step(-1, -i.uv_5.y);
				col = lerp(col, robeT, a);

				a = beltT.a;
				a *= step(0, i.uv_6.x);
				a *= step(-1, -i.uv_6.x);
				a *= step(0, i.uv_6.y);
				a *= step(-1, -i.uv_6.y);
				col = lerp(col, beltT, a);

				a = headT.a;
				a *= step(0, i.uv_7.x);
				a *= step(-1, -i.uv_7.x);
				a *= step(0, i.uv_7.y);
				a *= step(-1, -i.uv_7.y);
				col = lerp(col, headT, a);
	
				return col;
			}
			ENDCG
		}
	}
}
