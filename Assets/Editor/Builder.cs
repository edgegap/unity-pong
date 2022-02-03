using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;


public class BuildConfiguration
{
    private static string _scenesDirectory = "Assets/Scenes";
    private static string _buildClientOutputDirectory = "Build/Client";
    private static string _buildServerOutputDirectory = "Build/Server";
    public string ScenesDirectory { get => _scenesDirectory; }
    public string ClientBuildOutput { get => _buildClientOutputDirectory; }
    public string ServerBuildOutput { get => _buildServerOutputDirectory; }


}


public class Builder
{
    private void Build(BuildPlayerOptions options)
    {

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }

    public void BuildClient(BuildConfiguration configuration, BuildTarget target, BuildOptions options)
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { 
            $"{configuration.ScenesDirectory}/Menu.unity",
            $"{configuration.ScenesDirectory}/Arena.unity",
        };
        buildPlayerOptions.locationPathName = $"{configuration.ClientBuildOutput}/{target}/edgepong.exe";
        buildPlayerOptions.target = target;
        buildPlayerOptions.options = options;

        Build(buildPlayerOptions);
    }

    public void BuildServer(BuildConfiguration configuration, BuildTarget target, BuildOptions options)
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] {
            $"{configuration.ScenesDirectory}/Server.unity",
        };
        buildPlayerOptions.locationPathName = $"{configuration.ServerBuildOutput}/{target}/edgepong_sever.exe";
        buildPlayerOptions.target = target;
        buildPlayerOptions.options = options | BuildOptions.EnableHeadlessMode;

        Build(buildPlayerOptions);
    }
}
