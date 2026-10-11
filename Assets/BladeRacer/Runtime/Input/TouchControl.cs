using UnityEngine;
using UnityEngine.EventSystems;

namespace BladeRacer.Input
{
    public enum TouchControlKind { SteeringPad, Throttle, Brake, Boost, Reset }

    /// <summary>Attach to UI Images to drive a VehicleInputSource without scene-specific glue.</summary>
    public sealed class TouchControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private VehicleInputSource input;
        [SerializeField] private TouchControlKind kind;
        [SerializeField, Min(10f)] private float steeringRadius = 100f;
        private RectTransform rectTransform;

        public void Configure(VehicleInputSource source, TouchControlKind controlKind)
        { input = source; kind = controlKind; }

        private void Awake() => rectTransform = (RectTransform)transform;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!input) return;
            if (kind == TouchControlKind.Reset) input.QueueReset();
            else Apply(true, eventData);
        }

        public void OnDrag(PointerEventData eventData) => Apply(true, eventData);
        public void OnPointerUp(PointerEventData eventData) => Apply(false, eventData);

        private void Apply(bool pressed, PointerEventData eventData)
        {
            if (!input) return;
            switch (kind)
            {
                case TouchControlKind.SteeringPad:
                    if (!pressed) input.SetTouchSteering(0f);
                    else if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var local))
                        input.SetTouchSteering(Mathf.Clamp(local.x / steeringRadius, -1f, 1f));
                    break;
                case TouchControlKind.Throttle: input.SetTouchThrottle(pressed ? 1f : 0f); break;
                case TouchControlKind.Brake: input.SetTouchThrottle(pressed ? -1f : 0f); break;
                case TouchControlKind.Boost: input.SetTouchBoost(pressed); break;
            }
        }
    }
}
