Shader "Custom/URP_StencilObject_Lit"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Render Face (Cull)", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Cull [_Cull]

        Pass
        {
            Name "StencilObjectForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Stencil
            {
                Ref 2
                Comp [_StencilComp]
                Pass Keep
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes 
            { 
                float4 positionOS : POSITION; 
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings 
            { 
                float4 positionCS : SV_POSITION; 
                float3 positionWS : TEXCOORD0;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD1;
                float fogCoord : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half3 albedo = texColor.rgb * _BaseColor.rgb;
                half alpha = texColor.a * _BaseColor.a;

                half3 normalWS = normalize(input.normalWS);

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half NdotL = max(0.0, dot(normalWS, mainLight.direction)) 
                + max(0.0, dot(-normalWS, mainLight.direction));
                half3 lighting = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation * NdotL);
                
                half3 ambient = SampleSH(normalWS);
                
                half3 finalColor = albedo * (lighting + ambient);

                finalColor = MixFog(finalColor, input.fogCoord);

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}