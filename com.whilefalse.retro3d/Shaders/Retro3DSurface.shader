Shader "Retro3D/Surface"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _MainTex_ST("Texture Offset", Vector) = (1, 1, 0, 0)
		_Emissive ("Emissive Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1)
		_EmissiveColor("Emissive Tint", Color) = (0.0, 0.0, 0.0)
        _AlphaClip("Clip Threshold", Float) = 0.5
        [Toggle(VIEWMODEL_ON)] _ViewModel("Viewmodel Rendered", Float) = 0
        [Toggle(REFLECTIONS_ON)] _Reflections("Reflections", Float) = 0
        _ReflTex("Reflection Specular Texture", 2D) = "white" {}
        _ReflColor("Reflection Specular Color", Color) = (1, 1, 1, 0.5)
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

    CBUFFER_START(UnityPerMaterial)
		half4 _Color;
		half4 _EmissiveColor;
		half4 _ReflColor;
    CBUFFER_END

	struct Attributes
	{
		float4 position : POSITION;
		float2 texcoord : TEXCOORD0;
		float2 texcoord_lightmap : TEXCOORD1;
	};

	struct Varyings
	{
		float4 position : SV_POSITION;		
		float3 normal : NORMAL0;
		float3 viewDir : NORMAL1;

		PERSP_TEXCOORD texcoord : TEXCOORD0;
		PERSP_TEXCOORD texcoord_lightmap : TEXCOORD1;

		float3 worldPos : COLOR1;

		UNITY_FOG_COORDS(2)
	};

	Varyings Vertex(Attributes input)
	{
		Retro3DVertex vertexData = RetroLitVertex(input.position);

		Varyings output;

		output.worldPos = mul(unity_ObjectToWorld, input.position).xyz;

		output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
		output.texcoord_lightmap = input.texcoord_lightmap.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		output.position = vertexData.position;
		output.normal = vertexData.normal;
		output.viewDir = vertexData.viewDir;

#ifndef VIEWMODEL_ON
		UNITY_TRANSFER_FOG(output, output.position);
#endif
		return output;
	}

	half4 Fragment(Varyings input) : SV_Target
	{
		Retro3DSurface surface;

		float2 uv = input.texcoord;
		surface.diffuse = tex2D(_MainTex, uv) * _Color;
#ifdef EMISSION_ON
		surface.emission = tex2D(_Emissive, uv) * _EmissiveColor;
#else
		surface.emission = float4(0, 0, 0, 1);
#endif

#ifdef REFLECTIONS_ON
		float4 refl = tex2D(_ReflTex, uv) * _ReflColor;
		surface.specular = refl;
		surface.glossiness = refl.a;
#else
		surface.specular = float4(0, 0, 0, 1);
		surface.glossiness = 0;
#endif

		half4 litSurfaceColor = RetroLitSurface(surface, input.texcoord_lightmap, input.normal, input.viewDir, input.worldPos);

#ifndef VIEWMODEL_ON
		UNITY_APPLY_FOG(input.fogCoord, litSurfaceColor);
#endif
		return litSurfaceColor;
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
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }

    CustomEditor "WhileFalse.Retro3D.Editor.Retro3D_SurfaceEditor"
}