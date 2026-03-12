using UnityEngine;

[CreateAssetMenu(fileName = "CombatStat", menuName = "Combat/Combat Stat")]
public class CombatStat : ScriptableObject
{
    [Header("기본 스탯")]
    [SerializeField] private float baseHp = 100f;
    [SerializeField] private float baseAttack = 10f;
    [SerializeField] private float baseDefense = 5f;
    [SerializeField] private float baseCriticalChance = 0.05f;
    [SerializeField] private float baseCriticalDamage = 1.5f;

    [Header("레벨당 성장치")]
    [SerializeField] private float hpPerLevel = 10f;
    [SerializeField] private float attackPerLevel = 1f;
    [SerializeField] private float defensePerLevel = 0.5f;

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

    // 기본 스탯
    public float BaseHp => baseHp;
    public float BaseAttack => baseAttack;
    public float BaseDefense => baseDefense;
    public float BaseCriticalChance => baseCriticalChance;
    public float BaseCriticalDamage => baseCriticalDamage;

    // 레벨당 성장치
    public float HpPerLevel => hpPerLevel;
    public float AttackPerLevel => attackPerLevel;
    public float DefensePerLevel => defensePerLevel;

    // 스탯 배수
    public float HpMultiplier => hpMultiplier;
    public float AttackMultiplier => attackMultiplier;
    public float DefenseMultiplier => defenseMultiplier;
    public float CriticalChanceMultiplier => criticalChanceMultiplier;
    public float CriticalDamageMultiplier => criticalDamageMultiplier;

    public ResourceData[] Resources => resources;
    public IdentityData Identity => identity;

    /// <summary>
    /// 레벨에 따른 최종 스탯 계산
    /// </summary>
    public float GetHp(int level) => (baseHp + hpPerLevel * (level - 1)) * hpMultiplier;
    public float GetAttack(int level) => (baseAttack + attackPerLevel * (level - 1)) * attackMultiplier;
    public float GetDefense(int level) => (baseDefense + defensePerLevel * (level - 1)) * defenseMultiplier;
    public float GetCriticalChance() => baseCriticalChance * criticalChanceMultiplier;
    public float GetCriticalDamage() => baseCriticalDamage * criticalDamageMultiplier;
}