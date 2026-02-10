using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RTS.Editor
{
    public static class BuildScript
    {
        [MenuItem("RTS/Build/WebGL")]
        public static void BuildWebGL()
        {
            var scenes = new[]
            {
                "Assets/Scenes/SampleScene.unity"
            };

            var buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildOptions);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed");
            }
        }

        [MenuItem("RTS/Build/Windows")]
        public static void BuildWindows()
        {
            var scenes = new[]
            {
                "Assets/Scenes/SampleScene.unity"
            };

            var buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/Windows/RTS.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildOptions);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed");
            }
        }
    }
}
