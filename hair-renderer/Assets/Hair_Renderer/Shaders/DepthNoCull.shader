// http://williamchyr.com/2013/11/unity-shaders-depth-and-normal-textures/
Shader "Custom/DepthNoCull" {
	SubShader{
	Tags { "RenderType" = "Opaque" }
	//Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
	LOD 100
	ZWRITE On
	Cull Off

	Pass{
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"



	sampler2D _CameraDepthTexture;
	sampler2D _DepthCulled;

	struct v2f {
		float4 pos : SV_POSITION;
		float4 scrPos : TEXCOORD1;
	};

	//Vertex Shader
	v2f vert(appdata_base v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.scrPos = ComputeScreenPos(o.pos);

		return o;
	}

	//Fragment Shader
	half4 frag(v2f i) : COLOR{
		//float depthValue = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
		float culledDepth = tex2Dproj(_DepthCulled, UNITY_PROJ_COORD(i.scrPos)).r;
		float depthValue = Linear01Depth(i.pos.z);

		float d = depthValue;
		if (culledDepth < depthValue) {
			d = culledDepth;
		}

		half4 depth;

		depth.r = d;
		depth.g = d;
		depth.b = d;

		depth.a = 1;
		//discard;
		return depth;

		}
		ENDCG
		}
	}
		FallBack "Diffuse"
}