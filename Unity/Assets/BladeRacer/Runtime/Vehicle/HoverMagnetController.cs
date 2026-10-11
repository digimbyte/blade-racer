using System;
using BladeRacer.Track;
using UnityEngine;

namespace BladeRacer.Vehicle
{
    [Serializable]
    public struct HoverProbe
    {
        public Vector3 localPosition;
        public HoverProbe(Vector3 position) => localPosition = position;
    }

    public readonly struct HoverFrame
    {
        public readonly Vector3 point;
        public readonly Vector3 normal;
        public readonly Vector3 trackCenter;
        public readonly Vector3 trackForward;
        public readonly float contactFraction;

        public HoverFrame(Vector3 point, Vector3 normal, Vector3 center, Vector3 forward, float contacts)
        { this.point = point; this.normal = normal; trackCenter = center; trackForward = forward; contactFraction = contacts; }
    }

    /// <summary>Raycast-only sensor. It reports a magnetic track frame and never applies physics forces.</summary>
    public sealed class HoverMagnetController : MonoBehaviour
    {
        [SerializeField] private HoverProbe[] probes =
        {
            new(new Vector3(-.75f, -.2f, 1.3f)), new(new Vector3(.75f, -.2f, 1.3f)),
            new(new Vector3(-.75f, -.2f, -1.3f)), new(new Vector3(.75f, -.2f, -1.3f))
        };
        [SerializeField] private LayerMask trackLayers = ~0;
        [SerializeField, Min(.1f)] private float probeReach = 6f;
        [SerializeField, Range(4, 128)] private int guidanceSamples = 32;

        public bool Sample(out HoverFrame frame)
        {
            int contacts = 0;
            Vector3 pointSum = Vector3.zero;
            Vector3 normalSum = Vector3.zero;
            TrackSurface surface = null;
            foreach (var probe in probes)
            {
                Vector3 origin = transform.TransformPoint(probe.localPosition);
                if (!Physics.Raycast(origin, -transform.up, out var hit, probeReach, trackLayers, QueryTriggerInteraction.Ignore)) continue;
                if (hit.transform.IsChildOf(transform)) continue;
                contacts++; pointSum += hit.point; normalSum += hit.normal;
                if (!surface) surface = hit.collider.GetComponentInParent<TrackSurface>();
            }

            if (contacts == 0)
            {
                frame = default;
                return false;
            }

            Vector3 point = pointSum / contacts;
            Vector3 normal = normalSum.normalized;
            Vector3 center = point;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, normal).normalized;
            if (surface && surface.Spline)
            {
                var spline = surface.Spline;
                float progress = spline.FindNearestProgress(transform.position, guidanceSamples);
                spline.Evaluate(progress, out center, out forward, out _, out _);
                if (Vector3.Dot(forward, transform.forward) < 0f) forward = -forward;
            }
            frame = new HoverFrame(point, normal, center, forward, contacts / (float)probes.Length);
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            foreach (var probe in probes)
            {
                Vector3 origin = transform.TransformPoint(probe.localPosition);
                Gizmos.DrawSphere(origin, .08f);
                Gizmos.DrawLine(origin, origin - transform.up * probeReach);
            }
        }
    }
}
