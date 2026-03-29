using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Combat Traits (StatTree) 관련 에디터 도구 통합
/// - UI 프리팹 생성
/// - 캔버스 빌드
/// - 에셋 생성
/// </summary>
public class StatTreeTools : EditorWindow
{
    private const string DATA_PATH = "Assets/Data/StatTree/";
    private const string PREFAB_PATH = "Assets/Prefabs/UI/StatTree/";

    private StatTreeData statTreeData;
    private StatTreeSettings settings;
    private StatTreeUI targetCanvas;
    private StatTierUI tierPrefab;
    private StatNodeUI nodePrefab;
    private bool autoLoaded = false;

    private Vector2 scrollPosition;

    [MenuItem("Tools/StatTree/Open StatTree Tools", false, 0)]
    public static void ShowWindow()
    {
        var window = GetWindow<StatTreeTools>("StatTree Tools");
        window.AutoLoadResources();
    }

    #region Quick Actions (MenuItem)

    [MenuItem("Tools/StatTree/1. Create UI Prefabs", false, 100)]
    public static void CreateUIFromMenu()
    {
        CreateStatTreeUI();
    }

    [MenuItem("Tools/StatTree/2. Generate Assets", false, 101)]
    public static void GenerateAssetsFromMenu()
    {
        GenerateAssets();
    }

    [MenuItem("Tools/StatTree/3. Build Canvas", false, 102)]
    public static void BuildCanvasFromMenu()
    {
        var window = GetWindow<StatTreeTools>("StatTree Tools");
        window.AutoLoadResources();
        window.BuildCanvas();
    }

    #endregion

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
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("StatTree Tools", EditorStyles.boldLabel);
        GUILayout.Space(5);

        EditorGUILayout.HelpBox(
            "Combat Traits(전투 특성) 시스템 에디터 도구\n" +
            "1. UI 프리팹 생성 → 2. 에셋 생성 → 3. 캔버스 빌드",
            MessageType.Info);

        GUILayout.Space(10);

