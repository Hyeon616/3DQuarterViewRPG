using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraOcclusionController : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float characterHeight = 1.8f;
    [SerializeField] private float characterRadius = 0.3f;

    [Header("Transparency")]
    [SerializeField] private float transparentAlpha = 0.3f;
    [SerializeField] private float fadeSpeed = 5f;

    private Transform _target;
    private Camera _mainCamera;

    private Dictionary<Renderer, MaterialData> _affectedRenderers = new();
    private HashSet<Renderer> _currentFrameObstacles = new();
    private Shader _speedTreeTransparentShader;

    private struct MaterialData
    {
        public Material[] OriginalMaterials;
        public Material[] TransparentMaterials;
        public float CurrentAlpha;
    }

    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
        _mainCamera = Camera.main;
        LoadCustomShader();
    }

    private void LoadCustomShader()
    {
        if (_speedTreeTransparentShader == null)
            _speedTreeTransparentShader = Shader.Find("Custom/SpeedTree8_Transparent");
    }

    private void LateUpdate()
    {
        if (_target == null || _mainCamera == null) return;

        _currentFrameObstacles.Clear();
        DetectObstacles();
        UpdateObstacleTransparency();
    }

    private void DetectObstacles()
    {
        Vector3 cameraPos = _mainCamera.transform.position;
        Vector3 basePos = _target.position;

        // 캐릭터의 여러 포인트로 레이캐스트
        Vector3[] targetPoints = new Vector3[]
        {
            basePos + Vector3.up * characterHeight,                    // 머리
            basePos + Vector3.up * (characterHeight * 0.5f),           // 몸통 중앙
            basePos + Vector3.up * 0.1f,                               // 발
            basePos + Vector3.up * (characterHeight * 0.5f) + _target.right * characterRadius,  // 몸통 오른쪽
            basePos + Vector3.up * (characterHeight * 0.5f) - _target.right * characterRadius,  // 몸통 왼쪽
        };

        foreach (Vector3 targetPos in targetPoints)
        {
            Vector3 direction = targetPos - cameraPos;
            float distance = direction.magnitude;

            RaycastHit[] hits = Physics.RaycastAll(cameraPos, direction.normalized, distance, obstacleLayer);

            foreach (var hit in hits)
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();
                if (rend == null)
                    rend = hit.collider.GetComponentInChildren<Renderer>();

                if (rend != null)
                {
                    _currentFrameObstacles.Add(rend);

                    if (!_affectedRenderers.ContainsKey(rend))
                    {
                        RegisterObstacle(rend);
                    }
                }
            }
        }
    }

    private void RegisterObstacle(Renderer renderer)
    {
        Material[] originals = renderer.sharedMaterials;
        Material[] transparents = new Material[originals.Length];

        for (int i = 0; i < originals.Length; i++)
        {
            transparents[i] = new Material(originals[i]);
            SetMaterialTransparent(transparents[i]);
        }

        _affectedRenderers[renderer] = new MaterialData
        {
            OriginalMaterials = originals,
            TransparentMaterials = transparents,
            CurrentAlpha = 1f
        };

        renderer.materials = transparents;
    }

    private void UpdateObstacleTransparency()
    {
        List<Renderer> toRemove = new();
        Dictionary<Renderer, MaterialData> toUpdate = new();

        foreach (Renderer rend in _affectedRenderers.Keys.ToList())
        {
            MaterialData data = _affectedRenderers[rend];

            if (rend == null)
            {
                toRemove.Add(rend);
                continue;
            }

            bool isBlocking = _currentFrameObstacles.Contains(rend);
            float targetAlpha = isBlocking ? transparentAlpha : 1f;

            data.CurrentAlpha = Mathf.MoveTowards(data.CurrentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            toUpdate[rend] = data;

            // 알파값 업데이트
            foreach (var mat in data.TransparentMaterials)
            {
                SetMaterialAlpha(mat, data.CurrentAlpha);
            }

            // 완전히 불투명해지면 원래 머티리얼로 복원
            if (!isBlocking && Mathf.Approximately(data.CurrentAlpha, 1f))
            {
                rend.sharedMaterials = data.OriginalMaterials;

                // 투명 머티리얼 정리
                foreach (var mat in data.TransparentMaterials)
                {
                    Destroy(mat);
                }

                toRemove.Add(rend);
            }
        }

        // 업데이트 적용
        foreach (var kvp in toUpdate)
        {
            if (!toRemove.Contains(kvp.Key))
                _affectedRenderers[kvp.Key] = kvp.Value;
        }

        foreach (var rend in toRemove)
        {
            _affectedRenderers.Remove(rend);
        }
    }

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
    private static readonly int SurfaceId = Shader.PropertyToID("_Surface");
    private static readonly int BlendId = Shader.PropertyToID("_Blend");
    private static readonly int ModeId = Shader.PropertyToID("_Mode");
    private static readonly int SrcBlendId = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlendId = Shader.PropertyToID("_DstBlend");
    private static readonly int ZWriteId = Shader.PropertyToID("_ZWrite");
    private static readonly int AlphaMultiplierId = Shader.PropertyToID("_AlphaMultiplier");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int CutoffId = Shader.PropertyToID("_Cutoff");
    private static readonly int AlphaClipThresholdId = Shader.PropertyToID("_AlphaClipThreshold");
    private static readonly int HueVariationColorId = Shader.PropertyToID("_HueVariationColor");
    private static readonly int HueVariationKwToggleId = Shader.PropertyToID("_HueVariationKwToggle");

    private bool IsLeavesMaterial(Material mat)
    {
        return mat.name.ToLower().Contains("leaves");
    }

    private void SetMaterialAlpha(Material mat, float alpha)
    {
        // 나뭇잎: 커스텀 SpeedTree8 셰이더 사용
        if (IsLeavesMaterial(mat))
        {
            if (mat.HasProperty(AlphaMultiplierId))
                mat.SetFloat(AlphaMultiplierId, alpha);
            return;
        }

        // 일반 셰이더: 알파 블렌딩 방식
        if (mat.HasProperty(BaseColorId))
        {
            Color color = mat.GetColor(BaseColorId);
            color.a = alpha;
            mat.SetColor(BaseColorId, color);
        }
        if (mat.HasProperty(ColorId))
        {
            Color color = mat.GetColor(ColorId);
            color.a = alpha;
            mat.SetColor(ColorId, color);
        }
        if (mat.HasProperty(TintColorId))
        {
            Color color = mat.GetColor(TintColorId);
            color.a = alpha;
            mat.SetColor(TintColorId, color);
        }
    }

    private void SetMaterialTransparent(Material mat)
    {
        // 나뭇잎: 커스텀 SpeedTree8 투명 셰이더로 교체
        if (IsLeavesMaterial(mat))
        {
            if (_speedTreeTransparentShader != null)
            {
                // 셰이더 변경 전에 SpeedTree8 속성 저장
                Texture mainTex = mat.HasProperty(MainTexId) ? mat.GetTexture(MainTexId) : null;
                Color color = mat.HasProperty(ColorId) ? mat.GetColor(ColorId) : Color.white;
                float cutoff = mat.HasProperty(CutoffId) ? mat.GetFloat(CutoffId) : 0.33f;
                float alphaClipThreshold = mat.HasProperty(AlphaClipThresholdId) ? mat.GetFloat(AlphaClipThresholdId) : 0.33f;
                Color hueVariationColor = mat.HasProperty(HueVariationColorId) ? mat.GetColor(HueVariationColorId) : new Color(1f, 0.5f, 0f, 0.1f);
                float hueVariationToggle = mat.HasProperty(HueVariationKwToggleId) ? mat.GetFloat(HueVariationKwToggleId) : 0f;

                // 키워드 저장
                bool hasHueVariation = mat.IsKeywordEnabled("EFFECT_HUE_VARIATION");

                // 셰이더 변경
                mat.shader = _speedTreeTransparentShader;

                // SpeedTree8 속성 복원
                if (mainTex != null)
                    mat.SetTexture(MainTexId, mainTex);
                mat.SetColor(ColorId, color);
                mat.SetFloat(CutoffId, cutoff);
                mat.SetFloat(AlphaClipThresholdId, alphaClipThreshold);
                mat.SetColor(HueVariationColorId, hueVariationColor);
                mat.SetFloat(HueVariationKwToggleId, hueVariationToggle);
                mat.SetFloat(AlphaMultiplierId, 1f);

                // 키워드 복원
                if (hasHueVariation)
                    mat.EnableKeyword("EFFECT_HUE_VARIATION");
            }
            return;
        }

        // URP/Shader Graph
        if (mat.HasProperty(SurfaceId))
        {
            mat.SetFloat(SurfaceId, 1); // Transparent
            if (mat.HasProperty(BlendId))
                mat.SetFloat(BlendId, 0); // Alpha
        }

        // Standard
        if (mat.HasProperty(ModeId))
        {
            mat.SetFloat(ModeId, 3); // Transparent
        }

        if (mat.HasProperty(SrcBlendId))
            mat.SetInt(SrcBlendId, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (mat.HasProperty(DstBlendId))
            mat.SetInt(DstBlendId, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (mat.HasProperty(ZWriteId))
            mat.SetInt(ZWriteId, 0);

        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    private void OnDestroy()
    {
        // 정리
        foreach (var kvp in _affectedRenderers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.sharedMaterials = kvp.Value.OriginalMaterials;
            }

            foreach (var mat in kvp.Value.TransparentMaterials)
            {
                if (mat != null) Destroy(mat);
            }
        }
        _affectedRenderers.Clear();
    }
}