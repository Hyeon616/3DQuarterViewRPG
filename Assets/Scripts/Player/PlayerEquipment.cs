using Mirror;
using UnityEngine;
using System;
using Items;

/// <summary>
/// 플레이어 장비 관리
/// ItemManager의 장비 아이템 ID를 네트워크 동기화
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerEquipment : NetworkBehaviour
{
    [Header("기본 장비 (테스트용)")]
    [SerializeField] private int defaultWeaponItemId = -1;
    [SerializeField] private int defaultArmorItemId = -1;

    // -1 = 장비 없음 (ItemManager의 인덱스)
    [SyncVar(hook = nameof(OnWeaponChanged))]
    private int _weaponItemId = -1;

    [SyncVar(hook = nameof(OnArmorChanged))]
    private int _armorItemId = -1;

    private EquipmentData _cachedWeapon;
    private EquipmentData _cachedArmor;

    public EquipmentData CurrentWeapon => _cachedWeapon;
    public EquipmentData CurrentArmor => _cachedArmor;
    public int CurrentWeaponItemId => _weaponItemId;
    public int CurrentArmorItemId => _armorItemId;

    public event Action OnEquipmentChanged;

    public override void OnStartServer()
    {
        base.OnStartServer();

        // 기본 장비 설정
        if (defaultWeaponItemId >= 0)
        {
            _weaponItemId = defaultWeaponItemId;
        }
        if (defaultArmorItemId >= 0)
        {
            _armorItemId = defaultArmorItemId;
        }

        CacheEquipment();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        CacheEquipment();
    }

    private void OnWeaponChanged(int oldId, int newId)
    {
        CacheEquipment();
        OnEquipmentChanged?.Invoke();
    }

    private void OnArmorChanged(int oldId, int newId)
    {
        CacheEquipment();
        OnEquipmentChanged?.Invoke();
    }

    private void CacheEquipment()
    {
        var itemDb = ItemManager.Instance;
        if (itemDb == null) return;

        // 무기 캐싱
        if (_weaponItemId >= 0)
        {
            var item = itemDb.GetItem(_weaponItemId);
            if (item is EquipmentData equipItem && equipItem.EquipmentType == EquipmentType.Weapon)
            {
                _cachedWeapon = equipItem;
            }
            else
            {
                _cachedWeapon = null;
            }
        }
        else
        {
            _cachedWeapon = null;
        }

        // 방어구 캐싱
        if (_armorItemId >= 0)
        {
            var item = itemDb.GetItem(_armorItemId);
            if (item is EquipmentData equipItem && equipItem.EquipmentType == EquipmentType.Armor)
            {
                _cachedArmor = equipItem;
            }
            else
            {
                _cachedArmor = null;
            }
        }
        else
        {
            _cachedArmor = null;
        }
    }

    /// <summary>
    /// 무기 장착 by ItemDatabase ID (서버 호출)
    /// </summary>
    [Server]
    public void EquipWeaponByItemId(int itemId)
    {
        // 유효성 검사
        var item = ItemManager.Instance?.GetItem(itemId);
        if (item is EquipmentData equipItem && equipItem.EquipmentType == EquipmentType.Weapon)
        {
            _weaponItemId = itemId;
            CacheEquipment();
            OnEquipmentChanged?.Invoke();
        }
    }

    /// <summary>
    /// 방어구 장착 by ItemDatabase ID (서버 호출)
    /// </summary>
    [Server]
    public void EquipArmorByItemId(int itemId)
    {
        // 유효성 검사
        var item = ItemManager.Instance?.GetItem(itemId);
        if (item is EquipmentData equipItem && equipItem.EquipmentType == EquipmentType.Armor)
        {
            _armorItemId = itemId;
            CacheEquipment();
            OnEquipmentChanged?.Invoke();
        }
    }

    /// <summary>
    /// 무기 해제
    /// </summary>
    [Server]
    public void UnequipWeapon()
    {
        _weaponItemId = -1;
        CacheEquipment();
        OnEquipmentChanged?.Invoke();
    }

    /// <summary>
    /// 방어구 해제
    /// </summary>
    [Server]
    public void UnequipArmor()
    {
        _armorItemId = -1;
        CacheEquipment();
        OnEquipmentChanged?.Invoke();
    }

    /// <summary>
    /// 클라이언트에서 무기 장착 요청
    /// </summary>
    [Command]
    public void CmdEquipWeapon(int itemId)
    {
        EquipWeaponByItemId(itemId);
    }

    /// <summary>
    /// 클라이언트에서 방어구 장착 요청
    /// </summary>
    [Command]
    public void CmdEquipArmor(int itemId)
    {
        EquipArmorByItemId(itemId);
    }

    /// <summary>
    /// 클라이언트에서 무기 해제 요청
    /// </summary>
    [Command]
    public void CmdUnequipWeapon()
    {
        ServerUnequipWeaponToInventory();
    }

    /// <summary>
    /// 클라이언트에서 방어구 해제 요청
    /// </summary>
    [Command]
    public void CmdUnequipArmor()
    {
        ServerUnequipArmorToInventory();
    }

    /// <summary>
    /// 무기 해제 및 인벤토리 반환 (서버)
    /// </summary>
    [Server]
    private void ServerUnequipWeaponToInventory()
    {
        if (_weaponItemId < 0) return;

        var inventory = GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            // 인벤토리에 공간 체크
            int emptySlot = inventory.FindEmptySlot();
            if (emptySlot < 0)
            {
                Debug.LogWarning("[PlayerEquipment] No inventory space to unequip weapon");
                return;
            }

            // 인벤토리에 추가
            inventory.ServerAddItem(_weaponItemId, 1);

            // 장비 해제
            UnequipWeapon();
        }
        else
        {
            // 인벤토리 없으면 그냥 해제
            UnequipWeapon();
        }
    }

    /// <summary>
    /// 방어구 해제 및 인벤토리 반환 (서버)
    /// </summary>
    [Server]
    private void ServerUnequipArmorToInventory()
    {
        if (_armorItemId < 0) return;

        var inventory = GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            // 인벤토리에 공간 체크
            int emptySlot = inventory.FindEmptySlot();
            if (emptySlot < 0)
            {
                Debug.LogWarning("[PlayerEquipment] No inventory space to unequip armor");
                return;
            }

            // 인벤토리에 추가
            inventory.ServerAddItem(_armorItemId, 1);

            // 장비 해제
            UnequipArmor();
        }
        else
        {
            // 인벤토리 없으면 그냥 해제
            UnequipArmor();
        }
    }

    #region Stat Getters

    /// <summary>
    /// 현재 무기의 공격력 (레벨 적용)
    /// </summary>
    public float GetAttack(int level)
    {
        return _cachedWeapon != null ? _cachedWeapon.GetAttack(level) : 10f;
    }

    /// <summary>
    /// 현재 무기의 공격 속도
    /// </summary>
    public float GetAttackSpeed()
    {
        return _cachedWeapon != null ? _cachedWeapon.GetAttackSpeed() : 1f;
    }

    /// <summary>
    /// 현재 무기의 치명타 확률
    /// </summary>
    public float GetCriticalChance()
    {
        return _cachedWeapon != null ? _cachedWeapon.GetCriticalChance() : 5f;
    }

    /// <summary>
    /// 현재 무기의 치명타 데미지
    /// 반환: 배율 (2.0 = 100% 추가 = 2배 데미지)
    /// </summary>
    public float GetCriticalDamage()
    {
        return _cachedWeapon != null ? _cachedWeapon.GetCriticalDamage() : 2.0f;
    }

    /// <summary>
    /// 현재 방어구의 HP (레벨 적용)
    /// </summary>
    public float GetHp(int level)
    {
        return _cachedArmor != null ? _cachedArmor.GetHp(level) : 100f;
    }

    /// <summary>
    /// 현재 방어구의 방어력 (레벨 적용)
    /// </summary>
    public float GetDefense(int level)
    {
        return _cachedArmor != null ? _cachedArmor.GetDefense(level) : 0f;
    }

    #endregion
}
