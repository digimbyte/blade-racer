using System;
using System.Collections.Generic;
using UnityEngine;

namespace BladeRacer.Track
{
    [Serializable]
    public struct SplinePoint
    {
        public Vector3 position;
        public Vector3 up;
        [Min(1f)] public float width;

        public SplinePoint(Vector3 position, Vector3 up, float width = 12f)
        {
            this.position = position;
            this.up = up.sqrMagnitude > 0.001f ? up.normalized : Vector3.up;
            this.width = width;
        }
    }

    /// <summary>A dependency-free Catmull-Rom centreline stored in tile-local space.</summary>
    public sealed class TrackSpline : MonoBehaviour
    {
        [SerializeField] private bool closed;
        [SerializeField] private List<SplinePoint> points = new()
        {
            new SplinePoint(Vector3.zero, Vector3.up),
            new SplinePoint(Vector3.forward * 25f, Vector3.up),
            new SplinePoint(Vector3.forward * 50f, Vector3.up)
        };

        public IReadOnlyList<SplinePoint> Points => points;
        public bool Closed => closed;
        public int SegmentCount => closed ? points.Count : Mathf.Max(0, points.Count - 1);

        public void SetPoints(IEnumerable<SplinePoint> value)
        {
            points.Clear();
            points.AddRange(value);
        }

        public void Evaluate(float normalizedDistance, out Vector3 position, out Vector3 tangent, out Vector3 up, out float width)
        {
            if (points.Count == 0)
            {
                position = transform.position; tangent = transform.forward; up = transform.up; width = 1f; return;
            }
            if (points.Count == 1)
            {
                var only = points[0]; position = transform.TransformPoint(only.position); tangent = transform.forward;
                up = transform.TransformDirection(only.up).normalized; width = only.width; return;
            }

            float scaled = Mathf.Clamp01(normalizedDistance) * SegmentCount;
            int segment = Mathf.Min(Mathf.FloorToInt(scaled), SegmentCount - 1);
            float t = Mathf.Clamp01(scaled - segment);
            int i1 = Index(segment);
            int i2 = Index(segment + 1);
            Vector3 p0 = points[Index(segment - 1)].position;
            Vector3 p1 = points[i1].position;
            Vector3 p2 = points[i2].position;
            Vector3 p3 = points[Index(segment + 2)].position;
            Vector3 localPosition = CatmullRom(p0, p1, p2, p3, t);
            Vector3 localTangent = CatmullRomTangent(p0, p1, p2, p3, t).normalized;
            Vector3 localUp = Vector3.Slerp(points[i1].up, points[i2].up, t).normalized;
            position = transform.TransformPoint(localPosition);
            tangent = transform.TransformDirection(localTangent).normalized;
            up = transform.TransformDirection(localUp).normalized;
            width = Mathf.Lerp(points[i1].width, points[i2].width, t);
        }

        public Pose EvaluatePose(float t)
        {
            Evaluate(t, out var position, out var tangent, out var up, out _);
            return new Pose(position, Quaternion.LookRotation(tangent, up));
        }

        /// <summary>Approximate closest point query suitable for vehicle assist and recovery.</summary>
        public float FindNearestProgress(Vector3 worldPosition, int samples = 48)
        {
            float bestProgress = 0f;
            float bestDistance = float.PositiveInfinity;
            int count = Mathf.Max(4, samples);
            for (int i = 0; i <= count; i++)
            {
                float progress = i / (float)count;
                float distance = (EvaluatePose(progress).position - worldPosition).sqrMagnitude;
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                bestProgress = progress;
            }
            return bestProgress;
        }

        private int Index(int index) => closed
            ? (index % points.Count + points.Count) % points.Count
            : Mathf.Clamp(index, 0, points.Count - 1);

        private static Vector3 CatmullRom(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * ((2f * b) + (-a + c) * t + (2f * a - 5f * b + 4f * c - d) * t2 + (-a + 3f * b - 3f * c + d) * t3);
        }

        private static Vector3 CatmullRomTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            float t2 = t * t;
            return 0.5f * ((-a + c) + 2f * (2f * a - 5f * b + 4f * c - d) * t + 3f * (-a + 3f * b - 3f * c + d) * t2);
        }

        private void OnDrawGizmosSelected()
        {
            if (SegmentCount == 0) return;
            Gizmos.color = Color.cyan;
            Vector3 previous = EvaluatePose(0f).position;
            for (int i = 1; i <= SegmentCount * 12; i++)
            {
                Vector3 next = EvaluatePose(i / (float)(SegmentCount * 12)).position;
                Gizmos.DrawLine(previous, next); previous = next;
            }
        }
    }
}
