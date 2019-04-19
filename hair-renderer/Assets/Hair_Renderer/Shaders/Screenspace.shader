// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Screenspace"
{
    Properties
    {
        //_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        //_Glossiness ("Smoothness", Range(0,1)) = 0.5
        //_Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
	{
			Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
			};

			//float4 vert(appdata_base v) : SV_POSITION
			//{
			//	return UnityObjectToClipPos(v.vertex);
			//}

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			sampler2D _MainTex;
			float _Zoom;
			float _SpeedX;
			float _SpeedY;

			//fixed4 frag(float4 i : VPOS) : SV_Target
			//{
			//	// Screen space texture
			//	return tex2D(_MainTex, (i.xy / _ScreenParams.xy));
			//}


			fixed4 frag(v2f i) : SV_Target
			{
				// Screen space texture
				return tex2D(_MainTex, (i.pos.xy / _ScreenParams.xy));
			}
			ENDCG
		}

	}
    FallBack "Diffuse"
}
