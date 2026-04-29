Shader "Hidden/PlayerOutlineMask"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        // ZTest 대신 셰이더에서 _CameraDepthTexture를 직접 비교해 가려짐 판별
        // depth buffer 공유 없이 _maskRT에만 안전하게 기록
        Pass
        {
            Name "PlayerMask"
            ZTest Always
            ZWrite Off
            Cull Back
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                // ComputeScreenPos가 플랫폼별 y-flip을 처리해 올바른 UV 제공
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float sceneDepth = SampleSceneDepth(screenUV);
                float pixelDepth = input.positionCS.z;

                // UNITY_REVERSED_Z (DX/Metal): 1=near, 0=far
                // pixelDepth < sceneDepth → 현재 픽셀이 씬보다 뒤 = 가려짐
                #if defined(UNITY_REVERSED_Z)
                    bool isOccluded = pixelDepth < sceneDepth - 0.0001;
                #else
                    bool isOccluded = pixelDepth > sceneDepth + 0.0001;
                #endif

                // 가려진 영역 = 1.0, 보이는 영역 = 0.5 (composite에서 outline ring 계산에 활용)
                return isOccluded ? half4(1, 0, 0, 0) : half4(0.5, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
