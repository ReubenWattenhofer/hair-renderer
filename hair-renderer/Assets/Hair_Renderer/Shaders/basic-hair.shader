﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'

Shader "Custom/basic"
{
	Properties
	{
		_MainTex("Albedo Texture", 2D) = "white" {}
		_AlphaTex("Alpha Texture", 2D) = "white" {}
		_AmbientOcclusion("Ambient Occlusion", 2D) = "white" {}
		_Brightness("Brightness", 2D) = "white" {}

		_SpecularShift("Specular Shift", 2D) = "gray" {}

		_Test("Texel Test", 2D) = "white" {}

		_CutoutThresh("Alpha Cutoff", Range(0.0,1.0)) = 0.5
		_AlphaMultiplier("Alpha Multiplier", Range(0.0,1.0)) = 0.8
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_Highlight1("Primary Highlight", Color) = (1,1,1,1)
		_Highlight2("Secondary Highlight", Color) = (1,1,1,1)
		_SecondarySparkle("Secondary Highlight Sparkle", 2D) = "white" {}

		// These values are from the Blacksmith hair shader (Unity Asset Store)
		_PrimaryShift("Primary Shift", Range(-5.0, 5.0)) = 0.275
		_SecondaryShift("Secondary Shift", Range(-5.0, 5.0)) = -0.040
		_SpecExp1("Specularity Exponent 1", Float) = 64
		_SpecExp2("Specularity Exponent 2", Float) = 48

		_Ambient("Ambient Lighting", Range(0, 1)) = 0.7
		_AmbientColor("Ambient Color", Color) = (1,1,1,1)

		_OpacityOn("Activate Deep Opacity Map", Range(0, 1)) = 1
		_OpacityRGB("Display Deep Opacity Map Layers", Range(0, 1)) = 0
		_OpacityGreyscale("Display Deep Opacity Map Values", Range(0, 1)) = 0
	}

		SubShader
		{
			Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
			//Tags {"RenderType" = "Opaque" }
			LOD 100
			//Transparent
			ZWrite On
			//ZWrite Off
			//AlphaTest GEqual [_CutoutThresh]
			//Blend SrcAlpha OneMinusSrcAlpha
			Cull Off


			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				//#include "Lighting.cginc"
				#include "HairCommon.cginc"

				// For ambient lighting
			    //#include "UnityLightingCommon.cginc"

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				sampler2D _AmbientOcclusion;
				sampler2D _Brightness;

				sampler2D _SpecularShift;

				sampler2D _Test;
				// For offset and scaling; just here as an example.
				float4 _MainTex_ST;
				float4 _TintColor;
				float4 _Highlight1;
				float4 _Highlight2;
				float _PrimaryShift;
				float _SecondaryShift;
				sampler2D _SecondarySparkle;
				float _CutoutThresh;
				float _AlphaMultiplier;

				float _SpecExp1;
				float _SpecExp2;

				float _Ambient;
				float4 _AmbientColor;

				// Shader toggles
				float _OpacityOn;
				float _OpacityRGB;
				float _OpacityGreyscale;


				// camera depth texture; built-in
				sampler2D _CameraDepthTexture;
				// generated by MeshSorter script
				sampler2D _DeepOpacityMap;
				sampler2D _HeadDepth;
				// generated by ShadowMapTest
				sampler2D _ShadowMap;
				float _ShadowCascades;
				// generated by TestScreenPosRunner
				float4x4 _DepthView;
				//float4x4 _DepthProjection;
				float4x4 _DepthVP;
				float4 _DepthScreenParams;
				float4 _DepthZBufferParams;
				float4 _DepthCameraPlanes;

				float _Layer1Thickness;
				float _Layer2Thickness;
				float _Layer3Thickness;


				// https://en.wikibooks.org/wiki/Cg_Programming/Unity/Light_Attenuation
				uniform float4x4 unity_WorldToLight; // transformation 
				  // from world to light space (from Autolight.cginc)



				/**
				  Input to vertex shader.
				*/
				struct vertexInput {
					float4 pos : POSITION;
					float2 uv : TEXCOORD0;

					// Normal and tangent are in object space (which makes sense)
					// https://en.wikibooks.org/wiki/Cg_Programming/Unity/Debugging_of_Shaders
					float3 normal : NORMAL;
					float4 tangent : TANGENT;

				};

				/**
				  Output from vertex shader, input to fragment shader.
				*/
				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD1;
					float3 tangentWorldSpace : TEXCOORD2;
					float3 normalWorldSpace : TEXCOORD3;
					float3 biTangentWorldSpace : TEXCOORD4;

					float4 posModelSpace : TEXCOORD5;
					float4 deepOpacity : TEXCOORD6;

					float4 scrPos : TEXCOORD7;
				};


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
					//o.tangentWorldSpace = normalize(mul(float4(v.tangent.xyz, 0), UNITY_MATRIX_IT_MV));// normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0))); //normalize(UnityObjectToViewPos(v.tangent));					
					//https://forum.unity.com/threads/world-space-normal.58810/
					o.tangentWorldSpace = UnityObjectToWorldNormal(v.tangent);
					
					//o.tangentWorldSpace = normalize(mul(UNITY_MATRIX_IT_MV, float4(v.tangent.xyz, 0)));// normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0))); //normalize(UnityObjectToViewPos(v.tangent));					
					//o.normalWorldSpace = normalize(mul((float3x3)UNITY_MATRIX_MV, v.normal));// normalize(mul(unity_ObjectToWorld, float4(v.normal, 0))); //normalize(UnityObjectToViewPos(v.normal));
					//o.normalWorldSpace = normalize(mul(float4(v.normal.xyz, 0), UNITY_MATRIX_IT_MV));// normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0))); //normalize(UnityObjectToViewPos(v.tangent));					

					o.normalWorldSpace = UnityObjectToWorldNormal(v.normal);

					//o.normalWorldSpace = normalize(mul(UNITY_MATRIX_IT_MV, float4(v.normal.xyz, 0)));// normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0))); //normalize(UnityObjectToViewPos(v.tangent));					
					// We want to ignore the w otherwise normalization will fail horribly
					//o.tangentWorldSpace = normalize(mul(UNITY_MATRIX_MV, float4(v.tangent.xyz, 0) ) );// UnityObjectToViewPos(v.tangent));
					//o.normalWorldSpace = normalize(mul(UNITY_MATRIX_MV, float4(v.normal, 0) ) );//normalize(UnityObjectToViewPos(v.normal));
					o.biTangentWorldSpace = normalize(cross(o.normalWorldSpace, o.tangentWorldSpace)) * v.tangent.w;

					// https://docs.unity3d.com/Manual/SL-DepthTextures.html


					// http://williamchyr.com/2013/11/unity-shaders-depth-and-normal-textures/
					//o.scrPos = ComputeScreenPos(o.pos);
					o.scrPos = o.pos;
					//o.scrPos = ComputeScreenPos(UnityObjectToClipPos(v.pos));
					
					return o;
				}


				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col;
				
					// Get light direction in world space.
					// @TODO: add support for point lights (needs different approach)
					float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
					//lightDirection = normalize(mul((float3x3)UNITY_MATRIX_V, lightDirection));
					
					//lightDirection = reflect(lightDirection, i.normalWorldSpace);
					// Get view direction in world space.
					float3 viewDirection = normalize(WorldSpaceViewDir(i.posModelSpace) );
					//viewDirection = normalize(mul((float3x3)UNITY_MATRIX_V, viewDirection));

					/*normalize(_WorldSpaceCameraPos
						- mul(unity_ObjectToWorld, i.posModelSpace).xyz);
					viewDirection = normalize(mul(UNITY_MATRIX_MV, viewDirection));*/


					//float specExp1 = 64;
					//float specExp2 = 48;

					col.rgb = HairLighting(i.biTangentWorldSpace, i.normalWorldSpace, lightDirection, viewDirection,
						i.uv, tex2D(_AmbientOcclusion, i.uv).r,
						tex2D(_SpecularShift, i.uv).r - 0.5, _PrimaryShift, _SecondaryShift, tex2D(_MainTex, i.uv).rgb, _TintColor,
						_Highlight1, _SpecExp1, _Highlight2, _SpecExp2, tex2D(_SecondarySparkle, i.uv).r, _LightColor0,
						_Ambient * _AmbientColor
					);
					
					//float depthValue = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
					//float depthValue = Linear01Depth(tex2Dproj(_DeepOpacityMap, UNITY_PROJ_COORD(i.scrPos)).r);
					//float depthValue = (tex2Dproj(_DeepOpacityMap, UNITY_PROJ_COORD(i.scrPos)).r);

					// http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-16-shadow-mapping/#rendering-the-shadow-map
					//float4 shadowCoord = mul(_DepthProjection, mul(_DepthView, mul(unity_ObjectToWorld, float4(i.posModelSpace.xyz, 1.0)).xyz));
					float4 shadowWorld = mul(unity_ObjectToWorld, float4(i.posModelSpace.xyz, 1.0));
					//float4 shadowCoord = mul(_DepthView, shadowWorld);
					float4 shadowCoord = mul(_DepthVP, shadowWorld);
					//float4 shadowCoord = mul(UNITY_MATRIX_VP, shadowWorld);
					
					float4 shadowLightSpace = mul(_DepthView, shadowWorld);
					//float4 shadowLightSpace = shadowCoord;


					float4 o = shadowCoord * 0.5f;
					//float4 screenParams = float4(256, 256, 1 + 1 / 256, 1 + 1 / 256);
					#if defined(UNITY_HALF_TEXEL_OFFSET)
					//o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w * _ScreenParams.zw;
					o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w * _DepthScreenParams.zw;
					#else
					o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w;
					#endif
 
					o.zw = shadowCoord.zw;
					
					// "Computes texture coordinate for doing a screenspace-mapped texture sample. Input is clip space position."
					//shadowCoord = ComputeScreenPos(shadowCoord);
					shadowCoord = o;
					// https://www.ronja-tutorials.com/2019/01/20/screenspace-texture.html
					shadowCoord.xy /= shadowCoord.w;
					//i.scrPos.xy = floor(i.scrPos.xy * _ScreenParams.xy);
					//i.scrPos.xy = floor(i.scrPos.xy * 0.1) * 0.5;

					//i.scrPos.xy = i.scrPos.xy / _ScreenParams.xy;

					//i.scrPos.xy = floor(i.scrPos.xy * 0.1) * 0.5;
					//i.scrPos.xy /= i.scrPos.z;

					//i.scrPos.x /= i.scrPos.z;
					// from https://forum.unity.com/threads/how-do-i-render-my-own-shadow-map.471293/
					//float lightDepth = 1.0 - tex2Dproj(_ShadowTex, IN.shadowCoords).r;


					// Z buffer to linear 0..1 depth
					//inline float Linear01Depth(float z)
					//{
					//	return 1.0 / (_DepthZBufferParams.x * z + _DepthZBufferParams.y);
					//}
	
					//float lightDepth = tex2D(_ShadowMap, shadowCoord).r;
					float4 lightDepth = tex2D(_DeepOpacityMap, shadowCoord);
					float  headOcclusion = tex2D(_HeadDepth, shadowCoord).r;

					//float lightDepth = tex2Dproj(_DeepOpacityMap, UNITY_PROJ_COORD(shadowCoord)).r;
					//float lightDepth = tex2Dproj(_DeepOpacityMap, UNITY_PROJ_COORD(i.scrPos)).r;

					//float depthValue = (tex2Dproj(_ShadowMap, UNITY_PROJ_COORD(i.scrPos)).r);
					//float depthValue = (lightDepth - _DepthCameraPlanes.x)
					//	/ (_DepthCameraPlanes.y - _DepthCameraPlanes.x);
					
					// convert to worldspace z value
					// from solving for z in Linear01Depth
					// https://github.com/TwoTailsGames/Unity-Built-in-Shaders/blob/master/CGIncludes/UnityCG.cginc
					//lightDepth = ((1 / lightDepth) - _DepthZBufferParams.y) / _DepthZBufferParams.x;
					//lightDepth = _DepthCameraPlanes.x ;
					
					// DECODE_EYEDEPTH
					//lightDepth = 1.0 / (_DepthZBufferParams.z * lightDepth + _DepthZBufferParams.w);;

					//float z = 1.0 / (_DepthZBufferParams.x * -shadowLightSpace.z + _DepthZBufferParams.y);
				/*	float z = (-shadowLightSpace.z - _DepthCameraPlanes.x) 
						/ (_DepthCameraPlanes.y - _DepthCameraPlanes.x);*/

					//float z = Normalize_Depth(-shadowLightSpace.z, _DepthCameraPlanes.x, _DepthCameraPlanes.y);

					float culledDepth = Get_True_Depth(lightDepth.r, _DepthCameraPlanes.x, _DepthCameraPlanes.y);
					headOcclusion = Get_True_Depth(headOcclusion.r, _DepthCameraPlanes.x, _DepthCameraPlanes.y);

					culledDepth = min(culledDepth, headOcclusion);

					//float depthValue = Normalize_Depth(-i.viewPos.z, _DepthCameraPlanes.x, _DepthCameraPlanes.y);			   
					float z = -shadowLightSpace.z;


					float opacity = 0;// = z - lightDepth;// -lightDepth;// -lightDepth;
					float3 opacity_rgb = float3(0,0,0);

					// Is the head in the way?
					//if (headOcclusion < z)
					//{
					//	opacity = 1;
					//	opacity_rgb.b = 1;
					//}
					// layer 1
					if (z - _Layer1Thickness < culledDepth)
					{
						opacity = lerp(0, lightDepth.g, (z - culledDepth) / _Layer1Thickness);
						opacity_rgb.r = lerp(0, lightDepth.g, (z - culledDepth) / _Layer1Thickness);
					}
					// layer 2
					else if (z - _Layer1Thickness - _Layer2Thickness < culledDepth)
					{
						opacity = lerp(lightDepth.g, lightDepth.b, (z - _Layer1Thickness - culledDepth) / _Layer2Thickness);
						//float prevMax = lerp(0, lightDepth.g, (z - culledDepth) / _Layer1Thickness);
						float prevMax = lightDepth.g;
						opacity_rgb.g = lerp(0, lightDepth.b, (z - _Layer1Thickness - culledDepth) / _Layer2Thickness);
						opacity_rgb.r = lerp(prevMax, 0, (z - _Layer1Thickness - culledDepth) / _Layer2Thickness);
					}
					// layer 3
					else
					{
						opacity = lerp(lightDepth.b, lightDepth.a,
							(z - _Layer1Thickness - _Layer2Thickness - culledDepth) / _Layer3Thickness);

						float prevMax = lightDepth.b;
						//float prevMax = lerp(0, lightDepth.b, (z - _Layer1Thickness - culledDepth) / _Layer2Thickness);
						opacity_rgb.b = lerp(0, lightDepth.a,
							(z - _Layer1Thickness - _Layer2Thickness - culledDepth) / _Layer3Thickness);
						opacity_rgb.g = lerp(prevMax, 0,
							(z - _Layer1Thickness - _Layer2Thickness - culledDepth) / _Layer3Thickness);
					}
					
					opacity = max(0, opacity);
					//float _OpacityOn;
					//float _OpacityRGB;
					//float _OpacityGreyscale;

					opacity_rgb = lerp(opacity, opacity_rgb, _OpacityRGB);
					opacity_rgb = lerp(opacity_rgb, opacity, _OpacityGreyscale);
					opacity = lerp(0, opacity, _OpacityOn);

					// convert ambient lighting to greyscale
					// https://answers.unity.com/questions/343243/unlit-greyscale-shader.html
					float greyscaleAmbient = dot(_Ambient * _AmbientColor, float3(0.3, 0.59, 0.11));
					// Limit opacity to the ambient minimum
					opacity = min(1 - greyscaleAmbient, opacity);

					//opacity = 0;

					//float depthValue = ((_ProjectionParams.x * shadowLightSpace.z) - lightDepth) / 20;// -lightDepth;// -lightDepth;
					//float depthValue = 1.0 / (_DepthZBufferParams.x * lightDepth + _DepthZBufferParams.y);// -lightDepth;// -lightDepth;

					// discard fragment if white (or very close)
					//if (1 - depthValue < 0.1) {
					//	//discard;
					//}

					col.rgb = lerp(col.rgb * (1 - opacity), opacity_rgb, max(_OpacityRGB, _OpacityGreyscale));
					//col.rgb *= (1 - opacity);

					// ambient lighting
					// https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
					//col.rgb += ShadeSH9(half4(i.normalWorldSpace, 1));

					//col.rgb += _Ambient * _AmbientColor;

					//col.r = shadowCoord.x;
					//col.g = shadowCoord.y; 
					//col.b = 0; //depthValue

					//col.r = i.scrPos.x;
					//col.g = i.scrPos.y;
					//col.b = 0; //depthValue

					//col.r = opacity_rgb.r;
					//col.g = opacity_rgb.g;
					//col.b = opacity_rgb.b;

					//col = tex2D(_Test, i.scrPos.xy / _ScreenParams.xy);
					//col = tex2D(_Test, i.pos.xy / _ScreenParams.xy);

					//col = tex2D(_Test, shadowCoord);

					//col = tex2D(_Test, vpos.xy / _ScreenParams.xy);
					
					col.a = tex2D(_AlphaTex, i.uv).r * _AlphaMultiplier;

					// Alpha cutoff.
					//if (col.a >= _CutoutThresh) {
					//	col.a = 1;// _Transparency;
					//}
					// This could be put in an else statement, but wouldn't improve performance.
					//clip(col.a - _CutoutThresh);
					//clip(col.a < _CutoutThresh ? -1 : 1);
					if (col.a < _CutoutThresh) {
						discard;
					}
					return col;
				}

			ENDCG
			}
		}
}