using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Items;

/// <summary>
/// 인벤토리 슬롯 UI (드래그앤드롭, 클릭, 툴팁)
/// </summary>
public class InventoryItemSlotUI : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image backgroundImage;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color emptyColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    [SerializeField] private Color highlightColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private int _slotIndex;
    private InventorySlot _slotData;
    private PlayerInventory _inventory;
    private InventoryUI _parentUI;

    // 드래그 상태
    private static InventoryItemSlotUI _draggedSlot;

    public int SlotIndex => _slotIndex;
    public InventorySlot SlotData => _slotData;
    public bool IsEmpty => _slotData.IsEmpty;

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void Initialize(int slotIndex, PlayerInventory inventory, InventoryUI parentUI)
    {
        _slotIndex = slotIndex;
        _inventory = inventory;
        _parentUI = parentUI;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        UpdateVisuals();
    }

    /// <summary>
    /// 슬롯 데이터 업데이트
    /// </summary>
    public void UpdateSlot(InventorySlot slotData)
    {
        _slotData = slotData;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_slotData.IsEmpty)
        {
            // 빈 슬롯
            if (iconImage != null)
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
            }

            if (quantityText != null)
                quantityText.text = "";

            if (backgroundImage != null)
                backgroundImage.color = emptyColor;
        }
        else
        {
            // 아이템 있음
            var itemData = ItemManager.Instance?.GetItem(_slotData.itemId);
            if (itemData != null)
            {
                if (iconImage != null)
                {
                    iconImage.enabled = true;
                    iconImage.sprite = itemData.Icon;
                }

                if (quantityText != null)
                {
                    quantityText.text = _slotData.quantity > 1 ? _slotData.quantity.ToString() : "";
                }

                if (backgroundImage != null)
                    backgroundImage.color = normalColor;
            }
        }
    }

    #region Pointer Events

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_inventory == null || _slotData.IsEmpty) return;

        var itemData = ItemManager.Instance?.GetItem(_slotData.itemId);
        if (itemData == null) return;

        // 우클릭: 사용 또는 장착
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (itemData.IsEquippable)
            {
                _inventory.CmdEquipItem(_slotIndex);
            }
            else if (itemData.IsUsable)
            {
                // 쿨다운 체크
                if (_inventory.IsItemOnCooldown(_slotData.itemId))
                {
                    Debug.Log($"[InventorySlot] Item {itemData.Name} is on cooldown");
                    return;
                }

                _inventory.CmdUseItem(_slotIndex);
            }
        }
        // 좌클릭: 추후 확장 (분할 등)
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_slotData.IsEmpty)
        {
            if (backgroundImage != null)
                backgroundImage.color = highlightColor;

            _parentUI?.ShowTooltip(_slotData.itemId, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_slotData.IsEmpty)
        {
            if (backgroundImage != null)
                backgroundImage.color = normalColor;
        }

        _parentUI?.HideTooltip();
    }

    #endregion

    #region Drag and Drop

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_slotData.IsEmpty || _inventory == null || _parentUI == null) return;

        _draggedSlot = this;

        // 드래그 고스트 이미지 표시 (현재 슬롯 크기 참조)
        var itemData = ItemManager.Instance?.GetItem(_slotData.itemId);
        if (itemData != null)
        {
            var rectTransform = GetComponent<RectTransform>();
            _parentUI.ShowDragGhost(rectTransform, itemData.Icon, _slotData.quantity, eventData.position);
        }

        // 원본 슬롯 반투명 처리
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.5f;
        }

        _parentUI.HideTooltip();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggedSlot != this || _parentUI == null) return;

        // 드래그 고스트 이미지만 이동 (슬롯 자체는 이동하지 않음)
        _parentUI.UpdateDragGhost(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_draggedSlot != this) return;

        // 드래그 고스트 숨김
        _parentUI?.HideDragGhost();

        // 원본 슬롯 시각 효과 복원
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        _draggedSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_draggedSlot == null || _draggedSlot == this || _inventory == null) return;

        // 슬롯 이동 요청
        _inventory.CmdMoveItem(_draggedSlot.SlotIndex, _slotIndex);
    }

    #endregion
}
