// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Shader "Test/DepthTest"
//{
//	Properties
//	{
//		//_MainTex("Texture", 2D) = "white" {}
//	}
//	SubShader
//	{
//		Tags { "RenderType" = "Opaque" }
//		LOD 100
//		ZWRITE On
//
//		Pass
//		{
//			CGPROGRAM
//			#pragma vertex vert
//			#pragma fragment frag
//			// make fog work
//			//#pragma multi_compile_fog
//
//			#include "UnityCG.cginc"
//
//			struct appdata
//			{
//				float4 vertex : POSITION;
//				//float2 uv : TEXCOORD0;
//			};
//
//			//struct v2f
//			//{
//			//	float2 uv : TEXCOORD0;
//			//	//UNITY_FOG_COORDS(1)
//			//	float4 vertex : SV_POSITION;
//			//	float depth : TEXCOORD2;
//			//};
//
//			uniform sampler2D _CameraDepthTexture;
//			//sampler2D _CameraDepthTexture;
//
//			//v2f vert(appdata_img v)
//			//{
//			//	v2f o;
//			//	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
//			//	o.uv = v.texcoord.xy;
//			//	return o;
//			//}
//
//			//half4 fragThin(v2f i) : COLOR
//			//{
//			//	half4 depth = half4(Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r);
//
//			//	return depth;
//			//}
//
//			//sampler2D _MainTex;
//			//float4 _MainTex_ST;
//
//
//			struct v2f {
//				float4 pos : SV_POSITION;
//				float2 depth : TEXCOORD0;
//			};
//
//			v2f vert(appdata_base v) {
//				v2f o;
//				o.pos = UnityObjectToClipPos(v.vertex);
//				UNITY_TRANSFER_DEPTH(o.depth);
//				return o;
//			}
//
//			half4 frag(v2f i) : SV_Target{
//				UNITY_OUTPUT_DEPTH(i.depth);
//			}
//
//			//v2f vert(appdata v)
//			//{
//			//	v2f o;
//			//	o.vertex = UnityObjectToClipPos(v.vertex);
//			//	//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//			//	//UNITY_TRANSFER_FOG(o,o.vertex);
//			//	//o.uv = v.texcoord.xy;
//			//	o.uv = COMPUTE_EYEDEPTH(v.vertex);
//
//			//	return o;
//			//}
//
//			//fixed4 frag(v2f i) : SV_Target
//			//{
//			//	// sample the texture
//			//	//fixed4 col = tex2D(_MainTex, i.uv);
//			//	//fixed4 col;
//			//	//half4 col = half4(Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r));
//
//			//	//col.r = 1;
//			//	//col.a = 0.5;
//			//	//half4 depth = half4(Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r), 0, 0, 1);
//			//	//fixed4 depth = (tex2D(_CameraDepthTexture, i.uv).r);
//			//	float2 uv = i.vertex.xy / 2.0f + float2(0.5f, 0.5f);
//			//	//fixed4 depth = tex2D(_CameraDepthTexture, uv);
//			//	//fixed4 depth = Linear01Depth(tex2D(_CameraDepthTexture, uv).r);
//			//	fixed4 depth = i.depth;
//
//			//	return depth;
//			//}
//			ENDCG
//		}
//
//	}
//}

//Shader "Render Depth" {
//	SubShader{
//		Tags { "RenderType" = "Opaque" }
//
//		Pass {
//			CGPROGRAM
//
//			#pragma vertex vert
//			#pragma fragment frag
//			#include "UnityCG.cginc"
//
//			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
//			//sampler2D _CameraDepthTexture;
//
//			struct v2f {
//				float4 pos : SV_POSITION;
//				//float2 depth : TEXCOORD0;
//				//float4 projPos : TEXCOORD1;
//				//float4 screenuv : TEXCOORD2;
//				float4 scrPos:TEXCOORD3;
//			};
//
//			//v2f vert(appdata_base v) {
//			//	v2f o;
//			//	
//			//	//o.pos = UnityObjectToClipPos(v.vertex);
//			//	//UNITY_TRANSFER_DEPTH(o.depth);
//
//			//	//o.projPos = UnityObjectToClipPos(v.vertex);
//			//	//o.projPos.z = -UnityObjectToViewPos(v.vertex).z;
//
//			//	//o.pos = UnityObjectToClipPos(v.vertex);
//			//	o.pos = UnityObjectToClipPos(v.vertex);
//			//	o.screenuv = ComputeScreenPos(o.pos);
//			//	return o;
//			//}
//
//			//half4 frag(v2f i) : SV_Target// COLOR //SV_Target
//			//{
//			//	//UNITY_OUTPUT_DEPTH(i.depth);
//			//	//fixed4 depth = fixed4(1,0,0,0);
//			//	//return depth;
//
//			//	//return LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))) * _ProjectionParams.w;
//
//			//	// Gives error; code is 9 years old, probably deprecated
//			//	//half4 depth = half4(Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r);
//
//			//	//return depth;
//
//			//	float2 uv = i.screenuv.xy / i.screenuv.w;
//			//	float depth = 1 - Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
//			//	return fixed4(depth, depth, depth, 1);
//			//}
//
//			//Vertex Shader
//			v2f vert(appdata_base v) {
//				v2f o;
//				o.pos = UnityObjectToClipPos(v.vertex);
//				o.scrPos = ComputeScreenPos(o.pos);
//				//for some reason, the y position of the depth texture comes out inverted
//				//o.scrPos.y = 1 - o.scrPos.y;
//				return o;
//			}
//
//			//Fragment Shader
//			half4 frag(v2f i) : COLOR{
//			   float depthValue = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
//			   half4 depth;
//
//			   depth.r = depthValue;
//			   depth.g = depthValue;
//			   depth.b = depthValue;
//
//			   depth.a = 1;
//			   return depth;
//			}
//			ENDCG
//		}
//
//	}
//
//}

