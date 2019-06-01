#ifndef RETRO3D_H
#define RETRO3D_H

float _VertexPrecision;
float4x4 _ViewmodelProjMatrix;

#ifdef PERSPECTIVE_CORRECTION_ON
#define PERSP_TEXCOORD float2
#else
#define PERSP_TEXCOORD noperspective float2
#endif

struct Retro3DVertex
{
	float4 position;
	float3 normal;
	float3 viewDir;
};

struct Retro3DSurface
{
	float4 diffuse;
	float4 emission;
	float4 specular;
	float glossiness;
};

#define MAX_VISIBLE_LIGHTS 4

CBUFFER_START(_LightBuffer)
	float4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightDirectionsOrPositions[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightAttenuations[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightSpotDirections[MAX_VISIBLE_LIGHTS];
CBUFFER_END

CBUFFER_START(UnityPerDraw)
	float4 unity_LightIndicesOffsetAndCount;
	float4 unity_4LightIndices0, unity_4LightIndices1;
CBUFFER_END

float3 DiffuseLight(int index, float3 normal, float3 worldPos)
{
	float3 color = _VisibleLightColors[index].rgb;
	float4 dirpos = _VisibleLightDirectionsOrPositions[index];	
	float4 attenuation = _VisibleLightAttenuations[index];
	float3 spotDir = _VisibleLightSpotDirections[index].xyz;

	float3 lightvec = dirpos.xyz - worldPos * dirpos.w;
	float3 dir = normalize(lightvec);
	float3 diffuseLight = saturate(dot(normal, dir));

	float fadeRange = dot(lightvec, lightvec) * attenuation.x;
	fadeRange = saturate(1.0 - fadeRange * fadeRange);
	fadeRange *= fadeRange;

	float spotFade = dot(spotDir, dir);
	spotFade = saturate(spotFade * attenuation.z + attenuation.w);
	spotFade *= spotFade;

	float distSqrt = max(dot(lightvec, lightvec), 0.00001);
	diffuseLight *= spotFade * fadeRange / distSqrt;

	return diffuseLight * color;
}

Retro3DVertex RetroLitVertex(float4 position)
{ 
	Retro3DVertex output;

    float3 vp = UnityObjectToViewPos(position.xyz);
#ifdef VERTEX_PRECISION_ON
    vp = floor(vp * _VertexPrecision) / _VertexPrecision;
#endif

#ifdef VIEWMODEL_ON
    output.position = mul(_ViewmodelProjMatrix, float4(vp, 20));
#else
    output.position = UnityViewToClipPos(vp);
#endif	

	output.normal = normalize(UnityObjectToWorldNormal(position));
    output.viewDir = WorldSpaceViewDir(position);
    return output;
}

half4 RetroLitSurface(Retro3DSurface surf, float2 texcoord_lightmap, float3 normal, float3 viewDir, float3 position) : SV_Target
{
	float4 c = surf.diffuse;
#ifdef REFLECTIONS_ON
    float3 reflDir = reflect(viewDir, normal);
    float3 reflColor = DecodeHDR(UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflDir), unity_SpecCube0_HDR);
	c = lerp(c, float4(reflColor, c.a), surf.glossiness);
#endif

	float3 light = 0;
	for (int i = 0; i < MAX_VISIBLE_LIGHTS; i++)
	{
		light += DiffuseLight(i, normal, position);
	}

#ifdef LIGHTMAP_ON
	light += DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, texcoord_lightmap)).rgb;
#else
	light += ShadeSH9(float4(normal, 1));
#endif	

#if EMISSION_ON
	light += surf.emission;
#endif

	c.rgb *= light;

    return c;
}

#endif /* RETRO3D_H */