using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.IO;
using Items;

/// <summary>
/// JSON 기반 아이템 시스템 에디터 도구
/// - JSON 파일 직접 편집
/// - ItemIconDatabase 관리
/// - UI 빌더
/// - 런타임 테스트 지급
/// </summary>
public class ItemJsonEditorTools : EditorWindow
{
    private const string ITEMS_JSON_PATH = "Assets/Resources/Data/items.json";
    private const string TYPES_JSON_PATH = "Assets/Resources/Data/equipment_types.json";
    private const string ICON_DB_PATH = "Assets/Resources/ItemIconDatabase.asset";

    private int selectedTab = 0;
    private readonly string[] tabNames = { "아이템 추가", "타입 배율", "아이콘 매핑", "UI 빌더", "테스트 지급" };

    private Vector2 scrollPosition;

    // 아이템 추가 필드
    private int newItemId = 0;
    private string newItemName = "새 아이템";
    private string newItemDescription = "아이템 설명";
    private Items.ItemType newItemType = Items.ItemType.Equipment;
    private Items.ItemRarity newItemRarity = Items.ItemRarity.Common;
    private Items.EquipmentType newEquipmentType = Items.EquipmentType.Weapon;
    private Items.WeaponType newWeaponType = Items.WeaponType.TwoHandedSword;
    private Items.ArmorType newArmorType = Items.ArmorType.Plate;
    private float newBaseAttack = 10f;
    private float newBaseAttackSpeed = 1f;
    private float newBaseCritChance = 5f;
    private float newBaseCritDamage = 2f;
    private float newBaseHp = 50f;
    private float newBaseDefense = 5f;
    private int newSellPrice = 100;

    // 소비 아이템 필드
    private Items.ConsumableEffectType newConsumableEffect = Items.ConsumableEffectType.RestoreHP;
    private float newEffectValue = 50f;
    private float newCooldown = 5f;
    private int newMaxStack = 99;

    [MenuItem("Tools/Item System/JSON Editor", false, 0)]
    public static void ShowWindow()
    {
        GetWindow<ItemJsonEditorTools>("Item JSON Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Item JSON Editor", EditorStyles.boldLabel);
        GUILayout.Space(5);

        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        GUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        switch (selectedTab)
        {
            case 0: DrawAddItemTab(); break;
            case 1: DrawTypesTab(); break;
            case 2: DrawAssetMappingTab(); break;
            case 3: DrawUIBuilderTab(); break;
            case 4: DrawTestGiveTab(); break;
        }

        EditorGUILayout.EndScrollView();
    }

    #region Tab: Add Item

