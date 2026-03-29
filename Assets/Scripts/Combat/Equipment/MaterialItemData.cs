using UnityEngine;

/// <summary>
/// 재료 아이템 (제작 시스템용)
/// </summary>
[CreateAssetMenu(fileName = "Material_", menuName = "Items/Material Item")]
public class MaterialItemData : ItemData
{
    [Header("재료 정보")]
    [SerializeField] private MaterialType materialType;

    public MaterialType MaterialType => materialType;

    public MaterialItemData()
    {
        isStackable = true;
        maxStackSize = 99;
    }
}

public enum MaterialType
{
    Ore,        // 광석
    Herb,       // 약초
    Leather,    // 가죽
    Wood,       // 나무
    Crystal,    // 크리스탈
    Other       // 기타
}