// http://williamchyr.com/2013/11/unity-shaders-depth-and-normal-textures/
Shader "Custom/DepthGrayscale" {
	SubShader{
	Tags { "RenderType" = "Opaque" }
	//Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
	LOD 100
	ZWRITE On
	//Cull Off


	Pass{
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag

	// For lighting/shadowmaps
	#pragma multi_compile_fwdbase

	//#include "AutoLight.cginc"
	#include "UnityCG.cginc"

	#include "HairCommon.cginc"


	sampler2D _CameraDepthTexture;
	//UNITY_DECLARE_SHADOWMAP(shadow_map);
	//float4 _LightColor0;
	// generated by TestScreenPosRunner
	float4 _DepthCameraPlanes;

	float4x4 _DepthView;
	float4x4 _DepthVP;
	float4 _DepthScreenParams;
	float4 _DepthZBufferParams;


	struct v2f {
	   float4 pos : SV_POSITION;
	   float4 scrPos : TEXCOORD1;
	   float3 viewPos : TEXCOORD2;
	   // https://forum.unity.com/threads/adding-lighting_coords-makes-unity-3-comment-out-subshader-as-2-x-style.74103/
	   LIGHTING_COORDS(3, 4)
	};

	//Vertex Shader
	v2f vert(appdata_base v) {
	   v2f o;
	   o.pos = UnityObjectToClipPos(v.vertex);
	   o.scrPos = ComputeScreenPos(o.pos);
	   o.viewPos = UnityObjectToViewPos(v.vertex);
	   //for lighting coordinates or something
	   TRANSFER_VERTEX_TO_FRAGMENT(o);
	   return o;


		//float4 shadowWorld = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
		//float4 shadowCoord = mul(_DepthVP, shadowWorld);
		//			
		//float4 shadowLightSpace = mul(_DepthView, shadowWorld);
		//o.pos = shadowCoord;
		//o.viewPos = shadowLightSpace.xyz;


		////float4 o = shadowCoord * 0.5f;
		//////float4 screenParams = float4(256, 256, 1 + 1 / 256, 1 + 1 / 256);
		////#if defined(UNITY_HALF_TEXEL_OFFSET)
		//////o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w * _ScreenParams.zw;
		////o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w * _DepthScreenParams.zw;
		////#else
		////o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w;
		////#endif
 
		////o.zw = shadowCoord.zw;
		////			
		////// "Computes texture coordinate for doing a screenspace-mapped texture sample. Input is clip space position."
		//////shadowCoord = ComputeScreenPos(shadowCoord);
		////shadowCoord = o;
		////// https://www.ronja-tutorials.com/2019/01/20/screenspace-texture.html
		////shadowCoord.xy /= shadowCoord.w;

		//return o;
	}

	//Fragment Shader
	float4 frag(v2f i) : COLOR {
	   //float depthValue = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);

		float depthValue = Normalize_Depth(-i.viewPos.z, _DepthCameraPlanes.x, _DepthCameraPlanes.y, 1);
		
		 //float depthValue = (-i.viewPos.z - _DepthCameraPlanes.x)
			//			/ (_DepthCameraPlanes.y - _DepthCameraPlanes.x);

		//float depthValue = (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
		//float depthValue = Linear01Depth(i.pos.z);
		//float depthValue = shadow_map;

	float4 depth;

	   depth.r = depthValue;
	   depth.g = 0;// depthValue;
	   depth.b = 0;// depthValue;

	   depth.a = 0;// 1;
	   //discard;
	   return depth;
	   
	}
	ENDCG
	}
	}
		FallBack "Diffuse"
}