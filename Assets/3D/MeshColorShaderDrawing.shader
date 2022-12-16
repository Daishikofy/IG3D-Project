// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MeshColorShaderDrawing"
{
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

			struct VertexData
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 color : COLOR;
			};
		
			struct Interpolators
			{
				float4 position : SV_POSITION;
				float3 normal : TEXCOORD1;
				float4 color : TEXCOORD2;

			};

			Interpolators VertexProgram(VertexData vertexdata)
			{
				Interpolators i;
				i.color = vertexdata.color;
				i.normal = mul(unity_ObjectToWorld, float4(vertexdata.normal, 0));
				i.normal = normalize(i.normal);
				i.position = UnityObjectToClipPos(vertexdata.vertex);

				return i;
			}

			float4 FragmentProgram(Interpolators i) : SV_TARGET
			{
				float3 lightDir = _WorldSpaceLightPos0.xyz;
				float3 lightColor = _LightColor0.rgb;
				float3 ambiant = float3(0.2, 0.2, 0.2);

				//float3 color = i.color;
				return i.color;
				//float3 diffuse = color * lightColor * DotClamped(lightDir, i.normal);
				//return float4(ambiant + diffuse, 1);
			}

			ENDCG
		}
	}
}
