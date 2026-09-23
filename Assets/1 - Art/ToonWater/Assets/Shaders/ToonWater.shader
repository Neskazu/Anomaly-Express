Shader "Roystan/Toon/Water Tut"
{
    Properties
    {
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1

        _FoamColor("Foam Color", Color) = (1,1,1,1)

        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _SurfaceNoiseScroll("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)
        _SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.9

        _SurfaceDistortion("Surface Distortion", 2D) = "white" {}
        _SurfaceDistortionAmount("Surface Distortion Amount", Range(0, 1)) = 0.27

        _FoamMaxDistance("Foam Maximum Distance", Float) = 0.2

        _RippleColor("Ripple Color", Color) = (1,1,1,1)
        _RippleIntensity("Ripple Intensity", Range(0, 3)) = 1
        _RippleThickness("Ripple Thickness", Float) = 0.15
        _RippleDistortion("Ripple Distortion", Range(0, 1)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM

            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_nicest

            #include "UnityCG.cginc"

            #define SMOOTHSTEP_AA 0.01
            #define MAX_RIPPLES 8

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 noiseUV : TEXCOORD0;
                float2 distortUV : TEXCOORD1;
                float4 screenPosition : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            sampler2D _SurfaceNoise;
            float4 _SurfaceNoise_ST;

            sampler2D _SurfaceDistortion;
            float4 _SurfaceDistortion_ST;

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float4 _FoamColor;

            float _DepthMaxDistance;
            float _FoamMaxDistance;
            float _SurfaceNoiseCutoff;
            float _SurfaceDistortionAmount;

            float2 _SurfaceNoiseScroll;

            float4 _RippleColor;
            float _RippleIntensity;
            float _RippleThickness;
            float _RippleDistortion;

            float _RippleRadius;

            float _RippleMinLifetime;
            float _RippleMaxLifetime;
            float _RippleMinStrengthForLifetime;
            float _RippleMaxStrengthForLifetime;

            float4 _RippleData[MAX_RIPPLES];
            float _RippleCount;


            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.screenPosition = ComputeScreenPos(o.vertex);

                // Сохраняем настоящую eye-space depth поверхности воды.
                // Это безопаснее, чем использовать screenPosition.w
                // как глубину.
                COMPUTE_EYEDEPTH(o.screenPosition.z);

                o.distortUV = TRANSFORM_TEX(
                    v.uv,
                    _SurfaceDistortion
                );

                o.noiseUV = TRANSFORM_TEX(
                    v.uv,
                    _SurfaceNoise
                );

                o.worldPos =
                    mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }


            float4 frag(v2f i) : SV_Target
            {
                // --------------------------------------------------
                // DEPTH
                // --------------------------------------------------

                float existingDepth01 =
                    SAMPLE_DEPTH_TEXTURE_PROJ(
                        _CameraDepthTexture,
                        UNITY_PROJ_COORD(i.screenPosition)
                    );

                float existingDepthLinear =
                    LinearEyeDepth(existingDepth01);

                // Eye-space depth самой поверхности воды.
                float waterSurfaceDepth =
                    i.screenPosition.z;

                // Если depth texture отсутствует / вернула 0,
                // не даём ей превращать всю воду в пену.
                float depthValid =
                    step(0.0001, existingDepth01);

                existingDepthLinear =
                    lerp(
                        waterSurfaceDepth,
                        existingDepthLinear,
                        depthValid
                    );

                float depthDifference =
                    max(
                        0,
                        existingDepthLinear -
                        waterSurfaceDepth
                    );


                // --------------------------------------------------
                // WATER COLOR
                // --------------------------------------------------

                float waterDepthDifference01 =
                    saturate(
                        depthDifference /
                        max(
                            _DepthMaxDistance,
                            0.0001
                        )
                    );

                float4 waterColor =
                    lerp(
                        _DepthGradientShallow,
                        _DepthGradientDeep,
                        waterDepthDifference01
                    );


                // --------------------------------------------------
                // RIPPLES
                // --------------------------------------------------

                float rippleMask = 0;
                float2 rippleDistortion = 0;

                [unroll]
                for (int r = 0; r < MAX_RIPPLES; r++)
                {
                    float enabled =
                        step(
                            float(r),
                            _RippleCount - 0.5
                        );

                    float4 rd =
                        _RippleData[r];

                    float age =
                        _Time.y - rd.z;

                    float strengthT =
                        saturate(
                            (
                                rd.w -
                                _RippleMinStrengthForLifetime
                            ) /
                            max(
                                _RippleMaxStrengthForLifetime -
                                _RippleMinStrengthForLifetime,
                                0.0001
                            )
                        );

                    float rippleLifetime =
                        lerp(
                            _RippleMinLifetime,
                            _RippleMaxLifetime,
                            strengthT
                        );

                    rippleLifetime =
                        max(
                            rippleLifetime,
                            0.0001
                        );

                    float life01 =
                        saturate(
                            1.0 -
                            age /
                            rippleLifetime
                        );

                    float active =
                        life01 * life01;

                    float2 toRipple =
                        i.worldPos.xz -
                        rd.xy;

                    float dist =
                        length(toRipple);

                    float radius =
                        age *
                        _RippleRadius;

                    float edge =
                        abs(
                            dist -
                            radius
                        );

                    float aa = 0.05;

                    float ring =
                        1.0 -
                        smoothstep(
                            _RippleThickness - aa,
                            _RippleThickness + aa,
                            edge
                        );

                    float ripple =
                        ring *
                        active *
                        rd.w *
                        enabled;

                    rippleMask =
                        max(
                            rippleMask,
                            ripple
                        );

                    float2 dir =
                        toRipple /
                        max(
                            dist,
                            0.0001
                        );

                    rippleDistortion +=
                        dir *
                        ripple *
                        _RippleDistortion;
                }

                rippleMask =
                    saturate(
                        rippleMask *
                        _RippleIntensity
                    );


                // --------------------------------------------------
                // FOAM
                // --------------------------------------------------

                float foamDepthDifference01 =
                    saturate(
                        depthDifference /
                        max(
                            _FoamMaxDistance,
                            0.0001
                        )
                    );

                float shoreCutoff =
                    foamDepthDifference01 *
                    _SurfaceNoiseCutoff;


                // --------------------------------------------------
                // DISTORTION
                // --------------------------------------------------

                float2 distortSample =
                    (
                        tex2D(
                            _SurfaceDistortion,
                            i.distortUV
                        ).xy * 2 - 1
                    ) *
                    _SurfaceDistortionAmount;

                distortSample +=
                    rippleDistortion;


                // --------------------------------------------------
                // SURFACE NOISE
                // --------------------------------------------------

                float2 noiseUV =
                    float2(
                        i.noiseUV.x +
                        _Time.y *
                        _SurfaceNoiseScroll.x +
                        distortSample.x,

                        i.noiseUV.y +
                        _Time.y *
                        _SurfaceNoiseScroll.y +
                        distortSample.y
                    );

                float surfaceNoiseSample =
                    tex2D(
                        _SurfaceNoise,
                        noiseUV
                    ).r;


                // --------------------------------------------------
                // SHORE FOAM
                // --------------------------------------------------

                float shoreFoam =
                    smoothstep(
                        shoreCutoff -
                        SMOOTHSTEP_AA,

                        shoreCutoff +
                        SMOOTHSTEP_AA,

                        surfaceNoiseSample
                    );

                // Критическая защита:
                // если depth texture невалидна,
                // береговая пена полностью отключается.
                shoreFoam *= depthValid;


                // --------------------------------------------------
                // TOTAL FOAM
                // --------------------------------------------------

                float totalFoamAlpha =
                    saturate(
                        shoreFoam +
                        rippleMask
                    );


                // --------------------------------------------------
                // FOAM COLOR
                // --------------------------------------------------

                float4 finalFoamColor =
                    lerp(
                        _FoamColor,
                        _RippleColor,
                        rippleMask
                    );


                // --------------------------------------------------
                // FINAL COLOR
                // --------------------------------------------------

                float3 finalRGB =
                    lerp(
                        waterColor.rgb,
                        finalFoamColor.rgb,
                        totalFoamAlpha
                    );

                float finalAlpha =
                    lerp(
                        waterColor.a,
                        1.0,
                        totalFoamAlpha
                    );

                return float4(
                    finalRGB,
                    finalAlpha
                );
            }

            ENDCG
        }
    }
}