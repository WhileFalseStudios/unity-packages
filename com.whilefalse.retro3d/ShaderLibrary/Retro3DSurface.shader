Shader "Retro3D/Surface"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
		_Emissive ("Emissive Texture", 2D) = "black" {}
        _Color("Tint", Color) = (0.5, 0.5, 0.5)
		_EmissiveColor("Emissive Tint", Color) = (1.0, 1.0, 1.0)
    }

    HLSLINCLUDE

    #include "UnityCG.cginc"
	#include "Retro3D.cginc" 

    sampler2D _MainTex;
    float4 _MainTex_ST;
    half3 _Color;

	sampler2D _Emissive;
	float4 _Emissive_ST;
	half3 _EmissiveColor;

    struct Attributes
    {
        float4 position : POSITION;
        float2 texcoord : TEXCOORD0;
		//float3 normal : NORMAL;
		float2 texcoord_lightmap : TEXCOORD1;
    };

    struct Varyings
    {
        float4 position : SV_POSITION;
		float3 normal : NORMAL;

#ifdef PERSPECTIVE_CORRECTION_ON
		float2 texcoord : TEXCOORD0;
		float2 texcoord_lightmap : TEXCOORD1;
#else
        noperspective float2 texcoord : TEXCOORD0;
		noperspective float2 texcoord_lightmap : TEXCOORD1;
#endif
        UNITY_FOG_COORDS(2)
    };

    Varyings Vertex(Attributes input)
    { 
		Varyings output;
#ifdef VERTEX_PRECISION_ON
		float3 vp = UnityObjectToViewPos(input.position.xyz);
        vp = floor(vp * _VertexPrecision) / _VertexPrecision;
		output.position = RETRO_View2Clip(vp);
#else
		output.position = RETRO_Obj2Clip(input.position.xyz);
#endif                
        output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
		output.texcoord_lightmap = input.texcoord_lightmap.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		output.normal = normalize(UnityObjectToWorldNormal(input.position));
        UNITY_TRANSFER_FOG(output, output.position);
        return output;
    }

    half4 Fragment(Varyings input) : SV_Target
    {
        float2 uv = input.texcoord;
        //uv = floor(uv * 256) / 256;
        half4 c = tex2D(_MainTex, uv);
        //c = floor(c * 8) / 8;
        c.rgb *= _Color * 2;
#ifdef LIGHTMAP_ON
		c.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, input.texcoord_lightmap)).rgb;
#else
		c.rgb *= ShadeSH9(float4(input.normal, 1));
#endif

		c.rgb += tex2D(_Emissive, uv).rgb * _EmissiveColor;

        UNITY_APPLY_FOG(input.fogCoord, c);
        return c;
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "Base" }
			Fog { Mode Off }
			Cull Off
            HLSLPROGRAM
			#pragma multi_compile _ PERSPECTIVE_CORRECTION_ON
			#pragma multi_compile _ VERTEX_PRECISION_ON
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ VIEWMODEL_ON
            #pragma multi_compile_fog
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }
}
