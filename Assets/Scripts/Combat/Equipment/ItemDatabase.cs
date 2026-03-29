using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 아이템 데이터베이스
/// EquipmentDatabase와 통합 관리
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
            }
            return _instance;
        }
    }

    [Header("아이템 목록")]
    [SerializeField] private ItemData[] items;

    [Header("카테고리별 필터 (Inspector 전용)")]
    [SerializeField] private EquipmentItemData[] equipmentItems;
    [SerializeField] private ConsumableItemData[] consumableItems;
    [SerializeField] private MaterialItemData[] materialItems;
    [SerializeField] private QuestItemData[] questItems;

    private Dictionary<int, ItemData> _itemLookup;

    public ItemData[] Items => items;

    private void OnEnable()
    {
        BuildLookupTable();
    }

    private void BuildLookupTable()
    {
        _itemLookup = new Dictionary<int, ItemData>();

        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                    _itemLookup[i] = items[i];
            }
        }
    }

    /// <summary>
    /// ID로 아이템 조회 (-1 = 없음)
    /// </summary>
    public ItemData GetItem(int itemId)
    {
        if (_itemLookup == null) BuildLookupTable();
        return _itemLookup.TryGetValue(itemId, out var item) ? item : null;
    }

    /// <summary>
    /// 아이템의 ID 조회
    /// </summary>
    public int GetItemId(ItemData item)
    {
        if (items == null || item == null) return -1;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item) return i;
        }
        return -1;
    }

    /// <summary>
    /// 타입별 아이템 필터
    /// </summary>
    public ItemData[] GetItemsByType(ItemType type)
    {
        if (items == null) return new ItemData[0];

        List<ItemData> filtered = new List<ItemData>();
        foreach (var item in items)
        {
            if (item != null && item.ItemType == type)
                filtered.Add(item);
        }
        return filtered.ToArray();
    }
}