        // ========== Section 1: UI Prefab Creation ==========
        EditorGUILayout.LabelField("1. UI 프리팹 생성", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("StatTree UI 프리팹들을 생성합니다.", MessageType.None);

        if (GUILayout.Button("Create UI Prefabs", GUILayout.Height(30)))
        {
            CreateStatTreeUI();
        }

        GUILayout.Space(15);

        // ========== Section 2: Asset Generation ==========
        EditorGUILayout.LabelField("2. 에셋 생성", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("기본 StatTree 데이터 에셋을 생성합니다.", MessageType.None);

        if (GUILayout.Button("Generate Assets", GUILayout.Height(30)))
        {
            GenerateAssets();
        }

        GUILayout.Space(15);

        // ========== Section 3: Canvas Build ==========
        EditorGUILayout.LabelField("3. 캔버스 빌드", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Data Path:", EditorStyles.miniLabel, GUILayout.Width(70));
        EditorGUILayout.LabelField(DATA_PATH, EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Prefab Path:", EditorStyles.miniLabel, GUILayout.Width(70));
        EditorGUILayout.LabelField(PREFAB_PATH, EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        statTreeData = (StatTreeData)EditorGUILayout.ObjectField("Stat Tree Data", statTreeData, typeof(StatTreeData), false);
        settings = (StatTreeSettings)EditorGUILayout.ObjectField("Settings", settings, typeof(StatTreeSettings), false);
        targetCanvas = (StatTreeUI)EditorGUILayout.ObjectField("Target Canvas (Scene)", targetCanvas, typeof(StatTreeUI), true);
        tierPrefab = (StatTierUI)EditorGUILayout.ObjectField("Tier Prefab", tierPrefab, typeof(StatTierUI), false);
        nodePrefab = (StatNodeUI)EditorGUILayout.ObjectField("Node Prefab", nodePrefab, typeof(StatNodeUI), false);

        GUILayout.Space(5);

        if (GUILayout.Button("Refresh Resources"))
        {
            autoLoaded = false;
            AutoLoadResources();
        }

        GUILayout.Space(5);

        if (statTreeData == null || tierPrefab == null || nodePrefab == null)
        {
            EditorGUILayout.HelpBox(
                "리소스가 없습니다.\n" +
                "1. 먼저 'Create UI Prefabs' 실행\n" +
                "2. 그 다음 'Generate Assets' 실행",
                MessageType.Warning);
        }

        if (targetCanvas == null)
        {
            EditorGUILayout.HelpBox(
                "StatTreeCanvas가 씬에 없습니다.\n" +
                "Prefabs/UI/StatTree/StatTreeCanvas.prefab을 씬에 배치하세요.",
                MessageType.Warning);
        }

        GUI.enabled = statTreeData != null && targetCanvas != null && tierPrefab != null && nodePrefab != null;

        if (GUILayout.Button("Build Canvas", GUILayout.Height(35)))
        {
            BuildCanvas();
        }

        GUI.enabled = true;

        EditorGUILayout.EndScrollView();
    }

    #region UI Prefab Creation

    public static void CreateStatTreeUI()
    {
        var canvasGO = new GameObject("StatTreeCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.sortingOrder = 10;

        var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var panelGO = CreatePanel(canvasGO.transform, "Panel", new Vector2(1280, 960));
        panelGO.GetComponent<Image>().color = new Color(0, 0, 0, 0);

        var panelBGSolid = new GameObject("BackgroundSolid");
        panelBGSolid.transform.SetParent(panelGO.transform, false);
        panelBGSolid.transform.SetAsFirstSibling();
        var panelBGSolidRect = panelBGSolid.AddComponent<RectTransform>();
        panelBGSolidRect.anchorMin = Vector2.zero;
        panelBGSolidRect.anchorMax = Vector2.one;
        panelBGSolidRect.offsetMin = Vector2.zero;
        panelBGSolidRect.offsetMax = Vector2.zero;
        var panelBGSolidImage = panelBGSolid.AddComponent<Image>();
        panelBGSolidImage.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        panelBGSolidImage.raycastTarget = false;

        var panelBGPattern = new GameObject("BackgroundPattern");
        panelBGPattern.transform.SetParent(panelGO.transform, false);
        panelBGPattern.transform.SetSiblingIndex(1);
        var panelBGPatternRect = panelBGPattern.AddComponent<RectTransform>();
        panelBGPatternRect.anchorMin = Vector2.zero;
        panelBGPatternRect.anchorMax = Vector2.one;
        panelBGPatternRect.offsetMin = Vector2.zero;
        panelBGPatternRect.offsetMax = Vector2.zero;
        var panelBGPatternImage = panelBGPattern.AddComponent<Image>();
        panelBGPatternImage.color = new Color(1f, 1f, 1f, 15f / 255f);
        panelBGPatternImage.type = Image.Type.Sliced;
        panelBGPatternImage.raycastTarget = false;

        var headerGO = CreatePanel(panelGO.transform, "Header", new Vector2(1240, 100));
        var headerRect = headerGO.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.5f, 1f);
        headerRect.anchorMax = new Vector2(0.5f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0, -20);

        var titleText = CreateText(headerGO.transform, "TitleText", "Combat Traits", 40);
        var titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        var pointsText = CreateText(panelGO.transform, "AvailablePointsText", "0/15", 28);
        var pointsRect = pointsText.GetComponent<RectTransform>();
        pointsRect.anchorMin = new Vector2(1f, 1f);
        pointsRect.anchorMax = new Vector2(1f, 1f);
        pointsRect.pivot = new Vector2(1f, 1f);
        pointsRect.anchoredPosition = new Vector2(-100, -40);
        pointsRect.sizeDelta = new Vector2(140, 50);
        pointsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        var scrollViewGO = new GameObject("TiersScrollView");
        scrollViewGO.transform.SetParent(panelGO.transform, false);
        var scrollViewRect = scrollViewGO.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0, 0);
        scrollViewRect.anchorMax = new Vector2(1, 1);
        scrollViewRect.offsetMin = new Vector2(30, 40);
        scrollViewRect.offsetMax = new Vector2(-30, -130);
        scrollViewGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        var scrollRect = scrollViewGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 30f;

        var viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform, false);
        var viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportGO.AddComponent<Image>().color = new Color(1, 1, 1, 1);
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = viewportRect;

        var tiersContainer = new GameObject("TiersContainer");
        tiersContainer.transform.SetParent(viewportGO.transform, false);
        var tiersRect = tiersContainer.AddComponent<RectTransform>();
        tiersRect.anchorMin = new Vector2(0, 1);
        tiersRect.anchorMax = new Vector2(1, 1);
        tiersRect.pivot = new Vector2(0.5f, 1);
        tiersRect.anchoredPosition = Vector2.zero;
        tiersRect.sizeDelta = new Vector2(0, 0);
        var tiersLayout = tiersContainer.AddComponent<VerticalLayoutGroup>();
        tiersLayout.spacing = 15;
        tiersLayout.padding = new RectOffset(10, 10, 10, 10);
        tiersLayout.childAlignment = TextAnchor.UpperCenter;
        tiersLayout.childControlHeight = false;
        tiersLayout.childControlWidth = true;
        tiersLayout.childForceExpandHeight = false;
        tiersLayout.childForceExpandWidth = true;
        tiersContainer.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = tiersRect;

        var closeBtn = CreateButton(panelGO.transform, "CloseButton", "X", new Vector2(70, 70));
        var closeBtnRect = closeBtn.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(1, 1);
        closeBtnRect.anchorMax = new Vector2(1, 1);
        closeBtnRect.pivot = new Vector2(1, 1);
        closeBtnRect.anchoredPosition = new Vector2(-20, -20);
        var closeBtnText = closeBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (closeBtnText != null) closeBtnText.fontSize = 32;

        var tooltipGO = CreateTooltip(canvasGO.transform);

        var treeUI = canvasGO.AddComponent<StatTreeUI>();
        SerializedObject so = new SerializedObject(treeUI);
        so.FindProperty("panel").objectReferenceValue = panelGO;
        so.FindProperty("panelBackground").objectReferenceValue = panelBGPatternImage;
        so.FindProperty("titleText").objectReferenceValue = titleText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("availablePointsText").objectReferenceValue = pointsText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("tiersContainer").objectReferenceValue = tiersContainer.transform;
        so.FindProperty("closeButton").objectReferenceValue = closeBtn.GetComponent<Button>();
        so.FindProperty("nodeTooltip").objectReferenceValue = tooltipGO.GetComponent<StatNodeTooltip>();
        so.ApplyModifiedProperties();

        var tierPrefabGO = CreateTierPrefab();
        var nodePrefabGO = CreateNodePrefab();

        string prefabPath = "Assets/Prefabs/UI/StatTree/";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI/StatTree"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            AssetDatabase.CreateFolder("Assets/Prefabs/UI", "StatTree");
        }

        var tierPrefab = PrefabUtility.SaveAsPrefabAsset(tierPrefabGO, prefabPath + "StatTierUI.prefab");
        DestroyImmediate(tierPrefabGO);

        var nodePrefab = PrefabUtility.SaveAsPrefabAsset(nodePrefabGO, prefabPath + "StatNodeUI.prefab");
        DestroyImmediate(nodePrefabGO);

        so = new SerializedObject(treeUI);
        so.FindProperty("tierPrefab").objectReferenceValue = tierPrefab.GetComponent<StatTierUI>();
        so.FindProperty("nodePrefab").objectReferenceValue = nodePrefab.GetComponent<StatNodeUI>();
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(canvasGO, prefabPath + "StatTreeCanvas.prefab");

        Debug.Log("[StatTreeTools] UI prefabs created at: " + prefabPath);
        Selection.activeGameObject = canvasGO;
    }

    private static GameObject CreateTierPrefab()
    {
        var tierGO = new GameObject("StatTierUI");
        var tierRect = tierGO.AddComponent<RectTransform>();
        tierRect.sizeDelta = new Vector2(1180, 180);

        var tierLayoutElement = tierGO.AddComponent<LayoutElement>();
        tierLayoutElement.minHeight = 180;
        tierLayoutElement.preferredHeight = 180;

        tierGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);

        var tierLayout = tierGO.AddComponent<HorizontalLayoutGroup>();
        tierLayout.spacing = 20;
        tierLayout.padding = new RectOffset(15, 15, 10, 10);
        tierLayout.childAlignment = TextAnchor.MiddleLeft;
        tierLayout.childControlHeight = true;
        tierLayout.childControlWidth = true;
        tierLayout.childForceExpandHeight = true;
        tierLayout.childForceExpandWidth = false;

        var settings = LoadSettings();

        var leftInfo = new GameObject("LeftInfo");
        leftInfo.transform.SetParent(tierGO.transform, false);
        var leftInfoRect = leftInfo.AddComponent<RectTransform>();
        leftInfoRect.sizeDelta = new Vector2(160, 160);
        var leftInfoLayoutElement = leftInfo.AddComponent<LayoutElement>();
        leftInfoLayoutElement.minWidth = 160;
        leftInfoLayoutElement.preferredWidth = 160;
        leftInfoLayoutElement.minHeight = 160;
        leftInfoLayoutElement.preferredHeight = 160;

        var leftInfoBG = leftInfo.AddComponent<Image>();
        leftInfoBG.color = Color.white;
        if (settings != null && settings.LeftInfoBackgroundSprite != null)
            leftInfoBG.sprite = settings.LeftInfoBackgroundSprite;

        var tierGlowGO = new GameObject("TierGlowImage");
        tierGlowGO.transform.SetParent(tierGO.transform, false);
        tierGlowGO.transform.SetAsFirstSibling();
        var tierGlowRect = tierGlowGO.AddComponent<RectTransform>();
        tierGlowRect.anchorMin = new Vector2(0, 0.5f);
        tierGlowRect.anchorMax = new Vector2(0, 0.5f);
        tierGlowRect.pivot = new Vector2(0, 0.5f);
        tierGlowRect.anchoredPosition = new Vector2(15, 0);
        tierGlowRect.sizeDelta = new Vector2(160, 160);
        tierGlowGO.AddComponent<LayoutElement>().ignoreLayout = true;
        var tierGlowImage = tierGlowGO.AddComponent<Image>();
        tierGlowImage.sprite = (settings != null && settings.GlowSprite != null)
            ? settings.GlowSprite
            : AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        tierGlowImage.color = new Color(0.3f, 0.6f, 1f, 1f);
        tierGlowImage.raycastTarget = false;
        tierGlowGO.SetActive(false);

        var tierName = new GameObject("TierNameText");
        tierName.transform.SetParent(leftInfo.transform, false);
        var tierNameRect = tierName.AddComponent<RectTransform>();
        tierNameRect.anchorMin = new Vector2(0.5f, 0.5f);
        tierNameRect.anchorMax = new Vector2(0.5f, 0.5f);
        tierNameRect.pivot = new Vector2(0.5f, 0.5f);
        tierNameRect.anchoredPosition = Vector2.zero;
        tierNameRect.sizeDelta = new Vector2(100, 50);
        var tierNameTMP = tierName.AddComponent<TextMeshProUGUI>();
        tierNameTMP.text = "Tier 1";
        tierNameTMP.fontSize = 24;
        tierNameTMP.alignment = TextAlignmentOptions.Center;
        tierNameTMP.color = Color.white;

        var progressText = new GameObject("ProgressText");
        progressText.transform.SetParent(leftInfo.transform, false);
        var progressRect = progressText.AddComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0.5f, 0f);
        progressRect.anchorMax = new Vector2(0.5f, 0f);
        progressRect.pivot = new Vector2(0.5f, 0f);
        progressRect.anchoredPosition = new Vector2(0, 10);
        progressRect.sizeDelta = new Vector2(100, 35);
        var progressTMP = progressText.AddComponent<TextMeshProUGUI>();
        progressTMP.text = "0/40";
        progressTMP.fontSize = 18;
        progressTMP.alignment = TextAlignmentOptions.Center;
        progressTMP.color = new Color(0.8f, 0.8f, 0.8f);

