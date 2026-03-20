using Mirror;
using UnityEngine;
using System;

/// <summary>
/// 플레이어 장비 관리
/// 장비 ID를 네트워크 동기화하고, 변경 시 스탯 재계산
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerEquipment : NetworkBehaviour
{
    [Header("기본 장비 (테스트용)")]
    [SerializeField] private WeaponData defaultWeapon;
    [SerializeField] private ArmorData defaultArmor;

    // -1 = 장비 없음
    [SyncVar(hook = nameof(OnWeaponChanged))]
    private int _weaponId = -1;

    [SyncVar(hook = nameof(OnArmorChanged))]
    private int _armorId = -1;

    private WeaponData _cachedWeapon;
    private ArmorData _cachedArmor;

    public WeaponData CurrentWeapon => _cachedWeapon;
    public ArmorData CurrentArmor => _cachedArmor;

    public event Action OnEquipmentChanged;

    public override void OnStartServer()
    {
        base.OnStartServer();
        AutoAssignDefaults();

        // 기본 장비 설정
        if (defaultWeapon != null)
        {
            int id = EquipmentDatabase.Instance?.GetWeaponId(defaultWeapon) ?? -1;
            if (id >= 0) _weaponId = id;
        }
        if (defaultArmor != null)
        {
            int id = EquipmentDatabase.Instance?.GetArmorId(defaultArmor) ?? -1;
            if (id >= 0) _armorId = id;
        }

        CacheEquipment();
    }

    private void AutoAssignDefaults()
    {
        var defaults = PlayerDefaultSettings.Instance;
        if (defaults == null) return;

        if (defaultWeapon == null)
            defaultWeapon = defaults.DefaultWeapon;
        if (defaultArmor == null)
            defaultArmor = defaults.DefaultArmor;
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
        var db = EquipmentDatabase.Instance;
        if (db == null) return;

        _cachedWeapon = db.GetWeapon(_weaponId);
        _cachedArmor = db.GetArmor(_armorId);
    }

    /// <summary>
    /// 무기 장착 (서버 호출)
    /// </summary>
    [Server]
    public void EquipWeapon(WeaponData weapon)
    {
        int id = EquipmentDatabase.Instance?.GetWeaponId(weapon) ?? -1;
        EquipWeaponById(id);
    }

    /// <summary>
    /// 무기 장착 by ID (서버 호출)
    /// </summary>
    [Server]
    public void EquipWeaponById(int weaponId)
    {
        _weaponId = weaponId;
        CacheEquipment();
        OnEquipmentChanged?.Invoke();
    }

    /// <summary>
    /// 방어구 장착 (서버 호출)
    /// </summary>
    [Server]
    public void EquipArmor(ArmorData armor)
    {
        int id = EquipmentDatabase.Instance?.GetArmorId(armor) ?? -1;
        EquipArmorById(id);
    }

    /// <summary>
    /// 방어구 장착 by ID (서버 호출)
    /// </summary>
    [Server]
    public void EquipArmorById(int armorId)
    {
        _armorId = armorId;
        CacheEquipment();
        OnEquipmentChanged?.Invoke();
    }

    /// <summary>
    /// 무기 해제
    /// </summary>
    [Server]
    public void UnequipWeapon()
    {
        _weaponId = -1;
        CacheEquipment();
        OnEquipmentChanged?.Invoke();
    }

    /// <summary>
    /// 방어구 해제
    /// </summary>
    [Server]
    public void UnequipArmor()
    {
        _armorId = -1;
        CacheEquipment();
        OnEquipmentChanged?.Invoke();
    }

    /// <summary>
    /// 클라이언트에서 무기 장착 요청
    /// </summary>
    [Command]
    public void CmdEquipWeapon(int weaponId)
    {
        // 유효성 검사 (레벨 체크 등 추가 가능)
        var weapon = EquipmentDatabase.Instance?.GetWeapon(weaponId);
        if (weapon == null) return;

        EquipWeaponById(weaponId);
    }

    /// <summary>
    /// 클라이언트에서 방어구 장착 요청
    /// </summary>
    [Command]
    public void CmdEquipArmor(int armorId)
    {
        var armor = EquipmentDatabase.Instance?.GetArmor(armorId);
        if (armor == null) return;

        EquipArmorById(armorId);
    }

    #region Stat Getters

    /// <summary>
    /// 현재 무기의 공격력 (레벨 적용)
    /// </summary>
    public float GetAttack(int level)
    {
        return _cachedWeapon != null ? _cachedWeapon.GetAttack(level) : 0f;
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
        return _cachedWeapon != null ? _cachedWeapon.GetCriticalChance() : 0f;
    }

    /// <summary>
    /// 현재 무기의 치명타 데미지
    /// </summary>
    public float GetCriticalDamage()
    {
        return _cachedWeapon != null ? _cachedWeapon.GetCriticalDamage() : 1f;
    }

    /// <summary>
    /// 현재 방어구의 HP (레벨 적용)
    /// </summary>
    public float GetHp(int level)
    {
        return _cachedArmor != null ? _cachedArmor.GetHp(level) : 0f;
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
