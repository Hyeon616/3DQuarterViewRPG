using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Item/Equipment 관련 에디터 도구 통합
/// - 폴더 생성
/// - 장비 타입 생성
/// - 장비 아이템 생성
/// - 데이터베이스 빌드
/// - UI 빌더 (인벤토리 / 장비 분리)
/// </summary>
public class ItemEquipmentTools : EditorWindow
{
    private const string TYPES_FOLDER_PATH = "Assets/Data/EquipmentTypes";
    private const string ITEMS_FOLDER_PATH = "Assets/Data/Items";
    private const string EQUIPMENT_DATABASE_PATH = "Assets/Resources/EquipmentDatabase.asset";
    private const string ITEM_DATABASE_PATH = "Assets/Resources/ItemDatabase.asset";

    private int selectedTab = 0;
    private readonly string[] tabNames = { "폴더/타입", "장비 생성", "데이터베이스", "UI 빌더" };

    private Vector2 scrollPosition;

    // Equipment Creator fields
    private WeaponType selectedWeaponType = WeaponType.TwoHandedSword;
    private ArmorType selectedArmorType = ArmorType.Light;
    private string weaponBaseName = "검";
    private string armorBaseName = "갑옷";
    private int weaponCount = 5;
    private int armorCount = 5;

    [MenuItem("Tools/Item & Equipment/Open Tools Window", false, 0)]
    public static void ShowWindow()
    {
        GetWindow<ItemEquipmentTools>("Item & Equipment Tools");
    }

    #region Quick Actions (MenuItem)

    [MenuItem("Tools/Item & Equipment/Create Folders", false, 100)]
    public static void CreateFoldersFromMenu()
    {
        CreateAllFolders();
    }

    [MenuItem("Tools/Item & Equipment/Create Equipment Types", false, 101)]
    public static void CreateTypesFromMenu()
    {
        CreateAllEquipmentTypes();
    }

    [MenuItem("Tools/Item & Equipment/Rebuild Equipment Database", false, 200)]
    public static void RebuildEquipmentDatabaseFromMenu()
    {
        RebuildEquipmentDatabase();
    }

    [MenuItem("Tools/Item & Equipment/Rebuild Item Database", false, 201)]
    public static void RebuildItemDatabaseFromMenu()
    {
        RebuildItemDatabase();
    }

    [MenuItem("Tools/Item & Equipment/Create Inventory Slot Prefab", false, 300)]
    public static void CreateInventorySlotPrefabFromMenu()
    {
        CreateInventorySlotPrefab();
    }

    [MenuItem("Tools/Item & Equipment/Create Inventory UI", false, 301)]
    public static void CreateInventoryUIFromMenu()
    {
        CreateInventoryUI();
    }

    [MenuItem("Tools/Item & Equipment/Create Equipment UI", false, 302)]
    public static void CreateEquipmentUIFromMenu()
    {
        CreateEquipmentUI();
    }

    #endregion

    private void OnGUI()
    {
        GUILayout.Label("Item & Equipment Tools", EditorStyles.boldLabel);
        GUILayout.Space(5);

        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        GUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        switch (selectedTab)
        {
            case 0:
                DrawFolderAndTypeTab();
                break;
            case 1:
                DrawEquipmentCreatorTab();
                break;
            case 2:
                DrawDatabaseTab();
                break;
            case 3:
                DrawUIBuilderTab();
                break;
        }

        EditorGUILayout.EndScrollView();
    }

    #region Tab: Folder & Type

    private void DrawFolderAndTypeTab()
    {
        EditorGUILayout.LabelField("폴더 생성", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "장비 및 아이템 데이터를 저장할 폴더 구조를 생성합니다.",
            MessageType.Info);

        if (GUILayout.Button("Create All Folders", GUILayout.Height(30)))
        {
            CreateAllFolders();
        }

        GUILayout.Space(20);

        EditorGUILayout.LabelField("장비 타입 생성", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "무기/방어구 타입(배율) ScriptableObject를 생성합니다.\n" +
            "폴더가 먼저 생성되어 있어야 합니다.",
            MessageType.Info);

        if (GUILayout.Button("Create All Equipment Types", GUILayout.Height(30)))
        {
            CreateAllEquipmentTypes();
        }
    }

