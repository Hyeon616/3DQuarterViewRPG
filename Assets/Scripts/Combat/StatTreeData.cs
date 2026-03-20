using UnityEngine;

[System.Serializable]
public class StatTier
{
    [SerializeField] private string tierName;
    [SerializeField] private StatNodeData[] nodes;
    [SerializeField] private int maxTierPoints = 40;
    [SerializeField] private int requiredPointsToUnlockNext = 5;

    public string TierName => tierName;
    public StatNodeData[] Nodes => nodes;
    public int MaxTierPoints => maxTierPoints;
    public int RequiredPointsToUnlockNext => requiredPointsToUnlockNext;
}

[CreateAssetMenu(fileName = "StatTree", menuName = "Combat/Stat Tree")]
public class StatTreeData : ScriptableObject
{
    [SerializeField] private string treeName;
    [SerializeField] private StatTier[] tiers;

    public string TreeName => treeName;
    public StatTier[] Tiers => tiers;
    public int TierCount => tiers?.Length ?? 0;

    public StatTier GetTier(int tierIndex)
    {
        if (tiers == null || tierIndex < 0 || tierIndex >= tiers.Length)
            return null;
        return tiers[tierIndex];
    }

    public int GetNodeIndex(int tierIndex, StatNodeData node)
    {
        var tier = GetTier(tierIndex);
        if (tier?.Nodes == null) return -1;

        for (int i = 0; i < tier.Nodes.Length; i++)
        {
            if (tier.Nodes[i] == node) return i;
        }
        return -1;
    }
}
