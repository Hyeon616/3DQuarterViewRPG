using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EffectData))]
public class EffectDataEditor : Editor
{
    private const string EffectFolderPath = "Assets/Prefabs/Effect";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Refresh from Effect Folder", GUILayout.Height(30)))
        {
            RefreshEffectPrefabs();
        }

        EditorGUILayout.HelpBox(
            $"Effect 폴더 경로: {EffectFolderPath}\n" +
            "버튼을 클릭하면 해당 폴더의 모든 프리팹을 자동으로 등록합니다.",
            MessageType.Info);
    }

    private void RefreshEffectPrefabs()
    {
        var database = (EffectData)target;

        if (!AssetDatabase.IsValidFolder(EffectFolderPath))
        {
            Debug.LogError($"[EffectData] Folder not found: {EffectFolderPath}");
            return;
        }

        var prefabs = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { EffectFolderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }

        database.SetPrefabs(prefabs);
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        Debug.Log($"[EffectData] Registered {prefabs.Count} effect prefabs.");
    }
}

public class EffectDataAutoRefresh : AssetPostprocessor
{
    private const string EffectFolderPath = "Assets/Prefabs/Effect";
    private const string DatabasePath = "Assets/Data/EffectData.asset";

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        bool shouldRefresh = false;

        foreach (string path in importedAssets)
        {
            if (IsEffectPrefab(path))
            {
                shouldRefresh = true;
                break;
            }
        }

        if (!shouldRefresh)
        {
            foreach (string path in deletedAssets)
            {
                if (IsEffectPrefab(path))
                {
                    shouldRefresh = true;
                    break;
                }
            }
        }

        if (shouldRefresh)
        {
            EditorApplication.delayCall += RefreshDatabase;
        }
    }

    private static bool IsEffectPrefab(string path)
    {
        return path.StartsWith(EffectFolderPath) && path.EndsWith(".prefab");
    }

    private static void RefreshDatabase()
    {
        var database = AssetDatabase.LoadAssetAtPath<EffectData>(DatabasePath);
        if (database == null)
        {
            // Create database if it doesn't exist
            database = ScriptableObject.CreateInstance<EffectData>();

            string directory = Path.GetDirectoryName(DatabasePath);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            AssetDatabase.CreateAsset(database, DatabasePath);
        }

        var prefabs = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { EffectFolderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }

        database.SetPrefabs(prefabs);
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        Debug.Log($"[EffectData] Auto-refreshed: {prefabs.Count} effect prefabs registered.");
    }
}
