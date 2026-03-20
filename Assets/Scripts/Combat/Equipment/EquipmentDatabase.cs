using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 장비 데이터를 관리하는 데이터베이스
/// Resources 폴더에 배치하여 자동 로드
/// </summary>
[CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "Combat/Equipment/Equipment Database")]
public class EquipmentDatabase : ScriptableObject
{
    private static EquipmentDatabase _instance;
    public static EquipmentDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<EquipmentDatabase>("EquipmentDatabase");
            }
            return _instance;
        }
    }

    [Header("무기 목록")]
    [SerializeField] private WeaponData[] weapons;

    [Header("방어구 목록")]
    [SerializeField] private ArmorData[] armors;

    [Header("무기 타입 목록")]
    [SerializeField] private WeaponTypeData[] weaponTypes;

    [Header("방어구 타입 목록")]
    [SerializeField] private ArmorTypeData[] armorTypes;

    private Dictionary<int, WeaponData> _weaponLookup;
    private Dictionary<int, ArmorData> _armorLookup;

    public WeaponData[] Weapons => weapons;
    public ArmorData[] Armors => armors;
    public WeaponTypeData[] WeaponTypes => weaponTypes;
    public ArmorTypeData[] ArmorTypes => armorTypes;

    private void OnEnable()
    {
        BuildLookupTables();
    }

    private void BuildLookupTables()
    {
        _weaponLookup = new Dictionary<int, WeaponData>();
        _armorLookup = new Dictionary<int, ArmorData>();

        if (weapons != null)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                    _weaponLookup[i] = weapons[i];
            }
        }

        if (armors != null)
        {
            for (int i = 0; i < armors.Length; i++)
            {
                if (armors[i] != null)
                    _armorLookup[i] = armors[i];
            }
        }
    }

    /// <summary>
    /// ID로 무기 데이터 조회 (-1 = 없음)
    /// </summary>
    public WeaponData GetWeapon(int id)
    {
        if (_weaponLookup == null) BuildLookupTables();
        return _weaponLookup.TryGetValue(id, out var weapon) ? weapon : null;
    }

    /// <summary>
    /// ID로 방어구 데이터 조회 (-1 = 없음)
    /// </summary>
    public ArmorData GetArmor(int id)
    {
        if (_armorLookup == null) BuildLookupTables();
        return _armorLookup.TryGetValue(id, out var armor) ? armor : null;
    }

    /// <summary>
    /// 무기 데이터의 ID 조회
    /// </summary>
    public int GetWeaponId(WeaponData weapon)
    {
        if (weapons == null || weapon == null) return -1;
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == weapon) return i;
        }
        return -1;
    }

    /// <summary>
    /// 방어구 데이터의 ID 조회
    /// </summary>
    public int GetArmorId(ArmorData armor)
    {
        if (armors == null || armor == null) return -1;
        for (int i = 0; i < armors.Length; i++)
        {
            if (armors[i] == armor) return i;
        }
        return -1;
    }

    /// <summary>
    /// 특정 무기 타입의 WeaponTypeData 조회
    /// </summary>
    public WeaponTypeData GetWeaponTypeData(WeaponType type)
    {
        if (weaponTypes == null) return null;
        foreach (var typeData in weaponTypes)
        {
            if (typeData != null && typeData.WeaponType == type)
                return typeData;
        }
        return null;
    }

    /// <summary>
    /// 특정 방어구 타입의 ArmorTypeData 조회
    /// </summary>
    public ArmorTypeData GetArmorTypeData(ArmorType type)
    {
        if (armorTypes == null) return null;
        foreach (var typeData in armorTypes)
        {
            if (typeData != null && typeData.ArmorType == type)
                return typeData;
        }
        return null;
    }
}
