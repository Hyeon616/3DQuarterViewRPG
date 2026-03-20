using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StatTierUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI tierNameText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Transform nodesContainer;
    [SerializeField] private Image leftInfoBackground;
    [SerializeField] private Image lockIcon;

    [Header("Colors")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    [Header("Max Points Glow Effect")]
    [SerializeField] private Image glowImage;
    [SerializeField] private Color glowColor = new Color(0.3f, 0.6f, 1f, 1f);
    [SerializeField, Range(1f, 5f)] private float glowIntensity = 2.5f;

    [Header("Prefab")]
    [SerializeField] private StatNodeUI nodePrefab;

    private PlayerStatAllocation _allocation;
    private StatTier _tierData;
    private int _tierIndex;
    private StatNodeTooltip _tooltip;
    private List<StatNodeUI> _nodeUIs = new List<StatNodeUI>();
    private bool _isMaxed;
    private Material _glowMaterial;

    public void Initialize(PlayerStatAllocation allocation, StatTier tierData, int tierIndex, StatNodeUI nodePrefabOverride = null, StatNodeTooltip tooltip = null)
    {
        _allocation = allocation;
        _tierData = tierData;
        _tierIndex = tierIndex;
        _tooltip = tooltip;

        if (nodePrefabOverride != null)
            nodePrefab = nodePrefabOverride;

        if (tierNameText != null)
            tierNameText.text = tierData.TierName;

        CreateNodes();
        UpdateDisplay();
    }

    public void InitializePrebuilt(PlayerStatAllocation allocation, StatTier tierData, int tierIndex, StatNodeTooltip tooltip = null)
    {
        _allocation = allocation;
        _tierData = tierData;
        _tierIndex = tierIndex;
        _tooltip = tooltip;

        if (tierNameText != null)
            tierNameText.text = tierData.TierName;

        CollectAndInitializeNodes();
        UpdateDisplay();
    }

    private void CollectAndInitializeNodes()
    {
        _nodeUIs.Clear();

        if (nodesContainer == null || _tierData.Nodes == null) return;

        int nodeIndex = 0;
        for (int i = 0; i < nodesContainer.childCount && nodeIndex < _tierData.Nodes.Length; i++)
        {
            var nodeUI = nodesContainer.GetChild(i).GetComponent<StatNodeUI>();
            if (nodeUI == null) continue;

            var nodeData = _tierData.Nodes[nodeIndex];
            if (nodeData != null)
            {
                nodeUI.Initialize(_allocation, nodeData, _tierIndex, nodeIndex, _tooltip);
                _nodeUIs.Add(nodeUI);
            }
            nodeIndex++;
        }
    }

    private void CreateNodes()
    {
        foreach (var nodeUI in _nodeUIs)
        {
            if (nodeUI != null)
                Destroy(nodeUI.gameObject);
        }
        _nodeUIs.Clear();

        if (_tierData.Nodes == null || nodePrefab == null) return;

        for (int i = 0; i < _tierData.Nodes.Length; i++)
        {
            var node = _tierData.Nodes[i];
            if (node == null) continue;

            var nodeUI = Instantiate(nodePrefab, nodesContainer);
            nodeUI.Initialize(_allocation, node, _tierIndex, i, _tooltip);
            _nodeUIs.Add(nodeUI);
        }
    }

    public void UpdateDisplay()
    {
        if (_allocation == null || _tierData == null) return;

        int spent = _allocation.GetTierPointsSpent(_tierIndex);
        int maxTierPoints = _tierData.MaxTierPoints;

        if (progressText != null)
            progressText.text = $"{spent}/{maxTierPoints}";

        bool isActive = _allocation.IsTierActive(_tierIndex);

        if (lockIcon != null)
            lockIcon.gameObject.SetActive(!isActive);

        if (leftInfoBackground != null)
        {
            leftInfoBackground.color = isActive ? activeColor : inactiveColor;
        }

        foreach (var nodeUI in _nodeUIs)
        {
            nodeUI?.UpdateDisplay(isActive);
        }

        bool isNowMaxed = spent >= maxTierPoints && isActive;
        if (isNowMaxed != _isMaxed)
        {
            _isMaxed = isNowMaxed;
            if (_isMaxed)
                StartGlowEffect();
            else
                StopGlowEffect();
        }
    }

    private void OnDestroy()
    {
        if (_glowMaterial != null)
            Destroy(_glowMaterial);
    }

    private void SetupGlowMaterial()
    {
        if (glowImage == null || _glowMaterial != null) return;

        _glowMaterial = new Material(Shader.Find("UI/Default"));
        _glowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _glowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        glowImage.material = _glowMaterial;
    }

    private void StartGlowEffect()
    {
        if (glowImage == null) return;

        SetupGlowMaterial();
        glowImage.gameObject.SetActive(true);
        glowImage.color = new Color(
            glowColor.r * glowIntensity,
            glowColor.g * glowIntensity,
            glowColor.b * glowIntensity,
            glowColor.a
        );
    }

    private void StopGlowEffect()
    {
        if (glowImage != null)
            glowImage.gameObject.SetActive(false);
    }
}
