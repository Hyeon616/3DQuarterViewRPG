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

    [MenuItem("Tools/Build Combat Traits Canvas")]
    public static void ShowWindow()
    {
        var window = GetWindow<StatTreeCanvasBuilder>("Combat Traits Canvas Builder");
        window.AutoLoadResources();
    }

    private void OnEnable()
    {
        AutoLoadResources();
    }

    private void AutoLoadResources()
    {
        if (autoLoaded) return;

        if (statTreeData == null)
        {
            var guids = AssetDatabase.FindAssets("t:StatTreeData", new[] { DATA_PATH });
            if (guids.Length > 0)
                statTreeData = AssetDatabase.LoadAssetAtPath<StatTreeData>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        if (settings == null)
        {
            var guids = AssetDatabase.FindAssets("t:StatTreeSettings", new[] { DATA_PATH });
            if (guids.Length > 0)
                settings = AssetDatabase.LoadAssetAtPath<StatTreeSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        if (tierPrefab == null)
            tierPrefab = AssetDatabase.LoadAssetAtPath<StatTierUI>(PREFAB_PATH + "StatTierUI.prefab");

        if (nodePrefab == null)
            nodePrefab = AssetDatabase.LoadAssetAtPath<StatNodeUI>(PREFAB_PATH + "StatNodeUI.prefab");

        if (targetCanvas == null)
            targetCanvas = FindAnyObjectByType<StatTreeUI>();

        autoLoaded = true;
    }

    private void OnGUI()
    {
        GUILayout.Label("Combat Traits Canvas Builder", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Auto Load Path:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Data: {DATA_PATH}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField($"                    Prefab: {PREFAB_PATH}", EditorStyles.miniLabel);

        GUILayout.Space(10);

        statTreeData = (StatTreeData)EditorGUILayout.ObjectField("Stat Tree Data", statTreeData, typeof(StatTreeData), false);
        settings = (StatTreeSettings)EditorGUILayout.ObjectField("Settings", settings, typeof(StatTreeSettings), false);
        targetCanvas = (StatTreeUI)EditorGUILayout.ObjectField("Target Canvas (Scene)", targetCanvas, typeof(StatTreeUI), true);
        tierPrefab = (StatTierUI)EditorGUILayout.ObjectField("Tier Prefab", tierPrefab, typeof(StatTierUI), false);
        nodePrefab = (StatNodeUI)EditorGUILayout.ObjectField("Node Prefab", nodePrefab, typeof(StatNodeUI), false);

        GUILayout.Space(10);

        if (GUILayout.Button("Refresh Resources"))
        {
            autoLoaded = false;
            AutoLoadResources();
        }

        GUILayout.Space(10);

        if (statTreeData == null || tierPrefab == null || nodePrefab == null)
        {
            EditorGUILayout.HelpBox(
                "Some resources not found.\n" +
                "1. Run Tools > Create Combat Traits UI first\n" +
                "2. Run Tools > Generate StatTree Assets",
                MessageType.Warning);
        }

        if (targetCanvas == null)
        {
            EditorGUILayout.HelpBox(
                "StatTreeCanvas not found in Scene.\n" +
                "Place Prefabs/UI/StatTree/StatTreeCanvas.prefab in the scene.",
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
        var canvas = targetCanvas.GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
        {
            if (Camera.main != null)
            {
                canvas.worldCamera = Camera.main;
                EditorUtility.SetDirty(canvas);
            }
            else
            {
                Debug.LogWarning("Main Camera not found. Assign Render Camera manually.");
            }
        }

        SerializedObject canvasSO = new SerializedObject(targetCanvas);
        Transform tiersContainer = canvasSO.FindProperty("tiersContainer").objectReferenceValue as Transform;

        if (tiersContainer == null)
        {
            Debug.LogError("TiersContainer not found in StatTreeUI!");
            return;
        }

        while (tiersContainer.childCount > 0)
        {
            DestroyImmediate(tiersContainer.GetChild(0).gameObject);
        }

        for (int tierIdx = 0; tierIdx < statTreeData.TierCount; tierIdx++)
        {
            var tierData = statTreeData.GetTier(tierIdx);
            if (tierData == null) continue;

            var tierUI = (StatTierUI)PrefabUtility.InstantiatePrefab(tierPrefab, tiersContainer);
            tierUI.name = $"Tier_{tierIdx}_{tierData.TierName}";

            SerializedObject tierSO = new SerializedObject(tierUI);

            var tierNameText = tierSO.FindProperty("tierNameText").objectReferenceValue as TMPro.TextMeshProUGUI;
            if (tierNameText != null)
                tierNameText.text = tierData.TierName;

            var progressText = tierSO.FindProperty("progressText").objectReferenceValue as TMPro.TextMeshProUGUI;
            if (progressText != null)
                progressText.text = $"0/{tierData.MaxTierPoints}";

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

            if (nodesContainer != null && tierData.Nodes != null)
            {
                for (int nodeIdx = 0; nodeIdx < tierData.Nodes.Length; nodeIdx++)
                {
                    var nodeData = tierData.Nodes[nodeIdx];
                    if (nodeData == null) continue;

                    var nodeUI = (StatNodeUI)PrefabUtility.InstantiatePrefab(nodePrefab, nodesContainer);
                    nodeUI.name = $"Node_{nodeIdx}_{nodeData.NodeName}";

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

        Debug.Log($"Combat Traits Canvas build complete! {statTreeData.TierCount} tiers, {CountTotalNodes()} nodes total");
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
