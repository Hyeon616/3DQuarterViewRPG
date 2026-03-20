using UnityEngine;

/// <summary>
/// 무기 타입별 배율 정의
/// 각 무기 타입마다 하나의 에셋 생성
/// </summary>
[CreateAssetMenu(fileName = "WeaponType_", menuName = "Combat/Equipment/Weapon Type Data")]
public class WeaponTypeData : ScriptableObject
{
    [Header("타입 정보")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private string displayName;

    [Header("배율")]
    [SerializeField] private float attackMultiplier = 1f;
    [SerializeField] private float attackSpeedMultiplier = 1f;
    [SerializeField] private float criticalChanceMultiplier = 1f;
    [SerializeField] private float criticalDamageMultiplier = 1f;

    public WeaponType WeaponType => weaponType;
    public string DisplayName => displayName;
    public float AttackMultiplier => attackMultiplier;
    public float AttackSpeedMultiplier => attackSpeedMultiplier;
    public float CriticalChanceMultiplier => criticalChanceMultiplier;
    public float CriticalDamageMultiplier => criticalDamageMultiplier;
}
