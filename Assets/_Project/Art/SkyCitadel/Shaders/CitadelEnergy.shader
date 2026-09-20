Shader "RoyalSmash/Citadel Energy"
{
    Properties
    {
        _BaseColor ("Azure", Color) = (0.015,0.23,0.95,1)
        _GlowColor ("Lightning", Color) = (0.04,1.6,2.2,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float3 positionWS:TEXCOORD1; float3 local:TEXCOORD2; };
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _GlowColor;
            CBUFFER_END
            Varyings vert(Attributes v)
            {
                Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                o.positionWS=TransformObjectToWorld(v.positionOS.xyz); o.local=v.positionOS.xyz; return o;
            }
            half4 frag(Varyings i):SV_Target
            {
                float3 p=i.local*15;
                float flow=sin(p.x+sin(p.y*1.3)+_Time.y*.32)*sin(p.y+p.z*.8+cos(p.x));
                float vein=pow(saturate(1-abs(flow)*13),4);
                float facing=saturate(dot(normalize(i.normalWS),normalize(GetWorldSpaceViewDir(i.positionWS))));
                float rim=pow(1-facing,2);
                float cloud=.5+.5*sin(p.x*.45+sin(p.y*.7)+p.z*.5);
                half3 col=_BaseColor.rgb*(.65+.65*cloud)+_GlowColor.rgb*(vein*.7+rim*.45);
                return half4(col,1);
            }
            ENDHLSL
        }
    }
}
