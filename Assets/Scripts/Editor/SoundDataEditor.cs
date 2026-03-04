using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundData))]
public class SoundDataEditor : Editor
{
    private const string AudioFolderPath = "Assets/Audios";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Refresh from Audios Folder", GUILayout.Height(30)))
        {
            RefreshSoundClips();
        }

        EditorGUILayout.HelpBox(
            $"Audio 폴더 경로: {AudioFolderPath}\n" +
            "버튼을 클릭하면 해당 폴더의 모든 AudioClip을 자동으로 등록합니다.\n" +
            "(하위 폴더 포함)",
            MessageType.Info);
    }

    private void RefreshSoundClips()
    {
        var soundData = (SoundData)target;

        if (!AssetDatabase.IsValidFolder(AudioFolderPath))
        {
            Debug.LogError($"[SoundData] Folder not found: {AudioFolderPath}");
            return;
        }

        var clips = FindAllAudioClips(AudioFolderPath);

        soundData.SetClips(clips);
        EditorUtility.SetDirty(soundData);
        AssetDatabase.SaveAssets();

        Debug.Log($"[SoundData] Registered {clips.Count} audio clips.");
    }

    private static List<AudioClip> FindAllAudioClips(string folderPath)
    {
        var clips = new List<AudioClip>();
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

            if (clip != null)
            {
                clips.Add(clip);
            }
        }

        return clips;
    }
}

public class SoundDataAutoRefresh : AssetPostprocessor
{
    private const string AudioFolderPath = "Assets/Audios";
    private const string DataPath = "Assets/Data/SoundData.asset";

    private static readonly HashSet<string> AudioExtensions = new()
    {
        ".wav", ".mp3", ".ogg", ".aiff", ".aif"
    };

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        bool shouldRefresh = false;

        foreach (string path in importedAssets)
        {
            if (IsAudioFile(path))
            {
                shouldRefresh = true;
                break;
            }
        }

        if (!shouldRefresh)
        {
            foreach (string path in deletedAssets)
            {
                if (IsAudioFile(path))
                {
                    shouldRefresh = true;
                    break;
                }
            }
        }

        if (shouldRefresh)
        {
            EditorApplication.delayCall += RefreshSoundData;
        }
    }

    private static bool IsAudioFile(string path)
    {
        if (!path.StartsWith(AudioFolderPath))
            return false;

        string extension = Path.GetExtension(path).ToLowerInvariant();
        return AudioExtensions.Contains(extension);
    }

    private static void RefreshSoundData()
    {
        var soundData = AssetDatabase.LoadAssetAtPath<SoundData>(DataPath);
        if (soundData == null)
        {
            soundData = ScriptableObject.CreateInstance<SoundData>();

            string directory = Path.GetDirectoryName(DataPath);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            AssetDatabase.CreateAsset(soundData, DataPath);
        }

        var clips = new List<AudioClip>();
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { AudioFolderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

            if (clip != null)
            {
                clips.Add(clip);
            }
        }

        soundData.SetClips(clips);
        EditorUtility.SetDirty(soundData);
        AssetDatabase.SaveAssets();

        Debug.Log($"[SoundData] Auto-refreshed: {clips.Count} audio clips registered.");
    }
}