        var lockIcon = new GameObject("LockIcon");
        lockIcon.transform.SetParent(leftInfo.transform, false);
        var lockIconRect = lockIcon.AddComponent<RectTransform>();
        lockIconRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockIconRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockIconRect.pivot = new Vector2(0.5f, 0.5f);
        lockIconRect.anchoredPosition = Vector2.zero;
        lockIconRect.sizeDelta = new Vector2(50, 50);
        var lockIconImage = lockIcon.AddComponent<Image>();
        lockIconImage.color = Color.white;
        if (settings != null && settings.LockIconSprite != null)
            lockIconImage.sprite = settings.LockIconSprite;
        lockIcon.SetActive(false);

        var nodesContainer = new GameObject("NodesContainer");
        nodesContainer.transform.SetParent(tierGO.transform, false);
        nodesContainer.AddComponent<RectTransform>();
        nodesContainer.AddComponent<LayoutElement>().flexibleWidth = 1;
        var nodesLayout = nodesContainer.AddComponent<HorizontalLayoutGroup>();
        nodesLayout.spacing = 20;
        nodesLayout.childAlignment = TextAnchor.MiddleLeft;
        nodesLayout.childControlHeight = false;
        nodesLayout.childControlWidth = false;
        nodesLayout.childForceExpandHeight = false;
        nodesLayout.childForceExpandWidth = false;

