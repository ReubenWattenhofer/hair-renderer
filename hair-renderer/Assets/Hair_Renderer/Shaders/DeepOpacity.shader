Shader "Custom/DeepOpacity" {
	Properties
	{
		_Layer1("Layer 1 Size", Range(0.0,1.0)) = 0.2
		_Layer2("Layer 2 Size", Range(0.0,1.0)) = 0.4
		_Layer3("Layer 3 Size", Range(0.0,1.0)) = 0.8
		_Opacity1("Layer Opacity", Range(0.0,1.0)) = 0.1
		//_Opacity2("Layer 2 Opacity", Range(0.0,1.0)) = 0.1
		//_Opacity3("Layer 3 Opacity", Range(0.0,1.0)) = 0.1
	}

	SubShader{
		Tags { "RenderType" = "Opaque" }
		//Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100
		ZWRITE On
		//Cull Off

		// Not quite additive blending -- we ignore alpha since we're not using it as an alpha channel
		Blend One One

		Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		float _Layer1;
		float _Layer2;
		float _Layer3;

		sampler2D _CameraDepthTexture;
		sampler2D _DeepOpacityMap;

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
			float culledDepth = tex2Dproj(_DeepOpacityMap, UNITY_PROJ_COORD(i.scrPos)).r;
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