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
    private Canvas _canvas;

    // 드래그 고스트 (동적 생성)
    private GameObject _dragGhostObject;
    private Image _dragGhostImage;
    private TMP_Text _dragGhostQuantity;

    public bool IsOpen => panel != null && panel.activeSelf;

    /// <summary>
    /// UI 열림/닫힘 이벤트 (플레이어 입력 제어용)
    /// </summary>
    public event System.Action<bool> OnUIToggled;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        // Canvas 참조 (드래그 고스트 부모용)
        _canvas = GetComponentInParent<Canvas>();
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

        // 드래그 고스트 정리
        DestroyDragGhost();
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

    #region Drag Ghost

    /// <summary>
    /// 드래그 오브젝트 생성
    /// </summary>
    private void CreateDragGhostImage(RectTransform sourceSlot)
    {
        if (_dragGhostObject != null || _canvas == null) return;

        // 고스트 오브젝트 생성
        _dragGhostObject = new GameObject("DragGhost");
        _dragGhostObject.transform.SetParent(_canvas.transform, false);

        // RectTransform 설정
        var rectTransform = _dragGhostObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = sourceSlot.sizeDelta;

        // 이미지 설정
        _dragGhostImage = _dragGhostObject.AddComponent<Image>();
        _dragGhostImage.raycastTarget = false;

        // 반투명 효과
        var color = _dragGhostImage.color;
        color.a = 0.8f;
        _dragGhostImage.color = color;

        // 수량 텍스트
        var textObj = new GameObject("Quantity");
        textObj.transform.SetParent(_dragGhostObject.transform, false);

        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        _dragGhostQuantity = textObj.AddComponent<TextMeshProUGUI>();
        _dragGhostQuantity.alignment = TextAlignmentOptions.TopRight;
        _dragGhostQuantity.fontSize = 14;
        _dragGhostQuantity.raycastTarget = false;

        _dragGhostObject.SetActive(false);
    }

    /// <summary>
    /// 드래그 고스트 이미지 표시
    /// </summary>
    public void ShowDragGhost(RectTransform sourceSlot, Sprite icon, int quantity, Vector2 position)
    {
        if (_canvas == null || sourceSlot == null) return;

        // 고스트가 없으면 생성
        CreateDragGhostImage(sourceSlot);

        if (_dragGhostObject == null) return;

        // 이미지 업데이트
        _dragGhostImage.sprite = icon;

        // 수량 업데이트
        _dragGhostQuantity.text = quantity > 1 ? quantity.ToString() : "";

        // 위치 및 활성화
        _dragGhostObject.transform.position = position;
        _dragGhostObject.transform.SetAsLastSibling();
        _dragGhostObject.SetActive(true);
    }

    /// <summary>
    /// 드래그 고스트 이미지 위치 업데이트
    /// </summary>
    public void UpdateDragGhost(Vector2 position)
    {
        if (_dragGhostObject != null && _dragGhostObject.activeSelf)
        {
            _dragGhostObject.transform.position = position;
        }
    }

    /// <summary>
    /// 드래그 고스트 이미지 비활성화
    /// </summary>
    public void HideDragGhost()
    {
        if (_dragGhostObject != null)
        {
            _dragGhostObject.SetActive(false);
        }
    }

    /// <summary>
    /// 드래그 고스트 오브젝트 파괴 (OnDestroy 시)
    /// </summary>
    private void DestroyDragGhost()
    {
        if (_dragGhostObject != null)
        {
            Destroy(_dragGhostObject);
            _dragGhostObject = null;
            _dragGhostImage = null;
            _dragGhostQuantity = null;
        }
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
