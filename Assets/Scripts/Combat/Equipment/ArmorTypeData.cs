using UnityEngine;

/// <summary>
/// 방어구 타입별 배율 정의
/// 각 방어구 타입마다 하나의 에셋 생성
/// </summary>
[CreateAssetMenu(fileName = "ArmorType_", menuName = "Combat/Equipment/Armor Type Data")]
public class ArmorTypeData : ScriptableObject
{
    [Header("타입 정보")]
    [SerializeField] private ArmorType armorType;
    [SerializeField] private string displayName;

    [Header("배율")]
    [SerializeField] private float hpMultiplier = 1f;
    [SerializeField] private float defenseMultiplier = 1f;

    public ArmorType ArmorType => armorType;
    public string DisplayName => displayName;
    public float HpMultiplier => hpMultiplier;
    public float DefenseMultiplier => defenseMultiplier;
}
