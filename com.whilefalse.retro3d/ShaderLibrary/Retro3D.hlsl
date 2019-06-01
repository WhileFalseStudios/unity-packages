
float _VertexPrecision;
float4x4 _ViewmodelProjMatrix;

struct Attributes
{
    float4 position : POSITION;
    float2 texcoord : TEXCOORD0;
    float2 texcoord_lightmap : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 position : SV_POSITION;
    float3 normal : NORMAL0;
    float3 viewDir : NORMAL1;

#ifdef PERSPECTIVE_CORRECTION_ON
    float2 texcoord : TEXCOORD0;
    float2 texcoord_lightmap : TEXCOORD1;
#else
    noperspective float2 texcoord : TEXCOORD0;
    noperspective float2 texcoord_lightmap : TEXCOORD1;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID

    UNITY_FOG_COORDS(2)
};