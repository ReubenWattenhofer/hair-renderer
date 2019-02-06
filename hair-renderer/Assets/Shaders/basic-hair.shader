Shader "Custom/basic"
{
	Properties
	{
		_MainTex("Albedo Texture", 2D) = "white" {}
		_AlphaTex("Alpha Texture", 2D) = "white" {}
		_AmbientOcclusion("Ambient Occlusion", 2D) = "white" {}
		_Brightness("Brightness", 2D) = "white" {}

		_TintColor("Tint Color", Color) = (1,1,1,1)
		//_Transparency("Transparency", Range(0.0,0.5)) = 0.25
		_CutoutThresh("Alpha Cutoff", Range(0.0,1.0)) = 0.5
		//_Distance("Distance", Float) = 1
		//_Amplitude("Amplitude", Float) = 1
		//_Speed("Speed", Float) = 1
		//_Amount("Amount", Range(0.0,1.0)) = 1
	}

		SubShader
		{
			Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 100

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				sampler2D _AmbientOcclusion;
				sampler2D _Brightness;
				// From the starter code, I think it's for offset and scaling
				float4 _MainTex_ST;
				float4 _TintColor;
				//float _Transparency;
				float _CutoutThresh;
				//float _Distance;
				//float _Amplitude;
				//float _Speed;
				//float _Amount;

				v2f vert(appdata v)
				{
					v2f o;
					//v.vertex.x += sin(_Time.y * _Speed + v.vertex.y * _Amplitude) * _Distance * _Amount;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// sample the texture
					fixed4 col;
					// Apply brightness map to rgb value.
					// TODO: Apply ambient occlusion map to lighting values.
					col.rgb = tex2D(_MainTex, i.uv).rgb * tex2D(_Brightness, i.uv).r;
					col.a = tex2D(_AlphaTex, i.uv).a;
					//col.rgb = _TintColor.rgb;
					//col.a = tex2D(_MainTex, i.uv).a;
					
					if (col.a >= _CutoutThresh) {
						col.a = 1;// _Transparency;
					}
					// else statement isn't strictly necessary, since clip doesn't remove unless value < 0
					//else {
						// From Blacksmith shader (see Unity asset store)
						clip(col.a - _CutoutThresh);
					//}
					return col;
				}

			ENDCG
		}
		}
}