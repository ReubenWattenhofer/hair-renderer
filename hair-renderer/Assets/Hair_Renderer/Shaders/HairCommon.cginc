#include "UnityCG.cginc" 
#include "Lighting.cginc" 
#include "AutoLight.cginc"

// https://answers.unity.com/questions/59563/for-loop-in-shader.html
// for looping
#pragma target 4.0

// https://stackoverflow.com/questions/109023/how-to-count-the-number-of-set-bits-in-a-32-bit-integer
int numberOfSetBits(int i)
{
	int count = 0;
	i = max(0, i);
	while (i)
	{
		count += i & 1;
		i >>= 1;
	}
	return count;
	
	//return 1;
	//// Java: use >>> instead of >>
	//// C or C++: use uint32_t
	//i = i - ((i >> 1) & 0x55555555);
	//i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
	//return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
}

// Sets all upper bits of input to 0
uint mask(int n, int num_rightmost_bits)
{
	int result = n;

	num_rightmost_bits = min(16, num_rightmost_bits);

	int mask = 0;
	for (int i = 0; i < num_rightmost_bits; i++)
	{
		mask |= 1 << i;
	}

	result &= mask;

	return result;
}


float Normalize_Depth(float z, float near, float far, float scale) 
{
	return ((scale *z) - near) / (far - near);
}

// Convert from normalized to original depth
float Get_True_Depth(float z, float near, float far, float originalScale)
{
	return (z * (far - near) + near) / originalScale;
}

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
	float specExp1, float3 specularColor2, float specExp2, float secondarySparkle, float3 lightColor, float3 ambientColor)
{
	tangent *= -1;
	// shift tangents
	float shiftTex = specShift;// tex2D(tSpecShift, uv) –0.5;
	float3 t1 = ShiftTangent(tangent, normal, primaryShift + shiftTex);
	float3 t2 = ShiftTangent(tangent, normal, secondaryShift + shiftTex);
	// diffuse lighting: the lerp shifts the shadow boundary for a softer look
	float3 diffuse = saturate(lerp(0.25, 1.0, dot(normal, lightVec) + ambientColor));
	//float3 diffuse = saturate(lerp(0.25, 1.0, dot(normal, lightVec)));
	diffuse *= diffuseAlbedo * tint;
	
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

