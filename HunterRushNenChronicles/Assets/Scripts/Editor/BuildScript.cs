using UnityEngine;
using UnityEditor;
using System.IO;

namespace HunterRush.Editor
{
    /// <summary>
    /// Automated build script for Hunter Rush: Nen Chronicles
    /// Handles builds for multiple platforms with optimization
    /// </summary>
    public class BuildScript
    {
        private static readonly string[] scenes = {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/CharacterSelect.unity",
            "Assets/Scenes/Gameplay.unity",
            "Assets/Scenes/EndlessRun.unity",
            "Assets/Scenes/StoryMode.unity",
            "Assets/Scenes/BossRush.unity"
        };
        
        [MenuItem("Hunter Rush/Build All Platforms")]
        public static void BuildAllPlatforms()
        {
            BuildWindows();
            BuildMac();
            BuildAndroid();
            BuildiOS();
        }
        
        [MenuItem("Hunter Rush/Build Windows")]
        public static void BuildWindows()
        {
            string buildPath = "Builds/Windows/HunterRush.exe";
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions();
            buildOptions.scenes = scenes;
            buildOptions.locationPathName = buildPath;
            buildOptions.target = BuildTarget.StandaloneWindows64;
            buildOptions.options = BuildOptions.None;
            
            Debug.Log("Building Windows version...");
            BuildPipeline.BuildPlayer(buildOptions);
            Debug.Log("Windows build completed!");
        }
        
        [MenuItem("Hunter Rush/Build Mac")]
        public static void BuildMac()
        {
            string buildPath = "Builds/Mac/HunterRush.app";
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions();
            buildOptions.scenes = scenes;
            buildOptions.locationPathName = buildPath;
            buildOptions.target = BuildTarget.StandaloneOSX;
            buildOptions.options = BuildOptions.None;
            
            Debug.Log("Building Mac version...");
            BuildPipeline.BuildPlayer(buildOptions);
            Debug.Log("Mac build completed!");
        }
        
        [MenuItem("Hunter Rush/Build Android")]
        public static void BuildAndroid()
        {
            string buildPath = "Builds/Android/HunterRush.apk";
            
            // Configure Android settings
            PlayerSettings.Android.bundleVersionCode++;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions();
            buildOptions.scenes = scenes;
            buildOptions.locationPathName = buildPath;
            buildOptions.target = BuildTarget.Android;
            buildOptions.options = BuildOptions.None;
            
            Debug.Log("Building Android version...");
            BuildPipeline.BuildPlayer(buildOptions);
            Debug.Log("Android build completed!");
        }
        
        [MenuItem("Hunter Rush/Build iOS")]
        public static void BuildiOS()
        {
            string buildPath = "Builds/iOS";
            
            // Configure iOS settings
            PlayerSettings.iOS.buildNumber = (int.Parse(PlayerSettings.iOS.buildNumber) + 1).ToString();
            PlayerSettings.iOS.targetOSVersionString = "11.0";
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions();
            buildOptions.scenes = scenes;
            buildOptions.locationPathName = buildPath;
            buildOptions.target = BuildTarget.iOS;
            buildOptions.options = BuildOptions.None;
            
            Debug.Log("Building iOS version...");
            BuildPipeline.BuildPlayer(buildOptions);
            Debug.Log("iOS build completed!");
        }
        
        [MenuItem("Hunter Rush/Development Build")]
        public static void BuildDevelopment()
        {
            string buildPath = "Builds/Development/HunterRush.exe";
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions();
            buildOptions.scenes = scenes;
            buildOptions.locationPathName = buildPath;
            buildOptions.target = BuildTarget.StandaloneWindows64;
            buildOptions.options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler;
            
            Debug.Log("Building development version...");
            BuildPipeline.BuildPlayer(buildOptions);
            Debug.Log("Development build completed!");
        }
        
        [MenuItem("Hunter Rush/Clean Build Folders")]
        public static void CleanBuildFolders()
        {
            string buildsPath = "Builds";
            
            if (Directory.Exists(buildsPath))
            {
                Directory.Delete(buildsPath, true);
                Debug.Log("Build folders cleaned!");
            }
            
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Hunter Rush/Setup Build Folders")]
        public static void SetupBuildFolders()
        {
            string[] folders = {
                "Builds",
                "Builds/Windows",
                "Builds/Mac",
                "Builds/Android",
                "Builds/iOS",
                "Builds/Development"
            };
            
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            
            Debug.Log("Build folders created!");
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Hunter Rush/Optimize for Mobile")]
        public static void OptimizeForMobile()
        {
            // Graphics settings
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.gpuSkinning = true;
            PlayerSettings.graphicsJobs = true;
            
            // Android specific
            PlayerSettings.Android.blitType = AndroidBlitType.Always;
            PlayerSettings.Android.startInFullscreen = true;
            
            // iOS specific
            PlayerSettings.iOS.targetOSVersionString = "11.0";
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            
            // Quality settings
            QualitySettings.vSyncCount = 0; // Disable VSync for mobile
            QualitySettings.antiAliasing = 0; // Disable MSAA for performance
            
            Debug.Log("Mobile optimization applied!");
        }
        
        [MenuItem("Hunter Rush/Package for Distribution")]
        public static void PackageForDistribution()
        {
            // Create distribution package
            string packagePath = $"Distribution/HunterRush_v{PlayerSettings.bundleVersion}.unitypackage";
            
            string[] assetPaths = {
                "Assets/Scripts",
                "Assets/Prefabs",
                "Assets/Materials",
                "Assets/Shaders"
            };
            
            AssetDatabase.ExportPackage(assetPaths, packagePath, ExportPackageOptions.Recurse);
            Debug.Log($"Distribution package created: {packagePath}");
        }
    }
}