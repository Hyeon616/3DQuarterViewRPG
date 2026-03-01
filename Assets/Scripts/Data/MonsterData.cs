using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Data/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string monsterName;

    [Header("스탯")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float attack = 10f;
    [SerializeField] private float defense = 0f;

    public string MonsterName => monsterName;
    public float MaxHealth => maxHealth;
    public float Attack => attack;
    public float Defense => defense;
}