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

        // --- RIPPLE ---
        _RippleColor("Ripple Color", Color) = (1,1,1,1)
        _RippleIntensity("Ripple Intensity", Range(0, 3)) = 1
        _RippleThickness("Ripple Thickness", Float) = 0.15
        _RippleDistortion("Ripple Distortion", Range(0, 1)) = 0.08
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #define SMOOTHSTEP_AA 0.01
            #define MAX_RIPPLES 8

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 alphaBlend(float4 top, float4 bottom)
            {
                float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
                float alpha = top.a + bottom.a * (1 - top.a);
                return float4(color, alpha);
            }

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPosition = ComputeScreenPos(o.vertex);
                o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);
                o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float4 _FoamColor;

            float _DepthMaxDistance;
            float _FoamMaxDistance;
            float _SurfaceNoiseCutoff;
            float _SurfaceDistortionAmount;

            float2 _SurfaceNoiseScroll;

            sampler2D _CameraDepthTexture;
            
            // --- RIPPLE VARIABLES ---
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

            float4 frag (v2f i) : SV_Target
            {
                float screenW = max(i.screenPosition.w, 0.00001);
                float2 screenUV = saturate(i.screenPosition.xy / screenW);

                float existingDepth01 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
                float existingDepthLinear = LinearEyeDepth(existingDepth01);

                float depthDifference = max(0, existingDepthLinear - screenW + 0.01);

                float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);
                float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01);
                float rippleMask = 0;
                float2 rippleDistortion = 0;

                [unroll]
                for (int r = 0; r < MAX_RIPPLES; r++)
                {
                    if (r >= _RippleCount) break;

                    float4 rd = _RippleData[r];

                    float age = _Time.y - rd.z;

                    float strengthT = saturate(
                        (rd.w - _RippleMinStrengthForLifetime) /
                        max(_RippleMaxStrengthForLifetime - _RippleMinStrengthForLifetime, 0.0001)
                    );

                    float rippleLifetime = lerp(
                        _RippleMinLifetime,
                        _RippleMaxLifetime,
                        strengthT
                    );

                    float life01 = saturate(1.0 - age / rippleLifetime);
                    float active = life01 * life01;

                    float2 toRipple = i.worldPos.xz - rd.xy;
                    float dist = length(toRipple);

                    float radius = age * _RippleRadius;
                    float edge = abs(dist - radius);

                    float aa = 0.05;
                    float ring = 1.0 - smoothstep(
                        _RippleThickness - aa,
                        _RippleThickness + aa,
                        edge
                    );

                    float ripple = ring * active * rd.w;
                    rippleMask = max(rippleMask, ripple);

                    float2 dir = toRipple / max(dist, 0.0001);
                    rippleDistortion += dir * ripple * _RippleDistortion;
                }

                rippleMask = saturate(rippleMask * _RippleIntensity);
                float foamDepthDifference01 = saturate(depthDifference / _FoamMaxDistance);
                float shoreCutoff = foamDepthDifference01 * _SurfaceNoiseCutoff;

                float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;
                distortSample += rippleDistortion; 

                float2 noiseUV = float2((i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x, 
                                        (i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y);
                float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;
                float shoreFoam = smoothstep(shoreCutoff - SMOOTHSTEP_AA, shoreCutoff + SMOOTHSTEP_AA, surfaceNoiseSample);
                float totalFoamAlpha = saturate(shoreFoam + rippleMask);
                float4 finalFoamColor = lerp(_FoamColor, _RippleColor, rippleMask);
                finalFoamColor.a *= totalFoamAlpha;

                return alphaBlend(finalFoamColor, waterColor);
            }
            ENDCG
        }
    }
}