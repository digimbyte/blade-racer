using UnityEngine;

namespace BladeRacer.Track
{
    [DisallowMultipleComponent]
    public sealed class TrackTile : MonoBehaviour
    {
        [SerializeField] private string tileId = "tile";
        [SerializeField] private TrackSpline spline;
        [SerializeField] private TrackSocket entry;
        [SerializeField] private TrackSocket exit;
        [SerializeField] private Bounds localBounds = new(Vector3.forward * 25f, new Vector3(14f, 8f, 50f));

        public string TileId => tileId;
        public TrackSpline Spline => spline;
        public TrackSocket Entry => entry;
        public TrackSocket Exit => exit;
        public Bounds LocalBounds => localBounds;

        public void Configure(TrackSpline value, TrackSocket start, TrackSocket end)
        { spline = value; entry = start; exit = end; }

        public void ConfigureMetadata(string id, Bounds bounds)
        { tileId = id; localBounds = bounds; }

        public void SnapEntryTo(TrackSocket target)
        {
            if (!entry || !target || !entry.IsCompatibleWith(target)) return;
            Quaternion desiredEntryRotation = target.transform.rotation;
            transform.rotation = desiredEntryRotation * Quaternion.Inverse(entry.transform.localRotation);
            transform.position += target.transform.position - entry.transform.position;
        }

        private void Reset()
        {
            spline = GetComponentInChildren<TrackSpline>();
            foreach (var socket in GetComponentsInChildren<TrackSocket>())
            {
                if (socket.Direction == SocketDirection.Entry) entry = socket; else exit = socket;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 1f, .15f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(localBounds.center, localBounds.size);
        }
    }
}
