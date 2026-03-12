using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class StatTreeUICreator : EditorWindow
{
    [MenuItem("Tools/Create 전투 특성 UI")]
    public static void CreateStatTreeUI()
    {
        // Canvas 생성 (전투 특성)
        var canvasGO = new GameObject("StatTreeCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel 생성
        var panelGO = CreatePanel(canvasGO.transform, "Panel", new Vector2(600, 500));
        // Panel 자체의 Image는 투명하게 (Background 레이어 사용)
        panelGO.GetComponent<Image>().color = new Color(0, 0, 0, 0);

        // Layer 1: 검정 단색 배경 (빈 공간 채움)
        var panelBGSolid = new GameObject("BackgroundSolid");
        panelBGSolid.transform.SetParent(panelGO.transform, false);
        panelBGSolid.transform.SetAsFirstSibling();
        var panelBGSolidRect = panelBGSolid.AddComponent<RectTransform>();
        panelBGSolidRect.anchorMin = Vector2.zero;
        panelBGSolidRect.anchorMax = Vector2.one;
        panelBGSolidRect.offsetMin = Vector2.zero;
        panelBGSolidRect.offsetMax = Vector2.zero;
        var panelBGSolidImage = panelBGSolid.AddComponent<Image>();
        panelBGSolidImage.color = new Color(0.05f, 0.05f, 0.05f, 0.95f); // 거의 검정
        panelBGSolidImage.raycastTarget = false;

        // Layer 2: 문양 이미지 (패턴/데코레이션)
        var panelBGPattern = new GameObject("BackgroundPattern");
        panelBGPattern.transform.SetParent(panelGO.transform, false);
        panelBGPattern.transform.SetSiblingIndex(1); // 검정 배경 바로 위
        var panelBGPatternRect = panelBGPattern.AddComponent<RectTransform>();
        panelBGPatternRect.anchorMin = Vector2.zero;
        panelBGPatternRect.anchorMax = Vector2.one;
        panelBGPatternRect.offsetMin = Vector2.zero;
        panelBGPatternRect.offsetMax = Vector2.zero;
        var panelBGPatternImage = panelBGPattern.AddComponent<Image>();
        panelBGPatternImage.color = new Color(1f, 1f, 1f, 15f / 255f); // alpha 15
        panelBGPatternImage.type = Image.Type.Sliced;
        panelBGPatternImage.raycastTarget = false;

        // Header
        var headerGO = CreatePanel(panelGO.transform, "Header", new Vector2(580, 60));
        var headerRect = headerGO.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.5f, 1f);
        headerRect.anchorMax = new Vector2(0.5f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0, -10);

        var titleText = CreateText(headerGO.transform, "TitleText", "전투 특성", 24);
        var titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // Available Points (우측 상단)
        var pointsText = CreateText(panelGO.transform, "AvailablePointsText", "0/15", 16);
        var pointsRect = pointsText.GetComponent<RectTransform>();
        pointsRect.anchorMin = new Vector2(1f, 1f);
        pointsRect.anchorMax = new Vector2(1f, 1f);
        pointsRect.pivot = new Vector2(1f, 1f);
        pointsRect.anchoredPosition = new Vector2(-60, -20); // Close 버튼 왼쪽
        pointsRect.sizeDelta = new Vector2(80, 30);
        var pointsTMP = pointsText.GetComponent<TextMeshProUGUI>();
        pointsTMP.alignment = TextAlignmentOptions.Right;

        // Tiers Container
        var tiersContainer = new GameObject("TiersContainer");
        tiersContainer.transform.SetParent(panelGO.transform, false);
        var tiersRect = tiersContainer.AddComponent<RectTransform>();
        tiersRect.anchorMin = new Vector2(0, 0.15f);
        tiersRect.anchorMax = new Vector2(1, 0.85f);
        tiersRect.offsetMin = new Vector2(20, 0);
        tiersRect.offsetMax = new Vector2(-20, -40);
        var tiersLayout = tiersContainer.AddComponent<VerticalLayoutGroup>();
        tiersLayout.spacing = 10;
        tiersLayout.childAlignment = TextAnchor.UpperCenter;
        tiersLayout.childControlHeight = false;  // tier 크기 통일
        tiersLayout.childControlWidth = true;
        tiersLayout.childForceExpandHeight = false;
        tiersLayout.childForceExpandWidth = true;

        // Close Button
        var closeBtn = CreateButton(panelGO.transform, "CloseButton", "X", new Vector2(40, 40));
        var closeBtnRect = closeBtn.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(1, 1);
        closeBtnRect.anchorMax = new Vector2(1, 1);
        closeBtnRect.pivot = new Vector2(1, 1);
        closeBtnRect.anchoredPosition = new Vector2(-10, -10);

        // ===== Tooltip 생성 (Canvas 자식으로) =====
        var tooltipGO = CreateTooltip(canvasGO.transform);

        // StatTreeUI 컴포넌트 추가
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

        // ===== Tier Prefab 생성 =====
        var tierPrefabGO = CreateTierPrefab();

        // ===== Node Prefab 생성 =====
        var nodePrefabGO = CreateNodePrefab();

        // 프리팹 저장
        string prefabPath = "Assets/Prefabs/UI/StatTree/";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI/StatTree"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs/UI", "StatTree");
        }

        // Tier 프리팹 저장
        var tierPrefab = PrefabUtility.SaveAsPrefabAsset(tierPrefabGO, prefabPath + "StatTierUI.prefab");
        DestroyImmediate(tierPrefabGO);

        // Node 프리팹 저장
        var nodePrefab = PrefabUtility.SaveAsPrefabAsset(nodePrefabGO, prefabPath + "StatNodeUI.prefab");
        DestroyImmediate(nodePrefabGO);

        // Canvas 프리팹 저장 (프리팹 참조 연결)
        so = new SerializedObject(treeUI);
        so.FindProperty("tierPrefab").objectReferenceValue = tierPrefab.GetComponent<StatTierUI>();
        so.FindProperty("nodePrefab").objectReferenceValue = nodePrefab.GetComponent<StatNodeUI>();
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(canvasGO, prefabPath + "StatTreeCanvas.prefab");

        Debug.Log("전투 특성 UI prefabs created at: " + prefabPath);
        Selection.activeGameObject = canvasGO;
    }

    private static GameObject CreateTierPrefab()
    {
        var tierGO = new GameObject("StatTierUI");
        var tierRect = tierGO.AddComponent<RectTransform>();
        tierRect.sizeDelta = new Vector2(560, 100);

        // 고정 높이를 위한 LayoutElement
        var tierLayoutElement = tierGO.AddComponent<LayoutElement>();
        tierLayoutElement.minHeight = 100;
        tierLayoutElement.preferredHeight = 100;

        // 배경 투명
        var tierBG = tierGO.AddComponent<Image>();
        tierBG.color = new Color(0, 0, 0, 0);

        // HorizontalLayoutGroup으로 좌측(정보) + 우측(노드) 배치
        var tierLayout = tierGO.AddComponent<HorizontalLayoutGroup>();
        tierLayout.spacing = 10;
        tierLayout.padding = new RectOffset(10, 10, 5, 5);
        tierLayout.childAlignment = TextAnchor.MiddleLeft;
        tierLayout.childControlHeight = true;
        tierLayout.childControlWidth = false;
        tierLayout.childForceExpandHeight = true;
        tierLayout.childForceExpandWidth = false;

        // StatTreeSettings 로드
        var settings = LoadSettings();

        // Left Info Panel (Background + TierName 중앙 + Progress 하단)
        var leftInfo = new GameObject("LeftInfo");
        leftInfo.transform.SetParent(tierGO.transform, false);
        var leftInfoRect = leftInfo.AddComponent<RectTransform>();
        var leftInfoLayoutElement = leftInfo.AddComponent<LayoutElement>();
        leftInfoLayoutElement.minWidth = 70;
        leftInfoLayoutElement.preferredWidth = 70;

        // LeftInfo Background (설정에서 스프라이트 적용)
        var leftInfoBG = leftInfo.AddComponent<Image>();
        leftInfoBG.color = Color.white; // 기본색 흰색
        if (settings != null && settings.LeftInfoBackgroundSprite != null)
            leftInfoBG.sprite = settings.LeftInfoBackgroundSprite;

        // Tier Name (정중앙)
        var tierName = new GameObject("TierNameText");
        tierName.transform.SetParent(leftInfo.transform, false);
        var tierNameRect = tierName.AddComponent<RectTransform>();
        tierNameRect.anchorMin = new Vector2(0.5f, 0.5f);
        tierNameRect.anchorMax = new Vector2(0.5f, 0.5f);
        tierNameRect.pivot = new Vector2(0.5f, 0.5f);
        tierNameRect.anchoredPosition = Vector2.zero;
        tierNameRect.sizeDelta = new Vector2(60, 30);
        var tierNameTMP = tierName.AddComponent<TextMeshProUGUI>();
        tierNameTMP.text = "Tier 1";
        tierNameTMP.fontSize = 14;
        tierNameTMP.alignment = TextAlignmentOptions.Center;
        tierNameTMP.color = Color.white;

        // Progress Text (하단)
        var progressText = new GameObject("ProgressText");
        progressText.transform.SetParent(leftInfo.transform, false);
        var progressRect = progressText.AddComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0.5f, 0f);
        progressRect.anchorMax = new Vector2(0.5f, 0f);
        progressRect.pivot = new Vector2(0.5f, 0f);
        progressRect.anchoredPosition = new Vector2(0, 5);
        progressRect.sizeDelta = new Vector2(60, 20);
        var progressTMP = progressText.AddComponent<TextMeshProUGUI>();
        progressTMP.text = "0/40";
        progressTMP.fontSize = 11;
        progressTMP.alignment = TextAlignmentOptions.Center;
        progressTMP.color = new Color(0.8f, 0.8f, 0.8f);

        // Lock Icon (설정에서 스프라이트 적용)
        var lockIcon = new GameObject("LockIcon");
        lockIcon.transform.SetParent(leftInfo.transform, false);
        var lockIconRect = lockIcon.AddComponent<RectTransform>();
        lockIconRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockIconRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockIconRect.pivot = new Vector2(0.5f, 0.5f);
        lockIconRect.anchoredPosition = Vector2.zero;
        lockIconRect.sizeDelta = new Vector2(32, 32);
        var lockIconImage = lockIcon.AddComponent<Image>();
        lockIconImage.color = Color.white;
        if (settings != null && settings.LockIconSprite != null)
            lockIconImage.sprite = settings.LockIconSprite;
        lockIcon.SetActive(false); // 기본 비활성화 (Tier 1은 항상 활성)

        // Nodes Container
        var nodesContainer = new GameObject("NodesContainer");
        nodesContainer.transform.SetParent(tierGO.transform, false);
        var nodesRect = nodesContainer.AddComponent<RectTransform>();
        var nodesLayoutElement = nodesContainer.AddComponent<LayoutElement>();
        nodesLayoutElement.flexibleWidth = 1;
        var nodesLayout = nodesContainer.AddComponent<HorizontalLayoutGroup>();
        nodesLayout.spacing = 10;
        nodesLayout.childAlignment = TextAnchor.MiddleLeft;
        nodesLayout.childControlHeight = false;
        nodesLayout.childControlWidth = false;
        nodesLayout.childForceExpandHeight = false;
        nodesLayout.childForceExpandWidth = false;

        // StatTierUI 컴포넌트
        var tierUI = tierGO.AddComponent<StatTierUI>();
        SerializedObject so = new SerializedObject(tierUI);
        so.FindProperty("tierNameText").objectReferenceValue = tierNameTMP;
        so.FindProperty("progressText").objectReferenceValue = progressTMP;
        so.FindProperty("nodesContainer").objectReferenceValue = nodesContainer.transform;
        so.FindProperty("leftInfoBackground").objectReferenceValue = leftInfoBG;
        so.FindProperty("lockIcon").objectReferenceValue = lockIconImage;
        so.ApplyModifiedProperties();

        return tierGO;
    }

    private static GameObject CreateNodePrefab()
    {
        var nodeGO = new GameObject("StatNodeUI");
        var nodeRect = nodeGO.AddComponent<RectTransform>();
        nodeRect.sizeDelta = new Vector2(80, 80);

        // 전체 영역 레이캐스트용 투명 Image
        var raycastImage = nodeGO.AddComponent<Image>();
        raycastImage.color = new Color(0, 0, 0, 0); // 완전 투명
        raycastImage.raycastTarget = true;

        // 고정 크기를 위한 LayoutElement 추가
        var nodeLayoutElement = nodeGO.AddComponent<LayoutElement>();
        nodeLayoutElement.minWidth = 80;
        nodeLayoutElement.preferredWidth = 80;
        nodeLayoutElement.minHeight = 80;
        nodeLayoutElement.preferredHeight = 80;

        // Center (Icon + PointsRow) - 중앙 고정
        var centerGO = new GameObject("Center");
        centerGO.transform.SetParent(nodeGO.transform, false);
        var centerRect = centerGO.AddComponent<RectTransform>();
        centerRect.anchorMin = new Vector2(0.5f, 0);
        centerRect.anchorMax = new Vector2(0.5f, 1);
        centerRect.pivot = new Vector2(0.5f, 0.5f);
        centerRect.anchoredPosition = Vector2.zero;
        centerRect.sizeDelta = new Vector2(80, 0);

        var centerLayout = centerGO.AddComponent<VerticalLayoutGroup>();
        centerLayout.spacing = 4;
        centerLayout.padding = new RectOffset(0, 0, 5, 5);
        centerLayout.childAlignment = TextAnchor.MiddleCenter;
        centerLayout.childControlHeight = true;
        centerLayout.childControlWidth = true;
        centerLayout.childForceExpandHeight = false;
        centerLayout.childForceExpandWidth = true;

        // Icon Container (고정 크기)
        var iconContainerGO = new GameObject("IconContainer");
        iconContainerGO.transform.SetParent(centerGO.transform, false);
        var iconContainerRect = iconContainerGO.AddComponent<RectTransform>();
        var iconContainerLayout = iconContainerGO.AddComponent<LayoutElement>();
        iconContainerLayout.minHeight = 50;
        iconContainerLayout.preferredHeight = 50;
        iconContainerLayout.minWidth = 50;
        iconContainerLayout.preferredWidth = 50;

        // Background Frame (원형 프레임)
        var backgroundFrameGO = new GameObject("BackgroundFrame");
        backgroundFrameGO.transform.SetParent(iconContainerGO.transform, false);
        var backgroundFrameRect = backgroundFrameGO.AddComponent<RectTransform>();
        backgroundFrameRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundFrameRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundFrameRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundFrameRect.anchoredPosition = Vector2.zero;
        backgroundFrameRect.sizeDelta = new Vector2(50, 50);
        var backgroundFrameImage = backgroundFrameGO.AddComponent<Image>();
        backgroundFrameImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        backgroundFrameImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        // Icon Mask (원형 마스크)
        var iconMaskGO = new GameObject("IconMask");
        iconMaskGO.transform.SetParent(iconContainerGO.transform, false);
        var iconMaskRect = iconMaskGO.AddComponent<RectTransform>();
        iconMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconMaskRect.pivot = new Vector2(0.5f, 0.5f);
        iconMaskRect.anchoredPosition = Vector2.zero;
        iconMaskRect.sizeDelta = new Vector2(44, 44);
        var iconMaskImage = iconMaskGO.AddComponent<Image>();
        iconMaskImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        iconMaskImage.color = Color.white;
        var mask = iconMaskGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Icon Image (실제 아이콘 - 마스크의 자식)
        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(iconMaskGO.transform, false);
        var iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        var iconImage = iconGO.AddComponent<Image>();
        iconImage.color = Color.white;

        // Cost Text (IconContainer 하단에 겹쳐서 표시)
        var costTextGO = new GameObject("CostText");
        costTextGO.transform.SetParent(iconContainerGO.transform, false);
        var costTextRect = costTextGO.AddComponent<RectTransform>();
        costTextRect.anchorMin = new Vector2(0.5f, 0f);
        costTextRect.anchorMax = new Vector2(0.5f, 0f);
        costTextRect.pivot = new Vector2(0.5f, 0f);
        costTextRect.anchoredPosition = new Vector2(0, -2); // 아이콘 하단에서 약간 아래
        costTextRect.sizeDelta = new Vector2(30, 16);
        var costTextTMP = costTextGO.AddComponent<TextMeshProUGUI>();
        costTextTMP.text = "1p";
        costTextTMP.fontSize = 11;
        costTextTMP.alignment = TextAlignmentOptions.Center;
        costTextTMP.color = new Color(1f, 0.85f, 0.4f); // 노란색 계열

        // PointsRow (RemoveButton + PointsBackground + AddButton)
        var pointsRowGO = new GameObject("PointsRow");
        pointsRowGO.transform.SetParent(centerGO.transform, false);
        var pointsRowRect = pointsRowGO.AddComponent<RectTransform>();
        var pointsRowLayoutElement = pointsRowGO.AddComponent<LayoutElement>();
        pointsRowLayoutElement.minHeight = 20;
        pointsRowLayoutElement.preferredHeight = 20;
        var pointsRowLayout = pointsRowGO.AddComponent<HorizontalLayoutGroup>();
        pointsRowLayout.spacing = 2;
        pointsRowLayout.childAlignment = TextAnchor.MiddleCenter;
        pointsRowLayout.childControlHeight = true;
        pointsRowLayout.childControlWidth = false;
        pointsRowLayout.childForceExpandHeight = true;
        pointsRowLayout.childForceExpandWidth = false;

        // Remove Button (-)
        var removeBtn = CreateButton(pointsRowGO.transform, "RemoveButton", "-", new Vector2(18, 18));
        var removeBtnLayout = removeBtn.AddComponent<LayoutElement>();
        removeBtnLayout.minWidth = 18;
        removeBtnLayout.preferredWidth = 18;
        // 버튼 텍스트 노란색
        var removeBtnText = removeBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (removeBtnText != null) removeBtnText.color = Color.yellow;
        removeBtn.SetActive(false); // 기본 비활성화

        // PointsBackground (배경 이미지 + 텍스트)
        var pointsBgGO = new GameObject("PointsBackground");
        pointsBgGO.transform.SetParent(pointsRowGO.transform, false);
        var pointsBgRect = pointsBgGO.AddComponent<RectTransform>();
        pointsBgRect.sizeDelta = new Vector2(44, 20); // 아이콘과 같은 너비
        var pointsBgLayout = pointsBgGO.AddComponent<LayoutElement>();
        pointsBgLayout.minWidth = 44;
        pointsBgLayout.preferredWidth = 44;
        // flexibleWidth 제거하여 고정 크기로
        var pointsBgImage = pointsBgGO.AddComponent<Image>();
        pointsBgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // PointsText (PointsBackground의 자식)
        var pointsText = CreateText(pointsBgGO.transform, "PointsText", "0/5", 9);
        var pointsTextRect = pointsText.GetComponent<RectTransform>();
        pointsTextRect.anchorMin = Vector2.zero;
        pointsTextRect.anchorMax = Vector2.one;
        pointsTextRect.offsetMin = Vector2.zero;
        pointsTextRect.offsetMax = Vector2.zero;

        // Add Button (+)
        var addBtn = CreateButton(pointsRowGO.transform, "AddButton", "+", new Vector2(18, 18));
        var addBtnLayout = addBtn.AddComponent<LayoutElement>();
        addBtnLayout.minWidth = 18;
        addBtnLayout.preferredWidth = 18;
        // 버튼 텍스트 노란색
        var addBtnText = addBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (addBtnText != null) addBtnText.color = Color.yellow;
        addBtn.SetActive(false); // 기본 비활성화

        // StatNodeUI 컴포넌트
        var nodeUI = nodeGO.AddComponent<StatNodeUI>();
        SerializedObject so = new SerializedObject(nodeUI);
        so.FindProperty("raycastTarget").objectReferenceValue = raycastImage;
        so.FindProperty("backgroundFrame").objectReferenceValue = backgroundFrameImage;
        so.FindProperty("iconMask").objectReferenceValue = iconMaskImage;
        so.FindProperty("iconImage").objectReferenceValue = iconImage;
        so.FindProperty("iconContainer").objectReferenceValue = iconContainerRect;
        so.FindProperty("costText").objectReferenceValue = costTextTMP;
        so.FindProperty("pointsText").objectReferenceValue = pointsText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("pointsBackground").objectReferenceValue = pointsBgImage;
        so.FindProperty("addButton").objectReferenceValue = addBtn.GetComponent<Button>();
        so.FindProperty("removeButton").objectReferenceValue = removeBtn.GetComponent<Button>();
        // tooltip은 런타임에 StatTreeUI에서 전달됨
        so.ApplyModifiedProperties();

        return nodeGO;
    }

    private static GameObject CreateTooltip(Transform parent)
    {
        // Tooltip Container
        var tooltipGO = new GameObject("StatNodeTooltip");
        tooltipGO.transform.SetParent(parent, false);
        var tooltipRect = tooltipGO.AddComponent<RectTransform>();
        tooltipRect.sizeDelta = new Vector2(200, 180);
        // 중앙 anchor, 좌측 하단 pivot (툴팁이 오른쪽 위로 확장)
        tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
        tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
        tooltipRect.pivot = new Vector2(0f, 0.5f); // 좌측 중앙 pivot

        // Background
        var tooltipBG = tooltipGO.AddComponent<Image>();
        tooltipBG.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        // Vertical Layout
        var tooltipLayout = tooltipGO.AddComponent<VerticalLayoutGroup>();
        tooltipLayout.padding = new RectOffset(10, 10, 10, 10);
        tooltipLayout.spacing = 5;
        tooltipLayout.childAlignment = TextAnchor.UpperLeft;
        tooltipLayout.childControlHeight = true;
        tooltipLayout.childControlWidth = true;
        tooltipLayout.childForceExpandHeight = false;
        tooltipLayout.childForceExpandWidth = true;

        // ContentSizeFitter for auto-sizing
        var fitter = tooltipGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Name Text (상단)
        var nameText = CreateText(tooltipGO.transform, "NameText", "스킬 이름", 16);
        var nameTMP = nameText.GetComponent<TextMeshProUGUI>();
        nameTMP.alignment = TextAlignmentOptions.Left;
        nameTMP.fontStyle = TMPro.FontStyles.Bold;
        var nameLayout = nameText.AddComponent<LayoutElement>();
        nameLayout.minHeight = 24;

        // Icon Row (아이콘 + 현재 레벨)
        var iconRowGO = new GameObject("IconRow");
        iconRowGO.transform.SetParent(tooltipGO.transform, false);
        var iconRowLayout = iconRowGO.AddComponent<HorizontalLayoutGroup>();
        iconRowLayout.spacing = 10;
        iconRowLayout.childAlignment = TextAnchor.MiddleLeft;
        iconRowLayout.childControlHeight = false;
        iconRowLayout.childControlWidth = false;
        var iconRowLayoutElement = iconRowGO.AddComponent<LayoutElement>();
        iconRowLayoutElement.minHeight = 40;

        // Icon
        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(iconRowGO.transform, false);
        var iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(36, 36);
        var iconImage = iconGO.AddComponent<Image>();
        var iconLayout = iconGO.AddComponent<LayoutElement>();
        iconLayout.minWidth = 36;
        iconLayout.minHeight = 36;

        // Current Level Text
        var currentLevelText = CreateText(iconRowGO.transform, "CurrentLevelText", "레벨: 0/5", 12);
        var currentLevelTMP = currentLevelText.GetComponent<TextMeshProUGUI>();
        currentLevelTMP.alignment = TextAlignmentOptions.Left;

        // Current Effect Text
        var currentEffectText = CreateText(tooltipGO.transform, "CurrentEffectText", "현재 효과: +0%", 11);
        var currentEffectTMP = currentEffectText.GetComponent<TextMeshProUGUI>();
        currentEffectTMP.alignment = TextAlignmentOptions.Left;
        currentEffectTMP.color = new Color(0.6f, 1f, 0.6f);

        // Separator
        var separatorGO = new GameObject("Separator");
        separatorGO.transform.SetParent(tooltipGO.transform, false);
        var separatorImage = separatorGO.AddComponent<Image>();
        separatorImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        var separatorLayout = separatorGO.AddComponent<LayoutElement>();
        separatorLayout.minHeight = 1;
        separatorLayout.preferredHeight = 1;

        // Next Level Section
        var nextSectionGO = new GameObject("NextLevelSection");
        nextSectionGO.transform.SetParent(tooltipGO.transform, false);
        var nextSectionLayout = nextSectionGO.AddComponent<VerticalLayoutGroup>();
        nextSectionLayout.spacing = 3;
        nextSectionLayout.childControlHeight = true;
        nextSectionLayout.childControlWidth = true;

        // Next Level Text
        var nextLevelText = CreateText(nextSectionGO.transform, "NextLevelText", "다음 레벨: 1", 11);
        var nextLevelTMP = nextLevelText.GetComponent<TextMeshProUGUI>();
        nextLevelTMP.alignment = TextAlignmentOptions.Left;
        nextLevelTMP.color = new Color(1f, 0.9f, 0.5f);

        // Required Points Text
        var requiredPointsText = CreateText(nextSectionGO.transform, "RequiredPointsText", "필요 포인트: 1", 11);
        var requiredPointsTMP = requiredPointsText.GetComponent<TextMeshProUGUI>();
        requiredPointsTMP.alignment = TextAlignmentOptions.Left;

        // Next Effect Text
        var nextEffectText = CreateText(nextSectionGO.transform, "NextEffectText", "효과: +5%", 11);
        var nextEffectTMP = nextEffectText.GetComponent<TextMeshProUGUI>();
        nextEffectTMP.alignment = TextAlignmentOptions.Left;
        nextEffectTMP.color = new Color(0.5f, 0.8f, 1f);

        // StatNodeTooltip 컴포넌트 추가
        var tooltipComponent = tooltipGO.AddComponent<StatNodeTooltip>();
        SerializedObject so = new SerializedObject(tooltipComponent);
        so.FindProperty("nameText").objectReferenceValue = nameTMP;
        so.FindProperty("iconImage").objectReferenceValue = iconImage;
        so.FindProperty("currentLevelText").objectReferenceValue = currentLevelTMP;
        so.FindProperty("currentEffectText").objectReferenceValue = currentEffectTMP;
        so.FindProperty("nextLevelSection").objectReferenceValue = nextSectionGO;
        so.FindProperty("nextLevelText").objectReferenceValue = nextLevelTMP;
        so.FindProperty("requiredPointsText").objectReferenceValue = requiredPointsTMP;
        so.FindProperty("nextEffectText").objectReferenceValue = nextEffectTMP;
        so.ApplyModifiedProperties();

        // 기본으로 비활성화
        tooltipGO.SetActive(false);

        return tooltipGO;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        var image = go.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        return go;
    }

    private static GameObject CreateText(Transform parent, string name, string text, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
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
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        var image = go.AddComponent<Image>();
        image.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        var button = go.AddComponent<Button>();

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
        // Data/StatTree 폴더에서 StatTreeSettings 찾기
        var guids = AssetDatabase.FindAssets("t:StatTreeSettings", new[] { "Assets/Data/StatTree" });
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<StatTreeSettings>(path);
        }

        // 없으면 전체 프로젝트에서 검색
        guids = AssetDatabase.FindAssets("t:StatTreeSettings");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<StatTreeSettings>(path);
        }

        Debug.LogWarning("StatTreeSettings를 찾을 수 없습니다. Assets/Data/StatTree에 생성하세요. (Create > Combat > Stat Tree Settings)");
        return null;
    }
}
