Shader "RoyalSmash/Citadel Energy Reference"
{
    Properties
    {
        [MainColor] _BaseColor ("Deep Energy Color / Runtime Tint", Color) = (0.015,0.20,0.92,1)
        [HDR] _GlowColor ("Filament Glow", Color) = (0.025,1.15,1.8,1)
        [HDR] _CoreColor ("Filament Intersections", Color) = (0.3,1.65,2.25,1)
        _NoiseScale ("Energy Scale", Range(1,18)) = 8
        _FlowSpeed ("Flow Speed", Range(0,1)) = 0.13
        _RimStrength ("Glass Rim", Range(0,1)) = 0.34
        _SpecularStrength ("Surface Gloss", Range(0,1)) = 0.28
        _FilamentWidth ("Filament Width", Range(0.004,0.045)) = 0.012
        _FilamentStrength ("Filament Brightness", Range(0,2)) = 0.88
        _DepthStrength ("Interior Depth", Range(0,1)) = 0.65
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Back
        ZWrite On
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _GlowColor;
            half4 _CoreColor;
            float _NoiseScale;
            float _FlowSpeed;
            half _RimStrength;
            half _SpecularStrength;
            half _FilamentWidth;
            half _FilamentStrength;
            half _DepthStrength;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardEnergy"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex EnergyVertex
            #pragma fragment EnergyFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct EnergyAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct EnergyVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float3 local : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            EnergyVaryings EnergyVertex(EnergyAttributes input)
            {
                EnergyVaryings output = (EnergyVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.local = input.positionOS.xyz;
                output.fogFactor = ComputeFogFactor(position.positionCS.z);
                output.shadowCoord = GetShadowCoord(position);
                return output;
            }

            float EnergyHash(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }
            float EnergyNoise(float3 p)
            {
                float3 cell = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float lower = lerp(
                    lerp(EnergyHash(cell), EnergyHash(cell + float3(1,0,0)), f.x),
                    lerp(EnergyHash(cell + float3(0,1,0)), EnergyHash(cell + float3(1,1,0)), f.x), f.y);
                float upper = lerp(
                    lerp(EnergyHash(cell + float3(0,0,1)), EnergyHash(cell + float3(1,0,1)), f.x),
                    lerp(EnergyHash(cell + float3(0,1,1)), EnergyHash(cell + float3(1,1,1)), f.x), f.y);
                return lerp(lower, upper, f.z);
            }
            float EnergyLine(float field, float width)
            {
                float aa = max(fwidth(field), 0.002);
                // Subpixel lines lose energy rather than turning into a noisy full-pixel grid.
                return (1.0 - smoothstep(width, width + aa, abs(field))) * saturate(width / aa);
            }

            half4 EnergyFragment(EnergyVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float facing = saturate(dot(normalWS, viewWS));
                float3 viewOS = SafeNormalize(TransformWorldToObjectDir(viewWS));
                float3 p = input.local * _NoiseScale;
                // Object-space coordinates stay attached to moving, rotating and Mega-scaled meshes.
                // A small view offset suggests a layer under glass; no ray march or material textures.
                p += viewOS * (1.0 - facing) * _DepthStrength * 0.12;
                float3 drift = _Time.y * _FlowSpeed * float3(0.32, -0.64, 0.21);
                float low = EnergyNoise(p + drift);
                float detail = EnergyNoise(p * 1.91 - drift * 0.57 + float3(11.3, 7.1, 19.7));
                float field = low + (detail - 0.5) * 0.34 - 0.51;
                float branchField = detail + (low - 0.5) * 0.24 - 0.55;
                float mainArc = EnergyLine(field, _FilamentWidth);
                float branchArc = EnergyLine(branchField, _FilamentWidth * 0.60) * smoothstep(0.38, 0.70, low);
                float filaments = saturate(mainArc + branchArc * 0.62);
                float halo = (1.0 - smoothstep(0.015, 0.075 + fwidth(field), abs(field))) * 0.10;
                float intersection = mainArc * branchArc;
                float core = pow(saturate((low - 0.43) * 1.65), 3.0) * smoothstep(0.48, 0.85, detail);

                // Existing blue uses its authored palette. Runtime non-blue tints (MegaBall,
                // paint effects) recolor emission as well as the body instead of staying cyan.
                half peak = max(max(_BaseColor.r, _BaseColor.g), _BaseColor.b);
                half3 tint = _BaseColor.rgb / max(peak, 0.001h);
                half bluePalette = smoothstep(0.04h, 0.22h, _BaseColor.b - max(_BaseColor.r, _BaseColor.g));
                half glowPeak = max(max(_GlowColor.r, _GlowColor.g), _GlowColor.b);
                half corePeak = max(max(_CoreColor.r, _CoreColor.g), _CoreColor.b);
                half3 glowColor = lerp(lerp(tint, half3(1,1,1), 0.16h) * glowPeak, _GlowColor.rgb, bluePalette);
                half3 coreColor = lerp(lerp(tint, half3(1,1,1), 0.58h) * corePeak, _CoreColor.rgb, bluePalette);

                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    float4 shadowCoord = input.shadowCoord;
                #else
                    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                #endif
                Light light = GetMainLight(shadowCoord);
                float attenuation = light.distanceAttenuation * light.shadowAttenuation;
                float diffuse = saturate(dot(normalWS, light.direction)) * attenuation;
                float depth = lerp(1.0, 0.56 + facing * 0.44, _DepthStrength);
                half3 color = _BaseColor.rgb * (0.16 + low * 0.28 + detail * 0.08) * depth * (0.65 + diffuse * 0.35);
                color += glowColor * (filaments * _FilamentStrength + halo + core * 0.20);
                color += coreColor * (intersection * 0.38 + core * 0.40);
                float rim = pow(1.0 - facing, 4.0) * _RimStrength;
                color += glowColor * rim * 0.85;

                float3 halfDirection = SafeNormalize(light.direction + viewWS);
                float specular = saturate(dot(normalWS, halfDirection));
                float glint = pow(specular, 112.0) + pow(specular, 24.0) * 0.12;
                color += light.color * glint * _SpecularStrength * 2.2 * attenuation;
                float fog = InitializeInputDataFog(float4(input.positionWS, 1), input.fogFactor);
                // Opaque, as before: existing shrink/destruction controls silhouette visibility.
                return half4(MixFog(color, fog), _BaseColor.a);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ColorMask 0
            ZTest LEqual
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
