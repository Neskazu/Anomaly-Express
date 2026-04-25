Shader "Custom/URP_StencilMask"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Render Face (Cull)", Float) = 2
        _BaseMap ("Base Map (Alpha for Clip)", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-1" "RenderPipeline"="UniversalPipeline" }
        
        ColorMask 0
        ZWrite Off
        Cull [_Cull]

        Pass
        {
            Name "StencilMaskPass"
            
            Stencil
            {
                Ref 2
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes 
            { 
                float4 positionOS : POSITION; 
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings 
            { 
                float4 positionCS : SV_POSITION; 
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _Cutoff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a;
                clip(alpha - _Cutoff); 

                return half4(0,0,0,0);
            }
            ENDHLSL
        }
    }
}