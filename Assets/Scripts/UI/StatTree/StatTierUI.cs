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
    [SerializeField] private Image lockIcon; // 잠금/비활성 아이콘

    [Header("Colors")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    [Header("Prefab")]
    [SerializeField] private StatNodeUI nodePrefab;

    private PlayerStatAllocation _allocation;
    private StatTier _tierData;
    private int _tierIndex;
    private StatNodeTooltip _tooltip;
    private List<StatNodeUI> _nodeUIs = new List<StatNodeUI>();

    /// <summary>
    /// 런타임 생성 모드용 초기화
    /// </summary>
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

    /// <summary>
    /// 미리 빌드된 UI용 초기화 - 노드를 생성하지 않고 기존 자식들 사용
    /// </summary>
    public void InitializePrebuilt(PlayerStatAllocation allocation, StatTier tierData, int tierIndex, StatNodeTooltip tooltip = null)
    {
        _allocation = allocation;
        _tierData = tierData;
        _tierIndex = tierIndex;
        _tooltip = tooltip;

        if (tierNameText != null)
            tierNameText.text = tierData.TierName;

        // 기존 자식 노드들을 찾아서 Initialize
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
        // 기존 노드 제거
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

        // 상태 확인
        bool isActive = _allocation.IsTierActive(_tierIndex);

        // 잠금 아이콘: 비활성 상태일 때 표시 (잠금 또는 조건 미충족)
        if (lockIcon != null)
            lockIcon.gameObject.SetActive(!isActive);

        // 배경색 변경: 비활성 시 어둡게
        if (leftInfoBackground != null)
        {
            leftInfoBackground.color = isActive ? activeColor : inactiveColor;
        }

        // 노드 업데이트 (활성 상태 전달)
        foreach (var nodeUI in _nodeUIs)
        {
            nodeUI?.UpdateDisplay(isActive);
        }
    }
}
