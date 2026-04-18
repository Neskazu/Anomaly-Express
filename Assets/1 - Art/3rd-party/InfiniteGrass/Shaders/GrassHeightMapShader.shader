Shader "InfiniteGrass/GrassHeightMapShader_WithMask"
{
    //Можно еще убрать логику slope 
    Properties
    {
        _GrassMask("Grass Mask", 2D) = "white" {}
        _MaskThreshold("Mask Threshold", Range(0,1)) = 0.5
        _MaskTiling("Mask Tiling (UV)", Vector) = (1,1,0,0)
        _MaskWorldScale("Mask World Scale (XZ)", Vector) = (0.1,0.1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _GrassMask;
            float4 _MaskTiling;
            float4 _MaskWorldScale;
            float _MaskThreshold;

            struct appdata
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0; 
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 color : TEXCOORD0;  
                float2 maskUV : TEXCOORD1; 
                float3 worldPos : TEXCOORD2;
            };

            float2 _BoundsYMinMax;

            float Remap(float In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float rChannel = Remap(worldPos.y, _BoundsYMinMax, float2(0, 1));

                float vertexMask = v.color.r;

                float slopeMask = dot(normalize(mul((float3x3)unity_ObjectToWorld, v.normal)), float3(0,1,0));
                slopeMask = saturate(slopeMask); 

                float slopeThreshold = 0.6;
                float gChannel = (vertexMask > 0.5 && slopeMask >= slopeThreshold) ? 1.0 : 0.0;

                o.color = float2(rChannel, gChannel);

                float2 maskUV_fromUV = v.uv * _MaskTiling.xy;
                o.maskUV = maskUV_fromUV;

                o.worldPos = worldPos;

                return o;
            }

            float2 frag (v2f i) : SV_Target
            {
                float maskSample = tex2D(_GrassMask, i.maskUV).r;

                float rChannel = i.color.x;
                float gChannel = i.color.y;

                gChannel = gChannel * smoothstep(_MaskThreshold - 0.05, _MaskThreshold + 0.05, maskSample);

                return float2(rChannel, gChannel);
            }
            ENDCG
        }
    }
}

///alt
//       Shader "InfiniteGrass/GrassHeightMapShader_NoFlicker"
// {
//     Properties
//     {
//     }
//     SubShader
//     {
//         Tags { "RenderType"="Opaque" }

//         Pass
//         {
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag

//             #include "UnityCG.cginc"

//             struct appdata
//             {
//                 float4 vertex : POSITION;
//                 half4 color  : COLOR;
//                 float3 normal : NORMAL;
//             };

//             struct v2f
//             {
//                 float4 vertex : SV_POSITION;
//                 float4 data   : TEXCOORD0; // x = rChannel, y = slopeFactor, z = vertexMask, w = slopeMask
//                 float2 wp_xz  : TEXCOORD1; // world position XZ (for stable noise)
//             };

//             float2 _BoundsYMinMax;

//             float Remap(float In, float2 InMinMax, float2 OutMinMax)
//             {
//                 return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
//             }

//             // ---------------- Integer / uint cell hash (stable)
//             // Wang hash + mixing constants; returns uint pseudo-random
//             uint wangHash(uint x)
//             {
//                 x = (x ^ 61u) ^ (x >> 16);
//                 x *= 9u;
//                 x = x ^ (x >> 4);
//                 x *= 0x27d4eb2du;
//                 x = x ^ (x >> 15);
//                 return x;
//             }

//             float HashCell(uint2 cell)
//             {
//                 // combine cell coords into one uint, then hash
//                 // constants 73856093 / 19349663 are common coordinate hash primes
//                 uint h = cell.x * 73856093u ^ cell.y * 19349663u;
//                 h = wangHash(h);
//                 return (float)h / 4294967295.0; // normalize to [0,1]
//             }
//             // ----------------------------------------------------------------

//             v2f vert (appdata v)
//             {
//                 v2f o;
//                 o.vertex = UnityObjectToClipPos(v.vertex);

//                 float3 worldPos = mul(unity_ObjectToWorld, v.vertex);

//                 // нормализованная высота
//                 float rChannel = Remap(worldPos.y, _BoundsYMinMax, float2(0, 1));

//                 // vertex mask (0/1)
//                 float vertexMask = v.color.r;

//                 // slope in world space
//                 float slopeMask = dot(normalize(mul((float3x3)unity_ObjectToWorld, v.normal)), float3(0,1,0));
//                 slopeMask = saturate(slopeMask);

//                 // slope gradient (smooth)
//                 const float slopeMin = 0.45; // начало границы (регулируй)
//                 const float slopeMax = 0.85; // конец границы (регулируй)
//                 float slopeFactor = saturate((slopeMask - slopeMin) / (slopeMax - slopeMin));

//                 o.data = float4(rChannel, slopeFactor, vertexMask, slopeMask);
//                 o.wp_xz = worldPos.xz;

//                 return o;
//             }

//             float2 frag (v2f i) : SV_Target
//             {
//                 float rChannel = i.data.x;
//                 float slopeFactor = i.data.y;
//                 float vertexMask = i.data.z;

//                 // Если вёрт. маска выключена — ничего не растёт
//                 if (vertexMask <= 0.5)
//                     return float2(rChannel, 0.0);

//                 // Параметры шума (регулируй под масштаб сцены)
//                 const float cellSize0 = 1.5;   // low-frequency cell size (мировые единицы)
//                 const float cellSize1 = 0.35;  // high-frequency cell size (меньше -> мелкие пятна)
//                 const float mixHigh = 0.55;    // доля высокочастотного шума (0..1)

//                 // получение индексной клетки (uint2) — стабильно в мировых координатах
//                 uint2 cell0 = (uint2)floor(i.wp_xz / cellSize0);
//                 uint2 cell1 = (uint2)floor(i.wp_xz / cellSize1);

//                 // decorrelate second octave with offset
//                 cell1.x += 1337u;
//                 cell1.y += 777u;

//                 float n0 = HashCell(cell0);
//                 float n1 = HashCell(cell1);

//                 // смешивание октав
//                 float noise = lerp(n0, n1, mixHigh);

//                 // предельные случаи
//                 if (slopeFactor >= 0.9999)
//                     return float2(rChannel, 1.0);
//                 if (slopeFactor <= 0.0001)
//                     return float2(rChannel, 0.0);

//                 // стохастическое решение: spawn если noise < slopeFactor
//                 // Это даёт плавный разрежённый край
//                 float g = (noise < slopeFactor) ? 1.0 : 0.0;

//                 return float2(rChannel, g);
//             }
//             ENDCG
//         }
//     }
// }
