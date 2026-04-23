using System.Collections.Generic;
using System.IO;
using Items;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ItemIconEntry의 itemId 필드에 아이템 이름을 표시하는 커스텀 PropertyDrawer
/// </summary>
[CustomPropertyDrawer(typeof(ItemIconDatabase.ItemIconEntry))]
public class ItemIconEntryDrawer : PropertyDrawer
{
    private static Dictionary<int, string> _itemIdToName;
    private static bool _initialized;

    private static void Initialize()
    {
        if (_initialized) return;

        _itemIdToName = new Dictionary<int, string>();

        // items.json 로드
        string jsonPath = Path.Combine(Application.dataPath, "Resources/Data/items.json");
        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            var itemsData = JsonUtility.FromJson<ItemsJsonData>(json);

            if (itemsData.equipment != null)
                foreach (var item in itemsData.equipment)
                    _itemIdToName[item.id] = $"{item.name} [장비]";

            if (itemsData.consumables != null)
                foreach (var item in itemsData.consumables)
                    _itemIdToName[item.id] = $"{item.name} [소비]";

            if (itemsData.materials != null)
                foreach (var item in itemsData.materials)
                    _itemIdToName[item.id] = $"{item.name} [재료]";

            if (itemsData.quests != null)
                foreach (var item in itemsData.quests)
                    _itemIdToName[item.id] = $"{item.name} [퀘스트]";
        }

        _initialized = true;
    }

    public static void InvalidateCache()
    {
        _initialized = false;
        _itemIdToName = null;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Initialize();

        EditorGUI.BeginProperty(position, label, property);

        var itemIdProp = property.FindPropertyRelative("itemId");
        var iconProp = property.FindPropertyRelative("icon");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // 첫 번째 줄: Item ID 입력 + 아이템 이름 표시
        Rect line1 = new Rect(position.x, position.y, position.width, lineHeight);

        // ID 입력 필드 (왼쪽 40%)
        float fieldWidth = position.width * 0.4f;
        Rect idRect = new Rect(line1.x, line1.y, fieldWidth, lineHeight);
        EditorGUI.PropertyField(idRect, itemIdProp, new GUIContent("Item Id"));

        // 아이템 이름 표시 (오른쪽 60%)
        Rect nameRect = new Rect(idRect.xMax + 8, line1.y, position.width - fieldWidth - 8, lineHeight);
        int currentId = itemIdProp.intValue;

        if (currentId >= 0)
        {
            if (_itemIdToName != null && _itemIdToName.TryGetValue(currentId, out string name))
            {
                var style = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = new Color(0.4f, 0.8f, 0.4f) },
                    fontStyle = FontStyle.Bold
                };
                EditorGUI.LabelField(nameRect, $"-> {name}", style);
            }
            else
            {
                var style = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = new Color(1f, 0.5f, 0.5f) }
                };
                EditorGUI.LabelField(nameRect, "-> (알 수 없는 ID)", style);
            }
        }

        // 두 번째 줄: 아이콘
        Rect line2 = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
        EditorGUI.PropertyField(line2, iconProp, new GUIContent("Icon"));

        EditorGUI.EndProperty();
    }
}

/// <summary>
/// items.json 변경 감지를 위한 AssetPostprocessor
/// </summary>
public class ItemsJsonPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (path.EndsWith("items.json"))
            {
                ItemIconEntryDrawer.InvalidateCache();
                break;
            }
        }
    }
}
