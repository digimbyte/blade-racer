using UnityEngine;

namespace BladeRacer.Platform
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform target;
        private Rect lastSafeArea;
        private Vector2Int lastScreen;

        private void Awake() { target = GetComponent<RectTransform>(); Apply(); }
        private void Update()
        {
            if (Screen.safeArea != lastSafeArea || Screen.width != lastScreen.x || Screen.height != lastScreen.y) Apply();
        }

        private void Apply()
        {
            Rect area = Screen.safeArea;
            target.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            target.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            target.offsetMin = target.offsetMax = Vector2.zero;
            lastSafeArea = area;
            lastScreen = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
