using UnityEngine;

[CreateAssetMenu(fileName = "StatNode", menuName = "Combat/Stat Node")]
public class StatNodeData : ScriptableObject
{
    [Header("노드 정보")]
    [SerializeField] private string nodeName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;

    [Header("효과 (포인트당)")]
    [SerializeField] private StatModifier[] modifiersPerPoint;

    [Header("투자 제한")]
    [SerializeField] private int maxPoints = 5;
    [SerializeField] private int costPerPoint = 1; // 1회 투자 시 소모되는 포인트

    public string NodeName => nodeName;
    public string Description => description;
    public Sprite Icon => icon;
    public StatModifier[] ModifiersPerPoint => modifiersPerPoint;
    public int MaxPoints => maxPoints;
    public int CostPerPoint => costPerPoint;

    public StatModifier[] GetTotalModifiers(int investedPoints)
    {
        if (modifiersPerPoint == null || modifiersPerPoint.Length == 0)
            return System.Array.Empty<StatModifier>();

        var result = new StatModifier[modifiersPerPoint.Length];
        for (int i = 0; i < modifiersPerPoint.Length; i++)
        {
            result[i] = new StatModifier(
                modifiersPerPoint[i].StatType,
                modifiersPerPoint[i].Value * investedPoints
            );
        }
        return result;
    }
}
