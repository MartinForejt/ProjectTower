Shader "Custom/AnimatedWater"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.08, 0.25, 0.5, 0.7)
        _DeepColor ("Deep Color", Color) = (0.02, 0.08, 0.25, 1)
        _WaveSpeed ("Wave Speed", Float) = 1.5
        _WaveScale ("Wave Scale", Float) = 2.0
        _WaveHeight ("Wave Height", Float) = 0.03
        _FresnelPower ("Fresnel Power", Float) = 3.0
        _Glossiness ("Glossiness", Float) = 0.95
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _DeepColor;
                float _WaveSpeed;
                float _WaveScale;
                float _WaveHeight;
                float _FresnelPower;
                float _Glossiness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);

                // Multi-layer wave displacement
                float wave = sin(worldPos.x * _WaveScale + _Time.y * _WaveSpeed) * _WaveHeight;
                wave += sin(worldPos.z * _WaveScale * 1.3 + _Time.y * _WaveSpeed * 0.7) * _WaveHeight * 0.7;
                wave += sin((worldPos.x + worldPos.z) * _WaveScale * 0.8 + _Time.y * _WaveSpeed * 1.3) * _WaveHeight * 0.5;

                IN.positionOS.y += wave;

                // Wave normal from partial derivatives
                float dx = cos(worldPos.x * _WaveScale + _Time.y * _WaveSpeed) * _WaveScale * _WaveHeight
                         + cos((worldPos.x + worldPos.z) * _WaveScale * 0.8 + _Time.y * _WaveSpeed * 1.3) * _WaveScale * 0.8 * _WaveHeight * 0.5;
                float dz = cos(worldPos.z * _WaveScale * 1.3 + _Time.y * _WaveSpeed * 0.7) * _WaveScale * 1.3 * _WaveHeight * 0.7
                         + cos((worldPos.x + worldPos.z) * _WaveScale * 0.8 + _Time.y * _WaveSpeed * 1.3) * _WaveScale * 0.8 * _WaveHeight * 0.5;

                OUT.normalWS = normalize(float3(-dx, 1.0, -dz));
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - OUT.positionWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);
                float3 viewDir = normalize(IN.viewDirWS);

                // Fresnel - more reflective at grazing angles
                float fresnel = pow(1.0 - saturate(dot(normal, viewDir)), _FresnelPower);

                // Water color blend
                float4 waterColor = lerp(_DeepColor, _Color, fresnel);

                // Main light with shadows
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                float NdotL = saturate(dot(normal, mainLight.direction));
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float spec = pow(saturate(dot(normal, halfDir)), _Glossiness * 256.0);

                float3 lighting = waterColor.rgb * (NdotL * mainLight.color * mainLight.shadowAttenuation * 0.6 + 0.4);
                float3 specular = mainLight.color * spec * mainLight.shadowAttenuation;

                // Additional lights (dynamic point lights from projectiles, torches, etc.)
                uint lightCount = GetAdditionalLightsCount();
                for (uint i = 0u; i < lightCount; i++)
                {
                    Light light = GetAdditionalLight(i, IN.positionWS);
                    float addNdotL = saturate(dot(normal, light.direction));
                    float3 addHalf = normalize(light.direction + viewDir);
                    float addSpec = pow(saturate(dot(normal, addHalf)), _Glossiness * 256.0);
                    float atten = light.distanceAttenuation * light.shadowAttenuation;

                    lighting += waterColor.rgb * addNdotL * light.color * atten * 0.6;
                    specular += light.color * addSpec * atten;
                }

                float alpha = lerp(waterColor.a, 1.0, fresnel * 0.5);

                return half4(lighting + specular, alpha);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}
