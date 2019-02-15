#include "UnityCG.cginc" 
#include "Lighting.cginc" 
#include "AutoLight.cginc"

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

	float4 posModelSpace : TEXCOORD4;
};


// Good reference for lighting calculations (and Unity shader examples):
// https://en.wikibooks.org/wiki/Cg_Programming/Unity/Specular_Highlights

// The following three functions are from
// http://web.engr.oregonstate.edu/~mjb/cs519/Projects/Papers/HairRendering.pdf

float3 ShiftTangent(float3 T, float3 N, float shift)
{
	float3 shiftedT = T + shift * N;
	return normalize(shiftedT);
}

float StrandSpecular(float3 T, float3 V, float3 L, float exponent)
{
	float3 H = normalize(L + V);
	float dotTH = dot(T, H);
	float sinTH = sqrt(1.0 - dotTH * dotTH);
	float dirAtten = smoothstep(-1.0, 0.0, dot(T, H));
	return dirAtten * pow(sinTH, exponent);
}

float4 HairLighting(float3 tangent, float3 normal, float3 lightVec, float3 viewVec, float2 uv, float ambOcc,
	float specShift, float primaryShift, float secondaryShift, float3 diffuseAlbedo, float3 tint, float3 specularColor1,
	float specExp1, float3 specularColor2, float specExp2, float secondarySparkle, float3 lightColor)
{
	tangent *= -1;
	// shift tangents
	float shiftTex = specShift;// tex2D(tSpecShift, uv) �0.5;
	float3 t1 = ShiftTangent(tangent, normal, primaryShift + shiftTex);
	float3 t2 = ShiftTangent(tangent, normal, secondaryShift + shiftTex);
	// diffuse lighting: the lerp shifts the shadow boundary for a softer look
	float3 diffuse = saturate(lerp(0.25, 1.0, dot(normal, lightVec)));
	diffuse *= diffuseAlbedo;
	
	// specular lighting
	float3 specular = specularColor1 * StrandSpecular(t1, viewVec, lightVec, specExp1);
	// add 2nd specular term, modulated with noise texture
	float specMask = secondarySparkle;// tex2D(tSpecMask, uv); // approximate sparkles using texture
	specular += specularColor2 * specMask* StrandSpecular(t2, viewVec, lightVec, specExp2);
	// final color assembly
	float4 o;
	//o.rgb = (diffuse + specular) * tex2D(tBase, uv) * lightColor;
	o.rgb = (diffuse + specular) * diffuseAlbedo * lightColor;
	o.rgb *= ambOcc;	// modulate color by ambient occlusion term
	o.a = 1;// tex2D(tAlpha, uv);    // read alpha texture
	return o;

}

