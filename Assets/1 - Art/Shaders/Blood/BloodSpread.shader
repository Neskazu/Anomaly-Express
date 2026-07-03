Shader "Custom/BloodSpreadAnime"
{
    Properties
    {
        [Header(Anime Style Colors)]
        _BaseColor ("Blood Base Color", Color) = (0.7, 0.05, 0.05, 1.0)
        _DarkColor ("Blood Dark Color", Color) = (0.2, 0.01, 0.01, 1.0)
        _HighlightColor ("Blood Highlight Color", Color) = (1.0, 0.2, 0.2, 1.0)
        _SpecularColor ("Specular Highlight Color", Color) = (1.0, 0.9, 0.9, 1.0)
        
        [Header(Shapes Control)]
        _ColorNoiseScale ("Color Noise Scale", Float) = 5.0
        _DarkEdgeWidth ("Dark Edge Shadow Width", Range(0, 0.2)) = 0.07
        _DarkEdgeThreshold ("Dark Edge Shadow Cutoff", Range(0, 1)) = 0.55
        _DarkBlobThreshold ("Dark Blobs Cutoff", Range(0, 1)) = 0.65
        _LightBlobThreshold ("Light Blobs Cutoff", Range(0, 1)) = 0.6
        
        [Header(Spread Control)]
        _Flow ("Flow (Spread Amount)", Range(0, 1)) = 0.0
        _MaxRadius ("Max Puddle Radius", Float) = 0.45
        _SourcePoint ("Source UV (X, Y)", Vector) = (0.5, 0.5, 0.0, 0.0)
        _SpreadScale ("Spread Scale (X, Y)", Vector) = (1.0, 1.0, 0.0, 0.0)
        
        [Header(Shape and Viscosity)]
        _NoiseScale ("Noise Scale", Float) = 8.0
        _Distortion ("Distortion Strength", Float) = 0.25
        
        [Header(Volume and 3D Effect)]
        _Thickness ("Liquid Edge Thickness (Normals)", Float) = 40.0
        _VertDisplacement ("Vertex Displacement", Float) = 0.0
        _Glossiness ("Specular Sharpness (Anime Cutoff)", Range(0.0, 1.0)) = 0.9
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
        }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile _ FORWARD_PLUS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 normalWS : TEXCOORD3;
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _DarkColor;
                half4 _HighlightColor;
                half4 _SpecularColor;
                float _ColorNoiseScale;
                float _DarkEdgeWidth;
                float _DarkEdgeThreshold;
                float _DarkBlobThreshold;
                float _LightBlobThreshold;
                float _Flow;
                float _MaxRadius;
                float4 _SourcePoint;
                float4 _SpreadScale;
                float _NoiseScale;
                float _Distortion;
                float _Thickness;
                float _VertDisplacement;
                float _Glossiness;
            CBUFFER_END

            float2 random2(float2 st){
                st = float2( dot(st,float2(127.1,311.7)),
                             dot(st,float2(269.5,183.3)) );
                return -1.0 + 2.0*frac(sin(st)*43758.5453123);
            }

            float noise(float2 st) {
                float2 i = floor(st);
                float2 f = frac(st);
                float2 u = f*f*(3.0-2.0*f);
                return lerp( lerp( dot( random2(i + float2(0.0,0.0) ), f - float2(0.0,0.0) ),
                                   dot( random2(i + float2(1.0,0.0) ), f - float2(1.0,0.0) ), u.x),
                             lerp( dot( random2(i + float2(0.0,1.0) ), f - float2(0.0,1.0) ),
                                   dot( random2(i + float2(1.0,1.0) ), f - float2(1.0,1.0) ), u.x), u.y);
            }

            float fbm(float2 st) {
                float value = 0.0;
                float amplitude = 0.5;
                float2 shift = float2(100.0, 100.0);
                float c = cos(0.5);
                float s = sin(0.5);
                float2x2 rot = float2x2(c, -s, s, c);
                
                for (int i = 0; i < 4; ++i) {
                    value += amplitude * noise(st);
                    st = mul(rot, st) * 2.0 + shift;
                    amplitude *= 0.5;
                }
                return value;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float2 noiseUV = input.uv * _NoiseScale;
                float warpX = fbm(noiseUV);
                float warpY = fbm(noiseUV + float2(5.2, 1.3));
                float2 distortedUV = input.uv + float2(warpX, warpY) * _Distortion;
                
                float2 spreadVec = (distortedUV - _SourcePoint.xy) * _SpreadScale.xy;
                float dist = length(spreadVec) - fbm(noiseUV * 2.5) * (_Distortion * 0.4);
                float currentSpread = _Flow * _MaxRadius;
                
                float edge0 = currentSpread - 0.02;
                float edge1 = currentSpread + 0.0001;
                float h = (1.0 - smoothstep(edge0, edge1, dist)) * step(dist, currentSpread);
                
                float3 posOS = input.positionOS.xyz + (input.normalOS * h * _VertDisplacement);
                
                output.positionCS = TransformObjectToHClip(posOS);
                output.positionWS = TransformObjectToWorld(posOS);
                output.uv = input.uv;
                
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                
                float2 noiseUV = uv * _NoiseScale;
                float warpX = fbm(noiseUV);
                float warpY = fbm(noiseUV + float2(5.2, 1.3)); 
                float2 distortedUV = uv + float2(warpX, warpY) * _Distortion;
                
                float2 spreadVec = (distortedUV - _SourcePoint.xy) * _SpreadScale.xy;
                float dist = length(spreadVec) - fbm(noiseUV * 2.5) * (_Distortion * 0.4);
                float currentSpread = _Flow * _MaxRadius;
                
                // --- 1. Геометрия лужи ---
                float bevelWidth = 0.02;
                float edge0 = currentSpread - bevelWidth;
                float edge1 = currentSpread + 0.0001;
                float baseHeight = 1.0 - smoothstep(edge0, edge1, dist);
                
                float2 shapeUV = distortedUV * _ColorNoiseScale;
                
                // --- 2. ЛОГИКА ТЕНИ: ДИСКРЕТНЫЕ ОСТРОВА НА КРАЯХ ---
                // Выделяем зону у самого края лужи
                float nearEdge = step(currentSpread - _DarkEdgeWidth, dist);
                
                // Генерируем крупный независимый шум для теневых сегментов
                float edgeNoise = fbm(shapeUV * 0.8 + float2(142.1, -55.4)) * 0.5 + 0.5;
                
                // Жёсткий отсек: тень на краю появится ТОЛЬКО там, где шум преодолел порог.
                // Никаких сплошных линий. Это дает рваные, разрозненные дуги по периметру.
                float edgeShadowMask = nearEdge * step(_DarkEdgeThreshold, edgeNoise);
                
                // Независимые кляксы в центре лужи
                float centerDarkBlobs = step(_DarkBlobThreshold, fbm(shapeUV + float2(44.1, -12.2)) * 0.5 + 0.5);
                
                // Объединяем тёмные элементы в один векторный слой
                float finalDarkForm = saturate(edgeShadowMask + centerDarkBlobs);
                
                // --- 3. СВЕТЛЫЕ ФОРМЫ (Блики/Капли где угодно) ---
                float lightBlobs = step(_LightBlobThreshold, fbm(shapeUV * 1.3 + float2(-25.8, 88.3)) * 0.5 + 0.5);
                float finalHighlightForm = lightBlobs * step(dist, currentSpread - 0.01);
                
                // --- 4. Наложение слоёв (Cel Shading) ---
                half3 albedo = _BaseColor.rgb;
                albedo = lerp(albedo, _DarkColor.rgb, finalDarkForm);
                albedo = lerp(albedo, _HighlightColor.rgb, finalHighlightForm);
                
                // --- 5. Объем и Сел-Спекуляр ---
                float heightMap = baseHeight + (finalDarkForm * 0.02) + (finalHighlightForm * 0.01);
                float3 normalTS = normalize(float3(-ddx(heightMap) * _Thickness, -ddy(heightMap) * _Thickness, 1.0));
                float3 normalWS = normalize(normalTS.x * input.tangentWS + normalTS.y * input.bitangentWS + normalTS.z * input.normalWS);
                
                if (dist > currentSpread || _Flow <= 0.001) discard;

                float3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                half3 ambient = SampleSH(normalWS);
                half3 diffuseLight = ambient;
                half3 specularLight = half3(0, 0, 0);

                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                    Light mainLight = GetMainLight(shadowCoord);
                #else
                    Light mainLight = GetMainLight();
                #endif

                float NdotL = max(0.0, dot(normalWS, normalize(mainLight.direction)));
                float atten = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                float lit = step(0.01, NdotL * atten);
                
                diffuseLight += mainLight.color * lit;
                
                float3 halfVector = normalize(mainLight.direction + viewDir);
                float NdotH = max(0.0, dot(normalWS, halfVector));
                float animeSpecular = step(_Glossiness, NdotH) * lit;
                specularLight += _SpecularColor.rgb * mainLight.color * animeSpecular;

                #if defined(_ADDITIONAL_LIGHTS)
                    uint pixelLightCount = GetAdditionalLightsCount();
                    LIGHT_LOOP_BEGIN(pixelLightCount)
                        Light addLight = GetAdditionalLight(lightIndex, input.positionWS);
                        float addNdotL = max(0.0, dot(normalWS, normalize(addLight.direction)));
                        float addAtten = addLight.distanceAttenuation * addLight.shadowAttenuation;
                        float addLit = step(0.01, addNdotL * addAtten);
                        
                        diffuseLight += addLight.color * addLit;
                        
                        float3 addHalfVec = normalize(addLight.direction + viewDir);
                        float addNdotH = max(0.0, dot(normalWS, addHalfVec));
                        specularLight += _SpecularColor.rgb * addLight.color * step(_Glossiness, addNdotH) * addLit;
                    LIGHT_LOOP_END
                #endif

                half3 finalColor = (albedo * diffuseLight) + specularLight;
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
        
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}