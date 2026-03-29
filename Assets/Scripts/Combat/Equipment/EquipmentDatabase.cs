using UnityEngine;

/// <summary>
/// 장비 타입별 배율 데이터베이스
/// WeaponTypeData, ArmorTypeData 관리
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

    [Header("무기 타입 배율")]
    [SerializeField] private WeaponTypeData[] weaponTypes;

    [Header("방어구 타입 배율")]
    [SerializeField] private ArmorTypeData[] armorTypes;

    public WeaponTypeData[] WeaponTypes => weaponTypes;
    public ArmorTypeData[] ArmorTypes => armorTypes;

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
