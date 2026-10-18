#ifndef ENVIRONMENT_LIGHTING_INCLUDED
#define ENVIRONMENT_LIGHTING_INCLUDED

#if !defined(SHADERGRAPH_PREVIEW)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif

void GetMainLightShadow_float(
    float3 PositionWS,
    out float ShadowAttenuation)
{
#if defined(SHADERGRAPH_PREVIEW)
    ShadowAttenuation = 1.0;
#elif defined(UNIVERSAL_PIPELINE_CORE_INCLUDED)
    float4 shadowCoord = TransformWorldToShadowCoord(PositionWS);
    Light mainLight = GetMainLight(shadowCoord);
    ShadowAttenuation = mainLight.shadowAttenuation;
#else
	ShadowAttenuation = 1.0;
#endif
}

#endif