using System.Collections.Generic;
using UnityEngine;

namespace BladeRacer.Track
{
    public sealed class TrackAssembler : MonoBehaviour
    {
        [SerializeField] private List<TrackTile> tiles = new();
        [SerializeField, Min(0.001f)] private float snapTolerance = .05f;
        public IReadOnlyList<TrackTile> Tiles => tiles;

        public bool TrySample(float trackProgress, out Pose pose, out float width)
        {
            pose = default; width = 0f;
            if (tiles.Count == 0) return false;
            float scaled = Mathf.Clamp01(trackProgress) * tiles.Count;
            int index = Mathf.Min(Mathf.FloorToInt(scaled), tiles.Count - 1);
            var spline = tiles[index] ? tiles[index].Spline : null;
            if (!spline) return false;
            float localT = Mathf.Clamp01(scaled - index);
            spline.Evaluate(localT, out var p, out var tangent, out var up, out width);
            pose = new Pose(p, Quaternion.LookRotation(tangent, up));
            return true;
        }

        [ContextMenu("Collect Child Tiles")]
        public void CollectChildTiles()
        {
            tiles.Clear();
            tiles.AddRange(GetComponentsInChildren<TrackTile>());
        }

        [ContextMenu("Validate Connections")]
        public void ValidateConnections()
        {
            for (int i = 0; i < tiles.Count - 1; i++)
            {
                if (!tiles[i] || !tiles[i + 1] || !tiles[i].Exit || !tiles[i + 1].Entry) continue;
                float gap = Vector3.Distance(tiles[i].Exit.transform.position, tiles[i + 1].Entry.transform.position);
                if (gap > snapTolerance) Debug.LogWarning($"Track gap {gap:F3}m between {tiles[i].name} and {tiles[i + 1].name}.", tiles[i]);
            }
        }
    }
}
