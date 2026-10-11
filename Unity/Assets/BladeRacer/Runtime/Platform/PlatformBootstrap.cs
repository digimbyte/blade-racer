using UnityEngine;

namespace BladeRacer.Platform
{
    /// <summary>Applies conservative defaults and exposes mobile-only presentation roots.</summary>
    public sealed class PlatformBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject mobileControlsRoot;
        [SerializeField] private bool forceMobileControlsInEditor;
        [SerializeField] private int desktopTargetFrameRate = 120;
        [SerializeField] private int mobileTargetFrameRate = 60;
        [SerializeField] private bool preventMobileSleep = true;

        private void Awake()
        {
            bool mobile = Application.isMobilePlatform || forceMobileControlsInEditor;
            if (mobileControlsRoot) mobileControlsRoot.SetActive(mobile);
            QualitySettings.vSyncCount = mobile ? 0 : 1;
            Application.targetFrameRate = mobile ? mobileTargetFrameRate : desktopTargetFrameRate;
            if (mobile && preventMobileSleep) Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
