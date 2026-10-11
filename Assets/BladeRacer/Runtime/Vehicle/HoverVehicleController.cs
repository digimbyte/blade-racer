using BladeRacer.Input;
using UnityEngine;

namespace BladeRacer.Vehicle
{
    /// <summary>Deterministic arcade motor driven by raycast track frames, not forces.</summary>
    [RequireComponent(typeof(Rigidbody), typeof(VehicleInputSource), typeof(HoverMagnetController))]
    public sealed class HoverVehicleController : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private float topSpeed = 95f;
        [SerializeField] private float reverseSpeed = 20f;
        [SerializeField] private float acceleration = 42f;
        [SerializeField] private float braking = 65f;
        [SerializeField] private float coastDeceleration = 14f;
        [SerializeField] private float boostSpeed = 30f;

        [Header("Handling")]
        [SerializeField] private float steerDegreesPerSecond = 105f;
        [SerializeField, Range(0f, 1f)] private float highSpeedSteering = .45f;
        [SerializeField, Range(0f, 1f)] private float drift = .18f;
        [SerializeField, Min(0f)] private float collisionSkin = .05f;

        [Header("Hover / Assist")]
        [SerializeField] private float hoverHeight = 2f;
        [SerializeField] private float magnetSnapSpeed = 14f;
        [SerializeField] private float surfaceAlignSpeed = 9f;
        [SerializeField, Range(0f, 1f)] private float trackFollowBias = .25f;
        [SerializeField] private float centeringSpeed = 6f;
        [SerializeField] private float headingAssistSpeed = 3f;
        [SerializeField] private float airborneGravity = 24f;

        private Rigidbody body;
        private VehicleInputSource input;
        private HoverMagnetController magnet;
        private float speed;
        private float verticalSpeed;
        private Vector3 planarVelocity;

        public float Speed => speed;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            input = GetComponent<VehicleInputSource>();
            magnet = GetComponent<HoverMagnetController>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            UpdateSpeed(dt);
            bool grounded = magnet.Sample(out var frame);
            Vector3 up = grounded ? frame.normal : transform.up;

            float speedRatio = Mathf.Clamp01(Mathf.Abs(speed) / Mathf.Max(1f, topSpeed));
            float steeringScale = Mathf.Lerp(1f, highSpeedSteering, speedRatio);
            Quaternion steered = Quaternion.AngleAxis(input.Steering * steerDegreesPerSecond * steeringScale * dt, up) * body.rotation;
            Vector3 desiredForward = Vector3.ProjectOnPlane(steered * Vector3.forward, up).normalized;

            if (grounded && trackFollowBias > 0f)
            {
                float headingBlend = 1f - Mathf.Exp(-headingAssistSpeed * trackFollowBias * dt);
                desiredForward = Vector3.Slerp(desiredForward, frame.trackForward, headingBlend).normalized;
            }

            Quaternion targetRotation = Quaternion.LookRotation(desiredForward, up);
            float alignBlend = 1f - Mathf.Exp(-surfaceAlignSpeed * dt);
            Quaternion nextRotation = Quaternion.Slerp(body.rotation, targetRotation, alignBlend);

            Vector3 desiredPlanar = desiredForward * speed;
            planarVelocity = Vector3.Lerp(desiredPlanar, planarVelocity, drift);
            Vector3 nextPosition = body.position + planarVelocity * dt;

            if (grounded)
            {
                verticalSpeed = 0f;
                Vector3 hoverTarget = frame.point + frame.normal * hoverHeight;
                float magnetBlend = 1f - Mathf.Exp(-magnetSnapSpeed * frame.contactFraction * dt);
                nextPosition = Vector3.Lerp(nextPosition, Vector3.ProjectOnPlane(nextPosition - hoverTarget, frame.normal) + hoverTarget, magnetBlend);
                Vector3 centerOffset = Vector3.ProjectOnPlane(frame.trackCenter - nextPosition, frame.normal);
                nextPosition += Vector3.ClampMagnitude(centerOffset, centeringSpeed * trackFollowBias * dt);
            }
            else
            {
                verticalSpeed -= airborneGravity * dt;
                nextPosition += Vector3.up * verticalSpeed * dt;
            }

            Vector3 displacement = nextPosition - body.position;
            if (displacement.sqrMagnitude > .000001f && body.SweepTest(displacement.normalized, out var hit, displacement.magnitude + collisionSkin, QueryTriggerInteraction.Ignore))
                displacement = Vector3.ProjectOnPlane(displacement, hit.normal) * .5f;
            body.MovePosition(body.position + displacement);
            body.MoveRotation(nextRotation);

            if (input.ConsumeReset()) ResetVehicle();
        }

        private void UpdateSpeed(float dt)
        {
            float throttle = input.Throttle;
            float target = throttle >= 0f ? throttle * (topSpeed + (input.Boost ? boostSpeed : 0f)) : throttle * reverseSpeed;
            float rate = Mathf.Abs(target) < Mathf.Abs(speed) ? braking : acceleration;
            if (Mathf.Approximately(throttle, 0f)) rate = coastDeceleration;
            speed = Mathf.MoveTowards(speed, target, rate * dt);
        }

        private void ResetVehicle()
        {
            speed = 0f; verticalSpeed = 0f; planarVelocity = Vector3.zero;
            body.position += Vector3.up * 3f;
            body.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }
    }
}
