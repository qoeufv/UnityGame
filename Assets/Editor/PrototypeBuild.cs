using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace StrategyRPG.Editor
{
    public static class PrototypeBuild
    {
        private static readonly string[] PlayableScenes =
        {
            "Assets/Scenes/SpaceFlightPrototype.unity",
            "Assets/Scenes/Planet_AzureWorld.unity",
            "Assets/Scenes/Planet_PaleMoon.unity"
        };

        [MenuItem("Tools/Build Game Frontier/macOS Development")]
        public static void BuildMacDevelopment()
        {
            Build("Builds/macOS/Game Frontier.app", BuildTarget.StandaloneOSX);
        }

        [MenuItem("Tools/Build Game Frontier/Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            Build("Builds/Windows/GameFrontier.exe", BuildTarget.StandaloneWindows64);
        }

        private static void Build(string outputPath, BuildTarget target)
        {
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            var options = new BuildPlayerOptions
            {
                scenes = PlayableScenes,
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Game Frontier build failed: {report.summary.result}");

            Debug.Log($"Game Frontier build completed: {outputPath} ({report.summary.totalSize / (1024f * 1024f):0.0} MB)");
        }
    }
}