        var tierUI = tierGO.AddComponent<StatTierUI>();
        SerializedObject so = new SerializedObject(tierUI);
        so.FindProperty("tierNameText").objectReferenceValue = tierNameTMP;
        so.FindProperty("progressText").objectReferenceValue = progressTMP;
        so.FindProperty("nodesContainer").objectReferenceValue = nodesContainer.transform;
        so.FindProperty("leftInfoBackground").objectReferenceValue = leftInfoBG;
        so.FindProperty("lockIcon").objectReferenceValue = lockIconImage;
        so.FindProperty("glowImage").objectReferenceValue = tierGlowImage;
        so.ApplyModifiedProperties();

        return tierGO;
    }

    private static GameObject CreateNodePrefab()
    {
        var settings = LoadSettings();

        var nodeGO = new GameObject("StatNodeUI");
        var nodeRect = nodeGO.AddComponent<RectTransform>();
        nodeRect.sizeDelta = new Vector2(140, 140);

        var raycastImage = nodeGO.AddComponent<Image>();
        raycastImage.color = new Color(0, 0, 0, 0);
        raycastImage.raycastTarget = true;

        var nodeLayoutElement = nodeGO.AddComponent<LayoutElement>();
        nodeLayoutElement.minWidth = 140;
        nodeLayoutElement.preferredWidth = 140;
        nodeLayoutElement.minHeight = 140;
        nodeLayoutElement.preferredHeight = 140;

        var centerGO = new GameObject("Center");
        centerGO.transform.SetParent(nodeGO.transform, false);
        var centerRect = centerGO.AddComponent<RectTransform>();
        centerRect.anchorMin = new Vector2(0.5f, 0);
        centerRect.anchorMax = new Vector2(0.5f, 1);
        centerRect.pivot = new Vector2(0.5f, 0.5f);
        centerRect.anchoredPosition = Vector2.zero;
        centerRect.sizeDelta = new Vector2(140, 0);

        var centerLayout = centerGO.AddComponent<VerticalLayoutGroup>();
        centerLayout.spacing = 8;
        centerLayout.padding = new RectOffset(0, 0, 8, 8);
        centerLayout.childAlignment = TextAnchor.MiddleCenter;
        centerLayout.childControlHeight = true;
        centerLayout.childControlWidth = true;
        centerLayout.childForceExpandHeight = false;
        centerLayout.childForceExpandWidth = true;

        var iconContainerGO = new GameObject("IconContainer");
        iconContainerGO.transform.SetParent(centerGO.transform, false);
        iconContainerGO.AddComponent<RectTransform>();
        var iconContainerLayout = iconContainerGO.AddComponent<LayoutElement>();
        iconContainerLayout.minHeight = 90;
        iconContainerLayout.preferredHeight = 90;
        iconContainerLayout.minWidth = 90;
        iconContainerLayout.preferredWidth = 90;

        var glowGO = new GameObject("NodeGlowImage");
        glowGO.transform.SetParent(centerGO.transform, false);
        glowGO.transform.SetAsFirstSibling();
        var glowRect = glowGO.AddComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.pivot = new Vector2(0.5f, 0.5f);
        glowRect.anchoredPosition = new Vector2(0, 18);
        glowRect.sizeDelta = new Vector2(130, 130);
        glowGO.AddComponent<LayoutElement>().ignoreLayout = true;
        var glowImage = glowGO.AddComponent<Image>();
        glowImage.sprite = (settings != null && settings.GlowSprite != null)
            ? settings.GlowSprite
            : AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        glowImage.color = new Color(1f, 0.8f, 0.3f, 1f);
        glowImage.raycastTarget = false;
        glowGO.SetActive(false);

        var backgroundFrameGO = new GameObject("BackgroundFrame");
        backgroundFrameGO.transform.SetParent(iconContainerGO.transform, false);
        var backgroundFrameRect = backgroundFrameGO.AddComponent<RectTransform>();
        backgroundFrameRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundFrameRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundFrameRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundFrameRect.anchoredPosition = Vector2.zero;
        backgroundFrameRect.sizeDelta = new Vector2(90, 90);
        var backgroundFrameImage = backgroundFrameGO.AddComponent<Image>();
        backgroundFrameImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        backgroundFrameImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        var iconMaskGO = new GameObject("IconMask");
        iconMaskGO.transform.SetParent(iconContainerGO.transform, false);
        var iconMaskRect = iconMaskGO.AddComponent<RectTransform>();
        iconMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconMaskRect.pivot = new Vector2(0.5f, 0.5f);
        iconMaskRect.anchoredPosition = Vector2.zero;
        iconMaskRect.sizeDelta = new Vector2(78, 78);
        var iconMaskImage = iconMaskGO.AddComponent<Image>();
        iconMaskImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        iconMaskImage.color = Color.white;
        iconMaskGO.AddComponent<Mask>().showMaskGraphic = false;

        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(iconMaskGO.transform, false);
        var iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        var iconImage = iconGO.AddComponent<Image>();
        iconImage.color = Color.white;

        var costTextGO = new GameObject("CostText");
        costTextGO.transform.SetParent(iconContainerGO.transform, false);
        var costTextRect = costTextGO.AddComponent<RectTransform>();
        costTextRect.anchorMin = new Vector2(0.5f, 0f);
        costTextRect.anchorMax = new Vector2(0.5f, 0f);
        costTextRect.pivot = new Vector2(0.5f, 0f);
        costTextRect.anchoredPosition = new Vector2(0, -4);
        costTextRect.sizeDelta = new Vector2(50, 28);
        var costTextTMP = costTextGO.AddComponent<TextMeshProUGUI>();
        costTextTMP.text = "1p";
        costTextTMP.fontSize = 18;
        costTextTMP.alignment = TextAlignmentOptions.Center;
        costTextTMP.color = new Color(1f, 0.85f, 0.4f);

        var pointsRowGO = new GameObject("PointsRow");
        pointsRowGO.transform.SetParent(centerGO.transform, false);
        pointsRowGO.AddComponent<RectTransform>();
        var pointsRowLayoutElement = pointsRowGO.AddComponent<LayoutElement>();
        pointsRowLayoutElement.minHeight = 35;
        pointsRowLayoutElement.preferredHeight = 35;
        var pointsRowLayout = pointsRowGO.AddComponent<HorizontalLayoutGroup>();
        pointsRowLayout.spacing = 4;
        pointsRowLayout.childAlignment = TextAnchor.MiddleCenter;
        pointsRowLayout.childControlHeight = true;
        pointsRowLayout.childControlWidth = false;
        pointsRowLayout.childForceExpandHeight = true;
        pointsRowLayout.childForceExpandWidth = false;

        var removeBtn = CreateButton(pointsRowGO.transform, "RemoveButton", "-", new Vector2(30, 30));
        var removeBtnLayout = removeBtn.AddComponent<LayoutElement>();
        removeBtnLayout.minWidth = 30;
        removeBtnLayout.preferredWidth = 30;
        var removeBtnText = removeBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (removeBtnText != null)
        {
            removeBtnText.color = Color.yellow;
            removeBtnText.fontSize = 24;
        }
        removeBtn.SetActive(false);

        var pointsBgGO = new GameObject("PointsBackground");
        pointsBgGO.transform.SetParent(pointsRowGO.transform, false);
        var pointsBgRect = pointsBgGO.AddComponent<RectTransform>();
        pointsBgRect.sizeDelta = new Vector2(78, 35);
        var pointsBgLayout = pointsBgGO.AddComponent<LayoutElement>();
        pointsBgLayout.minWidth = 78;
        pointsBgLayout.preferredWidth = 78;
        var pointsBgImage = pointsBgGO.AddComponent<Image>();
        pointsBgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        var pointsText = CreateText(pointsBgGO.transform, "PointsText", "0/5", 16);
        var pointsTextRect = pointsText.GetComponent<RectTransform>();
        pointsTextRect.anchorMin = Vector2.zero;
        pointsTextRect.anchorMax = Vector2.one;
        pointsTextRect.offsetMin = Vector2.zero;
        pointsTextRect.offsetMax = Vector2.zero;

        var addBtn = CreateButton(pointsRowGO.transform, "AddButton", "+", new Vector2(30, 30));
        var addBtnLayout = addBtn.AddComponent<LayoutElement>();
        addBtnLayout.minWidth = 30;
        addBtnLayout.preferredWidth = 30;
        var addBtnText = addBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (addBtnText != null)
        {
            addBtnText.color = Color.yellow;
            addBtnText.fontSize = 24;
        }
        addBtn.SetActive(false);

        var nodeUI = nodeGO.AddComponent<StatNodeUI>();
        SerializedObject so = new SerializedObject(nodeUI);
        so.FindProperty("raycastTarget").objectReferenceValue = raycastImage;
        so.FindProperty("backgroundFrame").objectReferenceValue = backgroundFrameImage;
        so.FindProperty("iconMask").objectReferenceValue = iconMaskImage;
        so.FindProperty("iconImage").objectReferenceValue = iconImage;
        so.FindProperty("iconContainer").objectReferenceValue = iconContainerGO.GetComponent<RectTransform>();
        so.FindProperty("costText").objectReferenceValue = costTextTMP;
        so.FindProperty("pointsText").objectReferenceValue = pointsText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("pointsBackground").objectReferenceValue = pointsBgImage;
        so.FindProperty("addButton").objectReferenceValue = addBtn.GetComponent<Button>();
        so.FindProperty("removeButton").objectReferenceValue = removeBtn.GetComponent<Button>();
        so.FindProperty("glowImage").objectReferenceValue = glowImage;
        so.ApplyModifiedProperties();

        return nodeGO;
    }

    private static GameObject CreateTooltip(Transform parent)
    {
        var tooltipGO = new GameObject("StatNodeTooltip");
        tooltipGO.transform.SetParent(parent, false);
        var tooltipRect = tooltipGO.AddComponent<RectTransform>();
        tooltipRect.sizeDelta = new Vector2(200, 180);
        tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
        tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
        tooltipRect.pivot = new Vector2(0f, 0.5f);

        tooltipGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        var tooltipLayout = tooltipGO.AddComponent<VerticalLayoutGroup>();
        tooltipLayout.padding = new RectOffset(10, 10, 10, 10);
        tooltipLayout.spacing = 5;
        tooltipLayout.childAlignment = TextAnchor.UpperLeft;
        tooltipLayout.childControlHeight = true;
        tooltipLayout.childControlWidth = true;
        tooltipLayout.childForceExpandHeight = false;
        tooltipLayout.childForceExpandWidth = true;

        tooltipGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var nameText = CreateText(tooltipGO.transform, "NameText", "Trait Name", 16);
        var nameTMP = nameText.GetComponent<TextMeshProUGUI>();
        nameTMP.alignment = TextAlignmentOptions.Left;
        nameTMP.fontStyle = FontStyles.Bold;
        nameText.AddComponent<LayoutElement>().minHeight = 24;

        var iconRowGO = new GameObject("IconRow");
        iconRowGO.transform.SetParent(tooltipGO.transform, false);
        var iconRowLayout = iconRowGO.AddComponent<HorizontalLayoutGroup>();
        iconRowLayout.spacing = 10;
        iconRowLayout.childAlignment = TextAnchor.MiddleLeft;
        iconRowLayout.childControlHeight = false;
        iconRowLayout.childControlWidth = false;
        iconRowGO.AddComponent<LayoutElement>().minHeight = 40;

        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(iconRowGO.transform, false);
        var iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(36, 36);
        var iconImage = iconGO.AddComponent<Image>();
        var iconLayout = iconGO.AddComponent<LayoutElement>();
        iconLayout.minWidth = 36;
        iconLayout.minHeight = 36;

        var currentLevelText = CreateText(iconRowGO.transform, "CurrentLevelText", "Level: 0/5", 12);
        currentLevelText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        var currentEffectText = CreateText(tooltipGO.transform, "CurrentEffectText", "Current: +0%", 11);
        var currentEffectTMP = currentEffectText.GetComponent<TextMeshProUGUI>();
        currentEffectTMP.alignment = TextAlignmentOptions.Left;
        currentEffectTMP.color = new Color(0.6f, 1f, 0.6f);

        var separatorGO = new GameObject("Separator");
        separatorGO.transform.SetParent(tooltipGO.transform, false);
        separatorGO.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        var separatorLayout = separatorGO.AddComponent<LayoutElement>();
        separatorLayout.minHeight = 1;
        separatorLayout.preferredHeight = 1;

        var nextSectionGO = new GameObject("NextLevelSection");
        nextSectionGO.transform.SetParent(tooltipGO.transform, false);
        var nextSectionLayout = nextSectionGO.AddComponent<VerticalLayoutGroup>();
        nextSectionLayout.spacing = 3;
        nextSectionLayout.childControlHeight = true;
        nextSectionLayout.childControlWidth = true;

        var nextLevelText = CreateText(nextSectionGO.transform, "NextLevelText", "Next Level: 1", 11);
        var nextLevelTMP = nextLevelText.GetComponent<TextMeshProUGUI>();
        nextLevelTMP.alignment = TextAlignmentOptions.Left;
        nextLevelTMP.color = new Color(1f, 0.9f, 0.5f);

        var requiredPointsText = CreateText(nextSectionGO.transform, "RequiredPointsText", "Cost: 1", 11);
        requiredPointsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        var nextEffectText = CreateText(nextSectionGO.transform, "NextEffectText", "Effect: +5%", 11);
        var nextEffectTMP = nextEffectText.GetComponent<TextMeshProUGUI>();
        nextEffectTMP.alignment = TextAlignmentOptions.Left;
        nextEffectTMP.color = new Color(0.5f, 0.8f, 1f);

        var tooltipComponent = tooltipGO.AddComponent<StatNodeTooltip>();
        SerializedObject so = new SerializedObject(tooltipComponent);
        so.FindProperty("nameText").objectReferenceValue = nameTMP;
        so.FindProperty("iconImage").objectReferenceValue = iconImage;
        so.FindProperty("currentLevelText").objectReferenceValue = currentLevelText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("currentEffectText").objectReferenceValue = currentEffectTMP;
        so.FindProperty("nextLevelSection").objectReferenceValue = nextSectionGO;
        so.FindProperty("nextLevelText").objectReferenceValue = nextLevelTMP;
        so.FindProperty("requiredPointsText").objectReferenceValue = requiredPointsText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("nextEffectText").objectReferenceValue = nextEffectTMP;
        so.ApplyModifiedProperties();

        tooltipGO.SetActive(false);

        return tooltipGO;
    }

    #endregion

    #region Asset Generation

    public static void GenerateAssets()
    {
        string basePath = "Assets/Data/StatTree";
        string nodesPath = basePath + "/Nodes";

        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Data/StatTree"))
            AssetDatabase.CreateFolder("Assets/Data", "StatTree");
        if (!AssetDatabase.IsValidFolder(nodesPath))
            AssetDatabase.CreateFolder(basePath, "Nodes");

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

        Debug.Log("[StatTreeTools] Assets generated!");
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

    #endregion

    #region Canvas Build

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
                Debug.LogWarning("[StatTreeTools] Main Camera not found. Assign Render Camera manually.");
            }
        }

        SerializedObject canvasSO = new SerializedObject(targetCanvas);
        Transform tiersContainer = canvasSO.FindProperty("tiersContainer").objectReferenceValue as Transform;

        if (tiersContainer == null)
        {
            Debug.LogError("[StatTreeTools] TiersContainer not found in StatTreeUI!");
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

        Debug.Log($"[StatTreeTools] Canvas build complete! {statTreeData.TierCount} tiers, {CountTotalNodes()} nodes total");
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

    #endregion

    #region Helper Methods

    private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = size;
        go.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        return go;
    }

    private static GameObject CreateText(Transform parent, string name, string text, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = size;
        go.AddComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f);
        go.AddComponent<Button>();

        var textGO = CreateText(go.transform, "Text", text, 16);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return go;
    }

    private static StatTreeSettings LoadSettings()
    {
        var guids = AssetDatabase.FindAssets("t:StatTreeSettings", new[] { "Assets/Data/StatTree" });
        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<StatTreeSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));

        guids = AssetDatabase.FindAssets("t:StatTreeSettings");
        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<StatTreeSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));

        return null;
    }

    #endregion
}
