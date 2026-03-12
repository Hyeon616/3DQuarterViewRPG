using UnityEngine;
using UnityEditor;

public class StatTreeCanvasBuilder : EditorWindow
{
    private const string DATA_PATH = "Assets/Data/StatTree/";
    private const string PREFAB_PATH = "Assets/Prefabs/UI/StatTree/";

    private StatTreeData statTreeData;
    private StatTreeSettings settings;
    private StatTreeUI targetCanvas;
    private StatTierUI tierPrefab;
    private StatNodeUI nodePrefab;
    private bool autoLoaded = false;

    [MenuItem("Tools/Build 전투 특성 Canvas")]
    public static void ShowWindow()
    {
        var window = GetWindow<StatTreeCanvasBuilder>("전투 특성 Canvas Builder");
        window.AutoLoadResources();
    }

    private void OnEnable()
    {
        AutoLoadResources();
    }

    private void AutoLoadResources()
    {
        if (autoLoaded) return;

        // Data 자동 로드
        if (statTreeData == null)
        {
            var guids = AssetDatabase.FindAssets("t:StatTreeData", new[] { DATA_PATH });
            if (guids.Length > 0)
                statTreeData = AssetDatabase.LoadAssetAtPath<StatTreeData>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        // Settings 자동 로드
        if (settings == null)
        {
            var guids = AssetDatabase.FindAssets("t:StatTreeSettings", new[] { DATA_PATH });
            if (guids.Length > 0)
                settings = AssetDatabase.LoadAssetAtPath<StatTreeSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        // Prefab 자동 로드
        if (tierPrefab == null)
            tierPrefab = AssetDatabase.LoadAssetAtPath<StatTierUI>(PREFAB_PATH + "StatTierUI.prefab");

        if (nodePrefab == null)
            nodePrefab = AssetDatabase.LoadAssetAtPath<StatNodeUI>(PREFAB_PATH + "StatNodeUI.prefab");

        // Scene에서 StatTreeUI 찾기
        if (targetCanvas == null)
            targetCanvas = FindAnyObjectByType<StatTreeUI>();

        autoLoaded = true;
    }

    private void OnGUI()
    {
        GUILayout.Label("전투 특성 Canvas Builder", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 자동 로드 상태 표시
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("자동 로드 경로:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Data: {DATA_PATH}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField($"                    Prefab: {PREFAB_PATH}", EditorStyles.miniLabel);

        GUILayout.Space(10);

        // 필드 표시 (자동 로드되었지만 수동 변경 가능)
        statTreeData = (StatTreeData)EditorGUILayout.ObjectField("Stat Tree Data", statTreeData, typeof(StatTreeData), false);
        settings = (StatTreeSettings)EditorGUILayout.ObjectField("Settings", settings, typeof(StatTreeSettings), false);
        targetCanvas = (StatTreeUI)EditorGUILayout.ObjectField("Target Canvas (Scene)", targetCanvas, typeof(StatTreeUI), true);
        tierPrefab = (StatTierUI)EditorGUILayout.ObjectField("Tier Prefab", tierPrefab, typeof(StatTierUI), false);
        nodePrefab = (StatNodeUI)EditorGUILayout.ObjectField("Node Prefab", nodePrefab, typeof(StatNodeUI), false);

        GUILayout.Space(10);

        // 새로고침 버튼
        if (GUILayout.Button("리소스 새로고침"))
        {
            autoLoaded = false;
            AutoLoadResources();
        }

        GUILayout.Space(10);

        // 누락 리소스 안내
        if (statTreeData == null || tierPrefab == null || nodePrefab == null)
        {
            EditorGUILayout.HelpBox(
                "일부 리소스를 찾을 수 없습니다.\n" +
                "1. Tools > Create Stat Tree UI 먼저 실행\n" +
                "2. Tools > Generate StatTree Assets 실행",
                MessageType.Warning);
        }

        if (targetCanvas == null)
        {
            EditorGUILayout.HelpBox(
                "Scene에 StatTreeCanvas가 없습니다.\n" +
                "Prefabs/UI/StatTree/StatTreeCanvas.prefab을 Scene에 배치하세요.",
                MessageType.Warning);
        }

        GUILayout.Space(10);

        GUI.enabled = statTreeData != null && targetCanvas != null && tierPrefab != null && nodePrefab != null;

        if (GUILayout.Button("Build Canvas", GUILayout.Height(40)))
        {
            BuildCanvas();
        }

        GUI.enabled = true;
    }

    private void BuildCanvas()
    {
        // TiersContainer 찾기
        SerializedObject canvasSO = new SerializedObject(targetCanvas);
        Transform tiersContainer = canvasSO.FindProperty("tiersContainer").objectReferenceValue as Transform;

        if (tiersContainer == null)
        {
            Debug.LogError("TiersContainer not found in StatTreeUI!");
            return;
        }

        // 기존 자식 제거
        while (tiersContainer.childCount > 0)
        {
            DestroyImmediate(tiersContainer.GetChild(0).gameObject);
        }

        // Tier별로 생성
        for (int tierIdx = 0; tierIdx < statTreeData.TierCount; tierIdx++)
        {
            var tierData = statTreeData.GetTier(tierIdx);
            if (tierData == null) continue;

            // Tier UI 생성
            var tierUI = (StatTierUI)PrefabUtility.InstantiatePrefab(tierPrefab, tiersContainer);
            tierUI.name = $"Tier_{tierIdx}_{tierData.TierName}";

            // Tier 필드 설정
            SerializedObject tierSO = new SerializedObject(tierUI);

            var tierNameText = tierSO.FindProperty("tierNameText").objectReferenceValue as TMPro.TextMeshProUGUI;
            if (tierNameText != null)
                tierNameText.text = tierData.TierName;

            var progressText = tierSO.FindProperty("progressText").objectReferenceValue as TMPro.TextMeshProUGUI;
            if (progressText != null)
                progressText.text = $"0/{tierData.MaxTierPoints}";

            // Settings에서 스프라이트 적용
            if (settings != null)
            {
                var leftInfoBackground = tierSO.FindProperty("leftInfoBackground").objectReferenceValue as UnityEngine.UI.Image;
                if (leftInfoBackground != null && settings.LeftInfoBackgroundSprite != null)
                    leftInfoBackground.sprite = settings.LeftInfoBackgroundSprite;

                var lockIcon = tierSO.FindProperty("lockIcon").objectReferenceValue as UnityEngine.UI.Image;
                if (lockIcon != null && settings.LockIconSprite != null)
                    lockIcon.sprite = settings.LockIconSprite;
            }

            Transform nodesContainer = tierSO.FindProperty("nodesContainer").objectReferenceValue as Transform;

            // 노드 생성
            if (nodesContainer != null && tierData.Nodes != null)
            {
                for (int nodeIdx = 0; nodeIdx < tierData.Nodes.Length; nodeIdx++)
                {
                    var nodeData = tierData.Nodes[nodeIdx];
                    if (nodeData == null) continue;

                    // Node UI 생성
                    var nodeUI = (StatNodeUI)PrefabUtility.InstantiatePrefab(nodePrefab, nodesContainer);
                    nodeUI.name = $"Node_{nodeIdx}_{nodeData.NodeName}";

                    // Node 필드 설정
                    SerializedObject nodeSO = new SerializedObject(nodeUI);

                    var pointsText = nodeSO.FindProperty("pointsText").objectReferenceValue as TMPro.TextMeshProUGUI;
                    if (pointsText != null)
                        pointsText.text = $"0/{nodeData.MaxPoints}";

                    var iconImage = nodeSO.FindProperty("iconImage").objectReferenceValue as UnityEngine.UI.Image;
                    if (iconImage != null && nodeData.Icon != null)
                        iconImage.sprite = nodeData.Icon;

                    var costText = nodeSO.FindProperty("costText").objectReferenceValue as TMPro.TextMeshProUGUI;
                    if (costText != null)
                        costText.text = $"{nodeData.CostPerPoint}p";

                    nodeSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(nodeUI);
                }
            }

            tierSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(tierUI);
        }

        EditorUtility.SetDirty(targetCanvas);

        Debug.Log($"전투 특성 Canvas 빌드 완료! Tier {statTreeData.TierCount}개, 총 노드 {CountTotalNodes()}개");
    }

    private int CountTotalNodes()
    {
        int count = 0;
        for (int i = 0; i < statTreeData.TierCount; i++)
        {
            var tier = statTreeData.GetTier(i);
            if (tier?.Nodes != null)
                count += tier.Nodes.Length;
        }
        return count;
    }
}
