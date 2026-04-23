using System;
using System.Collections.Generic;
using Items;

/// <summary>
/// 플레이어 저장 데이터 (JSON 직렬화용)
/// </summary>
[Serializable]
public class PlayerSaveData
{
    // 버전 (마이그레이션용)
    public int version = 1;

    // 기본 정보
    public string playerId;
    public string playerName;
    public long lastSaveTime;

    // 레벨/경험치
    public int level = 1;
    public long experience;

    // 장비 (ID 기반)
    public int equippedWeaponId = -1;
    public int equippedArmorId = -1;

    // StatTree 투자 (tierIndex_nodeIndex = points)
    public List<StatAllocationEntry> statAllocations = new List<StatAllocationEntry>();
    public int availableStatPoints;
    public int totalStatPoints;

    // 인벤토리 (추후 확장)
    public List<InventoryItemEntry> inventory = new List<InventoryItemEntry>();

    // 재화
    public long gold;
    public long gems;

    // 무기 숙련도 (무기 타입별 경험치)
    public List<WeaponMasteryEntry> weaponMasteries = new List<WeaponMasteryEntry>();

    public PlayerSaveData()
    {
        playerId = Guid.NewGuid().ToString();
        lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void UpdateSaveTime()
    {
        lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

[Serializable]
public class StatAllocationEntry
{
    public int tierIndex;
    public int nodeIndex;
    public int points;

    public StatAllocationEntry() { }

    public StatAllocationEntry(int tier, int node, int pts)
    {
        tierIndex = tier;
        nodeIndex = node;
        points = pts;
    }
}

[Serializable]
public class InventoryItemEntry
{
    public int itemId;
    public int quantity;
    public int slotIndex;

    public InventoryItemEntry() { }

    public InventoryItemEntry(int id, int qty, int slot)
    {
        itemId = id;
        quantity = qty;
        slotIndex = slot;
    }
}

[Serializable]
public class WeaponMasteryEntry
{
    public int weaponType; // WeaponType enum value
    public long experience;
    public int level;

    public WeaponMasteryEntry() { }

    public WeaponMasteryEntry(WeaponType type, long exp, int lvl)
    {
        weaponType = (int)type;
        experience = exp;
        level = lvl;
    }
}
