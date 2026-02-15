using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;

public static class MultiplayerBuilder
{
    private static string BuildPath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds"));
    private static string ServerExe => Path.Combine(BuildPath, "Server", "Server.exe");
    private static string ClientExe => Path.Combine(BuildPath, "Client", ClientFileName);

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

    [MenuItem("Build/Server")]
    public static void BuildServer()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
        Build(ServerExe);
    }

    [MenuItem("Build/Client")]
    public static void BuildClient()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, ClientBuildTarget);
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
        Build(ClientExe);
    }

    [MenuItem("Build/Run 2 Clients")]
    public static void Run2() => Run(2);

    [MenuItem("Build/Run 3 Clients")]
    public static void Run3() => Run(3);

    [MenuItem("Build/Run 4 Clients")]
    public static void Run4() => Run(4);

    private static void Run(int count)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, ClientBuildTarget);
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;

        if (File.Exists(ServerExe)) Process.Start(ServerExe);
        for (int i = 0; i < count; i++)
            if (File.Exists(ClientExe)) Process.Start(ClientExe);
    }

    private static void Build(string exePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(exePath));

        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = exePath,
            target = EditorUserBuildSettings.activeBuildTarget,
            subtarget = (int)EditorUserBuildSettings.standaloneBuildSubtarget,
            options = BuildOptions.Development
        });

        Debug.Log(result.summary.result == BuildResult.Succeeded ? $"Build OK: {exePath}" : $"Build Failed");
    }
}
