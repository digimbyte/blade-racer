using System.Collections.Generic;
using UnityEngine;

namespace BladeRacer.Track
{
    public enum TrackShape { Straight, Curve, Climb, BankedTurn, Chicane, Twist, Loop }

    /// <summary>Parametric source for a reusable tile spline. Mesh generation is handled separately.</summary>
    [ExecuteAlways, RequireComponent(typeof(TrackTile), typeof(TrackSpline))]
    public sealed class ProceduralTrackPiece : MonoBehaviour
    {
        [SerializeField] private TrackShape shape;
        [SerializeField, Min(5f)] private float length = 50f;
        [SerializeField, Min(4f)] private float width = 12f;
        [SerializeField, Min(4f)] private float radius = 25f;
        [SerializeField, Range(-180f, 180f)] private float turnAngle = 90f;
        [SerializeField, Range(-80f, 80f)] private float bankAngle = 35f;
        [SerializeField] private float elevation = 20f;
        [SerializeField, Range(-2, 2)] private int twistTurns = 1;
        [SerializeField, Range(4, 64)] private int controlPoints = 16;
        [SerializeField] private bool rebuildOnValidate = true;

        public TrackShape Shape => shape;

        public void Configure(TrackShape value)
        {
            shape = value;
            switch (shape)
            {
                case TrackShape.Straight: length = 50f; controlPoints = 6; break;
                case TrackShape.Curve: radius = 30f; turnAngle = 90f; bankAngle = 0f; controlPoints = 16; break;
                case TrackShape.Climb: length = 60f; elevation = 25f; controlPoints = 12; break;
                case TrackShape.BankedTurn: radius = 35f; turnAngle = 90f; bankAngle = 45f; controlPoints = 20; break;
                case TrackShape.Chicane: length = 70f; radius = 12f; controlPoints = 20; break;
                case TrackShape.Twist: length = 65f; twistTurns = 1; controlPoints = 24; break;
                case TrackShape.Loop: length = 55f; radius = 18f; controlPoints = 40; break;
            }
            RebuildSpline();
        }

        [ContextMenu("Rebuild Spline")]
        public void RebuildSpline()
        {
            var spline = GetComponent<TrackSpline>();
            var points = new List<SplinePoint>(controlPoints + 1);
            int count = Mathf.Max(4, controlPoints);
            for (int i = 0; i <= count; i++)
            {
                float t = i / (float)count;
                EvaluateLocal(t, out var position, out var forward, out var up);
                points.Add(new SplinePoint(position, up, width));
            }
            spline.SetPoints(points);
            UpdateSockets(spline);
            GetComponent<TrackSurface>()?.Rebuild();
        }

        public void EvaluateLocal(float t, out Vector3 position, out Vector3 forward, out Vector3 up)
        {
            float epsilon = .001f;
            position = Position(Mathf.Clamp01(t));
            forward = (Position(Mathf.Clamp01(t + epsilon)) - Position(Mathf.Clamp01(t - epsilon))).normalized;
            if (forward.sqrMagnitude < .01f) forward = Vector3.forward;
            float roll = shape switch
            {
                TrackShape.BankedTurn or TrackShape.Curve => bankAngle * Mathf.Sin(Mathf.PI * t) * Mathf.Sign(turnAngle),
                TrackShape.Twist => 360f * twistTurns * t,
                _ => 0f
            };
            Vector3 referenceUp = shape == TrackShape.Loop
                ? Quaternion.AngleAxis(360f * t, Vector3.right) * Vector3.up
                : Vector3.up;
            up = Quaternion.AngleAxis(-roll, forward) * referenceUp;
        }

        private Vector3 Position(float t)
        {
            switch (shape)
            {
                case TrackShape.Curve:
                case TrackShape.BankedTurn:
                    float radians = turnAngle * Mathf.Deg2Rad * t;
                    float sign = Mathf.Sign(turnAngle);
                    return new Vector3(sign * radius * (1f - Mathf.Cos(radians)), 0f, sign * radius * Mathf.Sin(radians));
                case TrackShape.Climb:
                    return new Vector3(0f, elevation * Smooth(t), length * t);
                case TrackShape.Chicane:
                    return new Vector3(radius * Mathf.Sin(t * Mathf.PI * 2f), 0f, length * t);
                case TrackShape.Loop:
                    float loop = t * Mathf.PI * 2f;
                    return new Vector3(0f, radius * (1f - Mathf.Cos(loop)), length * t + radius * Mathf.Sin(loop));
                default:
                    return Vector3.forward * length * t;
            }
        }

        private void UpdateSockets(TrackSpline spline)
        {
            var tile = GetComponent<TrackTile>();
            if (!tile.Entry || !tile.Exit) return;
            tile.Entry.Configure(SocketDirection.Entry, tile.Entry.Profile, width);
            tile.Exit.Configure(SocketDirection.Exit, tile.Exit.Profile, width);
            SetSocket(tile.Entry.transform, spline.EvaluatePose(0f));
            SetSocket(tile.Exit.transform, spline.EvaluatePose(1f));
            Bounds bounds = CalculateBounds(spline);
            tile.ConfigureMetadata(shape.ToString().ToLowerInvariant(), bounds);
        }

        private void SetSocket(Transform socket, Pose worldPose)
        {
            socket.localPosition = transform.InverseTransformPoint(worldPose.position);
            socket.localRotation = Quaternion.Inverse(transform.rotation) * worldPose.rotation;
        }

        private Bounds CalculateBounds(TrackSpline spline)
        {
            var bounds = new Bounds(transform.InverseTransformPoint(spline.EvaluatePose(0f).position), Vector3.one);
            for (int i = 1; i <= controlPoints; i++) bounds.Encapsulate(transform.InverseTransformPoint(spline.EvaluatePose(i / (float)controlPoints).position));
            bounds.Expand(new Vector3(width, 4f, width));
            return bounds;
        }

        private static float Smooth(float t) => t * t * (3f - 2f * t);
        private void OnValidate() { if (rebuildOnValidate && isActiveAndEnabled) RebuildSpline(); }
    }
}
