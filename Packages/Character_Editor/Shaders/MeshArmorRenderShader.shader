Shader "CharacterEditor/MeshArmorRenderShader"
{
	Properties
	{
		_HandLeftTex("Texture HandLeft", 2D) = "white" {}
		_HandRightTex("Texture HandRight", 2D) = "white" {}
		_TorsoTex("Texture Torso", 2D) = "white" {}
		_TorsoAddTex("Texture TorsoAdd", 2D) = "white" {}
		_ShoulderLeftTex("Texture ShoulderLeft", 2D) = "white" {}
		_ShoulderRightTex("Texture ShoulderRight", 2D) = "white" {}
		_ArmLeftTex("Texture ArmLeft", 2D) = "white" {}
		_ArmRightTex("Texture ArmRight", 2D) = "white" {}
		_HelmTex("Texture Helm", 2D) = "white" {}
		_BeltTex("Texture Belt", 2D) = "white" {}
		_BeltAddTex("Texture BeltAdd", 2D) = "white" {}
		_LegLeftTex("Texture LegLeft", 2D) = "white" {}
		_LegRightTex("Texture LegRight", 2D) = "white" {}
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
				float2 uv_9 : TEXCOORD9;
				float2 uv_10 : TEXCOORD10;
				float2 uv_11 : TEXCOORD11;
				float2 uv_12 : TEXCOORD12;
				float4 vertex : SV_POSITION;
			};

			sampler2D _HandLeftTex;
			sampler2D _HandRightTex;
			sampler2D _TorsoTex;
			sampler2D _TorsoAddTex;
			sampler2D _ShoulderLeftTex;
			sampler2D _ShoulderRightTex;
			sampler2D _ArmLeftTex;
			sampler2D _ArmRightTex;
			sampler2D _HelmTex;
			sampler2D _BeltTex;
			sampler2D _BeltAddTex;
			sampler2D _LegLeftTex;
			sampler2D _LegRightTex;

			float4 _HandLeftTex_ST;
			float4 _HandRightTex_ST;
			float4 _TorsoTex_ST;
			float4 _TorsoAddTex_ST;
			float4 _ShoulderLeftTex_ST;
			float4 _ShoulderRightTex_ST;
			float4 _ArmLeftTex_ST;
			float4 _ArmRightTex_ST;
			float4 _HelmTex_ST;
			float4 _BeltTex_ST;
			float4 _BeltAddTex_ST;
			float4 _LegLeftTex_ST;
			float4 _LegRightTex_ST;
			
			v2f vert(appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv_0 = TRANSFORM_TEX(v.uv, _HandLeftTex);

				o.uv_1 = v.uv.xy * _HandRightTex_ST.xy - _HandRightTex_ST.zw;
				o.uv_2 = v.uv.xy * _TorsoTex_ST.xy - _TorsoTex_ST.zw;
				o.uv_3 = v.uv.xy * _TorsoAddTex_ST.xy - _TorsoAddTex_ST.zw;
				o.uv_4 = v.uv.xy * _ShoulderLeftTex_ST.xy - _ShoulderLeftTex_ST.zw;
				o.uv_5 = v.uv.xy * _ShoulderRightTex_ST.xy - _ShoulderRightTex_ST.zw;
				o.uv_6 = v.uv.xy * _ArmLeftTex_ST.xy - _ArmLeftTex_ST.zw;
				o.uv_7 = v.uv.xy * _ArmRightTex_ST.xy - _ArmRightTex_ST.zw;
				o.uv_8 = v.uv.xy * _HelmTex_ST.xy - _HelmTex_ST.zw;
				o.uv_9 = v.uv.xy * _BeltTex_ST.xy - _BeltTex_ST.zw;
				o.uv_10 = v.uv.xy * _BeltAddTex_ST.xy - _BeltAddTex_ST.zw;
				o.uv_11 = v.uv.xy * _LegLeftTex_ST.xy - _LegLeftTex_ST.zw;
				o.uv_12 = v.uv.xy * _LegRightTex_ST.xy - _LegRightTex_ST.zw;
				return o;
			}
	
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 handLeftT = tex2D(_HandLeftTex, i.uv_0);
				fixed4 handRightT = tex2D(_HandRightTex, i.uv_1);
				fixed4 torsoT = tex2D(_TorsoTex, i.uv_2);
				fixed4 torsoAddT = tex2D(_TorsoAddTex, i.uv_3);
				fixed4 shoulderLT = tex2D(_ShoulderLeftTex, i.uv_4);
				fixed4 shoulderRT = tex2D(_ShoulderRightTex, i.uv_5);
				fixed4 armLT = tex2D(_ArmLeftTex, i.uv_6);
				fixed4 armRT = tex2D(_ArmRightTex, i.uv_7);
				fixed4 helmT = tex2D(_HelmTex, i.uv_8);
				fixed4 beltT = tex2D(_BeltTex, i.uv_9);
				fixed4 beltAddT = tex2D(_BeltAddTex, i.uv_10);
				fixed4 legLT = tex2D(_LegLeftTex, i.uv_11);
				fixed4 legRT = tex2D(_LegRightTex, i.uv_12);

				fixed4 col = handLeftT * step(0, i.uv_0.x) * step(-1, -i.uv_0.x) * step(0, i.uv_0.y) * step(-1, -i.uv_0.y);
				col += handRightT * step(0, i.uv_1.x) * step(-1, -i.uv_1.x) * step(0, i.uv_1.y) * step(-1, -i.uv_1.y);
				col += torsoT * step(0, i.uv_2.x) * step(-1, -i.uv_2.x) * step(0, i.uv_2.y) * step(-1, -i.uv_2.y);
				col += torsoAddT * step(0, i.uv_3.x) * step(-1, -i.uv_3.x) * step(0, i.uv_3.y) * step(-1, -i.uv_3.y);
				col += shoulderLT * step(0, i.uv_4.x) * step(-1, -i.uv_4.x) * step(0, i.uv_4.y) * step(-1, -i.uv_4.y);
				col += shoulderRT * step(0, i.uv_5.x) * step(-1, -i.uv_5.x) * step(0, i.uv_5.y) * step(-1, -i.uv_5.y);
				col += armLT * step(0, i.uv_6.x) * step(-1, -i.uv_6.x) * step(0, i.uv_6.y) * step(-1, -i.uv_6.y);
				col += armRT * step(0, i.uv_7.x) * step(-1, -i.uv_7.x) * step(0, i.uv_7.y) * step(-1, -i.uv_7.y);
				col += helmT * step(0, i.uv_8.x) * step(-1, -i.uv_8.x) * step(0, i.uv_8.y) * step(-1, -i.uv_8.y);
				col += beltT * step(0, i.uv_9.x) * step(-1, -i.uv_9.x) * step(0, i.uv_9.y) * step(-1, -i.uv_9.y);
				col += beltAddT * step(0, i.uv_10.x) * step(-1, -i.uv_10.x) * step(0, i.uv_10.y) * step(-1, -i.uv_10.y);
				col += legLT * step(0, i.uv_11.x) * step(-1, -i.uv_11.x) * step(0, i.uv_11.y) * step(-1, -i.uv_11.y);
				col += legRT * step(0, i.uv_12.x) * step(-1, -i.uv_12.x) * step(0, i.uv_12.y) * step(-1, -i.uv_12.y);
	
				return col;
			}
			
			ENDCG
		}
	}
}
