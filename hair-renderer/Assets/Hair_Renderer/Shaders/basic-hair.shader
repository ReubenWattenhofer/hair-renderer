﻿Shader "Custom/basic"
{
	Properties
	{
		_MainTex("Albedo Texture", 2D) = "white" {}
		_AlphaTex("Alpha Texture", 2D) = "white" {}
		_AmbientOcclusion("Ambient Occlusion", 2D) = "white" {}
		_Brightness("Brightness", 2D) = "white" {}

		_SpecularShift("Specular Shift", 2D) = "gray" {}

		_CutoutThresh("Alpha Cutoff", Range(0.0,1.0)) = 0.5
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_Highlight1("Primary Highlight", Color) = (1,1,1,1)
		_Highlight2("Secondary Highlight", Color) = (1,1,1,1)
		_SecondarySparkle("Secondary Highlight Sparkle", 2D) = "white" {}

		// These values are from the Blacksmith hair shader (Unity Asset Store)
		_PrimaryShift("Primary Shift", Range(-5.0, 5.0)) = 0.275
		_SecondaryShift("Secondary Shift", Range(-5.0, 5.0)) = -0.040
		_SpecExp1("Specularity Exponent 1", Float) = 64
		_SpecExp2("Specularity Exponent 2", Float) = 48

	}

		SubShader
		{
			Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 100

			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				//#include "Lighting.cginc"
				#include "HairCommon.cginc"

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				sampler2D _AmbientOcclusion;
				sampler2D _Brightness;

				sampler2D _SpecularShift;
				// For offset and scaling; just here as an example.
				float4 _MainTex_ST;
				float4 _TintColor;
				float4 _Highlight1;
				float4 _Highlight2;
				float _PrimaryShift;
				float _SecondaryShift;
				sampler2D _SecondarySparkle;
				float _CutoutThresh;

				float _SpecExp1;
				float _SpecExp2;

				v2f vert(vertexInput v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.pos);
					o.posModelSpace = v.pos;
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					// @TODO: Why do these methods (UnityObjectToViewPos vs mul(UNITY_MATRIX_MV, vector) )
					// produce different results?

					//https://forum.unity.com/threads/transform-direction-from-world-space-to-view-space.413620/
					//o.tangentWorldSpace = normalize(mul((float3x3)UNITY_MATRIX_MV, v.tangent));// normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0))); //normalize(UnityObjectToViewPos(v.tangent));
					o.tangentWorldSpace = normalize(mul( float4(v.tangent.xyz, 0), UNITY_MATRIX_IT_MV));// normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0))); //normalize(UnityObjectToViewPos(v.tangent));					
					//o.tangentWorldSpace = normalize(mul(UNITY_MATRIX_IT_MV, float4(v.tangent.xyz, 0)));// normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0))); //normalize(UnityObjectToViewPos(v.tangent));					
					//o.normalWorldSpace = normalize(mul((float3x3)UNITY_MATRIX_MV, v.normal));// normalize(mul(unity_ObjectToWorld, float4(v.normal, 0))); //normalize(UnityObjectToViewPos(v.normal));
					o.normalWorldSpace = normalize(mul(float4(v.normal.xyz, 0), UNITY_MATRIX_IT_MV));// normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0))); //normalize(UnityObjectToViewPos(v.tangent));					
					//o.normalWorldSpace = normalize(mul(UNITY_MATRIX_IT_MV, float4(v.normal.xyz, 0)));// normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0))); //normalize(UnityObjectToViewPos(v.tangent));					
					// We want to ignore the w otherwise normalization will fail horribly
					//o.tangentWorldSpace = normalize(mul(UNITY_MATRIX_MV, float4(v.tangent.xyz, 0) ) );// UnityObjectToViewPos(v.tangent));
					//o.normalWorldSpace = normalize(mul(UNITY_MATRIX_MV, float4(v.normal, 0) ) );//normalize(UnityObjectToViewPos(v.normal));
					o.biTangentWorldSpace = normalize(cross(o.normalWorldSpace, o.tangentWorldSpace)) * v.tangent.w;

					return o;
				}


				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col;
				
					// Get light direction in world space.
					// @TODO: add support for point lights (needs different approach)
					float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
					lightDirection = normalize(mul((float3x3)UNITY_MATRIX_V, lightDirection));
					// Get view direction in world space.
					float3 viewDirection = normalize(WorldSpaceViewDir(i.posModelSpace) );
					viewDirection = normalize(mul((float3x3)UNITY_MATRIX_V, viewDirection));

					/*normalize(_WorldSpaceCameraPos
						- mul(unity_ObjectToWorld, i.posModelSpace).xyz);
					viewDirection = normalize(mul(UNITY_MATRIX_MV, viewDirection));*/


					//float specExp1 = 64;
					//float specExp2 = 48;

					col.rgb = HairLighting(i.biTangentWorldSpace, i.normalWorldSpace, lightDirection, viewDirection,
						i.uv, tex2D(_AmbientOcclusion, i.uv).r,
						tex2D(_SpecularShift, i.uv).r - 0.5, _PrimaryShift, _SecondaryShift, tex2D(_MainTex, i.uv).rgb, _TintColor,
						_Highlight1, _SpecExp1, _Highlight2, _SpecExp2, tex2D(_SecondarySparkle, i.uv).r, _LightColor0
						);

					col.a = tex2D(_AlphaTex, i.uv).a;

					// Alpha cutoff.
					if (col.a >= _CutoutThresh) {
						col.a = 1;// _Transparency;
					}
					// This could be put in an else statement, but wouldn't improve performance.
					clip(col.a - _CutoutThresh);

					return col;
				}

			ENDCG
			}
		}
}