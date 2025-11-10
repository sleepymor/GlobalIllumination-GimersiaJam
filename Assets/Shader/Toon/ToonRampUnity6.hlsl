void ToonShading_float(
    in float3 Normal, 
    in float ToonRampSmoothness, 
    in float4 ClipSpacePos, 
    in float3 WorldPos, 
    in float3 ToonRampTinting,
    in float ToonRampOffset, 
    in float ToonRampOffsetPoint, 
    in float Ambient,
    out float3 ToonRampOutput, 
    out float3 Direction)
{
    #ifdef SHADERGRAPH_PREVIEW
        ToonRampOutput = float3(0.5, 0.5, 0);
        Direction = float3(0.5, 0.5, 0);
    #else

        // SHADOW COORD
        #if SHADOWS_SCREEN
            float4 shadowCoord = ComputeScreenPos(ClipSpacePos);
        #else
            float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
        #endif 

        // MAIN LIGHT
        #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS)
            Light light = GetMainLight(shadowCoord);
        #else
            Light light = GetMainLight();
        #endif

        float d = saturate(dot(Normal, light.direction) * 0.5 + 0.5);
        float toonRamp = smoothstep(ToonRampOffset, ToonRampOffset + ToonRampSmoothness, d);
        toonRamp *= light.shadowAttenuation;

        // FORWARD LIGHTS LOOP
        float3 extraLights = float3(0, 0, 0);
        uint lightsCount = GetAdditionalLightsCount();

        InputData inputData = (InputData)0;
        inputData.positionWS = WorldPos;
        inputData.normalWS = Normal;
        inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(WorldPos);

        float4 screenPos = float4(ClipSpacePos.x, (_ScaledScreenParams.y - ClipSpacePos.y), 0, 0);
        inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(screenPos);

        LIGHT_LOOP_BEGIN(lightsCount)
            Light aLight = GetAdditionalLight(lightIndex, WorldPos, float4(1,1,1,1));
            float dp = saturate(dot(Normal, aLight.direction) * 0.5 + 0.5);
            float ramp = smoothstep(ToonRampOffsetPoint, ToonRampOffsetPoint + ToonRampSmoothness, dp);
            float3 attenuatedColor = aLight.color * aLight.distanceAttenuation * aLight.shadowAttenuation;
            extraLights += ramp * attenuatedColor;
        LIGHT_LOOP_END

        // FINAL COLOR CALCULATION
        float3 directionalLight = light.color * (toonRamp + ToonRampTinting);

        // AMBIENT DIFFUSE (softened)
        float3 ambientLight = Ambient * (0.2 + ToonRampTinting);

        ToonRampOutput = saturate(directionalLight + ambientLight + extraLights);

        // DIRECTION OUT
        Direction = normalize(light.direction);
    #endif
}
