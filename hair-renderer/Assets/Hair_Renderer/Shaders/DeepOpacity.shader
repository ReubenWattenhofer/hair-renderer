﻿// http://williamchyr.com/2013/11/unity-shaders-depth-and-normal-textures/
Shader "Custom/DeepOpacity" {
	SubShader{
	//Tags { "RenderType" = "Opaque" }
	Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
	LOD 100
	ZWRITE Off
	Cull Off

	Blend One One

	Pass{
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"

	#include "HairCommon.cginc"


	sampler2D _CameraDepthTexture;
	sampler2D _DepthCulled;

	// generated by TestScreenPosRunner
	float4 _DepthCameraPlanes;
	float _Layer1Thickness;
	float _Layer2Thickness;
	float _OpacityPerFragment;


	struct v2f {
		float4 pos : SV_POSITION;
		float4 scrPos : TEXCOORD1;
		float3 viewPos : TEXCOORD2;
	};

	//Vertex Shader
	v2f vert(appdata_base v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.scrPos = ComputeScreenPos(o.pos);
		o.viewPos = UnityObjectToViewPos(v.vertex);

		return o;
	}

	//Fragment Shader
	half4 frag(v2f i) : COLOR{
		float culledDepth = tex2D(_DepthCulled, i.scrPos).r;
		culledDepth = Get_True_Depth(culledDepth, _DepthCameraPlanes.x, _DepthCameraPlanes.y);

		float z = -i.viewPos.z;

		half4 depth;

		// layer 1
		if (z - _Layer1Thickness < culledDepth) {
			depth.g = _OpacityPerFragment;
			depth.b = _OpacityPerFragment;
			depth.a = _OpacityPerFragment;
		}
		// layer 2
		else if (z - _Layer1Thickness - _Layer2Thickness < culledDepth) {
			depth.g = 0;
			depth.b = _OpacityPerFragment;
			depth.a = _OpacityPerFragment;
		}
		// layer 3
		else {
			depth.g = 0;
			depth.b = 0;
			depth.a = _OpacityPerFragment;
		}

		return depth;

	}
	ENDCG
	}
	}
		//FallBack "Diffuse"
}