using UnityEngine;
using UnityEditor;
using System.IO;

public class StatTreeAssetGenerator : EditorWindow
{
    [MenuItem("Tools/Generate StatTree Assets")]
    public static void GenerateAssets()
    {
        string basePath = "Assets/Data/StatTree";
        string nodesPath = basePath + "/Nodes";

        // 폴더 생성
        if (!AssetDatabase.IsValidFolder("Assets/Data/StatTree"))
        {
            AssetDatabase.CreateFolder("Assets/Data", "StatTree");
        }
        if (!AssetDatabase.IsValidFolder(nodesPath))
        {
            AssetDatabase.CreateFolder(basePath, "Nodes");
        }

        // ===== Tier 1 노드 (3개) - costPerPoint: 1, maxPoints: 30 =====
        // 티어 전체 최대: 40 포인트
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

        // ===== Tier 2 노드 (3개) - costPerPoint: 10 =====
        // 티어 전체 최대: 30 포인트 (3번 투자 가능)
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

        // ===== Tier 3 노드 (2개) - costPerPoint: 15 =====
        // 티어 전체 최대: 30 포인트 (2번 투자 가능)
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

        // ===== StatTree 생성 =====
        var statTree = ScriptableObject.CreateInstance<StatTreeData>();

        // SerializedObject로 private 필드 접근
        SerializedObject so = new SerializedObject(statTree);
        so.FindProperty("treeName").stringValue = "전투 특성";

        var tiersProperty = so.FindProperty("tiers");
        tiersProperty.arraySize = 3;

        // Tier 1: 최대 40포인트, 40포인트 투자해야 Tier 2 해금
        SetupTier(tiersProperty.GetArrayElementAtIndex(0), "기초", 40, 40,
            new StatNodeData[] { node_T1_CritChance, node_T1_Damage, node_T1_AtkSpd_Cool });

        // Tier 2: 최대 30포인트, 30포인트 투자해야 Tier 3 해금
        SetupTier(tiersProperty.GetArrayElementAtIndex(1), "중급", 30, 30,
            new StatNodeData[] { node_T2_Mana_Cool, node_T2_Crit_Damage, node_T2_Damage });

        // Tier 3: 최대 30포인트, 마지막 티어 (해금 조건 없음)
        SetupTier(tiersProperty.GetArrayElementAtIndex(2), "고급", 30, 0,
            new StatNodeData[] { node_T3_Crit_CritDmg, node_T3_Damage_AtkSpd });

        so.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(statTree, basePath + "/CombatTraitTree.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("전투 특성 에셋 생성 완료!");
        Debug.Log($"- Tier 1: 노드 3개, 각 노드 maxPoints=30, costPerPoint=1, 해금 필요: 40포인트");
        Debug.Log($"- Tier 2: 노드 3개, 각 노드 costPerPoint=10, 해금 필요: 30포인트");
        Debug.Log($"- Tier 3: 노드 2개, 각 노드 costPerPoint=15");
        Debug.Log($"- 전체 테스트용 포인트: 100");

        // 생성된 에셋 선택
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
