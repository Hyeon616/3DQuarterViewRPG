using Mirror;
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 플레이어 인벤토리 관리
/// SyncList로 슬롯 동기화
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerInventory : NetworkBehaviour
{
    [Header("인벤토리 설정")]
    [SerializeField] private int inventorySize = 100; // 슬롯 수 (10x10)

    [Header("테스트 아이템")]
    [SerializeField] private int[] testItemIds;
    [SerializeField] private int[] testItemQuantities;

    // 네트워크 동기화
    private readonly SyncList<InventorySlot> _slots = new SyncList<InventorySlot>();

    // 쿨다운 관리
    private Dictionary<int, float> _itemCooldowns = new Dictionary<int, float>();

    public int InventorySize => inventorySize;
    public SyncList<InventorySlot> Slots => _slots;

    public event Action OnInventoryChanged;

    public override void OnStartServer()
    {
        base.OnStartServer();
        InitializeInventory();

        // 테스트 아이템 지급
        if (testItemIds != null && testItemQuantities != null)
        {
            for (int i = 0; i < Mathf.Min(testItemIds.Length, testItemQuantities.Length); i++)
            {
                ServerAddItem(testItemIds[i], testItemQuantities[i]);
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        _slots.Callback += OnSlotsChanged;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        _slots.Callback -= OnSlotsChanged;
    }

    private void Update()
    {
        if (!isOwned) return;
        UpdateCooldowns();
    }

    private void InitializeInventory()
    {
        _slots.Clear();
        for (int i = 0; i < inventorySize; i++)
        {
            _slots.Add(InventorySlot.Empty);
        }
    }

    private void OnSlotsChanged(SyncList<InventorySlot>.Operation op, int index, InventorySlot oldSlot, InventorySlot newSlot)
    {
        OnInventoryChanged?.Invoke();
    }

    #region Public Queries (Client-side)

    /// <summary>
    /// 슬롯 조회
    /// </summary>
    public InventorySlot GetSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count)
            return InventorySlot.Empty;
        return _slots[slotIndex];
    }

    /// <summary>
    /// 아이템 개수 조회
    /// </summary>
    public int GetItemCount(int itemId)
    {
        int total = 0;
        foreach (var slot in _slots)
        {
            if (slot.itemId == itemId)
                total += slot.quantity;
        }
        return total;
    }

    /// <summary>
    /// 빈 슬롯 찾기
    /// </summary>
    public int FindEmptySlot()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].IsEmpty)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// 아이템을 스택할 수 있는 슬롯 찾기
    /// </summary>
    public int FindStackableSlot(int itemId)
    {
        var itemData = ItemDatabase.Instance?.GetItem(itemId);
        if (itemData == null || !itemData.IsStackable) return -1;

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot.itemId == itemId && slot.quantity < itemData.MaxStackSize)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// 아이템 쿨다운 확인
    /// </summary>
    public bool IsItemOnCooldown(int itemId)
    {
        return _itemCooldowns.ContainsKey(itemId) && _itemCooldowns[itemId] > 0f;
    }

    public float GetItemCooldown(int itemId)
    {
        return _itemCooldowns.TryGetValue(itemId, out float cooldown) ? cooldown : 0f;
    }

    #endregion

    #region Commands (Client → Server)

    /// <summary>
    /// 아이템 추가 요청
    /// </summary>
    [Command]
    public void CmdAddItem(int itemId, int quantity)
    {
        ServerAddItem(itemId, quantity);
    }

    /// <summary>
    /// 아이템 제거 요청
    /// </summary>
    [Command]
    public void CmdRemoveItem(int slotIndex, int quantity)
    {
        ServerRemoveItem(slotIndex, quantity);
    }

    /// <summary>
    /// 슬롯 이동/스왑 요청
    /// </summary>
    [Command]
    public void CmdMoveItem(int fromSlot, int toSlot)
    {
        ServerMoveItem(fromSlot, toSlot);
    }

    /// <summary>
    /// 아이템 사용 요청
    /// </summary>
    [Command]
    public void CmdUseItem(int slotIndex)
    {
        ServerUseItem(slotIndex);
    }

    /// <summary>
    /// 장비 장착 요청
    /// </summary>
    [Command]
    public void CmdEquipItem(int slotIndex)
    {
        ServerEquipItem(slotIndex);
    }

    #endregion

    #region Server Logic

    [Server]
    public void ServerAddItem(int itemId, int quantity)
    {
        var itemData = ItemDatabase.Instance?.GetItem(itemId);
        if (itemData == null || quantity <= 0) return;

        int remaining = quantity;

        // 스택 가능한 경우 기존 슬롯에 추가
        if (itemData.IsStackable)
        {
            for (int i = 0; i < _slots.Count && remaining > 0; i++)
            {
                var slot = _slots[i];
                if (slot.itemId == itemId && slot.quantity < itemData.MaxStackSize)
                {
                    int addAmount = Mathf.Min(remaining, itemData.MaxStackSize - slot.quantity);
                    _slots[i] = new InventorySlot(itemId, slot.quantity + addAmount);
                    remaining -= addAmount;
                }
            }
        }

        // 새 슬롯에 추가
        while (remaining > 0)
        {
            int emptySlot = FindEmptySlot();
            if (emptySlot < 0)
            {
                Debug.LogWarning($"[Inventory] No space for item {itemId}");
                break;
            }

            int addAmount = itemData.IsStackable
                ? Mathf.Min(remaining, itemData.MaxStackSize)
                : 1;

            _slots[emptySlot] = new InventorySlot(itemId, addAmount);
            remaining -= addAmount;
        }
    }

    [Server]
    public void ServerRemoveItem(int slotIndex, int quantity)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count) return;

        var slot = _slots[slotIndex];
        if (slot.IsEmpty) return;

        int newQuantity = slot.quantity - quantity;
        if (newQuantity <= 0)
        {
            _slots[slotIndex] = InventorySlot.Empty;
        }
        else
        {
            _slots[slotIndex] = new InventorySlot(slot.itemId, newQuantity);
        }
    }

    [Server]
    public void ServerMoveItem(int fromSlot, int toSlot)
    {
        if (fromSlot < 0 || fromSlot >= _slots.Count) return;
        if (toSlot < 0 || toSlot >= _slots.Count) return;
        if (fromSlot == toSlot) return;

        var from = _slots[fromSlot];
        var to = _slots[toSlot];

        // 빈 슬롯으로 이동
        if (to.IsEmpty)
        {
            _slots[toSlot] = from;
            _slots[fromSlot] = InventorySlot.Empty;
            return;
        }

        // 같은 아이템 스택
        if (from.itemId == to.itemId)
        {
            var itemData = ItemDatabase.Instance?.GetItem(from.itemId);
            if (itemData != null && itemData.IsStackable)
            {
                int totalQuantity = from.quantity + to.quantity;
                int maxStack = itemData.MaxStackSize;

                if (totalQuantity <= maxStack)
                {
                    _slots[toSlot] = new InventorySlot(to.itemId, totalQuantity);
                    _slots[fromSlot] = InventorySlot.Empty;
                }
                else
                {
                    _slots[toSlot] = new InventorySlot(to.itemId, maxStack);
                    _slots[fromSlot] = new InventorySlot(from.itemId, totalQuantity - maxStack);
                }
                return;
            }
        }

        // 슬롯 스왑
        _slots[fromSlot] = to;
        _slots[toSlot] = from;
    }

    [Server]
    public void ServerUseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count) return;

        var slot = _slots[slotIndex];
        if (slot.IsEmpty) return;

        var itemData = ItemDatabase.Instance?.GetItem(slot.itemId);
        if (itemData == null || !itemData.IsUsable) return;

        // 쿨다운 체크 (서버 측)
        if (itemData is ConsumableItemData consumable)
        {
            // 아이템 효과 적용
            ApplyConsumableEffects(consumable);

            // 쿨다운 설정 (클라이언트에 RPC로 전달)
            if (consumable.Cooldown > 0)
            {
                TargetSetItemCooldown(consumable.Cooldown, slot.itemId);
            }

            // 아이템 소모
            ServerRemoveItem(slotIndex, 1);
        }
    }

    [Server]
    public void ServerEquipItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count) return;

        var slot = _slots[slotIndex];
        if (slot.IsEmpty) return;

        var itemData = ItemDatabase.Instance?.GetItem(slot.itemId);
        if (itemData == null || !itemData.IsEquippable) return;

        var equipmentItem = itemData as EquipmentItemData;
        if (equipmentItem == null) return;

        var equipment = GetComponent<PlayerEquipment>();
        if (equipment == null) return;

        // 기존 장비 해제 및 인벤토리에 추가
        if (equipmentItem.EquipmentType == EquipmentType.Weapon)
        {
            // 기존 무기를 인벤토리로 반환
            if (equipment.CurrentWeaponItemId >= 0)
            {
                ServerAddItem(equipment.CurrentWeaponItemId, 1);
            }

            // 새 무기 장착
            equipment.EquipWeaponByItemId(slot.itemId);
        }
        else if (equipmentItem.EquipmentType == EquipmentType.Armor)
        {
            // 기존 방어구를 인벤토리로 반환
            if (equipment.CurrentArmorItemId >= 0)
            {
                ServerAddItem(equipment.CurrentArmorItemId, 1);
            }

            // 새 방어구 장착
            equipment.EquipArmorByItemId(slot.itemId);
        }

        // 인벤토리에서 제거
        ServerRemoveItem(slotIndex, 1);
    }

    [Server]
    private void ApplyConsumableEffects(ConsumableItemData consumable)
    {
        // TODO: 실제 효과 적용 로직 (HP 회복 등)
        // 현재는 플레이스홀더
        foreach (var effect in consumable.Effects)
        {
            switch (effect.effectType)
            {
                case ConsumableEffect.EffectType.RestoreHP:
                    // HP 회복 로직
                    Debug.Log($"[Inventory] Restore HP: {effect.value}");
                    break;
                case ConsumableEffect.EffectType.RestoreMana:
                    // 마나 회복 로직
                    Debug.Log($"[Inventory] Restore Mana: {effect.value}");
                    break;
                case ConsumableEffect.EffectType.Buff:
                    // 버프 적용 로직
                    Debug.Log($"[Inventory] Apply Buff: {effect.duration}s");
                    break;
                case ConsumableEffect.EffectType.Debuff:
                    // 디버프 적용 로직
                    Debug.Log($"[Inventory] Apply Debuff: {effect.duration}s");
                    break;
            }
        }
    }

    #endregion

    #region Client RPCs

    [TargetRpc]
    private void TargetSetItemCooldown(float cooldownDuration, int itemId)
    {
        _itemCooldowns[itemId] = cooldownDuration;
    }

    #endregion

    #region Cooldown Management

    private void UpdateCooldowns()
    {
        var keys = new List<int>(_itemCooldowns.Keys);
        foreach (var itemId in keys)
        {
            _itemCooldowns[itemId] -= Time.deltaTime;
            if (_itemCooldowns[itemId] <= 0f)
            {
                _itemCooldowns.Remove(itemId);
            }
        }
    }

    #endregion
}
