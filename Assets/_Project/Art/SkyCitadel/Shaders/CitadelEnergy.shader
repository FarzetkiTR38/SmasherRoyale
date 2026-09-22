Shader "RoyalSmash/Citadel Energy"
{
    Properties
    {
        _BaseColor ("Deep Azure", Color) = (0.015,0.20,0.92,1)
        [HDR] _GlowColor ("Cyan Clouds", Color) = (0.025,1.15,1.8,1)
        [HDR] _CoreColor ("Soft Energy Cores", Color) = (0.3,1.65,2.25,1)
        _NoiseScale ("Cloud Scale", Range(1,18)) = 8
        _FlowSpeed ("Flow Speed", Range(0,1)) = 0.13
        _RimStrength ("Glass Rim", Range(0,1)) = 0.34
        _SpecularStrength ("Surface Gloss", Range(0,1)) = 0.28
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float3 positionWS:TEXCOORD1; float3 local:TEXCOORD2; };
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _GlowColor;
            half4 _CoreColor;
            float _NoiseScale;
            float _FlowSpeed;
            half _RimStrength;
            half _SpecularStrength;
            CBUFFER_END
            Varyings vert(Attributes v)
            {
                Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                o.positionWS=TransformObjectToWorld(v.positionOS.xyz); o.local=v.positionOS.xyz; return o;
            }
            float CloudHash(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float CloudNoise(float3 p)
            {
                float3 cell = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float lower = lerp(
                    lerp(CloudHash(cell), CloudHash(cell + float3(1,0,0)), f.x),
                    lerp(CloudHash(cell + float3(0,1,0)), CloudHash(cell + float3(1,1,0)), f.x), f.y);
                float upper = lerp(
                    lerp(CloudHash(cell + float3(0,0,1)), CloudHash(cell + float3(1,0,1)), f.x),
                    lerp(CloudHash(cell + float3(0,1,1)), CloudHash(cell + float3(1,1,1)), f.x), f.y);
                return lerp(lower, upper, f.z);
            }

            half4 frag(Varyings i):SV_Target
            {
                float3 normalWS = normalize(i.normalWS);
                float3 viewWS = SafeNormalize(GetWorldSpaceViewDir(i.positionWS));
                float3 viewOS = SafeNormalize(TransformWorldToObjectDir(viewWS));
                float facing = saturate(dot(normalWS, viewWS));
                float3 p = i.local * _NoiseScale;
                float3 drift = _Time.y * _FlowSpeed * float3(0.32, -0.64, 0.21);
                float3 warp = sin(p.yzx * 0.61 + drift.zxy) * 0.52;
                float3 flow = p + warp + drift;

                // Three cloud octaves, plus one view-offset sample to suggest depth.
                // No ray marching, texture reads, transparency or extra render passes.
                float cloud = CloudNoise(flow) * 0.57;
                cloud += CloudNoise(flow * 2.03 - drift * 1.4 + 7.1) * 0.29;
                cloud += CloudNoise(flow * 4.09 + drift * 1.7 + 19.4) * 0.14;
                float depth = CloudNoise(p * 0.58 + viewOS * 0.85 - drift * 0.65 + 31.7);
                float mist = smoothstep(0.22, 0.76, cloud);
                float core = smoothstep(0.52, 0.77, cloud + (depth - 0.5) * 0.16);
                core *= core;

                // Broad, antialiased wisps keep small blocks from sparkling or aliasing.
                float edgeWidth = max(fwidth(cloud) * 1.5, 0.018);
                float wisps = 1.0 - smoothstep(0.012, 0.065 + edgeWidth, abs(cloud - 0.56));
                wisps *= smoothstep(0.24, 0.75, depth) * 0.22;
                float rim = pow(1.0 - facing, 3.0) * _RimStrength;
                float curvature = 0.58 + 0.42 * facing;

                half3 color = _BaseColor.rgb * (0.42 + mist * 0.80) * curvature;
                color += _GlowColor.rgb * (mist * 0.24 + depth * 0.07 + wisps + rim);
                color += _CoreColor.rgb * core * 0.88;

                Light mainLight = GetMainLight();
                float3 halfDirection = SafeNormalize(mainLight.direction + viewWS);
                float gloss = pow(saturate(dot(normalWS, halfDirection)), 64.0);
                color += mainLight.color * gloss * _SpecularStrength * half3(0.65, 0.88, 1.0);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
