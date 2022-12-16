// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MeshColorShaderVisualisation"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_bLight("Use Lighting", Integer) = 1
	}

	Subshader
	{
		Pass
		{
			Tags 
			{
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM

			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram

			#include "UnityStandardBRDF.cginc"

			sampler2D _MainTex;
			int _bLight;

			struct VertexData
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};
		
			struct Interpolators
			{
				float4 position : SV_POSITION;
				float3 normal : TEXCOORD1;
				float2 uv : TEXCOORD0;

			};

			Interpolators VertexProgram(VertexData vertexdata)
			{
				Interpolators i;
				i.normal = mul(unity_ObjectToWorld, float4(vertexdata.normal, 0));
				i.normal = normalize(i.normal);
				i.position = UnityObjectToClipPos(vertexdata.vertex);
				i.uv = vertexdata.uv;

				return i;
			}

			float4 FragmentProgram(Interpolators i) : SV_TARGET
			{
				float3 lightDir = _WorldSpaceLightPos0.xyz;
				float3 lightColor = _LightColor0.rgb;
				float3 ambiant = float3(0.2, 0.2, 0.2);

				float3 color = tex2D(_MainTex, i.uv);

				float3 diffuse = color * lightColor * DotClamped(lightDir, i.normal);

				if (_bLight == 1)
					return float4(ambiant + diffuse, 1);
				return float4(color, 1);
				
			}

			ENDCG
		}
	}
}
