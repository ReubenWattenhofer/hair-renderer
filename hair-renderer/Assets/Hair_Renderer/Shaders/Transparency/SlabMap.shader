﻿Shader "Custom/Transparency/SlabMap" {
	// from "Hair Self Shadowing and Transparency Depth Ordering Using Occupancy maps"
	Properties
	{
		_AlphaTex("Alpha Texture", 2D) = "white" {}
	}

		SubShader{
		Tags { "RenderType" = "Opaque" }
		//Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100
		//ZWRITE On
			ZWRITE Off
			Cull Off

		// Additive blending
		Blend One One


		Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		#include "../HairCommon.cginc"

		sampler2D _AlphaTex;

		// generated by TransparencySorting
		sampler2D _MainDepth;
		sampler2D _HeadMainDepth;
		float _AlphaMultiplier;
		float _CutoutThresh;

		struct v2f {
			float4 pos : SV_POSITION;
			float4 scrPos : TEXCOORD1;
			float3 viewPos : TEXCOORD2;

			float2 uv : TEXCOORD3;
		};

		//Vertex Shader
		v2f vert(appdata_base v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.scrPos = ComputeScreenPos(o.pos);
			o.viewPos = UnityObjectToViewPos(v.vertex);
			o.uv = v.texcoord;

			return o;
		}

		//Fragment Shader
		float4 frag(v2f i) : COLOR {
			i.scrPos /= i.scrPos.w;

			float4 nearFar = tex2D(_MainDepth, i.scrPos);

			float depthValue = Normalize_Depth(-i.viewPos.z, _ProjectionParams.y, _ProjectionParams.z, 10);
					   
			// Get relative depth of texel
			float relativeDepth = (depthValue - nearFar.r) / (nearFar.a - nearFar.r);
			// Get closest slab
			int slab = floor(relativeDepth * 4);

			float4 headNearFar = tex2D(_HeadMainDepth, i.scrPos);

			float alpha = tex2D(_AlphaTex, i.uv).r * _AlphaMultiplier;

			if (depthValue >= headNearFar.r || alpha < _CutoutThresh) {
				discard;
			}

			float4 depth = float4(0, 0, 0, 0);
			if (slab == 0)
			{
				depth.r = 1;
			}
			else if (slab == 1)
			{
				depth.g = 1;
			}
			else if (slab == 2)
			{
				depth.b = 1;
			}
			else if (slab == 3 && nearFar.a - nearFar.r > 0.001)
			{
				depth.a = 1;// 1;
			}

			//depth = 0;
			//discard;
			return depth;

		}
		ENDCG
		}
	}
		FallBack "Diffuse"
}