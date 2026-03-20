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
    [SerializeField] private bool usePrebuiltUI = true;

    [Header("Runtime Mode")]
    [SerializeField] private StatTierUI tierPrefab;
    [SerializeField] private StatNodeUI nodePrefab;

    [Header("Tooltip")]
    [SerializeField] private StatNodeTooltip nodeTooltip;

    private PlayerStatAllocation _allocation;
    private List<StatTierUI> _tierUIs = new List<StatTierUI>();
    private bool _isOpen;
    private Canvas _canvas;

    public event Action<bool> OnUIToggled;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceCamera && _canvas.worldCamera == null)
        {
            _canvas.worldCamera = Camera.main;
        }
    }

    public void Initialize(PlayerStatAllocation allocation)
    {
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
            BuildPrebuiltUI(statTree);
        }
        else
        {
            BuildRuntimeUI(statTree);
        }

        UpdateDisplay();
    }

    private void BuildPrebuiltUI(StatTreeData statTree)
    {
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
        if (nodeTooltip != null)
            nodeTooltip.Hide();

        if (panel != null)
            panel.SetActive(false);
        _isOpen = false;
        OnUIToggled?.Invoke(false);
    }
}
