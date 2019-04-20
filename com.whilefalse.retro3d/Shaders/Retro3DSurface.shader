Shader "Retro3D/Surface"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _MainTex_ST("Texture Offset", Vector) = (1, 1, 0, 0)
		_Emissive ("Emissive Texture", 2D) = "black" {}
        _Color("Tint", Color) = (1, 1, 1)
		_EmissiveColor("Emissive Tint", Color) = (1.0, 1.0, 1.0)
        _AlphaClip("Clip Threshold", Float) = 0.5
        [Toggle(VIEWMODEL_ON)] _ViewModel("Viewmodel Rendered", Float) = 0
        [Toggle(REFLECTIONS_ON)] _Reflections("Reflections", Float) = 0
        _ReflTex("Reflection Specular Texture", 2D) = "white" {}
        _ReflColor("Reflection Specular Color", Color) = (1, 1, 1, 1)
        _CullMode("Cull Mode", Int) = 2
        _SurfaceMode("Surface Mode", Int) = 0
    }

    HLSLINCLUDE

    #include "UnityCG.cginc"
	#include "Packages/com.whilefalse.retro3d/ShaderLibrary/Retro3D.hlsl"

    sampler2D _MainTex;
    float4 _MainTex_ST;
    #if EMISSION_ON
	sampler2D _Emissive;
    #endif
    float _AlphaClip;
    #if REFLECTIONS_ON
    sampler2D _ReflTex;
    #endif

    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
           UNITY_DEFINE_INSTANCED_PROP(half3, _Color)
           #if EMISSION_ON
           UNITY_DEFINE_INSTANCED_PROP(half3, _EmissiveColor)
           #endif
           #if REFLECTIONS_ON
            UNITY_DEFINE_INSTANCED_PROP(half3, _ReflColor)
           #endif
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

    Varyings Vertex(Attributes input)
    { 
		Varyings output;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(output, input);

        float3 vp = UnityObjectToViewPos(input.position.xyz);
        #ifdef VERTEX_PRECISION_ON
        vp = floor(vp * _VertexPrecision) / _VertexPrecision;
        #endif

        #ifdef VIEWMODEL_ON
        output.position = mul(_ViewmodelProjMatrix, float4(vp, 20));
        #else
        output.position = UnityViewToClipPos(vp);
        #endif	

        output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
		output.texcoord_lightmap = input.texcoord_lightmap.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		output.normal = normalize(UnityObjectToWorldNormal(input.position));
        output.viewDir = WorldSpaceViewDir(input.position);
        #ifndef VIEWMODEL_ON
        UNITY_TRANSFER_FOG(output, output.position);
        #endif
        return output;
    }

    half4 Fragment(Varyings input) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);

        float2 uv = input.texcoord;
        half4 c = tex2D(_MainTex, uv);
        c.rgb *= UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Color);
#ifdef LIGHTMAP_ON
		c.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, input.texcoord_lightmap)).rgb;
#else
		c.rgb *= ShadeSH9(float4(input.normal, 1));
#endif

#ifdef REFLECTIONS_ON
        float3 reflDir = reflect(input.viewDir, input.normal);
        c.rgb *= DecodeHDR(UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflDir), unity_SpecCube0_HDR);
#endif

#if EMISSION_ON
		c.rgb += tex2D(_Emissive, uv).rgb * UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _EmissiveColor);
#endif

        UNITY_APPLY_FOG(input.fogCoord, c);
        return c;
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "Base" }
			Fog { Mode Off } // For some reason fog works anyway. Is this some legacy thing that doesn't do anything?
			Cull [_CullMode]
            HLSLPROGRAM
			#pragma multi_compile _ PERSPECTIVE_CORRECTION_ON
			#pragma multi_compile _ VERTEX_PRECISION_ON
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ VIEWMODEL_ON
            #pragma multi_compile _ REFLECTIONS_ON
            #pragma multi_compile _ EMISSION_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }

    CustomEditor "Retro3D_SurfaceEditor"
}