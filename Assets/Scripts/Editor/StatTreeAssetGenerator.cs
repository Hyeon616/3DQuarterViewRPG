using UnityEngine;
using UnityEditor;

public class StatTreeAssetGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Combat Traits Assets")]
    public static void GenerateAssets()
    {
        string basePath = "Assets/Data/StatTree";
        string nodesPath = basePath + "/Nodes";

        if (!AssetDatabase.IsValidFolder("Assets/Data/StatTree"))
        {
            AssetDatabase.CreateFolder("Assets/Data", "StatTree");
        }
        if (!AssetDatabase.IsValidFolder(nodesPath))
        {
            AssetDatabase.CreateFolder(basePath, "Nodes");
        }

        // Tier 1 nodes (3) - costPerPoint: 1, maxPoints: 30, tier max: 40
        var node_T1_CritChance = CreateNode(nodesPath, "T1_CritChance", "치명타 확률",
            "치명타 확률을 증가시킵니다.",
            new StatModifier[] { new StatModifier(StatType.CriticalChance, 0.5f) }, 30, 1);

        var node_T1_Damage = CreateNode(nodesPath, "T1_Damage", "데미지 증가",
            "데미지를 증가시킵니다.",
            new StatModifier[] { new StatModifier(StatType.DamageIncrease, 0.5f) }, 30, 1);

        var node_T1_AtkSpd_Cool = CreateNode(nodesPath, "T1_AtkSpd_Cool", "공속 + 쿨감",
            "공격 속도와 쿨타임 감소를 증가시킵니다.",
            new StatModifier[] {
                new StatModifier(StatType.AttackSpeed, 0.3f),
                new StatModifier(StatType.CooldownReduction, 0.3f)
            }, 30, 1);

        // Tier 2 nodes (3) - costPerPoint: 10, tier max: 30
        var node_T2_Mana_Cool = CreateNode(nodesPath, "T2_Mana_Cool", "마나 + 쿨감",
            "마나 소모량과 쿨타임을 감소시킵니다.",
            new StatModifier[] {
                new StatModifier(StatType.ManaReduction, 3f),
                new StatModifier(StatType.CooldownReduction, 2f)
            }, 3, 10);

        var node_T2_Crit_Damage = CreateNode(nodesPath, "T2_Crit_Damage", "치확 + 데미지",
            "치명타 확률과 데미지를 증가시킵니다.",
            new StatModifier[] {
                new StatModifier(StatType.CriticalChance, 2f),
                new StatModifier(StatType.DamageIncrease, 2f)
            }, 3, 10);

        var node_T2_Damage = CreateNode(nodesPath, "T2_Damage", "데미지",
            "데미지를 증가시킵니다.",
            new StatModifier[] { new StatModifier(StatType.DamageIncrease, 5f) }, 3, 10);

        // Tier 3 nodes (2) - costPerPoint: 15, tier max: 30
        var node_T3_Crit_CritDmg = CreateNode(nodesPath, "T3_Crit_CritDmg", "치확 + 치피",
            "치명타 확률과 치명타 피해를 증가시킵니다.",
            new StatModifier[] {
                new StatModifier(StatType.CriticalChance, 5f),
                new StatModifier(StatType.CriticalDamage, 10f)
            }, 2, 15);

        var node_T3_Damage_AtkSpd = CreateNode(nodesPath, "T3_Damage_AtkSpd", "데미지 + 공속",
            "데미지와 공격 속도를 증가시킵니다.",
            new StatModifier[] {
                new StatModifier(StatType.DamageIncrease, 8f),
                new StatModifier(StatType.AttackSpeed, 5f)
            }, 2, 15);

        var statTree = ScriptableObject.CreateInstance<StatTreeData>();

        SerializedObject so = new SerializedObject(statTree);
        so.FindProperty("treeName").stringValue = "Combat Traits";

        var tiersProperty = so.FindProperty("tiers");
        tiersProperty.arraySize = 3;

        SetupTier(tiersProperty.GetArrayElementAtIndex(0), "기초", 40, 40,
            new StatNodeData[] { node_T1_CritChance, node_T1_Damage, node_T1_AtkSpd_Cool });

        SetupTier(tiersProperty.GetArrayElementAtIndex(1), "중급", 30, 30,
            new StatNodeData[] { node_T2_Mana_Cool, node_T2_Crit_Damage, node_T2_Damage });

        SetupTier(tiersProperty.GetArrayElementAtIndex(2), "고급", 30, 0,
            new StatNodeData[] { node_T3_Crit_CritDmg, node_T3_Damage_AtkSpd });

        so.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(statTree, basePath + "/CombatTraitTree.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Combat Traits assets generated!");
        Selection.activeObject = statTree;
    }

    private static StatNodeData CreateNode(string path, string fileName, string nodeName, string description, StatModifier[] modifiers, int maxPoints, int costPerPoint = 1)
    {
        var node = ScriptableObject.CreateInstance<StatNodeData>();

        SerializedObject so = new SerializedObject(node);
        so.FindProperty("nodeName").stringValue = nodeName;
        so.FindProperty("description").stringValue = description;
        so.FindProperty("maxPoints").intValue = maxPoints;
        so.FindProperty("costPerPoint").intValue = costPerPoint;

        var modifiersProperty = so.FindProperty("modifiersPerPoint");
        modifiersProperty.arraySize = modifiers.Length;

        for (int i = 0; i < modifiers.Length; i++)
        {
            var element = modifiersProperty.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("statType").enumValueIndex = (int)modifiers[i].StatType;
            element.FindPropertyRelative("value").floatValue = modifiers[i].Value;
        }

        so.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(node, $"{path}/{fileName}.asset");
        return node;
    }

    private static void SetupTier(SerializedProperty tierProperty, string tierName, int maxTierPoints, int requiredPointsToUnlock, StatNodeData[] nodes)
    {
        tierProperty.FindPropertyRelative("tierName").stringValue = tierName;
        tierProperty.FindPropertyRelative("maxTierPoints").intValue = maxTierPoints;
        tierProperty.FindPropertyRelative("requiredPointsToUnlockNext").intValue = requiredPointsToUnlock;

        var nodesProperty = tierProperty.FindPropertyRelative("nodes");
        nodesProperty.arraySize = nodes.Length;

        for (int i = 0; i < nodes.Length; i++)
        {
            nodesProperty.GetArrayElementAtIndex(i).objectReferenceValue = nodes[i];
        }
    }
}
