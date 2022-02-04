using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildMenu : MonoBehaviour
{
    private static BuildConfiguration _buildConfiguration = new BuildConfiguration();
    private static Builder _builder = new Builder();

    // Start is called before the first frame update
    void Start()
    {

    }

    [MenuItem("Build/Client/Win64")]
    public static void ClientWin64()
    {
        _builder.BuildClient(_buildConfiguration, BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

    [MenuItem("Build/Client/OSX")]
    public static void ClientOSX()
    {
        _builder.BuildClient(_buildConfiguration, BuildTarget.StandaloneOSX, BuildOptions.None);
    }

    [MenuItem("Build/Server/Win64")]
    public static void ServerWin64()
    {
        _builder.BuildServer(_buildConfiguration, BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

    [MenuItem("Build/Server/Linux64")]
    public static void ServerLinux()
    {
        _builder.BuildServer(_buildConfiguration, BuildTarget.StandaloneLinux64, BuildOptions.None);
    }

    [MenuItem("Build/Build Image")]
    public static void ServerBuildImage()
    {
        ExecuteScript("./Scripts/buildImage.ps1");
    }

    [MenuItem("Build/Push Image")]
    public static void ServerPushImage()
    {
        ExecuteScript("./Scripts/pushImage.ps1");
    }

    private static void ExecuteScript(string ps1File)
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy unrestricted -file \"{ps1File}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true //This option means it will take anything the process outputs and put it in test.StandardOutput ie from PS's "write-Output $user"
        };
        Process build = Process.Start(startInfo); //creates a powershell process with the above start options
        build.WaitForExit(); //We need this because Start() simply launches the script it does not wait for it to finish or the below line would not have any data

        if (build.ExitCode != 0)
        {
            UnityEngine.Debug.LogError(build.StandardOutput.ReadToEnd());
        }
        else
        {
            UnityEngine.Debug.Log(build.StandardOutput.ReadToEnd());
        }
    }
}
