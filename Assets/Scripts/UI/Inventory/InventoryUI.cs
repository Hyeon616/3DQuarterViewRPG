using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 인벤토리 UI 메인 패널 (StatTreeUI 패턴)
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button closeButton;

    [Header("Slots")]
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Tooltip")]
    [SerializeField] private InventoryTooltip tooltip;

    [Header("Info")]
    [SerializeField] private TMP_Text infoText;

    private PlayerInventory _inventory;
    private List<InventoryItemSlotUI> _slotUIList = new List<InventoryItemSlotUI>();
    private bool _isInitialized = false;

    public bool IsOpen => panel != null && panel.activeSelf;

    /// <summary>
    /// UI 열림/닫힘 이벤트 (플레이어 입력 제어용)
    /// </summary>
    public event System.Action<bool> OnUIToggled;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        // panel.SetActive(false)는 여기서 하지 않음
        // 이미 비활성화 상태로 생성되며, 첫 Open 시 Awake가 호출되어 다시 닫히는 문제 방지
    }

    /// <summary>
    /// 인벤토리와 연결
    /// </summary>
    public void Initialize(PlayerInventory inventory)
    {
        if (_isInitialized) return;

        _inventory = inventory;

        // 슬롯 생성
        BuildSlots();

        // 이벤트 구독
        if (_inventory != null)
        {
            _inventory.OnInventoryChanged += UpdateAllSlots;
        }

        _isInitialized = true;

        // 초기 업데이트
        UpdateAllSlots();
    }

    private void OnDestroy()
    {
        if (_inventory != null)
        {
            _inventory.OnInventoryChanged -= UpdateAllSlots;
        }

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    /// <summary>
    /// 슬롯 UI 동적 생성
    /// </summary>
    private void BuildSlots()
    {
        if (_inventory == null || slotsContainer == null || slotPrefab == null) return;

        // 기존 슬롯 제거
        foreach (var slot in _slotUIList)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        _slotUIList.Clear();

        // 새 슬롯 생성
        int slotCount = _inventory.InventorySize;
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            var slotUI = slotObj.GetComponent<InventoryItemSlotUI>();

            if (slotUI != null)
            {
                slotUI.Initialize(i, _inventory, this);
                _slotUIList.Add(slotUI);
            }
        }
    }

    /// <summary>
    /// 모든 슬롯 업데이트
    /// </summary>
    private void UpdateAllSlots()
    {
        if (_inventory == null) return;

        for (int i = 0; i < _slotUIList.Count; i++)
        {
            var slot = _inventory.GetSlot(i);
            _slotUIList[i].UpdateSlot(slot);
        }

        UpdateInfo();
    }

    /// <summary>
    /// 정보 텍스트 업데이트
    /// </summary>
    private void UpdateInfo()
    {
        if (infoText == null || _inventory == null) return;

        int usedSlots = 0;
        for (int i = 0; i < _inventory.Slots.Count; i++)
        {
            if (!_inventory.GetSlot(i).IsEmpty)
                usedSlots++;
        }

        infoText.text = $"인벤토리: {usedSlots}/{_inventory.InventorySize}";
    }

    #region Tooltip

    public void ShowTooltip(int itemId, Vector3 position)
    {
        if (tooltip != null)
            tooltip.Show(itemId, position);
    }

    public void HideTooltip()
    {
        if (tooltip != null)
            tooltip.Hide();
    }

    #endregion

    #region Panel Control

    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (panel != null)
        {
            panel.SetActive(true);
            UpdateAllSlots();
            OnUIToggled?.Invoke(true);
        }
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);

        HideTooltip();
        OnUIToggled?.Invoke(false);
    }

    #endregion
}
