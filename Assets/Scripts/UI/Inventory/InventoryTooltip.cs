using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 아이템 툴팁
/// </summary>
public class InventoryTooltip : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemTypeText;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Layout")]
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private Vector2 offset = new Vector2(10f, 10f);

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color uncommonColor = Color.green;
    [SerializeField] private Color rareColor = Color.blue;
    [SerializeField] private Color epicColor = new Color(0.6f, 0f, 1f); // Purple
    [SerializeField] private Color legendaryColor = new Color(1f, 0.5f, 0f); // Orange

    private Canvas _canvas;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (tooltipRect == null)
            tooltipRect = GetComponent<RectTransform>();

        _canvas = GetComponentInParent<Canvas>();

        Hide();
    }

    /// <summary>
    /// 툴팁 표시
    /// </summary>
    public void Show(int itemId, Vector3 worldPosition)
    {
        var itemData = ItemDatabase.Instance?.GetItem(itemId);
        if (itemData == null)
        {
            Hide();
            return;
        }

        // 텍스트 설정
        if (itemNameText != null)
        {
            itemNameText.text = itemData.ItemName;
            itemNameText.color = GetRarityColor(itemData.Rarity);
        }

        if (itemTypeText != null)
        {
            itemTypeText.text = GetItemTypeString(itemData.ItemType);
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = itemData.GetTooltipInfo();
        }

        if (iconImage != null)
        {
            iconImage.sprite = itemData.Icon;
            iconImage.enabled = itemData.Icon != null;
        }

        // 위치 설정
        UpdatePosition(worldPosition);

        // 표시
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
        }
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 툴팁 숨기기
    /// </summary>
    public void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
    }

    private void UpdatePosition(Vector3 worldPosition)
    {
        if (tooltipRect == null || _canvas == null) return;

        // 월드 좌표 → 캔버스 좌표 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            worldPosition,
            _canvas.worldCamera,
            out localPoint
        );

        // 오프셋 적용
        localPoint += offset;

        // 화면 밖으로 나가지 않도록 클램핑
        var canvasRect = _canvas.GetComponent<RectTransform>();
        float halfWidth = tooltipRect.rect.width * 0.5f;
        float halfHeight = tooltipRect.rect.height * 0.5f;

        localPoint.x = Mathf.Clamp(localPoint.x, -canvasRect.rect.width * 0.5f + halfWidth, canvasRect.rect.width * 0.5f - halfWidth);
        localPoint.y = Mathf.Clamp(localPoint.y, -canvasRect.rect.height * 0.5f + halfHeight, canvasRect.rect.height * 0.5f - halfHeight);

        tooltipRect.localPosition = localPoint;
    }

    private Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => commonColor,
            ItemRarity.Uncommon => uncommonColor,
            ItemRarity.Rare => rareColor,
            ItemRarity.Epic => epicColor,
            ItemRarity.Legendary => legendaryColor,
            _ => Color.white
        };
    }

    private string GetItemTypeString(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Equipment => "장비",
            ItemType.Consumable => "소비 아이템",
            ItemType.Material => "재료",
            ItemType.Quest => "퀘스트 아이템",
            ItemType.Currency => "화폐",
            _ => ""
        };
    }
}
