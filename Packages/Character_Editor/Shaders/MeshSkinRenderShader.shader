Shader "CharacterEditor/MeshSkinRenderShader"
{
	Properties
	{
		_HairTex ("Texture Hair", 2D) = "white" {}
		_BeardTex("Texture Beard", 2D) = "white" {}
		_FaceFeatureTex("Texture FaceFeature", 2D) = "white" {}

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
				float4 vertex : SV_POSITION;
			};

			sampler2D _HairTex;
			sampler2D _BeardTex;
			sampler2D _FaceFeatureTex;

			float4 _HairTex_ST;
			float4 _BeardTex_ST;
			float4 _FaceFeatureTex_ST;

			v2f vert (appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv_0 = TRANSFORM_TEX(v.uv, _HairTex);
				o.uv_1 = TRANSFORM_TEX(v.uv, _BeardTex);
				o.uv_2 = TRANSFORM_TEX(v.uv, _FaceFeatureTex);

				return o;
			}
	
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 hairT = tex2D(_HairTex, i.uv_0);
				fixed4 beardT = tex2D(_BeardTex, i.uv_1);
				fixed4 faceFT = tex2D(_FaceFeatureTex, i.uv_2);

				fixed4 col = hairT * step(0, i.uv_0.x) * step(-1, -i.uv_0.x) * step(0, i.uv_0.y) * step(-1, -i.uv_0.y);
				col += beardT * step(0, i.uv_1.x) * step(-1, -i.uv_1.x) * step(0, i.uv_1.y) * step(-1, -i.uv_1.y);
				col += faceFT * step(0, i.uv_2.x) * step(-1, -i.uv_2.x) * step(0, i.uv_2.y) * step(-1, -i.uv_2.y);

				return col;
			}
			
			ENDCG
		}
	}
}
