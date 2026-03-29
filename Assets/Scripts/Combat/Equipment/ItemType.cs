/// <summary>
/// 아이템 타입 분류
/// </summary>
public enum ItemType
{
    Equipment,      // 장비 (무기/방어구)
    Consumable,     // 소비 아이템
    Material,       // 재료
    Quest,          // 퀘스트 아이템
    Currency        // 화폐 (추후 확장)
}

/// <summary>
/// 아이템 등급 (추후 드롭 시스템용)
/// </summary>
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
