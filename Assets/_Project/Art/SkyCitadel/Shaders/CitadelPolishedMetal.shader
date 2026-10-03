// Broad object-anchored polish layered over existing URP lighting and reflections.
Shader "RoyalSmash/Citadel Polished Metal"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,0.73,0.22,1)
        _Metallic("Metallic", Range(0,1)) = 0.64
        _Smoothness("Smoothness", Range(0,1)) = 0.74
        [HDR] _EmissionColor("Legacy Emission Color", Color) = (0,0,0,1)
        _EmissionMap("Legacy Emission Map", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5

        _ShadeStrength("Broad Tonal Depth", Range(0,1)) = 1
        _ShadeTint("Shadow RGB Multipliers (linear)", Vector) = (0.18,0.095,0.025,0)
        _HighlightTint("Polish Highlight Tint", Color) = (1,0.92,0.67,1)
        _FaceContrast("UV Face Polish Blend", Range(0,1)) = 0.82
        _HighlightBlend("Highlight Tint Blend", Range(0,1)) = 0.58
        _PolishAxisOS("Object Polish Axis", Vector) = (0.82,0.57,0.05,0)
        _PolishFrequency("Broad Polish Cycles Per Metre", Range(0.1,2)) = 0.58
        _PolishPhase("Polish Phase (radians)", Float) = 0.35
        _PolishNormalBend("Broad Shading Normal Bend", Range(0,0.15)) = 0.075
        _PolishSpecular("Additional Lit Polish", Range(0,0.65)) = 0.55
        _StripWidth("Broad Highlight Width", Range(0.15,0.65)) = 0.42
        _CurvatureStrength("Rounded Edge Accent", Range(0,0.2)) = 0.075

        // Keep the existing opaque Lit material's render state and editor values.
        [HideInInspector] _WorkflowMode("Workflow", Float) = 1
        [HideInInspector] _Surface("Surface", Float) = 0
        [HideInInspector] _AlphaClip("Alpha Clip", Float) = 0
        [HideInInspector] _Cull("Cull", Float) = 2
        [HideInInspector] _ZWrite("ZWrite", Float) = 1
        [HideInInspector] _ReceiveShadows("Receive Shadows", Float) = 1
        [HideInInspector] _SpecularHighlights("Specular Highlights", Float) = 1
        [HideInInspector] _EnvironmentReflections("Environment Reflections", Float) = 1
        [HideInInspector] _SmoothnessTextureChannel("Smoothness Channel", Float) = 0
        [HideInInspector] _SrcBlend("Src Blend", Float) = 1
        [HideInInspector] _DstBlend("Dst Blend", Float) = 0
        [HideInInspector] _SrcBlendAlpha("Src Alpha Blend", Float) = 1
        [HideInInspector] _DstBlendAlpha("Dst Alpha Blend", Float) = 0
        [HideInInspector] _AlphaToMask("Alpha To Mask", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "UniversalMaterialType"="Lit" "Queue"="Geometry" }
        Cull [_Cull]
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // Supplies SampleAlbedoAlpha/Alpha used by the URP ShadowCaster/DepthOnly includes.
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
        // One layout shared by every pass/keyword variant; no per-pass LitInput CBUFFER.
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _EmissionColor;
            half4 _HighlightTint;
            float4 _ShadeTint;
            float4 _PolishAxisOS;
            half _Metallic, _Smoothness, _Cutoff, _ShadeStrength;
            half _FaceContrast, _HighlightBlend;
            float _PolishFrequency, _PolishPhase;
            half _PolishNormalBend, _PolishSpecular, _StripWidth, _CurvatureStrength;
            half _WorkflowMode, _Surface, _AlphaClip, _Cull;
            half _ZWrite, _ReceiveShadows, _SpecularHighlights, _EnvironmentReflections;
            half _SmoothnessTextureChannel, _SrcBlend, _DstBlend, _SrcBlendAlpha;
            half _DstBlendAlpha, _AlphaToMask;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            ZWrite [_ZWrite]
            AlphaToMask [_AlphaToMask]
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex MetalVertex
            #pragma fragment MetalFragment
            #pragma multi_compile_instancing
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct MetalAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct MetalVaryings
            {
                float4 positionCS : SV_POSITION;
                // xy: transformed albedo UV; zw: original per-face UV for stable part-sized polish.
                float4 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                half4 fogAndVertexLight : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 5);
                float3 objectVectorWS : TEXCOORD6;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            MetalVaryings MetalVertex(MetalAttributes input)
            {
                MetalVaryings o = (MetalVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                VertexPositionInputs p = GetVertexPositionInputs(input.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.uv = float4(TRANSFORM_TEX(input.uv, _BaseMap), input.uv);
                // Object anchored, world-sized vector: no translation or screen-space swimming.
                o.objectVectorWS = TransformObjectToWorldDir(input.positionOS.xyz, false);
                o.fogAndVertexLight = half4(ComputeFogFactor(p.positionCS.z), VertexLighting(p.positionWS, o.normalWS));
                o.shadowCoord = GetShadowCoord(p);
                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, o.lightmapUV);
                OUTPUT_SH(o.normalWS, o.vertexSH);
                return o;
            }
            half4 MetalFragment(MetalVaryings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half4 texel = SampleAlbedoAlpha(i.uv.xy, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
                half alpha = Alpha(texel.a, _BaseColor, _Cutoff);
                float3 geometryNormal = normalize(i.normalWS);
                float3 axisWS = TransformObjectToWorldDir(_PolishAxisOS.xyz);
                float phase = dot(i.objectVectorWS, axisWS) * _PolishFrequency * TWO_PI + _PolishPhase;
                float wave = 0.5 + 0.5 * sin(phase);
                float3 tangentWS = SafeNormalize(axisWS - geometryNormal * dot(axisWS, geometryNormal));
                // Gentle virtual bow for planar caps; geometry, shadows and bounds remain unchanged.
                float3 shadingNormal = normalize(geometryNormal + tangentWS * (_PolishNormalBend * sin(phase + 0.8)));

                InputData data = (InputData)0;
                data.positionWS = i.positionWS;
                data.positionCS = i.positionCS;
                data.normalWS = shadingNormal;
                data.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    data.shadowCoord = i.shadowCoord;
                #else
                    data.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                #endif
                data.fogCoord = InitializeInputDataFog(float4(i.positionWS, 1), i.fogAndVertexLight.x);
                data.vertexLighting = i.fogAndVertexLight.yzw;
                data.bakedGI = SAMPLE_GI(i.lightmapUV, i.vertexSH, shadingNormal);
                data.shadowMask = SAMPLE_SHADOWMASK(i.lightmapUV);
                data.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);

                half4 shadowMask = CalculateShadowMask(data);
                // Same light, shadow mask and AO APIs as UniversalFragmentPBR in the installed URP.
                SurfaceData surface = (SurfaceData)0;
                surface.occlusion = 1;
                AmbientOcclusionFactor ao = CreateAmbientOcclusionFactor(data, surface);
                Light mainLight = GetMainLight(data, shadowMask, ao);
                half facing = saturate(dot(geometryNormal, mainLight.direction));
                // Flat box faces retain UV0 [0,1] after mesh combination. Finish therefore
                // varies across each small cap as well as the broad combined metal surface.
                // Saturation keeps lathe UVs outside [0,1] bounded without wrapping seams.
                half faceV = saturate(i.uv.w);
                half faceBody = smoothstep(0.07h, 0.93h, faceV);
                half objectTone = saturate(0.40h * wave + 0.60h * facing);
                half faceTone = saturate(0.72h * faceBody + 0.28h * facing);
                half tone = lerp(objectTone, faceTone, saturate(_FaceContrast));
                half3 shadeMultiplier = lerp(saturate(_ShadeTint.rgb), half3(1,1,1), tone);
                half3 baseAlbedo = saturate(texel.rgb * _BaseColor.rgb);
                surface.albedo = baseAlbedo * lerp(half3(1,1,1), shadeMultiplier, _ShadeStrength);
                surface.metallic = saturate(_Metallic);
                surface.smoothness = saturate(_Smoothness);
                #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
                    surface.smoothness *= texel.a;
                #endif
                surface.normalTS = half3(0,0,1);
                surface.alpha = alpha;
                // Preserve legitimate legacy emission; polish never writes into emission.
                surface.emission = SampleEmission(i.uv.xy, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
                half4 color = UniversalFragmentPBR(data, surface);

                #ifndef _SPECULARHIGHLIGHTS_OFF
                    float stripWave = 0.5 + 0.5 * sin(phase + 1.15);
                    half objectStrip = smoothstep(1.0 - _StripWidth, 1.0, stripWave);
                    // Smooth shoulders and a short plateau make a broad reflective band,
                    // not a one-pixel edge. Fillet UV seams still require rendered review.
                    half faceStrip = smoothstep(0.43h, 0.70h, faceV) * (1.0h - smoothstep(0.76h, 0.95h, faceV));
                    half strip = lerp(objectStrip, faceStrip, saturate(_FaceContrast));
                    float3 halfDirection = SafeNormalize(mainLight.direction + data.viewDirectionWS);
                    // Broad directional lobe; ordinary URP PBR still supplies the sharp specular/probe.
                    half lobe = pow(saturate(dot(shadingNormal, halfDirection)), 4.0);
                    float dx = length(ddx(i.positionWS)), dy = length(ddy(i.positionWS));
                    float curvature = (length(ddx(geometryNormal)) + length(ddy(geometryNormal))) / max(dx + dy, 0.0001);
                    half edge = smoothstep(0.35, 4.0, curvature) * _CurvatureStrength;
                    half attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                    half intensity = (_PolishSpecular * strip + edge) * lobe * facing * attenuation;
                    #ifdef _LIGHT_LAYERS
                        if (!IsMatchingLightLayer(mainLight.layerMask, GetMeshRenderingLayer())) intensity = 0;
                    #endif
                    // Metallic chroma stays visible. No white additive rim or final-color clipping.
                    half3 polishColor = lerp(baseAlbedo, saturate(_HighlightTint.rgb), saturate(_HighlightBlend));
                    color.rgb += polishColor * mainLight.color * intensity;
                #endif
                return half4(MixFog(color.rgb, data.fogCoord), alpha);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            // PC Forward+ SSAO requests the depth/normal prepass. Unsupported normal maps
            // are rejected by the revision helper, so retain the actual geometry normals.
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask R
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
