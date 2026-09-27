Shader "RoyalSmash/Citadel Floor"
{
    Properties
    {
        [MainTexture] _BaseMap("Marble, Gold and Sapphire Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        _MarbleMetallic("Marble Metallic", Range(0,1)) = 0.02
        _MarbleSmoothness("Marble Smoothness", Range(0,1)) = 0.62
        _GoldMetallic("Gold Metallic", Range(0,1)) = 0.70
        _GoldSmoothness("Gold Smoothness", Range(0,1)) = 0.76
        _BlueMetallic("Sapphire Metallic", Range(0,1)) = 0.25
        _BlueSmoothness("Sapphire Smoothness", Range(0,1)) = 0.86
        _MicroHeight("Micro Relief (Metres)", Range(0,0.002)) = 0.0006
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "UniversalMaterialType"="Lit" "Queue"="Geometry" }
        Cull Back
        ZWrite On
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        // Identical material layout in every pass keeps this shader SRP Batcher compatible.
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _MarbleMetallic, _MarbleSmoothness, _GoldMetallic, _GoldSmoothness;
            half _BlueMetallic, _BlueSmoothness;
            float _MicroHeight;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex FloorVertex
            #pragma fragment FloorFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct FloorAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct FloorVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                half4 fogAndVertexLight : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 5);
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            FloorVaryings FloorVertex(FloorAttributes input)
            {
                FloorVaryings o = (FloorVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                VertexPositionInputs p = GetVertexPositionInputs(input.positionOS.xyz);
                o.positionCS = p.positionCS; o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                o.fogAndVertexLight = half4(ComputeFogFactor(p.positionCS.z), VertexLighting(p.positionWS, o.normalWS));
                o.shadowCoord = GetShadowCoord(p);
                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, o.lightmapUV);
                OUTPUT_SH(o.normalWS, o.vertexSH);
                return o;
            }
            half4 FloorFragment(FloorVaryings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                // sRGB albedo is decoded by Unity; classify untinted linear chroma so tint cannot move the inlays.
                half3 texel = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb;
                half peak = max(max(texel.r, texel.g), texel.b);
                half saturation = (peak - min(min(texel.r, texel.g), texel.b)) / max(peak, 0.02h);
                half blue = smoothstep(0.02h, 0.13h, (texel.b - texel.r) / max(peak, 0.02h)) * smoothstep(0.08h, 0.28h, saturation);
                half gold = smoothstep(0.10h, 0.30h, (min(texel.r, texel.g) - texel.b) / max(peak, 0.02h)) * smoothstep(0.30h, 0.58h, saturation) * (1.0h - blue);
                SurfaceData surface = (SurfaceData)0;
                surface.albedo = texel * _BaseColor.rgb;
                surface.metallic = lerp(lerp(_MarbleMetallic, _GoldMetallic, gold), _BlueMetallic, blue);
                surface.smoothness = lerp(lerp(_MarbleSmoothness, _GoldSmoothness, gold), _BlueSmoothness, blue);
                surface.normalTS = half3(0,0,1); surface.occlusion = 1; surface.alpha = 1;

                // One albedo fetch: world-space surface derivatives produce submillimetre relief without a normal map.
                float height = clamp(_MicroHeight, 0.0, 0.002) * (dot(texel, half3(0.2126,0.7152,0.0722)) * 0.18 - gold * 0.35 - blue * 0.10);
                float3 n = normalize(i.normalWS), dpdx = ddx(i.positionWS), dpdy = ddy(i.positionWS);
                float3 rx = cross(dpdy, n), ry = cross(n, dpdx);
                float determinant = dot(dpdx, rx);
                float3 gradient = (ddx(height) * rx + ddy(height) * ry) * ((determinant < 0 ? -1.0 : 1.0) / max(abs(determinant), 1e-8));
                gradient *= min(1.0, 0.12 * rsqrt(max(dot(gradient, gradient), 1e-8)));
                InputData data = (InputData)0;
                data.positionWS = i.positionWS; data.positionCS = i.positionCS;
                data.normalWS = normalize(n - gradient);
                data.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    data.shadowCoord = i.shadowCoord;
                #else
                    data.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                #endif
                data.fogCoord = InitializeInputDataFog(float4(i.positionWS, 1), i.fogAndVertexLight.x);
                data.vertexLighting = i.fogAndVertexLight.yzw;
                data.bakedGI = SAMPLE_GI(i.lightmapUV, i.vertexSH, data.normalWS);
                data.shadowMask = SAMPLE_SHADOWMASK(i.lightmapUV);
                data.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                half4 color = UniversalFragmentPBR(data, surface);
                return half4(MixFog(color.rgb, data.fogCoord), 1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZTest LEqual
            ColorMask 0
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ColorMask R
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