    private void DrawAddItemTab()
    {
        EditorGUILayout.LabelField("아이템 추가", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("새 아이템을 items.json에 추가합니다.", MessageType.Info);

        GUILayout.Space(10);

        // 기본 정보
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        newItemId = EditorGUILayout.IntField("아이템 ID", newItemId);
        newItemName = EditorGUILayout.TextField("이름", newItemName);
        newItemDescription = EditorGUILayout.TextField("설명", newItemDescription);
        newItemType = (Items.ItemType)EditorGUILayout.EnumPopup("타입", newItemType);
        newItemRarity = (Items.ItemRarity)EditorGUILayout.EnumPopup("희귀도", newItemRarity);
        newSellPrice = EditorGUILayout.IntField("판매 가격", newSellPrice);

        GUILayout.Space(10);

        if (newItemType == Items.ItemType.Equipment)
        {
            DrawEquipmentFields();
        }
        else if (newItemType == Items.ItemType.Consumable)
        {
            DrawConsumableFields();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("아이템 추가", GUILayout.Height(35)))
        {
            AddItemToJson();
        }

        GUILayout.Space(10);

        EditorGUILayout.LabelField("빠른 생성", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("기본 무기 세트 (6종)"))
        {
            AddDefaultWeaponSet();
        }
        if (GUILayout.Button("기본 방어구 세트 (5종)"))
        {
            AddDefaultArmorSet();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("기본 포션 세트 (6종)"))
        {
            AddDefaultPotionSet();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEquipmentFields()
    {
        EditorGUILayout.LabelField("장비 정보", EditorStyles.boldLabel);
        newEquipmentType = (Items.EquipmentType)EditorGUILayout.EnumPopup("장비 타입", newEquipmentType);

        if (newEquipmentType == Items.EquipmentType.Weapon)
        {
            newWeaponType = (Items.WeaponType)EditorGUILayout.EnumPopup("무기 타입", newWeaponType);
            newBaseAttack = EditorGUILayout.FloatField("기본 공격력", newBaseAttack);
            newBaseAttackSpeed = EditorGUILayout.FloatField("공격 속도", newBaseAttackSpeed);
            newBaseCritChance = EditorGUILayout.FloatField("치명타 확률", newBaseCritChance);
            newBaseCritDamage = EditorGUILayout.FloatField("치명타 데미지", newBaseCritDamage);
        }
        else
        {
            newArmorType = (Items.ArmorType)EditorGUILayout.EnumPopup("방어구 타입", newArmorType);
            newBaseHp = EditorGUILayout.FloatField("기본 HP", newBaseHp);
            newBaseDefense = EditorGUILayout.FloatField("기본 방어력", newBaseDefense);
        }
    }

    private void DrawConsumableFields()
    {
        EditorGUILayout.LabelField("소비 아이템 정보", EditorStyles.boldLabel);
        newConsumableEffect = (Items.ConsumableEffectType)EditorGUILayout.EnumPopup("효과 타입", newConsumableEffect);
        newEffectValue = EditorGUILayout.FloatField("효과 수치", newEffectValue);
        newCooldown = EditorGUILayout.FloatField("쿨다운 (초)", newCooldown);
        newMaxStack = EditorGUILayout.IntSlider("최대 스택", newMaxStack, 1, 999);
    }

    private int GetNextItemId(ItemsJsonData data)
    {
        int maxId = -1;
        if (data.equipment != null)
            foreach (var item in data.equipment)
                if (item.id > maxId) maxId = item.id;
        if (data.consumables != null)
            foreach (var item in data.consumables)
                if (item.id > maxId) maxId = item.id;
        if (data.materials != null)
            foreach (var item in data.materials)
                if (item.id > maxId) maxId = item.id;
        if (data.quests != null)
            foreach (var item in data.quests)
                if (item.id > maxId) maxId = item.id;
        return maxId + 1;
    }

    private void AddItemToJson()
    {
        string jsonPath = Path.Combine(Application.dataPath, "Resources/Data/items.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError("[ItemJsonEditor] items.json not found!");
            return;
        }

        string jsonContent = File.ReadAllText(jsonPath);
        var data = JsonUtility.FromJson<ItemsJsonData>(jsonContent);

        if (newItemType == Items.ItemType.Equipment)
        {
            var item = new EquipmentJsonData
            {
                id = newItemId,
                name = newItemName,
                description = newItemDescription,
                itemType = "Equipment",
                rarity = newItemRarity.ToString(),
                sellPrice = newSellPrice,
                equipmentType = newEquipmentType.ToString(),
                weaponType = newEquipmentType == Items.EquipmentType.Weapon ? newWeaponType.ToString() : "",
                armorType = newEquipmentType == Items.EquipmentType.Armor ? newArmorType.ToString() : "",
                baseAttack = newBaseAttack,
                baseAttackSpeed = newBaseAttackSpeed,
                baseCriticalChance = newBaseCritChance,
                baseCriticalDamage = newBaseCritDamage,
                baseHp = newBaseHp,
                baseDefense = newBaseDefense,
                isTradeable = true,
                isDroppable = true
            };

            if (data.equipment == null) data.equipment = new List<EquipmentJsonData>();
            data.equipment.Add(item);
        }
        else if (newItemType == Items.ItemType.Consumable)
        {
            var item = new ConsumableJsonData
            {
                id = newItemId,
                name = newItemName,
                description = newItemDescription,
                itemType = "Consumable",
                rarity = newItemRarity.ToString(),
                sellPrice = newSellPrice,
                maxStackSize = newMaxStack,
                cooldown = newCooldown,
                effects = new List<ConsumableEffectJsonData>
                {
                    new ConsumableEffectJsonData
                    {
                        type = newConsumableEffect.ToString(),
                        value = newEffectValue,
                        duration = 0
                    }
                },
                isTradeable = true,
                isDroppable = true
            };

            if (data.consumables == null) data.consumables = new List<ConsumableJsonData>();
            data.consumables.Add(item);
        }

        string newJson = JsonUtility.ToJson(data, true);
        File.WriteAllText(jsonPath, newJson);
        AssetDatabase.Refresh();

        Debug.Log($"[ItemJsonEditor] Added: {newItemName} (ID: {newItemId})");

        // 다음 ID로 자동 증가
        newItemId++;
    }

    private void AddDefaultWeaponSet()
    {
        string jsonPath = Path.Combine(Application.dataPath, "Resources/Data/items.json");
        string jsonContent = File.ReadAllText(jsonPath);
        var data = JsonUtility.FromJson<ItemsJsonData>(jsonContent);

        if (data.equipment == null) data.equipment = new List<EquipmentJsonData>();

        int nextId = GetNextItemId(data);

        var weaponTypes = new[]
        {
            (Items.WeaponType.TwoHandedSword, "양손검", 15f, 0.8f),
            (Items.WeaponType.SwordAndShield, "검과 방패", 10f, 1.0f),
            (Items.WeaponType.Spear, "창", 12f, 1.1f),
            (Items.WeaponType.Hammer, "해머", 20f, 0.6f),
            (Items.WeaponType.Gauntlet, "건틀릿", 8f, 1.4f),
            (Items.WeaponType.Bow, "활", 11f, 1.0f)
        };

        foreach (var (type, name, atk, spd) in weaponTypes)
        {
            var item = new EquipmentJsonData
            {
                id = nextId++,
                name = $"기본 {name}",
                description = $"기본적인 {name}입니다.",
                itemType = "Equipment",
                rarity = "Common",
                equipmentType = "Weapon",
                weaponType = type.ToString(),
                baseAttack = atk,
                baseAttackSpeed = spd,
                baseCriticalChance = 5f,
                baseCriticalDamage = 2f,
                attackPerLevel = 2f,
                sellPrice = 100,
                isTradeable = true,
                isDroppable = true
            };
            data.equipment.Add(item);
        }

        File.WriteAllText(jsonPath, JsonUtility.ToJson(data, true));
        AssetDatabase.Refresh();
        Debug.Log("[ItemJsonEditor] Added 6 basic weapons");
    }

    private void AddDefaultArmorSet()
    {
        string jsonPath = Path.Combine(Application.dataPath, "Resources/Data/items.json");
        string jsonContent = File.ReadAllText(jsonPath);
        var data = JsonUtility.FromJson<ItemsJsonData>(jsonContent);

        if (data.equipment == null) data.equipment = new List<EquipmentJsonData>();

        int nextId = GetNextItemId(data);

        var armorTypes = new[]
        {
            (Items.ArmorType.Cloth, "천 로브", 30f, 3f),
            (Items.ArmorType.Leather, "가죽 갑옷", 50f, 5f),
            (Items.ArmorType.Light, "경갑", 70f, 8f),
            (Items.ArmorType.Heavy, "중갑", 90f, 12f),
            (Items.ArmorType.Plate, "판금 갑옷", 120f, 18f)
        };

        foreach (var (type, name, hp, def) in armorTypes)
        {
            var item = new EquipmentJsonData
            {
                id = nextId++,
                name = $"기본 {name}",
                description = $"기본적인 {name}입니다.",
                itemType = "Equipment",
                rarity = "Common",
                equipmentType = "Armor",
                armorType = type.ToString(),
                baseHp = hp,
                baseDefense = def,
                hpPerLevel = 10f,
                defensePerLevel = 1f,
                sellPrice = 100,
                isTradeable = true,
                isDroppable = true
            };
            data.equipment.Add(item);
        }

        File.WriteAllText(jsonPath, JsonUtility.ToJson(data, true));
        AssetDatabase.Refresh();
        Debug.Log("[ItemJsonEditor] Added 5 basic armors");
    }

    private void AddDefaultPotionSet()
    {
        string jsonPath = Path.Combine(Application.dataPath, "Resources/Data/items.json");
        string jsonContent = File.ReadAllText(jsonPath);
        var data = JsonUtility.FromJson<ItemsJsonData>(jsonContent);

        if (data.consumables == null) data.consumables = new List<ConsumableJsonData>();

        int nextId = GetNextItemId(data);

        var potions = new[]
        {
            ("소형 HP 포션", "RestoreHP", 50f, 10),
            ("중형 HP 포션", "RestoreHP", 150f, 30),
            ("대형 HP 포션", "RestoreHP", 300f, 80),
            ("소형 마나 포션", "RestoreMana", 30f, 10),
            ("중형 마나 포션", "RestoreMana", 80f, 30),
            ("대형 마나 포션", "RestoreMana", 150f, 80)
        };

        foreach (var (name, effect, value, price) in potions)
        {
            var item = new ConsumableJsonData
            {
                id = nextId++,
                name = name,
                description = $"{(effect == "RestoreHP" ? "HP" : "마나")}를 {value} 회복합니다.",
                itemType = "Consumable",
                rarity = price <= 10 ? "Common" : price <= 30 ? "Uncommon" : "Rare",
                maxStackSize = 99,
                cooldown = 5f,
                effects = new List<ConsumableEffectJsonData>
                {
                    new ConsumableEffectJsonData { type = effect, value = value, duration = 0 }
                },
                sellPrice = price,
                isTradeable = true,
                isDroppable = true
            };
            data.consumables.Add(item);
        }

        File.WriteAllText(jsonPath, JsonUtility.ToJson(data, true));
        AssetDatabase.Refresh();
        Debug.Log("[ItemJsonEditor] Added 6 basic potions");
    }

    #endregion

    #region Tab: Types

    private void DrawTypesTab()
    {
        EditorGUILayout.LabelField("장비 타입 배율", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("equipment_types.json의 타입 배율을 표시합니다.", MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("JSON 파일 열기"))
        {
            string path = Path.Combine(Application.dataPath, "Resources/Data/equipment_types.json");
            if (File.Exists(path))
            {
                System.Diagnostics.Process.Start(path);
            }
        }

        GUILayout.Space(10);

        // 현재 타입 배율 표시
        string jsonPath = Path.Combine(Application.dataPath, "Resources/Data/equipment_types.json");
        if (File.Exists(jsonPath))
        {
            string content = File.ReadAllText(jsonPath);
            var data = JsonUtility.FromJson<EquipmentTypesJsonData>(content);

            EditorGUILayout.LabelField("무기 타입", EditorStyles.boldLabel);
            if (data.weaponTypes != null)
            {
                foreach (var wt in data.weaponTypes)
                {
                    EditorGUILayout.LabelField($"  {wt.type}: 공격 {wt.attackMultiplier:F1}x, 속도 {wt.attackSpeedMultiplier:F1}x");
                }
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("방어구 타입", EditorStyles.boldLabel);
            if (data.armorTypes != null)
            {
                foreach (var at in data.armorTypes)
                {
                    EditorGUILayout.LabelField($"  {at.type}: HP {at.hpMultiplier:F1}x, 방어 {at.defenseMultiplier:F1}x");
                }
            }
        }
    }

    #endregion

    #region Tab: Icon Mapping

    private void DrawAssetMappingTab()
    {
        EditorGUILayout.LabelField("아이콘 매핑 (ItemIconDatabase)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "아이템 ID와 아이콘 스프라이트를 매핑합니다.\n" +
            "ItemIconDatabase ScriptableObject를 Resources 폴더에 배치하세요.",
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("ItemIconDatabase 생성/열기", GUILayout.Height(30)))
        {
            CreateOrOpenIconDatabase();
        }

        GUILayout.Space(10);

        // 현재 아이콘 DB 정보 표시
        var iconDb = Resources.Load<ItemIconDatabase>("ItemIconDatabase");
        if (iconDb != null)
        {
            var entries = iconDb.GetAllEntries();
            EditorGUILayout.LabelField($"등록된 아이콘: {entries.Count}개");

            GUILayout.Space(5);

            foreach (var entry in entries)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"ID: {entry.itemId}", GUILayout.Width(80));
                EditorGUILayout.ObjectField(entry.icon, typeof(Sprite), false, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("ItemIconDatabase가 없습니다. 생성 버튼을 클릭하세요.", MessageType.Warning);
        }
    }

    private void CreateOrOpenIconDatabase()
    {
        string path = "Assets/Resources/ItemIconDatabase.asset";
        var iconDb = AssetDatabase.LoadAssetAtPath<ItemIconDatabase>(path);

        if (iconDb == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            iconDb = ScriptableObject.CreateInstance<ItemIconDatabase>();
            AssetDatabase.CreateAsset(iconDb, path);
            AssetDatabase.SaveAssets();
            Debug.Log("[ItemJsonEditor] ItemIconDatabase created");
        }

        Selection.activeObject = iconDb;
        EditorGUIUtility.PingObject(iconDb);
    }

    #endregion

    #region Tab: UI Builder

    private void DrawUIBuilderTab()
    {
        EditorGUILayout.LabelField("UI 빌더", EditorStyles.boldLabel);

        GUILayout.Space(10);

        if (GUILayout.Button("인벤토리 슬롯 프리팹 생성", GUILayout.Height(30)))
        {
            CreateInventorySlotPrefab();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("인벤토리 UI 생성", GUILayout.Height(30)))
        {
            CreateInventoryUI();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("장비 UI 생성", GUILayout.Height(30)))
        {
            CreateEquipmentUI();
        }
    }

    private static void CreateInventorySlotPrefab()
    {
        GameObject slotObj = new GameObject("InventorySlot");
        RectTransform slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(55, 55);

        CanvasGroup canvasGroup = slotObj.AddComponent<CanvasGroup>();
        Image backgroundImage = slotObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(4, 4);
        iconRect.offsetMax = new Vector2(-4, -4);

        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.enabled = false;
        iconImage.raycastTarget = false;

        GameObject quantityObj = new GameObject("Quantity");
        quantityObj.transform.SetParent(slotObj.transform, false);
        RectTransform quantityRect = quantityObj.AddComponent<RectTransform>();
        quantityRect.anchorMin = new Vector2(1, 1);
        quantityRect.anchorMax = new Vector2(1, 1);
        quantityRect.pivot = new Vector2(1, 1);
        quantityRect.sizeDelta = new Vector2(30, 18);
        quantityRect.anchoredPosition = new Vector2(-2, -2);

        TextMeshProUGUI quantityText = quantityObj.AddComponent<TextMeshProUGUI>();
        quantityText.fontSize = 12;
        quantityText.fontStyle = FontStyles.Bold;
        quantityText.alignment = TextAlignmentOptions.Right;
        quantityText.raycastTarget = false;

        InventoryItemSlotUI slotUI = slotObj.AddComponent<InventoryItemSlotUI>();
        SerializedObject serializedSlot = new SerializedObject(slotUI);
        serializedSlot.FindProperty("iconImage").objectReferenceValue = iconImage;
        serializedSlot.FindProperty("quantityText").objectReferenceValue = quantityText;
        serializedSlot.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedSlot.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;
        serializedSlot.ApplyModifiedProperties();

        string prefabPath = "Assets/Prefabs/UI/";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        PrefabUtility.SaveAsPrefabAsset(slotObj, prefabPath + "InventorySlot.prefab");
        DestroyImmediate(slotObj);

        Debug.Log("[ItemJsonEditor] InventorySlot prefab created");
    }

    private static void CreateInventoryUI()
    {
        Debug.Log("[ItemJsonEditor] Use the original ItemEquipmentTools for full UI creation, or manually create UI");
    }

    private static void CreateEquipmentUI()
    {
        Debug.Log("[ItemJsonEditor] Use the original ItemEquipmentTools for full UI creation, or manually create UI");
    }

    #endregion

    #region Tab: Test Give

    private int testGiveIndex = 0;
    private int testGiveQuantity = 1;

    private void DrawTestGiveTab()
    {
        EditorGUILayout.LabelField("런타임 아이템 지급 (테스트)", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 모드에서만 사용 가능합니다.", MessageType.Warning);
            return;
        }

        var itemManager = ItemManager.Instance;
        if (!itemManager.IsInitialized)
        {
            EditorGUILayout.HelpBox("ItemManager가 초기화되지 않았습니다.", MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField($"등록된 아이템 수: {itemManager.ItemCount}");

        GUILayout.Space(10);

        testGiveIndex = EditorGUILayout.IntField("아이템 인덱스", testGiveIndex);
        testGiveQuantity = EditorGUILayout.IntSlider("수량", testGiveQuantity, 1, 99);

        var item = itemManager.GetItem(testGiveIndex);
        if (item != null)
        {
            EditorGUILayout.LabelField($"선택: {item.Name} ({item.ItemType})");
        }

        GUILayout.Space(10);

        if (GUILayout.Button("지급", GUILayout.Height(30)))
        {
            GiveItemToLocalPlayer(testGiveIndex, testGiveQuantity);
        }
    }

    private static void GiveItemToLocalPlayer(int index, int quantity)
    {
        var playerControllers = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var pc in playerControllers)
        {
            if (pc.isOwned || pc.isLocalPlayer)
            {
                var inventory = pc.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    inventory.ServerAddItem(index, quantity);
                    Debug.Log($"[ItemJsonEditor] Given item index {index} x{quantity}");
                    return;
                }
            }
        }
        Debug.LogWarning("[ItemJsonEditor] No local player found");
    }

    #endregion
}
