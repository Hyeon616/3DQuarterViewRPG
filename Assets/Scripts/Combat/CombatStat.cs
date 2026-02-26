using UnityEngine;

[CreateAssetMenu(fileName = "CombatStat", menuName = "Combat/Combat Stat")]
public class CombatStat : ScriptableObject
{
    [Header("스탯 배수")]
    [SerializeField] private float hpMultiplier = 1f;
    [SerializeField] private float attackMultiplier = 1f;
    [SerializeField] private float defenseMultiplier = 1f;
    [SerializeField] private float criticalChanceMultiplier = 1f;
    [SerializeField] private float criticalDamageMultiplier = 1f;

    [Header("자원")]
    [SerializeField] private ResourceData[] resources;

    [Header("아이덴티티")]
    [SerializeField] private IdentityData identity;

    public float HpMultiplier => hpMultiplier;
    public float AttackMultiplier => attackMultiplier;
    public float DefenseMultiplier => defenseMultiplier;
    public float CriticalChanceMultiplier => criticalChanceMultiplier;
    public float CriticalDamageMultiplier => criticalDamageMultiplier;
    public ResourceData[] Resources => resources;
    public IdentityData Identity => identity;
}