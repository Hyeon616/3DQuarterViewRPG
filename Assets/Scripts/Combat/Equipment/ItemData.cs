using UnityEngine;

/// <summary>
/// 모든 아이템의 기본 클래스
/// </summary>
public abstract class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] protected string itemName;
    [SerializeField] protected Sprite icon;
    [SerializeField, TextArea] protected string description;
    [SerializeField] protected ItemType itemType;
    [SerializeField] protected ItemRarity rarity = ItemRarity.Common;

    [Header("스택")]
    [SerializeField] protected int maxStackSize = 1;
    [SerializeField] protected bool isStackable = false;

    [Header("게임플레이")]
    [SerializeField] protected bool isTradeable = true;
    [SerializeField] protected bool isDroppable = true;
    [SerializeField] protected int sellPrice = 0;

    // Properties
    public string ItemName => itemName;
    public Sprite Icon => icon;
    public string Description => description;
    public ItemType ItemType => itemType;
    public ItemRarity Rarity => rarity;
    public int MaxStackSize => maxStackSize;
    public bool IsStackable => isStackable;
    public bool IsTradeable => isTradeable;
    public bool IsDroppable => isDroppable;
    public int SellPrice => sellPrice;

    /// <summary>
    /// 아이템 사용 가능 여부
    /// </summary>
    public virtual bool IsUsable => false;

    /// <summary>
    /// 아이템 장착 가능 여부
    /// </summary>
    public virtual bool IsEquippable => false;

    /// <summary>
    /// 툴팁에 표시할 추가 정보
    /// </summary>
    public virtual string GetTooltipInfo()
    {
        return description;
    }
}
