Shader "Custom/SpeedTree8_Transparent"
{
    Properties
    {
        // SpeedTree8 원본 셰이더와 동일한 프로퍼티 이름 사용
        _MainTex ("Base (RGB) Transparency (A)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.33
        _AlphaClipThreshold ("Alpha Clip Threshold", Range(0,1)) = 0.33

        _AlphaMultiplier ("Alpha Multiplier", Range(0, 1)) = 1.0

        [Toggle(EFFECT_HUE_VARIATION)] _HueVariationKwToggle("Hue Variation", Float) = 0
        _HueVariationColor ("Hue Variation Color", Color) = (1.0,0.5,0.0,0.1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "AlphaTest"
            "RenderType" = "TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "DisableBatching" = "LODFading"
        }
        LOD 200

        // Forward Lit 패스 - 디더링 투명도
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma shader_feature_local EFFECT_HUE_VARIATION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _HueVariationColor;
                half _AlphaMultiplier;
                half _Cutoff;
                half _AlphaClipThreshold;
            CBUFFER_END

            // 4x4 Bayer dithering matrix
            static const float bayerMatrix[16] = {
                0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                12.0/16.0, 4.0/16.0, 14.0/16.0,  6.0/16.0,
                3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
                15.0/16.0, 7.0/16.0, 13.0/16.0,  5.0/16.0
            };

            float GetDitherThreshold(float2 screenPos)
            {
                int2 pixel = int2(fmod(screenPos, 4.0));
                int index = pixel.y * 4 + pixel.x;
                return bayerMatrix[index];
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
                half4 color : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = input.texcoord.xy;
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                #ifdef EFFECT_HUE_VARIATION
                    float3 treePos = float3(UNITY_MATRIX_M[0].w, UNITY_MATRIX_M[1].w, UNITY_MATRIX_M[2].w);
                    float hueVariationAmount = frac(treePos.x + treePos.y + treePos.z);
                    output.color.g = saturate(hueVariationAmount * _HueVariationColor.a);
                #endif

                return output;
            }

            half4 frag(Varyings input, half facing : VFACE) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 color = texColor * _Color;

                // Apply vertex color (AO in red channel)
                color.rgb *= input.color.r;

                // Hue variation
                #ifdef EFFECT_HUE_VARIATION
                    half3 shiftedColor = lerp(color.rgb, _HueVariationColor.rgb, input.color.g);
                    half maxBase = max(color.r, max(color.g, color.b));
                    half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
                    maxBase /= newMaxBase;
                    maxBase = maxBase * 0.5f + 0.5f;
                    shiftedColor.rgb *= maxBase;
                    color.rgb = saturate(shiftedColor);
                #endif

                // Base alpha from texture
                half baseAlpha = texColor.a;

                // Screen-space dithering for consistent transparency
                float2 screenPos = input.positionCS.xy;
                float ditherThreshold = GetDitherThreshold(screenPos);

                float effectiveCutoff = _Cutoff + (1.0 - _AlphaMultiplier) * (1.0 - _Cutoff);
                float ditherCutoff = lerp(effectiveCutoff, effectiveCutoff + ditherThreshold * (1.0 - _AlphaMultiplier) * 0.5, 1.0 - _AlphaMultiplier);

                clip(baseAlpha - ditherCutoff);

                // Flip normal for backface
                float3 normalWS = input.normalWS;
                normalWS = facing > 0 ? normalWS : -normalWS;
                normalWS = normalize(normalWS);

                // Lighting
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = mainLight.color * NdotL * mainLight.shadowAttenuation;
                half3 ambient = SampleSH(normalWS);

                #ifdef _ADDITIONAL_LIGHTS
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    half NdotL2 = saturate(dot(normalWS, light.direction));
                    diffuse += light.color * NdotL2 * light.distanceAttenuation * light.shadowAttenuation;
                }
                #endif

                half3 finalColor = color.rgb * (diffuse + ambient);
                finalColor = MixFog(finalColor, input.fogFactor);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // 원본 SpeedTree8 셰이더의 ShadowCaster 패스 사용
        UsePass "Universal Render Pipeline/Nature/SpeedTree8/ShadowCaster"

        // 원본 SpeedTree8 셰이더의 DepthOnly 패스 사용
        UsePass "Universal Render Pipeline/Nature/SpeedTree8/DepthOnly"

        // GBuffer pass for deferred (optional)
        UsePass "Universal Render Pipeline/Nature/SpeedTree8/GBuffer"

        // DepthNormals pass
        UsePass "Universal Render Pipeline/Nature/SpeedTree8/DepthNormals"
    }

    FallBack "Universal Render Pipeline/Nature/SpeedTree8"
}