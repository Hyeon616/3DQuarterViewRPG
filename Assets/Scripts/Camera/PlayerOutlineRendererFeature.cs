using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerOutlineRendererFeature : ScriptableRendererFeature
{
    [Serializable]
    public class OutlineSettings
    {
        public Color outlineColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
        [Range(1, 8)] public int outlineSize = 4;
        [Range(0f, 1f)] public float fillOpacity = 0.7f;
    }

    public OutlineSettings settings = new();

    private PlayerMaskRenderPass _maskPass;
    private PlayerOutlineCompositePass _compositePass;

    public override void Create()
    {
        _maskPass = new PlayerMaskRenderPass
        {
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques
        };

        _compositePass = new PlayerOutlineCompositePass(settings)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 셰이더 컴파일 타이밍 문제로 Create()에서 실패했을 경우 매 프레임 재시도
        if (!_maskPass.EnsureMaterial() || !_compositePass.EnsureMaterial()) return;

        if (PlayerOutlineController.ActiveControllers.Count == 0) return;

        float maxAlpha = 0f;
        foreach (var c in PlayerOutlineController.ActiveControllers)
            maxAlpha = Mathf.Max(maxAlpha, c.CurrentAlpha);

        if (maxAlpha < 0.001f) return;

        _compositePass.Setup(_maskPass, maxAlpha);
        renderer.EnqueuePass(_maskPass);
        renderer.EnqueuePass(_compositePass);
    }

    protected override void Dispose(bool disposing)
    {
        _maskPass?.Dispose();
        _compositePass?.Dispose();
    }

    // ─── Mask Pass ──────────────────────────────────────────────────────────

    class PlayerMaskRenderPass : ScriptableRenderPass
    {
        private RTHandle _maskRT;
        private Material _maskMaterial;

        public RTHandle MaskRT => _maskRT;

        public bool EnsureMaterial()
        {
            if (_maskMaterial != null) return true;
            _maskMaterial = CoreUtils.CreateEngineMaterial("Hidden/PlayerOutlineMask");
            return _maskMaterial != null;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.colorFormat = RenderTextureFormat.R8;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;

            RenderingUtils.ReAllocateIfNeeded(ref _maskRT, desc,
                FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_PlayerMaskRT");

            // depth buffer 공유 없이 maskRT만 타겟으로 설정
            // 셰이더가 _CameraDepthTexture를 직접 샘플링해 가려짐 판별
            ConfigureTarget(_maskRT);
            ConfigureClear(ClearFlag.Color, Color.black);
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_maskMaterial == null || _maskRT == null || _maskRT.rt == null) return;

            var cmd = CommandBufferPool.Get("PlayerOutline_Mask");

            foreach (var controller in PlayerOutlineController.ActiveControllers)
            {
                foreach (var rend in controller.Renderers)
                {
                    if (rend == null || !rend.enabled || !rend.gameObject.activeInHierarchy) continue;
                    int subMeshCount = rend.sharedMesh != null ? rend.sharedMesh.subMeshCount : 1;
                    for (int s = 0; s < subMeshCount; s++)
                        cmd.DrawRenderer(rend, _maskMaterial, s, 0);
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }

        public void Dispose()
        {
            _maskRT?.Release();
            CoreUtils.Destroy(_maskMaterial);
        }
    }

    // ─── Composite Pass ─────────────────────────────────────────────────────

    class PlayerOutlineCompositePass : ScriptableRenderPass
    {
        private readonly OutlineSettings _settings;
        private Material _compositeMaterial;
        private RTHandle _tempRT;
        private PlayerMaskRenderPass _maskPass;
        private float _alpha;

        private static readonly int MaskTexId          = Shader.PropertyToID("_MaskTex");
        private static readonly int MaskTexTexelSizeId = Shader.PropertyToID("_MaskTex_TexelSize");
        private static readonly int OutlineColorId     = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineSizeId      = Shader.PropertyToID("_OutlineSize");
        private static readonly int FillOpacityId      = Shader.PropertyToID("_FillOpacity");
        private static readonly int AlphaId            = Shader.PropertyToID("_Alpha");

        public PlayerOutlineCompositePass(OutlineSettings settings)
        {
            _settings = settings;
        }

        public bool EnsureMaterial()
        {
            if (_compositeMaterial != null) return true;
            _compositeMaterial = CoreUtils.CreateEngineMaterial("Hidden/PlayerOutlineComposite");
            return _compositeMaterial != null;
        }

        public void Setup(PlayerMaskRenderPass maskPass, float alpha)
        {
            _maskPass = maskPass;
            _alpha = alpha;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref _tempRT, desc,
                FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_PlayerOutlineTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_compositeMaterial == null || _maskPass?.MaskRT == null) return;

            // 스폰 직후 첫 프레임에 RT가 아직 준비되지 않았을 수 있으므로 null 검사
            if (_maskPass.MaskRT.rt == null || _tempRT == null || _tempRT.rt == null) return;

            RTHandle cameraColor = renderingData.cameraData.renderer.cameraColorTargetHandle;
            if (cameraColor == null || cameraColor.rt == null) return;

            var cmd = CommandBufferPool.Get("PlayerOutline_Composite");

            var maskRt = _maskPass.MaskRT.rt;
            _compositeMaterial.SetTexture(MaskTexId, _maskPass.MaskRT);
            _compositeMaterial.SetVector(MaskTexTexelSizeId, new Vector4(
                1f / maskRt.width, 1f / maskRt.height, maskRt.width, maskRt.height));
            _compositeMaterial.SetColor(OutlineColorId, _settings.outlineColor);
            _compositeMaterial.SetFloat(OutlineSizeId, _settings.outlineSize);
            _compositeMaterial.SetFloat(FillOpacityId, _settings.fillOpacity);
            _compositeMaterial.SetFloat(AlphaId, _alpha);

            // 씬 → tempRT (composite 적용) → 씬으로 복사
            Blitter.BlitCameraTexture(cmd, cameraColor, _tempRT, _compositeMaterial, 0);
            Blitter.BlitCameraTexture(cmd, _tempRT, cameraColor);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }

        public void Dispose()
        {
            _tempRT?.Release();
            CoreUtils.Destroy(_compositeMaterial);
        }
    }
}
