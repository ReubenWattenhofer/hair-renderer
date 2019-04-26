﻿Shader "Custom/Transparency/Background_hair_combiner" {
	// from "Hair Self Shadowing and Transparency Depth Ordering Using Occupancy maps"
	SubShader{
	//Tags { "RenderType" = "Opaque" }
	//Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
	Tags {"RenderType" = "Transparent" }
	Tags {"Queue" = "Overlay"}
	//Tags { "ForceNoShadowCasting" = "True"}

	LOD 100
		ZWRITE On
		//ZWRITE Off
		Cull Off

		// Bitwise OR
		//BlendOp LogicalOr

		// https://docs.unity3d.com/Manual/SL-GrabPass.html
		// Grab the screen behind the object into _BackgroundTexture
		GrabPass
		{
			"_BackgroundTexture"
		}

		Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		#include "../HairCommon.cginc"

		// generated by TransparencySorting
		sampler2D _MainDepth;
		sampler2D _Hair;
		sampler2D _MainSlab;
		//sampler2D _Background;
		sampler2D _BackgroundTexture;

		struct v2f {
			float4 pos : SV_POSITION;
			float4 scrPos : TEXCOORD1;
			float3 viewPos : TEXCOORD2;

			float4 grabPos : TEXCOORD0;
		};

		//Vertex Shader
		v2f vert(appdata_base v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.scrPos = ComputeScreenPos(o.pos);
			o.viewPos = UnityObjectToViewPos(v.vertex);

			// use ComputeGrabScreenPos function from UnityCG.cginc
			// to get the correct texture coordinate
			o.grabPos = ComputeGrabScreenPos(o.pos);

			return o;
		}

		//Fragment Shader
		float4 frag(v2f i) : COLOR {
			i.scrPos /= i.scrPos.w;

			//float4 nearFar = tex2D(_MainDepth, i.scrPos);
			float4 slabs = tex2D(_MainSlab, i.scrPos) * 1;

			float4 background = tex2Dproj(_BackgroundTexture, i.grabPos);
			//float4 background = tex2D(_BackgroundTexture, i.scrPos);
			//float4 background = tex2D(_Background, i.scrPos);

			float4 hair = tex2D(_Hair, i.scrPos);

			float allFragments = slabs.r + slabs.g + slabs.b + slabs.a;
			//float allFragments = slabs.a;

			//allFragments = min(2, allFragments);
			allFragments = max(0, allFragments);

			float4 col = float4(0, 0, 0, 0);
			if (hair.r == 0) 
			{
				col = background;// +(background * pow(1 - 0.8, allFragments));
			}
			else
			{
			//col = slabs;//slabs// hair + (background * pow(1 - 0.8, allFragments));
			//col = tex2D(_MainDepth, i.scrPos);
			//col.g = tex2D(_MainDepth, i.scrPos).a;
				col = hair + (background * pow(1 - 0.8, allFragments));
				//col = hair + background;
			}
					   
			return col;

		}
		ENDCG
		}
	}
		//FallBack "Diffuse"
}