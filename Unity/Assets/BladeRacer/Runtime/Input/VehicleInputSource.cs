using UnityEngine;
using UnityEngine.InputSystem;

namespace BladeRacer.Input
{
    /// <summary>Combines desktop/gamepad input with values supplied by an on-screen UI.</summary>
    public sealed class VehicleInputSource : MonoBehaviour
    {
        [SerializeField] private bool readLocalDevices = true;
        [SerializeField, Range(0f, .5f)] private float gamepadDeadzone = .12f;

        private float touchSteer;
        private float touchThrottle;
        private bool touchBoost;
        private bool resetQueued;

        public float Steering { get; private set; }
        public float Throttle { get; private set; }
        public bool Boost { get; private set; }

        private void Update()
        {
            float steer = touchSteer;
            float throttle = touchThrottle;
            bool boost = touchBoost;

            if (readLocalDevices)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    steer += Digital(keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed,
                        keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed);
                    throttle += Digital(keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed,
                        keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed);
                    boost |= keyboard.spaceKey.isPressed;
                    resetQueued |= keyboard.rKey.wasPressedThisFrame;
                }

                var gamepad = Gamepad.current;
                if (gamepad != null)
                {
                    Vector2 stick = gamepad.leftStick.ReadValue();
                    if (Mathf.Abs(stick.x) >= gamepadDeadzone) steer += stick.x;
                    float triggers = gamepad.rightTrigger.ReadValue() - gamepad.leftTrigger.ReadValue();
                    throttle += Mathf.Abs(triggers) >= gamepadDeadzone ? triggers : stick.y;
                    boost |= gamepad.buttonSouth.isPressed || gamepad.rightShoulder.isPressed;
                    resetQueued |= gamepad.selectButton.wasPressedThisFrame;
                }
            }

            Steering = Mathf.Clamp(steer, -1f, 1f);
            Throttle = Mathf.Clamp(throttle, -1f, 1f);
            Boost = boost;
        }

        public void SetTouchSteering(float value) => touchSteer = Mathf.Clamp(value, -1f, 1f);
        public void SetTouchThrottle(float value) => touchThrottle = Mathf.Clamp(value, -1f, 1f);
        public void SetTouchBoost(bool value) => touchBoost = value;
        public void QueueReset() => resetQueued = true;

        public bool ConsumeReset()
        {
            bool value = resetQueued;
            resetQueued = false;
            return value;
        }

        private static float Digital(bool positive, bool negative) => (positive ? 1f : 0f) - (negative ? 1f : 0f);
    }
}
