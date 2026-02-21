using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;

public static class MultiplayerBuilder
{
    private static string BuildPath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds"));
    private static string ServerExe => Path.Combine(BuildPath, "Server", "Server.exe");
    private static string ClientExe => Path.Combine(BuildPath, "Client", ClientFileName);

    private const string EmptyShaderVariantPath = "Assets/Settings/EmptyShaderVariants.shadervariants";

    private static string ClientFileName
    {
        get
        {
#if UNITY_EDITOR_WIN
            return "Client.exe";
#elif UNITY_EDITOR_OSX
            return "Client.app";
#else
            return "Client";
#endif
        }
    }

    private static BuildTarget ClientBuildTarget
    {
        get
        {
#if UNITY_EDITOR_WIN
            return BuildTarget.StandaloneWindows64;
#elif UNITY_EDITOR_OSX
            return BuildTarget.StandaloneOSX;
#else
            return BuildTarget.StandaloneLinux64;
#endif
        }
    }

    private static void SwitchToServerPlatform()
    {
       
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        }
       
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
    }

    private static void SwitchToClientPlatform()
    {
        
        if (EditorUserBuildSettings.activeBuildTarget != ClientBuildTarget)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, ClientBuildTarget);
        }
        
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
    }

    /// <summary>
    /// 서버 빌드용 빈 ShaderVariantCollection 생성 및 적용
    /// </summary>
    private static ShaderVariantCollection[] ApplyEmptyShaderVariants()
    {
        
        var graphicsSettings = GraphicsSettings.GetGraphicsSettings();
        var serializedObject = new SerializedObject(graphicsSettings);
        var originalShaders = serializedObject.FindProperty("m_PreloadedShaders");

        // 기존 설정 백업
        var backupList = new ShaderVariantCollection[originalShaders.arraySize];
        for (int i = 0; i < originalShaders.arraySize; i++)
        {
            backupList[i] = originalShaders.GetArrayElementAtIndex(i).objectReferenceValue as ShaderVariantCollection;
        }

        // 빈 ShaderVariantCollection 생성
        var emptyCollection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(EmptyShaderVariantPath);
        if (emptyCollection == null)
        {
            // Settings 폴더 생성
            var settingsDir = Path.GetDirectoryName(EmptyShaderVariantPath);
            if (!AssetDatabase.IsValidFolder(settingsDir))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }

            emptyCollection = new ShaderVariantCollection();
            AssetDatabase.CreateAsset(emptyCollection, EmptyShaderVariantPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created empty ShaderVariantCollection: {EmptyShaderVariantPath}");
        }

        // 빈 컬렉션만 적용
        originalShaders.ClearArray();
        originalShaders.arraySize = 1;
        originalShaders.GetArrayElementAtIndex(0).objectReferenceValue = emptyCollection;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        return backupList;
    }

    /// <summary>
    /// 원래 ShaderVariantCollection 설정 복원
    /// </summary>
    private static void RestoreShaderVariants(ShaderVariantCollection[] backupList)
    {
        var graphicsSettings = GraphicsSettings.GetGraphicsSettings();
        var serializedObject = new SerializedObject(graphicsSettings);
        var shadersProp = serializedObject.FindProperty("m_PreloadedShaders");

        shadersProp.ClearArray();
        shadersProp.arraySize = backupList.Length;
        for (int i = 0; i < backupList.Length; i++)
        {
            shadersProp.GetArrayElementAtIndex(i).objectReferenceValue = backupList[i];
        }
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    [MenuItem("Build/Server (Full)")]
    public static void BuildServer()
    {
        SwitchToServerPlatform();

        // 서버 빌드 시 빈 셰이더 적용
        var backup = ApplyEmptyShaderVariants();
        try
        {
            Build(ServerExe, BuildOptions.Development | BuildOptions.CompressWithLz4);
        }
        finally
        {
            // 원래 설정 복원
            RestoreShaderVariants(backup);
        }

        SwitchToClientPlatform();

        // 플랫폼 변경 완료 후 서버 실행
        if (File.Exists(ServerExe))
            Process.Start(ServerExe);
    }

    [MenuItem("Build/Server (Scripts Only)")]
    public static void BuildServerScriptsOnly()
    {
        SwitchToServerPlatform();

        // 스크립트만 빌드
        Build(ServerExe, BuildOptions.Development | BuildOptions.CompressWithLz4 | BuildOptions.BuildScriptsOnly);

        // 빌드 후 먼저 platform 변경
        SwitchToClientPlatform();

        // 서버 실행
        if (File.Exists(ServerExe))
            Process.Start(ServerExe);
    }

    [MenuItem("Build/Client")]
    public static void BuildClient()
    {
        SwitchToClientPlatform();
        Build(ClientExe, BuildOptions.Development | BuildOptions.CompressWithLz4);
    }

    [MenuItem("Build/Run 2 Clients")]
    public static void Run2() => Run(2);

    [MenuItem("Build/Run 3 Clients")]
    public static void Run3() => Run(3);

    [MenuItem("Build/Run 4 Clients")]
    public static void Run4() => Run(4);

    private static void Run(int count)
    {
        SwitchToClientPlatform();

        if (File.Exists(ServerExe))
            Process.Start(ServerExe);

        for (int i = 0; i < count; i++)
        {
            if (File.Exists(ClientExe))
                Process.Start(ClientExe);
        }
    }

    private static void Build(string exePath, BuildOptions options)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(exePath));

        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = exePath,
            target = EditorUserBuildSettings.activeBuildTarget,
            subtarget = (int)EditorUserBuildSettings.standaloneBuildSubtarget,
            options = options
        });

        Debug.Log(result.summary.result == BuildResult.Succeeded ? $"Build OK: {exePath}" : $"Build Failed");
    }
}
