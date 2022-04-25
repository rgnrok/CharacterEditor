// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Outline"
{
	Properties
	{
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(.002, 0.06)) = .005
		_Stencil("Stencil", int) = 4
	}


	SubShader
	{
		Pass
		{
			// Won't draw where it sees ref value 4
			Cull OFF
			ZWrite OFF
			Stencil
			{
				Ref[_Stencil]
				Comp notequal
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// Properties
			uniform float _OutlineSize;
			uniform float _Outline;
			uniform float4 _OutlineColor;

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
}