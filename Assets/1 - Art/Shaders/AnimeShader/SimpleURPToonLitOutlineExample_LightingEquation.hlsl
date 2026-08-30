// For more information, visit -> https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample

// This file is intented for you to edit and experiment with different lighting equation.
// Add or edit whatever code you want here

// #pragma once is a safe guard best practice in almost every .hlsl (need Unity2020 or up), 
// doing this can make sure your .hlsl's user can include this .hlsl anywhere anytime without producing any multi include conflict
#pragma once

half3 ShadeGI(ToonSurfaceData surfaceData, ToonLightingData lightingData)
{
    // “¬Œ… Œ–»√»Õ¿À‹Õ€…  Œƒ (ŒÚ‡·‡Ú˚‚‡ÂÚ IndirectLight ÍÓÂÍÚÌÓ)
    half3 averageSH = SampleSH(0);
    averageSH = max(_IndirectLightMinColor, averageSH);
    half indirectOcclusion = lerp(1, surfaceData.occlusion, 0.5);
    return averageSH * indirectOcclusion;
}

// Most important part: lighting equation, edit it according to your needs, write whatever you want here, be creative!
// This function will be used by all direct lights (directional/point/spot)
half3 ShadeSingleLight(ToonSurfaceData surfaceData, ToonLightingData lightingData, Light light, bool isAdditionalLight)
{
    half3 N = lightingData.normalWS;
    half3 L = light.direction;

    half NoL = dot(N, L);
    half distanceAttenuation = min(4, light.distanceAttenuation);

    half selfShadowArea = smoothstep(_CelShadeMidPoint - _CelShadeSoftness, _CelShadeMidPoint + _CelShadeSoftness, NoL);
    selfShadowArea *= surfaceData.occlusion;
    selfShadowArea = _IsFace ? lerp(0.5, 1, selfShadowArea) : selfShadowArea;

    half3 litOrShadowColor = lerp(_ShadowMapColor, 1, selfShadowArea);

    half castShadow = lerp(1, light.shadowAttenuation, _ReceiveShadowMappingAmount);

    half3 lightAttenuationRGB = litOrShadowColor * distanceAttenuation * castShadow;

    return saturate(light.color) * lightAttenuationRGB * (isAdditionalLight ? 0.55 : 1);
}

half3 ShadeEmission(ToonSurfaceData surfaceData, ToonLightingData lightingData)
{
    half3 emissionResult = lerp(surfaceData.emission, surfaceData.emission * surfaceData.albedo, _EmissionMulByBaseColor); // optional mul albedo
    return emissionResult;
}

half3 CompositeAllLightResults(half3 indirectResult, half3 mainLightResult, half3 additionalLightSumResult,
                               half3 emissionResult, ToonSurfaceData surfaceData, ToonLightingData lightingData)
{
    half3 rawLightSum = max(indirectResult, mainLightResult + additionalLightSumResult);
    
    float lightIntensity = saturate(max(rawLightSum.x, max(rawLightSum.y, rawLightSum.z)));
    
    emissionResult *= smoothstep(0.0, 0.2, lightIntensity);

    half3 diff_orig = surfaceData.albedo * rawLightSum;
    half3 N = lightingData.normalWS;
    half3 V = lightingData.viewDirectionWS;
    half3 L = _LightDirection;
    half3 H = normalize(L + V);
    half NoL = saturate(dot(N, L));
    half NoH = saturate(dot(N, H));
    half3 F0 = lerp(0.04, surfaceData.albedo, surfaceData.metallic);
    half shininess = surfaceData.smoothness * 127 + 1;
    
    half3 spec_orig = F0 * pow(NoH, shininess) * NoL;
    
    half3 origResult = diff_orig + spec_orig + emissionResult;
    
    if (surfaceData.metallic <= 0.0)
        return origResult;

    // metal
    half lum = dot(surfaceData.albedo, half3(0.3, 0.6, 0.1));
    float lightLum = max(rawLightSum.x, max(rawLightSum.y, rawLightSum.z));
    
    float diffRamp = smoothstep(0.1, 0.25, lum * lightLum) + step(0.5, lum * lightLum) * 2.0;
    half3 diff_toon = surfaceData.albedo * (diffRamp / 3.0);
    
    float ramp = smoothstep(0.6, 0.8, NoH) + smoothstep(0.8, 0.95, NoH) * 4.0;
    ramp = saturate(ramp / 3.0);
    half3 spec_toon = F0 * ramp * NoL;
    
    float fEdge = pow(1.0 - saturate(dot(N, V)), 2.0);
    spec_toon += F0 * fEdge * 0.5;
    
    float rim = pow(saturate(1.0 - dot(N, V)), 3.0);
    diff_toon += surfaceData.albedo * rim * 0.2;

    half3 toonResult = diff_toon + spec_toon + emissionResult;

    half m = saturate(surfaceData.metallic);
    return lerp(origResult, toonResult, m);
}
