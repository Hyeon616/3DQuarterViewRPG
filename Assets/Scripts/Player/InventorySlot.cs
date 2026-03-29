using System;

/// <summary>
/// 인벤토리 슬롯 데이터 (네트워크 동기화용)
/// </summary>
[Serializable]
public struct InventorySlot
{
    public int itemId;      // -1 = 빈 슬롯
    public int quantity;    // 수량

    public InventorySlot(int id, int qty)
    {
        itemId = id;
        quantity = qty;
    }

    public bool IsEmpty => itemId < 0 || quantity <= 0;

    public static InventorySlot Empty => new InventorySlot(-1, 0);
}
