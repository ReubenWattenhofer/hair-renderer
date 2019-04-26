﻿Shader "Custom/Transparency/Background_hair_combiner" {
	// from "Hair Self Shadowing and Transparency Depth Ordering Using Occupancy maps"
	SubShader{
	Tags { "RenderType" = "Opaque" }
	//Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
	LOD 100
		ZWRITE On
		//ZWRITE Off
		Cull Off

		// Bitwise OR
		//BlendOp LogicalOr

		Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		#include "../HairCommon.cginc"

		// generated by TransparencySorting
		sampler2D _MainDepth;
		sampler2D _Hair;

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
		int4 frag(v2f i) : COLOR {
			i.scrPos /= i.scrPos.w;

			float4 nearFar = tex2D(_MainDepth, i.scrPos);

			float depthValue = Normalize_Depth(-i.viewPos.z, _ProjectionParams.y, _ProjectionParams.z);

			// Get relative depth of texel
			float relativeDepth = (depthValue - nearFar.r) / (nearFar.a - nearFar.r);
			// Get closest slice
			float slice = floor(relativeDepth * 64);

			// Figure out which color channel to put the data into
			int color = floor(slice / 16);
			// bit in color channel (should be between 0 and 31, inclusive);
			int relativeSlice;


			int4 depth = int4(0, 0, 0, 0);
			if (color == 0)
			{
				relativeSlice = slice;
				depth.r |= 1 << relativeSlice;
			}
			else if (color == 1)
			{
				relativeSlice = slice - 16 * 1;
				depth.g |= 1 << relativeSlice;
			}
			else if (color == 2)
			{
				relativeSlice = slice - 16 * 2;
				depth.b |= 1 << relativeSlice;
			}
			else
			{
				relativeSlice = slice - 16 * 3;
				depth.a |= 1 << relativeSlice;
			}

			//discard;
			return depth;

		}
		ENDCG
		}
	}
		FallBack "Diffuse"
}