    public static void CreateAllFolders()
    {
        // Equipment Type 폴더
        string[] typeFolders = new[]
        {
            "Assets/Data",
            "Assets/Data/EquipmentTypes",
            "Assets/Data/EquipmentTypes/WeaponTypes",
            "Assets/Data/EquipmentTypes/ArmorTypes"
        };

        foreach (string folder in typeFolders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = System.IO.Path.GetDirectoryName(folder).Replace("\\", "/");
                string name = System.IO.Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, name);
                Debug.Log($"[ItemEquipmentTools] Created folder: {folder}");
            }
        }

        // Item 폴더
        string[] itemFolders = new[]
        {
            "Assets/Data/Items",
            "Assets/Data/Items/Equipment",
            "Assets/Data/Items/Equipment/Weapons",
            "Assets/Data/Items/Equipment/Armors",
            "Assets/Data/Items/Consumables",
            "Assets/Data/Items/Materials",
            "Assets/Data/Items/Quests"
        };

        foreach (string folder in itemFolders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = System.IO.Path.GetDirectoryName(folder).Replace("\\", "/");
                string name = System.IO.Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, name);
                Debug.Log($"[ItemEquipmentTools] Created folder: {folder}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("[ItemEquipmentTools] All folders created!");
    }

    public static void CreateAllEquipmentTypes()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data/EquipmentTypes"))
        {
            Debug.LogError("[ItemEquipmentTools] 폴더가 없습니다. 먼저 'Create All Folders'를 실행하세요.");
            return;
        }

        CreateWeaponTypes();
        CreateArmorTypes();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ItemEquipmentTools] 모든 장비 타입 생성 완료!");
        Debug.Log("'Rebuild Equipment Database'를 실행하여 등록하세요.");
    }

    private static void CreateWeaponTypes()
    {
        string folderPath = "Assets/Data/EquipmentTypes/WeaponTypes";

        CreateWeaponType(folderPath, WeaponType.TwoHandedSword, "양손검", 1.3f, 0.7f);
        CreateWeaponType(folderPath, WeaponType.SwordAndShield, "검+방패", 1.0f, 1.0f);
        CreateWeaponType(folderPath, WeaponType.Spear, "창", 0.9f, 1.2f);
        CreateWeaponType(folderPath, WeaponType.Hammer, "해머", 1.5f, 0.6f);
        CreateWeaponType(folderPath, WeaponType.Gauntlet, "건틀릿", 0.7f, 1.5f);
        CreateWeaponType(folderPath, WeaponType.Bow, "활", 1.1f, 0.9f);

        Debug.Log("[ItemEquipmentTools] 무기 타입 6개 생성 완료");
    }

    private static void CreateArmorTypes()
    {
        string folderPath = "Assets/Data/EquipmentTypes/ArmorTypes";

        CreateArmorType(folderPath, ArmorType.Cloth, "천", 0.7f, 0.6f);
        CreateArmorType(folderPath, ArmorType.Leather, "가죽", 0.9f, 0.8f);
        CreateArmorType(folderPath, ArmorType.Light, "경갑", 1.0f, 1.0f);
        CreateArmorType(folderPath, ArmorType.Heavy, "중갑", 1.2f, 1.3f);
        CreateArmorType(folderPath, ArmorType.Plate, "판금", 1.4f, 1.5f);

        Debug.Log("[ItemEquipmentTools] 방어구 타입 5개 생성 완료");
    }

    private static void CreateWeaponType(string folderPath, WeaponType type, string displayName,
        float attackMultiplier, float attackSpeedMultiplier)
    {
        string fileName = $"WeaponType_{type}.asset";
        string assetPath = $"{folderPath}/{fileName}";

        if (AssetDatabase.LoadAssetAtPath<WeaponTypeData>(assetPath) != null)
        {
            Debug.LogWarning($"이미 존재합니다: {fileName}");
            return;
        }

        WeaponTypeData weaponType = ScriptableObject.CreateInstance<WeaponTypeData>();

        SerializedObject serializedType = new SerializedObject(weaponType);
        serializedType.FindProperty("weaponType").enumValueIndex = (int)type;
        serializedType.FindProperty("displayName").stringValue = displayName;
        serializedType.FindProperty("attackMultiplier").floatValue = attackMultiplier;
        serializedType.FindProperty("attackSpeedMultiplier").floatValue = attackSpeedMultiplier;
        serializedType.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(weaponType, assetPath);
        Debug.Log($"생성됨: {displayName} (공격: {attackMultiplier:F1}x, 속도: {attackSpeedMultiplier:F1}x)");
    }

    private static void CreateArmorType(string folderPath, ArmorType type, string displayName,
        float hpMultiplier, float defenseMultiplier)
    {
        string fileName = $"ArmorType_{type}.asset";
        string assetPath = $"{folderPath}/{fileName}";

        if (AssetDatabase.LoadAssetAtPath<ArmorTypeData>(assetPath) != null)
        {
            Debug.LogWarning($"이미 존재합니다: {fileName}");
            return;
        }

        ArmorTypeData armorType = ScriptableObject.CreateInstance<ArmorTypeData>();

        SerializedObject serializedType = new SerializedObject(armorType);
        serializedType.FindProperty("armorType").enumValueIndex = (int)type;
        serializedType.FindProperty("displayName").stringValue = displayName;
        serializedType.FindProperty("hpMultiplier").floatValue = hpMultiplier;
        serializedType.FindProperty("defenseMultiplier").floatValue = defenseMultiplier;
        serializedType.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(armorType, assetPath);
        Debug.Log($"생성됨: {displayName} (HP: {hpMultiplier:F1}x, 방어: {defenseMultiplier:F1}x)");
    }

    #endregion

    #region Tab: Equipment Creator

    private void DrawEquipmentCreatorTab()
    {
        EditorGUILayout.LabelField("장비 아이템 대량 생성", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 무기 섹션
        EditorGUILayout.LabelField("무기", EditorStyles.boldLabel);
        selectedWeaponType = (WeaponType)EditorGUILayout.EnumPopup("무기 타입", selectedWeaponType);
        weaponBaseName = EditorGUILayout.TextField("기본 이름", weaponBaseName);
        weaponCount = EditorGUILayout.IntSlider("생성 개수", weaponCount, 1, 20);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("생성될 무기 목록:", EditorStyles.miniBoldLabel);
        for (int i = 0; i < Mathf.Min(weaponCount, 5); i++)
        {
            EditorGUILayout.LabelField($"  - {weaponBaseName} Lv.{i + 1}");
        }
        if (weaponCount > 5)
            EditorGUILayout.LabelField($"  ... 외 {weaponCount - 5}개");

        EditorGUILayout.Space();
        if (GUILayout.Button($"무기 {weaponCount}개 생성", GUILayout.Height(30)))
        {
            CreateWeapons();
        }

        EditorGUILayout.Space(20);

        // 방어구 섹션
        EditorGUILayout.LabelField("방어구", EditorStyles.boldLabel);
        selectedArmorType = (ArmorType)EditorGUILayout.EnumPopup("방어구 타입", selectedArmorType);
        armorBaseName = EditorGUILayout.TextField("기본 이름", armorBaseName);
        armorCount = EditorGUILayout.IntSlider("생성 개수", armorCount, 1, 20);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("생성될 방어구 목록:", EditorStyles.miniBoldLabel);
        for (int i = 0; i < Mathf.Min(armorCount, 5); i++)
        {
            EditorGUILayout.LabelField($"  - {armorBaseName} Lv.{i + 1}");
        }
        if (armorCount > 5)
            EditorGUILayout.LabelField($"  ... 외 {armorCount - 5}개");

        EditorGUILayout.Space();
        if (GUILayout.Button($"방어구 {armorCount}개 생성", GUILayout.Height(30)))
        {
            CreateArmors();
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button($"무기 + 방어구 전체 생성 ({weaponCount + armorCount}개)", GUILayout.Height(40)))
        {
            CreateWeapons();
            CreateArmors();
        }
    }

    private void CreateWeapons()
    {
        string folderPath = "Assets/Data/Items/Equipment/Weapons";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError($"폴더가 없습니다: {folderPath}. 먼저 'Create All Folders'를 실행하세요.");
            return;
        }

        for (int i = 0; i < weaponCount; i++)
        {
            int level = i + 1;
            string itemName = $"{weaponBaseName} Lv.{level}";
            string fileName = $"Weapon_{selectedWeaponType}_{weaponBaseName}_Lv{level:D2}.asset";
            string assetPath = $"{folderPath}/{fileName}";

            if (AssetDatabase.LoadAssetAtPath<EquipmentItemData>(assetPath) != null)
            {
                Debug.LogWarning($"이미 존재합니다: {fileName}");
                continue;
            }

            EquipmentItemData item = ScriptableObject.CreateInstance<EquipmentItemData>();

            SerializedObject serializedItem = new SerializedObject(item);
            serializedItem.FindProperty("itemName").stringValue = itemName;
            serializedItem.FindProperty("description").stringValue = $"레벨 {level} {weaponBaseName}";
            serializedItem.FindProperty("itemType").enumValueIndex = (int)ItemType.Equipment;
            serializedItem.FindProperty("rarity").enumValueIndex = GetRarityByLevel(level);

            serializedItem.FindProperty("equipmentType").enumValueIndex = (int)EquipmentType.Weapon;
            serializedItem.FindProperty("weaponType").enumValueIndex = (int)selectedWeaponType;

            float baseAttack = 10f + (level - 1) * 5f;
            serializedItem.FindProperty("baseAttack").floatValue = baseAttack;
            serializedItem.FindProperty("baseAttackSpeed").floatValue = 1.0f;
            serializedItem.FindProperty("baseCriticalChance").floatValue = 5f + level * 0.5f;
            serializedItem.FindProperty("baseCriticalDamage").floatValue = 2.0f;
            serializedItem.FindProperty("attackPerLevel").floatValue = 2f;

            serializedItem.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(item, assetPath);
            Debug.Log($"생성됨: {itemName}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ItemEquipmentTools] 무기 {weaponCount}개 생성 완료!");
        Debug.Log("'Rebuild Item Database'를 실행하여 등록하세요.");
    }

    private void CreateArmors()
    {
        string folderPath = "Assets/Data/Items/Equipment/Armors";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError($"폴더가 없습니다: {folderPath}. 먼저 'Create All Folders'를 실행하세요.");
            return;
        }

        for (int i = 0; i < armorCount; i++)
        {
            int level = i + 1;
            string itemName = $"{armorBaseName} Lv.{level}";
            string fileName = $"Armor_{selectedArmorType}_{armorBaseName}_Lv{level:D2}.asset";
            string assetPath = $"{folderPath}/{fileName}";

            if (AssetDatabase.LoadAssetAtPath<EquipmentItemData>(assetPath) != null)
            {
                Debug.LogWarning($"이미 존재합니다: {fileName}");
                continue;
            }

            EquipmentItemData item = ScriptableObject.CreateInstance<EquipmentItemData>();

            SerializedObject serializedItem = new SerializedObject(item);
            serializedItem.FindProperty("itemName").stringValue = itemName;
            serializedItem.FindProperty("description").stringValue = $"레벨 {level} {armorBaseName}";
            serializedItem.FindProperty("itemType").enumValueIndex = (int)ItemType.Equipment;
            serializedItem.FindProperty("rarity").enumValueIndex = GetRarityByLevel(level);

            serializedItem.FindProperty("equipmentType").enumValueIndex = (int)EquipmentType.Armor;
            serializedItem.FindProperty("armorType").enumValueIndex = (int)selectedArmorType;

            float baseHp = 100f + (level - 1) * 20f;
            float baseDefense = 5f + (level - 1) * 2f;
            serializedItem.FindProperty("baseHp").floatValue = baseHp;
            serializedItem.FindProperty("baseDefense").floatValue = baseDefense;
            serializedItem.FindProperty("hpPerLevel").floatValue = 10f;
            serializedItem.FindProperty("defensePerLevel").floatValue = 1f;

            serializedItem.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(item, assetPath);
            Debug.Log($"생성됨: {itemName}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ItemEquipmentTools] 방어구 {armorCount}개 생성 완료!");
        Debug.Log("'Rebuild Item Database'를 실행하여 등록하세요.");
    }

    private int GetRarityByLevel(int level)
    {
        if (level <= 5) return (int)ItemRarity.Common;
        if (level <= 10) return (int)ItemRarity.Uncommon;
        if (level <= 15) return (int)ItemRarity.Rare;
        if (level <= 20) return (int)ItemRarity.Epic;
        return (int)ItemRarity.Legendary;
    }

    #endregion

    #region Tab: Database

    private void DrawDatabaseTab()
    {
        EditorGUILayout.LabelField("Equipment Database", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "WeaponTypeData, ArmorTypeData를 EquipmentDatabase에 등록합니다.",
            MessageType.Info);

        if (GUILayout.Button("Rebuild Equipment Database", GUILayout.Height(30)))
        {
            RebuildEquipmentDatabase();
        }

        GUILayout.Space(20);

        EditorGUILayout.LabelField("Item Database", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "모든 아이템(Equipment, Consumable, Material, Quest)을 ItemDatabase에 등록합니다.",
            MessageType.Info);

        if (GUILayout.Button("Rebuild Item Database", GUILayout.Height(30)))
        {
            RebuildItemDatabase();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Rebuild All Databases", GUILayout.Height(40)))
        {
            RebuildEquipmentDatabase();
            RebuildItemDatabase();
        }
    }

    public static void RebuildEquipmentDatabase()
    {
        EquipmentDatabase database = AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(EQUIPMENT_DATABASE_PATH);
        if (database == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            database = ScriptableObject.CreateInstance<EquipmentDatabase>();
            AssetDatabase.CreateAsset(database, EQUIPMENT_DATABASE_PATH);
            Debug.Log($"[ItemEquipmentTools] Created new EquipmentDatabase");
        }

        List<WeaponTypeData> weaponTypes = new List<WeaponTypeData>();
        string[] weaponGuids = AssetDatabase.FindAssets("t:WeaponTypeData", new[] { TYPES_FOLDER_PATH });
        foreach (string guid in weaponGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponTypeData typeData = AssetDatabase.LoadAssetAtPath<WeaponTypeData>(path);
            if (typeData != null)
            {
                weaponTypes.Add(typeData);
            }
        }

        List<ArmorTypeData> armorTypes = new List<ArmorTypeData>();
        string[] armorGuids = AssetDatabase.FindAssets("t:ArmorTypeData", new[] { TYPES_FOLDER_PATH });
        foreach (string guid in armorGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ArmorTypeData typeData = AssetDatabase.LoadAssetAtPath<ArmorTypeData>(path);
            if (typeData != null)
            {
                armorTypes.Add(typeData);
            }
        }

        weaponTypes = weaponTypes.OrderBy(t => t.WeaponType).ToList();
        armorTypes = armorTypes.OrderBy(t => t.ArmorType).ToList();

        SerializedObject serializedDatabase = new SerializedObject(database);

        SerializedProperty weaponTypesProperty = serializedDatabase.FindProperty("weaponTypes");
        weaponTypesProperty.arraySize = weaponTypes.Count;
        for (int i = 0; i < weaponTypes.Count; i++)
        {
            weaponTypesProperty.GetArrayElementAtIndex(i).objectReferenceValue = weaponTypes[i];
        }

        SerializedProperty armorTypesProperty = serializedDatabase.FindProperty("armorTypes");
        armorTypesProperty.arraySize = armorTypes.Count;
        for (int i = 0; i < armorTypes.Count; i++)
        {
            armorTypesProperty.GetArrayElementAtIndex(i).objectReferenceValue = armorTypes[i];
        }

        serializedDatabase.ApplyModifiedProperties();
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ItemEquipmentTools] Equipment Database rebuilt!");
        Debug.Log($"  - Weapon Types: {weaponTypes.Count}");
        Debug.Log($"  - Armor Types: {armorTypes.Count}");

        Selection.activeObject = database;
        EditorGUIUtility.PingObject(database);
    }

    public static void RebuildItemDatabase()
    {
        ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(ITEM_DATABASE_PATH);
        if (database == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            database = ScriptableObject.CreateInstance<ItemDatabase>();
            AssetDatabase.CreateAsset(database, ITEM_DATABASE_PATH);
            Debug.Log($"[ItemEquipmentTools] Created new ItemDatabase");
        }

        List<ItemData> allItems = new List<ItemData>();

        // EquipmentItemData
        string[] equipmentGuids = AssetDatabase.FindAssets("t:EquipmentItemData", new[] { ITEMS_FOLDER_PATH });
        foreach (string guid in equipmentGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EquipmentItemData item = AssetDatabase.LoadAssetAtPath<EquipmentItemData>(path);
            if (item != null) allItems.Add(item);
        }

        // ConsumableItemData
        string[] consumableGuids = AssetDatabase.FindAssets("t:ConsumableItemData", new[] { ITEMS_FOLDER_PATH });
        foreach (string guid in consumableGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ConsumableItemData item = AssetDatabase.LoadAssetAtPath<ConsumableItemData>(path);
            if (item != null) allItems.Add(item);
        }

        // MaterialItemData
        string[] materialGuids = AssetDatabase.FindAssets("t:MaterialItemData", new[] { ITEMS_FOLDER_PATH });
        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MaterialItemData item = AssetDatabase.LoadAssetAtPath<MaterialItemData>(path);
            if (item != null) allItems.Add(item);
        }

        // QuestItemData
        string[] questGuids = AssetDatabase.FindAssets("t:QuestItemData", new[] { ITEMS_FOLDER_PATH });
        foreach (string guid in questGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            QuestItemData item = AssetDatabase.LoadAssetAtPath<QuestItemData>(path);
            if (item != null) allItems.Add(item);
        }

        allItems = allItems.OrderBy(item => item.name).ToList();

        SerializedObject serializedDatabase = new SerializedObject(database);
        SerializedProperty itemsProperty = serializedDatabase.FindProperty("items");
        itemsProperty.arraySize = allItems.Count;

        for (int i = 0; i < allItems.Count; i++)
        {
            itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue = allItems[i];
        }

        serializedDatabase.ApplyModifiedProperties();
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int equipCount = allItems.Count(i => i is EquipmentItemData);
        int consumableCount = allItems.Count(i => i is ConsumableItemData);
        int materialCount = allItems.Count(i => i is MaterialItemData);
        int questCount = allItems.Count(i => i is QuestItemData);

        Debug.Log($"[ItemEquipmentTools] Item Database rebuilt! Total: {allItems.Count}");
        Debug.Log($"  - Equipment: {equipCount}");
        Debug.Log($"  - Consumable: {consumableCount}");
        Debug.Log($"  - Material: {materialCount}");
        Debug.Log($"  - Quest: {questCount}");

        Selection.activeObject = database;
        EditorGUIUtility.PingObject(database);
    }

    #endregion

    #region Tab: UI Builder

    private void DrawUIBuilderTab()
    {
        EditorGUILayout.LabelField("UI 빌더", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "인벤토리 UI와 장비 UI를 각각 분리하여 생성합니다.\n" +
            "장비 UI: 화면 왼쪽 / 인벤토리 UI: 화면 오른쪽",
            MessageType.Info);

        GUILayout.Space(10);

        // 슬롯 프리팹
        EditorGUILayout.LabelField("1. 인벤토리 슬롯 프리팹", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("드래그앤드롭이 가능한 인벤토리 슬롯 프리팹을 생성합니다.\n(우측 상단 개수 표시, 배경)", MessageType.None);
        if (GUILayout.Button("Create Inventory Slot Prefab", GUILayout.Height(30)))
        {
            CreateInventorySlotPrefab();
        }

        GUILayout.Space(15);

        // 장비 UI (왼쪽)
        EditorGUILayout.LabelField("2. 장비 UI (왼쪽)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("캐릭터 장비 슬롯과 스탯을 표시하는 장비 패널을 생성합니다.", MessageType.None);
        if (GUILayout.Button("Create Equipment UI", GUILayout.Height(30)))
        {
            CreateEquipmentUI();
        }

        GUILayout.Space(15);

        // 인벤토리 UI (오른쪽)
        EditorGUILayout.LabelField("3. 인벤토리 UI (오른쪽)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("아이템 슬롯 그리드가 있는 인벤토리 패널을 생성합니다.", MessageType.None);
        if (GUILayout.Button("Create Inventory UI", GUILayout.Height(30)))
        {
            CreateInventoryUI();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Create All (Slot + Equipment + Inventory)", GUILayout.Height(40)))
        {
            CreateInventorySlotPrefab();
            CreateEquipmentUI();
            CreateInventoryUI();
        }
    }

    public static void CreateInventorySlotPrefab()
    {
        // 슬롯 루트 (55x55)
        GameObject slotObj = new GameObject("InventorySlot");
        RectTransform slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(55, 55);

        // CanvasGroup for drag alpha
        CanvasGroup canvasGroup = slotObj.AddComponent<CanvasGroup>();

        // Background
        Image backgroundImage = slotObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // Icon (중앙, 약간 작게)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(4, 4);
        iconRect.offsetMax = new Vector2(-4, -4);

        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = Color.white;
        iconImage.enabled = false;
        iconImage.raycastTarget = false;

        // Quantity Text (우측 상단)
        GameObject quantityObj = new GameObject("Quantity");
        quantityObj.transform.SetParent(slotObj.transform, false);
        RectTransform quantityRect = quantityObj.AddComponent<RectTransform>();
        quantityRect.anchorMin = new Vector2(1, 1);
        quantityRect.anchorMax = new Vector2(1, 1);
        quantityRect.pivot = new Vector2(1, 1);
        quantityRect.sizeDelta = new Vector2(30, 18);
        quantityRect.anchoredPosition = new Vector2(-2, -2);

        TextMeshProUGUI quantityText = quantityObj.AddComponent<TextMeshProUGUI>();
        quantityText.text = "";
        quantityText.fontSize = 12;
        quantityText.fontStyle = FontStyles.Bold;
        quantityText.alignment = TextAlignmentOptions.Right;
        quantityText.color = Color.white;
        quantityText.raycastTarget = false;

        // Outline for quantity (가독성)
        // quantityText에 자동으로 outline 효과가 있으면 좋지만 TMP 설정으로 해야 함

        // InventoryItemSlotUI 컴포넌트 추가
        InventoryItemSlotUI slotUI = slotObj.AddComponent<InventoryItemSlotUI>();
        SerializedObject serializedSlot = new SerializedObject(slotUI);
        serializedSlot.FindProperty("iconImage").objectReferenceValue = iconImage;
        serializedSlot.FindProperty("quantityText").objectReferenceValue = quantityText;
        serializedSlot.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedSlot.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;
        serializedSlot.FindProperty("normalColor").colorValue = new Color(0.2f, 0.2f, 0.2f, 1f);
        serializedSlot.FindProperty("emptyColor").colorValue = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        serializedSlot.FindProperty("highlightColor").colorValue = new Color(0.35f, 0.35f, 0.35f, 1f);
        serializedSlot.ApplyModifiedProperties();

        // 프리팹 저장
        string prefabPath = "Assets/Prefabs/UI/";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(slotObj, prefabPath + "InventorySlot.prefab");
        DestroyImmediate(slotObj);

        Debug.Log("[ItemEquipmentTools] Inventory Slot Prefab created!");
        Debug.Log("InventoryUI의 slotPrefab에 이 프리팹을 할당하세요.");

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }

    public static void CreateInventoryUI()
    {
        Canvas canvas = FindOrCreateCanvas();

        // 인벤토리 패널 (화면 오른쪽)
        GameObject panelObj = new GameObject("InventoryPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0.5f);
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.pivot = new Vector2(1, 0.5f);
        panelRect.sizeDelta = new Vector2(620, 720);
        panelRect.anchoredPosition = new Vector2(-30, 0);

        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

        // 타이틀 바
        CreateTitleBar(panelObj.transform, "인벤토리", out Button closeButton);

        // 인벤토리 그리드
        GameObject slotsContainerObj = new GameObject("SlotsContainer");
        slotsContainerObj.transform.SetParent(panelObj.transform, false);
        RectTransform slotsContainerRect = slotsContainerObj.AddComponent<RectTransform>();
        slotsContainerRect.anchorMin = new Vector2(0, 0);
        slotsContainerRect.anchorMax = new Vector2(1, 1);
        slotsContainerRect.offsetMin = new Vector2(15, 50);
        slotsContainerRect.offsetMax = new Vector2(-15, -60);

        GridLayoutGroup grid = slotsContainerObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(55, 55);
        grid.spacing = new Vector2(5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 10;
        grid.childAlignment = TextAnchor.UpperLeft;

        // 정보 텍스트
        GameObject infoTextObj = new GameObject("InfoText");
        infoTextObj.transform.SetParent(panelObj.transform, false);
        RectTransform infoTextRect = infoTextObj.AddComponent<RectTransform>();
        infoTextRect.anchorMin = new Vector2(0, 0);
        infoTextRect.anchorMax = new Vector2(1, 0);
        infoTextRect.pivot = new Vector2(0.5f, 0);
        infoTextRect.sizeDelta = new Vector2(0, 40);
        infoTextRect.anchoredPosition = new Vector2(0, 5);

        TextMeshProUGUI infoText = infoTextObj.AddComponent<TextMeshProUGUI>();
        infoText.text = "인벤토리: 0/100";
        infoText.fontSize = 14;
        infoText.alignment = TextAlignmentOptions.Center;
        infoText.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        // 툴팁
        GameObject tooltipObj = CreateInventoryTooltip(panelObj.transform, out InventoryTooltip tooltip);

        // 슬롯 프리팹 찾기
        string slotPrefabPath = "Assets/Prefabs/UI/InventorySlot.prefab";
        GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slotPrefabPath);

        // InventoryUI 컴포넌트
        InventoryUI inventoryUI = panelObj.AddComponent<InventoryUI>();
        SerializedObject serializedUI = new SerializedObject(inventoryUI);
        serializedUI.FindProperty("panel").objectReferenceValue = panelObj;
        serializedUI.FindProperty("closeButton").objectReferenceValue = closeButton;
        serializedUI.FindProperty("slotsContainer").objectReferenceValue = slotsContainerRect;
        serializedUI.FindProperty("tooltip").objectReferenceValue = tooltip;
        serializedUI.FindProperty("infoText").objectReferenceValue = infoText;

        if (slotPrefab != null)
        {
            serializedUI.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
        }

        serializedUI.ApplyModifiedProperties();

        panelObj.SetActive(false);

        // 프리팹 저장
        string prefabPath = "Assets/Prefabs/UI/";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        PrefabUtility.SaveAsPrefabAsset(panelObj, prefabPath + "InventoryPanel.prefab");

        if (slotPrefab != null)
        {
            Debug.Log("[ItemEquipmentTools] Inventory UI created! (slotPrefab auto-assigned)");
        }
        else
        {
            Debug.Log("[ItemEquipmentTools] Inventory UI created!");
            Debug.LogWarning("InventorySlot 프리팹이 없습니다. 'Create Inventory Slot Prefab'을 먼저 실행하세요.");
        }

        Selection.activeGameObject = panelObj;
    }

    public static void CreateEquipmentUI()
    {
        Canvas canvas = FindOrCreateCanvas();

        // 장비 패널 (화면 왼쪽)
        GameObject panelObj = new GameObject("EquipmentPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.5f);
        panelRect.anchorMax = new Vector2(0, 0.5f);
        panelRect.pivot = new Vector2(0, 0.5f);
        panelRect.sizeDelta = new Vector2(380, 550);
        panelRect.anchoredPosition = new Vector2(30, 0);

        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

        // 타이틀 바
        CreateTitleBar(panelObj.transform, "장비", out Button closeButton);

        // 장비 슬롯들
        float slotY = -70;
        CreateEquipmentSlot("WeaponSlot", panelObj.transform, slotY, "무기",
            out Image weaponIcon, out TMP_Text weaponNameText, out Button unequipWeaponBtn);

        slotY -= 150;
        CreateEquipmentSlot("ArmorSlot", panelObj.transform, slotY, "방어구",
            out Image armorIcon, out TMP_Text armorNameText, out Button unequipArmorBtn);

        // 스탯 표시
        GameObject statsObj = new GameObject("Stats");
        statsObj.transform.SetParent(panelObj.transform, false);
        RectTransform statsRect = statsObj.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 0);
        statsRect.anchorMax = new Vector2(1, 1);
        statsRect.offsetMin = new Vector2(15, 15);
        statsRect.offsetMax = new Vector2(-15, -380);

        TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.text = "<b><color=yellow>스탯</color></b>\n\n로딩 중...";
        statsText.fontSize = 14;
        statsText.alignment = TextAlignmentOptions.TopLeft;
        statsText.color = Color.white;

        // EquipmentUI 컴포넌트
        EquipmentUI equipmentUI = panelObj.AddComponent<EquipmentUI>();
        SerializedObject serializedUI = new SerializedObject(equipmentUI);
        serializedUI.FindProperty("panel").objectReferenceValue = panelObj;
        serializedUI.FindProperty("weaponIcon").objectReferenceValue = weaponIcon;
        serializedUI.FindProperty("weaponNameText").objectReferenceValue = weaponNameText;
        serializedUI.FindProperty("unequipWeaponButton").objectReferenceValue = unequipWeaponBtn;
        serializedUI.FindProperty("armorIcon").objectReferenceValue = armorIcon;
        serializedUI.FindProperty("armorNameText").objectReferenceValue = armorNameText;
        serializedUI.FindProperty("unequipArmorButton").objectReferenceValue = unequipArmorBtn;
        serializedUI.FindProperty("statsText").objectReferenceValue = statsText;
        serializedUI.ApplyModifiedProperties();

        panelObj.SetActive(false);

        // 프리팹 저장
        string prefabPath = "Assets/Prefabs/UI/";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        PrefabUtility.SaveAsPrefabAsset(panelObj, prefabPath + "EquipmentPanel.prefab");

        Debug.Log("[ItemEquipmentTools] Equipment UI created!");
        Selection.activeGameObject = panelObj;
    }

    private static Canvas FindOrCreateCanvas()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("[ItemEquipmentTools] Canvas created");
        }
        return canvas;
    }

    private static void CreateTitleBar(Transform parent, string title, out Button closeButton)
    {
        GameObject titleBarObj = new GameObject("TitleBar");
        titleBarObj.transform.SetParent(parent, false);
        RectTransform titleBarRect = titleBarObj.AddComponent<RectTransform>();
        titleBarRect.anchorMin = new Vector2(0, 1);
        titleBarRect.anchorMax = new Vector2(1, 1);
        titleBarRect.pivot = new Vector2(0.5f, 1);
        titleBarRect.sizeDelta = new Vector2(0, 50);
        titleBarRect.anchoredPosition = Vector2.zero;

        Image titleBarBg = titleBarObj.AddComponent<Image>();
        titleBarBg.color = new Color(0.12f, 0.12f, 0.12f, 1f);

        GameObject titleTextObj = new GameObject("Title");
        titleTextObj.transform.SetParent(titleBarObj.transform, false);
        RectTransform titleTextRect = titleTextObj.AddComponent<RectTransform>();
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = Vector2.one;
        titleTextRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 22;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(titleBarObj.transform, false);
        RectTransform closeButtonRect = closeButtonObj.AddComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(1, 0);
        closeButtonRect.anchorMax = new Vector2(1, 1);
        closeButtonRect.pivot = new Vector2(1, 0.5f);
        closeButtonRect.sizeDelta = new Vector2(40, 0);
        closeButtonRect.anchoredPosition = new Vector2(-5, 0);

        Image closeButtonImg = closeButtonObj.AddComponent<Image>();
        closeButtonImg.color = new Color(0.7f, 0.2f, 0.2f, 1f);

        closeButton = closeButtonObj.AddComponent<Button>();

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeButtonObj.transform, false);
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "X";
        closeText.fontSize = 18;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;
    }

    private static GameObject CreateEquipmentSlot(string name, Transform parent, float yPos, string label,
        out Image iconImage, out TMP_Text nameText, out Button unequipButton)
    {
        GameObject slotObj = new GameObject(name);
        slotObj.transform.SetParent(parent, false);
        RectTransform slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0, 1);
        slotRect.anchorMax = new Vector2(1, 1);
        slotRect.pivot = new Vector2(0.5f, 1);
        slotRect.sizeDelta = new Vector2(-30, 130);
        slotRect.anchoredPosition = new Vector2(0, yPos);

        Image slotBg = slotObj.AddComponent<Image>();
        slotBg.color = new Color(0.12f, 0.12f, 0.12f, 1f);

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(slotObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.pivot = new Vector2(0.5f, 1);
        labelRect.sizeDelta = new Vector2(0, 25);
        labelRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 14;
        labelText.fontStyle = FontStyles.Bold;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 1);
        iconRect.anchorMax = new Vector2(0, 1);
        iconRect.pivot = new Vector2(0, 1);
        iconRect.sizeDelta = new Vector2(70, 70);
        iconRect.anchoredPosition = new Vector2(15, -30);

        iconImage = iconObj.AddComponent<Image>();
        iconImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(slotObj.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.sizeDelta = new Vector2(-100, 50);
        nameRect.anchoredPosition = new Vector2(45, -35);

        nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "없음";
        nameText.fontSize = 14;
        nameText.alignment = TextAlignmentOptions.TopLeft;
        nameText.color = Color.white;

        GameObject unequipBtnObj = new GameObject("UnequipButton");
        unequipBtnObj.transform.SetParent(slotObj.transform, false);
        RectTransform unequipBtnRect = unequipBtnObj.AddComponent<RectTransform>();
        unequipBtnRect.anchorMin = new Vector2(0.5f, 0);
        unequipBtnRect.anchorMax = new Vector2(0.5f, 0);
        unequipBtnRect.pivot = new Vector2(0.5f, 0);
        unequipBtnRect.sizeDelta = new Vector2(100, 28);
        unequipBtnRect.anchoredPosition = new Vector2(0, 8);

        Image unequipBtnImg = unequipBtnObj.AddComponent<Image>();
        unequipBtnImg.color = new Color(0.6f, 0.2f, 0.2f, 1f);

        unequipButton = unequipBtnObj.AddComponent<Button>();

        GameObject unequipTextObj = new GameObject("Text");
        unequipTextObj.transform.SetParent(unequipBtnObj.transform, false);
        RectTransform unequipTextRect = unequipTextObj.AddComponent<RectTransform>();
        unequipTextRect.anchorMin = Vector2.zero;
        unequipTextRect.anchorMax = Vector2.one;
        unequipTextRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI unequipText = unequipTextObj.AddComponent<TextMeshProUGUI>();
        unequipText.text = "해제";
        unequipText.fontSize = 13;
        unequipText.alignment = TextAlignmentOptions.Center;
        unequipText.color = Color.white;

        return slotObj;
    }

    private static GameObject CreateInventoryTooltip(Transform parent, out InventoryTooltip tooltip)
    {
        GameObject tooltipObj = new GameObject("Tooltip");
        tooltipObj.transform.SetParent(parent, false);
        RectTransform tooltipRect = tooltipObj.AddComponent<RectTransform>();
        tooltipRect.anchorMin = new Vector2(0, 0);
        tooltipRect.anchorMax = new Vector2(0, 0);
        tooltipRect.pivot = new Vector2(0, 0);
        tooltipRect.sizeDelta = new Vector2(280, 180);
        tooltipRect.anchoredPosition = Vector2.zero;

        CanvasGroup tooltipCanvasGroup = tooltipObj.AddComponent<CanvasGroup>();
        tooltipCanvasGroup.alpha = 0;
        tooltipCanvasGroup.blocksRaycasts = false;

        Image tooltipBg = tooltipObj.AddComponent<Image>();
        tooltipBg.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);

        GameObject tooltipIconObj = new GameObject("Icon");
        tooltipIconObj.transform.SetParent(tooltipObj.transform, false);
        RectTransform tooltipIconRect = tooltipIconObj.AddComponent<RectTransform>();
        tooltipIconRect.anchorMin = new Vector2(0, 1);
        tooltipIconRect.anchorMax = new Vector2(0, 1);
        tooltipIconRect.pivot = new Vector2(0, 1);
        tooltipIconRect.sizeDelta = new Vector2(45, 45);
        tooltipIconRect.anchoredPosition = new Vector2(10, -10);

        Image tooltipIcon = tooltipIconObj.AddComponent<Image>();
        tooltipIcon.color = Color.white;

        GameObject tooltipNameObj = new GameObject("ItemName");
        tooltipNameObj.transform.SetParent(tooltipObj.transform, false);
        RectTransform tooltipNameRect = tooltipNameObj.AddComponent<RectTransform>();
        tooltipNameRect.anchorMin = new Vector2(0, 1);
        tooltipNameRect.anchorMax = new Vector2(1, 1);
        tooltipNameRect.pivot = new Vector2(0.5f, 1);
        tooltipNameRect.sizeDelta = new Vector2(-70, 28);
        tooltipNameRect.anchoredPosition = new Vector2(32, -10);

        TextMeshProUGUI tooltipNameText = tooltipNameObj.AddComponent<TextMeshProUGUI>();
        tooltipNameText.text = "아이템 이름";
        tooltipNameText.fontSize = 16;
        tooltipNameText.fontStyle = FontStyles.Bold;
        tooltipNameText.color = Color.white;

        GameObject tooltipTypeObj = new GameObject("ItemType");
        tooltipTypeObj.transform.SetParent(tooltipObj.transform, false);
        RectTransform tooltipTypeRect = tooltipTypeObj.AddComponent<RectTransform>();
        tooltipTypeRect.anchorMin = new Vector2(0, 1);
        tooltipTypeRect.anchorMax = new Vector2(1, 1);
        tooltipTypeRect.pivot = new Vector2(0.5f, 1);
        tooltipTypeRect.sizeDelta = new Vector2(-70, 20);
        tooltipTypeRect.anchoredPosition = new Vector2(32, -42);

        TextMeshProUGUI tooltipTypeText = tooltipTypeObj.AddComponent<TextMeshProUGUI>();
        tooltipTypeText.text = "장비";
        tooltipTypeText.fontSize = 12;
        tooltipTypeText.color = new Color(0.7f, 0.7f, 0.7f, 1f);

        GameObject tooltipDescObj = new GameObject("ItemDescription");
        tooltipDescObj.transform.SetParent(tooltipObj.transform, false);
        RectTransform tooltipDescRect = tooltipDescObj.AddComponent<RectTransform>();
        tooltipDescRect.anchorMin = new Vector2(0, 0);
        tooltipDescRect.anchorMax = new Vector2(1, 1);
        tooltipDescRect.offsetMin = new Vector2(10, 10);
        tooltipDescRect.offsetMax = new Vector2(-10, -65);

        TextMeshProUGUI tooltipDescText = tooltipDescObj.AddComponent<TextMeshProUGUI>();
        tooltipDescText.text = "아이템 설명";
        tooltipDescText.fontSize = 13;
        tooltipDescText.color = Color.white;
        tooltipDescText.alignment = TextAlignmentOptions.TopLeft;

        tooltip = tooltipObj.AddComponent<InventoryTooltip>();
        SerializedObject serializedTooltip = new SerializedObject(tooltip);
        serializedTooltip.FindProperty("itemNameText").objectReferenceValue = tooltipNameText;
        serializedTooltip.FindProperty("itemTypeText").objectReferenceValue = tooltipTypeText;
        serializedTooltip.FindProperty("itemDescriptionText").objectReferenceValue = tooltipDescText;
        serializedTooltip.FindProperty("iconImage").objectReferenceValue = tooltipIcon;
        serializedTooltip.FindProperty("canvasGroup").objectReferenceValue = tooltipCanvasGroup;
        serializedTooltip.FindProperty("tooltipRect").objectReferenceValue = tooltipRect;
        serializedTooltip.ApplyModifiedProperties();

        return tooltipObj;
    }

    #endregion
}
