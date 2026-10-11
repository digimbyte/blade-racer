using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace BladeRacer.Editor
{
    public static class PlatformSetup
    {
        [MenuItem("Blade Racer/Apply Recommended Platform Settings")]
        public static void Apply()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Android, ApiCompatibilityLevel.NET_Unity_4_8);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.iOS, ApiCompatibilityLevel.NET_Unity_4_8);
            AssetDatabase.SaveAssets();
            Debug.Log("Blade Racer platform defaults applied: landscape mobile, Android API 26+, iOS 15+, IL2CPP.");
        }
    }
}
