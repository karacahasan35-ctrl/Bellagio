using UnityEditor;
using System.IO;
using UnityEngine;

public class BuildScript
{
    public static void BuildWindows()
    {
        string[] scenes = { "Assets/Scenes/SampleScene.unity" };
        string buildPath = "Builds/Windows/Bellagio.exe";
        
        // Ensure build directory exists
        string directory = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Debug.Log("Starting Windows Build...");
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else if (summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            Debug.LogError("Build failed!");
        }
    }
}
