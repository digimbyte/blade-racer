using UnityEngine;

namespace BladeRacer.Track
{
    public enum SocketDirection { Entry, Exit }

    public sealed class TrackSocket : MonoBehaviour
    {
        [SerializeField] private SocketDirection direction;
        [SerializeField] private string profile = "standard";
        [SerializeField, Min(0.01f)] private float width = 12f;

        public SocketDirection Direction => direction;
        public string Profile => profile;
        public float Width => width;
        public bool IsCompatibleWith(TrackSocket other) => other && direction != other.direction && profile == other.profile;

        public void Configure(SocketDirection socketDirection, string socketProfile, float socketWidth)
        {
            direction = socketDirection;
            profile = socketProfile;
            width = Mathf.Max(.01f, socketWidth);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = direction == SocketDirection.Entry ? Color.green : Color.magenta;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(Vector3.left * width * .5f, Vector3.right * width * .5f);
            Gizmos.DrawRay(Vector3.zero, Vector3.forward * 3f);
        }
    }
}
