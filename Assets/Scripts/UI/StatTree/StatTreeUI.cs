using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class StatTreeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image panelBackground;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI availablePointsText;
    [SerializeField] private Transform tiersContainer;
    [SerializeField] private Button closeButton;

    [Header("Pre-built Mode")]
    [Tooltip("체크하면 tiersContainer의 자식들을 사용 (에디터에서 미리 빌드)")]
    [SerializeField] private bool usePrebuiltUI = true;

    [Header("Runtime Mode (usePrebuiltUI = false)")]
    [SerializeField] private StatTierUI tierPrefab;
    [SerializeField] private StatNodeUI nodePrefab;

    [Header("Tooltip")]
    [SerializeField] private StatNodeTooltip nodeTooltip;

    private PlayerStatAllocation _allocation;
    private List<StatTierUI> _tierUIs = new List<StatTierUI>();
    private bool _isOpen;

    public event Action<bool> OnUIToggled;

    public void Initialize(PlayerStatAllocation allocation)
    {
        // 기존 구독 해제 (재초기화 시 중복 방지)
        if (_allocation != null)
        {
            _allocation.OnAllocationChanged -= UpdateDisplay;
        }
        closeButton?.onClick.RemoveListener(Close);

        _allocation = allocation;

        if (_allocation != null)
        {
            _allocation.OnAllocationChanged += UpdateDisplay;
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        BuildUI();
        Close();
    }

    private void OnDestroy()
    {
        if (_allocation != null)
        {
            _allocation.OnAllocationChanged -= UpdateDisplay;
        }

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    private void BuildUI()
    {
        _tierUIs.Clear();

        if (_allocation?.StatTree == null) return;

        var statTree = _allocation.StatTree;

        if (titleText != null)
            titleText.text = statTree.TreeName;

        if (usePrebuiltUI)
        {
            // 미리 빌드된 UI 사용 - 기존 자식들을 찾아서 Initialize만 호출
            BuildPrebuiltUI(statTree);
        }
        else
        {
            // 런타임 생성 모드
            BuildRuntimeUI(statTree);
        }

        UpdateDisplay();
    }

    private void BuildPrebuiltUI(StatTreeData statTree)
    {
        // tiersContainer의 자식 StatTierUI들을 수집
        for (int i = 0; i < tiersContainer.childCount && i < statTree.TierCount; i++)
        {
            var tierUI = tiersContainer.GetChild(i).GetComponent<StatTierUI>();
            if (tierUI == null) continue;

            var tier = statTree.GetTier(i);
            if (tier == null) continue;

            tierUI.InitializePrebuilt(_allocation, tier, i, nodeTooltip);
            _tierUIs.Add(tierUI);
        }
    }

    private void BuildRuntimeUI(StatTreeData statTree)
    {
        // 기존 동적 생성 로직
        foreach (Transform child in tiersContainer)
        {
            Destroy(child.gameObject);
        }

        if (tierPrefab == null) return;

        for (int i = 0; i < statTree.TierCount; i++)
        {
            var tier = statTree.GetTier(i);
            if (tier == null) continue;

            var tierUI = Instantiate(tierPrefab, tiersContainer);
            tierUI.Initialize(_allocation, tier, i, nodePrefab, nodeTooltip);
            _tierUIs.Add(tierUI);
        }
    }

    public void UpdateDisplay()
    {
        if (_allocation == null) return;

        if (availablePointsText != null)
            availablePointsText.text = $"{_allocation.AvailablePoints}/{_allocation.TotalPoints}";

        foreach (var tierUI in _tierUIs)
        {
            tierUI?.UpdateDisplay();
        }
    }

    public void Toggle()
    {
        if (_isOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (panel != null)
            panel.SetActive(true);
        _isOpen = true;
        UpdateDisplay();
        OnUIToggled?.Invoke(true);
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
        _isOpen = false;
        OnUIToggled?.Invoke(false);
    }
}
