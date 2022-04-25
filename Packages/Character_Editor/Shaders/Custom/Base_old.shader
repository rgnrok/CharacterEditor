// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BaseOld"
{
	Properties
	{
		_Color("Main Color", Color) = (.5,.5,.5,1)
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(.002, 0.06)) = .005
		_MainTex("Base (RGB)", 2D) = "white" { }
		_Stencil("Stencil", int) = 4

	}



	

	SubShader
	{
		Stencil
	{
		Ref [_Stencil]
		Comp always
		Pass replace
	}

		//Tags {"Queue" = "Geometry+100" }
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG

		// note that a vertex shader is specified here but its using the one above
			Pass
		{
			// Won't draw where it sees ref value 4
			Cull OFF
			ZWrite OFF
			Stencil
		{
			Ref [_Stencil]
			Comp notequal
		}

			CGPROGRAM
#pragma vertex vert
#pragma fragment frag

			// Properties
		uniform float _OutlineSize;


		uniform float _Outline;
		uniform float4 _OutlineColor;
		sampler2D _MainTex;

		struct vertexInput
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float3 texCoord : TEXCOORD0;
			float4 color : TEXCOORD1;
		};

		struct vertexOutput
		{
			float4 pos : SV_POSITION;
			float4 color : TEXCOORD0;
		};

		vertexOutput vert(vertexInput input)
		{
			vertexOutput output;

			float4 newPos = input.vertex;

			// normal extrusion technique
			float3 normal = normalize(input.normal);
			newPos += float4(normal, 0.0) * _Outline;

			// convert to world space
			output.pos = UnityObjectToClipPos(newPos);
			output.color = _OutlineColor;
			return output;
		}

		float4 frag(vertexOutput input) : COLOR
		{
			//float4 color = input.color * _OutlineColor;
			return input.color;
		}

			ENDCG
		}
	}


	Fallback "Diffuse"
}