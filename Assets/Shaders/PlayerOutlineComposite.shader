Shader "Hidden/PlayerOutlineComposite"
{
    Properties
    {
        _MaskTex ("Mask Texture", 2D) = "black" {}
        _OutlineColor ("Outline Color", Color) = (0.4, 0.8, 1.0, 1.0)
        _OutlineSize ("Outline Size", Float) = 4.0
        _FillOpacity ("Fill Opacity", Float) = 0.7
        _Alpha ("Alpha", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "PlayerOutlineComposite"
            ZTest Always
            ZWrite Off
            Cull Off
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            float4 _MaskTex_TexelSize;

            half4 _OutlineColor;
            float _OutlineSize;
            float _FillOpacity;
            float _Alpha;

            // 최대 8픽셀 반경까지 지원 (인스펙터의 OutlineSize로 실제 반경 제어)
            #define MAX_RADIUS 8

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                float mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv).r;

                bool isOccluded = mask > 0.75;  // = 1.0 (가려진 플레이어)
                bool isAnyPlayer = mask > 0.25; // = 0.5 또는 1.0 (보이거나 가려진 모두)

                // 같은 루프에서 가려진 픽셀(dilatedOccluded)과 보이는 플레이어 픽셀(dilatedVisible)을 동시에 감지
                // dilatedVisible이 1이면 보이는 플레이어가 근처에 있으므로 해당 배경 픽셀에는 outline을 그리지 않음
                float dilatedOccluded = 0;
                float dilatedVisible  = 0;
                float radiusSq = _OutlineSize * _OutlineSize;

                [loop]
                for (int x = -MAX_RADIUS; x <= MAX_RADIUS; x++)
                {
                    [loop]
                    for (int y = -MAX_RADIUS; y <= MAX_RADIUS; y++)
                    {
                        if ((float)(x * x + y * y) > radiusSq) continue;
                        float2 sampleUV = uv + float2((float)x, (float)y) * _MaskTex_TexelSize.xy;
                        float s = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, sampleUV).r;
                        dilatedOccluded = max(dilatedOccluded, step(0.75, s));
                        // 0.25 < s < 0.75 → 보이는 플레이어 픽셀 (mask=0.5)
                        dilatedVisible  = max(dilatedVisible,  step(0.25, s) * (1.0 - step(0.75, s)));
                    }
                }

                // 외곽선: 가려진 영역 팽창 내부 + 플레이어 픽셀 아님 + 보이는 플레이어 근처 아님
                // → 보이는 하체 주변으로 outline이 번지는 현상 제거
                float outlineMask = dilatedOccluded * (isAnyPlayer ? 0.0 : 1.0) * (1.0 - dilatedVisible);

                half3 finalColor = sceneColor.rgb;

                // 검은 실루엣 fill (가려진 영역)
                float fillAmount = (isOccluded ? 1.0 : 0.0) * _FillOpacity * _Alpha;
                finalColor = lerp(finalColor, half3(0, 0, 0), fillAmount);

                // 연파랑 외곽선 (Additive → 발광 효과)
                finalColor += _OutlineColor.rgb * _OutlineColor.a * outlineMask * _Alpha;

                return half4(finalColor, sceneColor.a);
            }
            ENDHLSL
        }
    }
}
