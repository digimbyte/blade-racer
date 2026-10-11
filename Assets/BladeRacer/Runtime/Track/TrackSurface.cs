using System.Collections.Generic;
using UnityEngine;

namespace BladeRacer.Track
{
    /// <summary>Builds a lightweight ribbon mesh and collision surface from a TrackSpline.</summary>
    [ExecuteAlways, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public sealed class TrackSurface : MonoBehaviour
    {
        [SerializeField] private TrackSpline spline;
        [SerializeField, Range(4, 256)] private int segments = 48;
        [SerializeField, Min(.05f)] private float thickness = .5f;
        [SerializeField, Min(.1f)] private float uvMetersPerTile = 8f;
        private Mesh generatedMesh;
        public TrackSpline Spline => spline;

        public void Configure(TrackSpline value) => spline = value;

        private void OnEnable()
        {
            if (!GetComponent<MeshFilter>().sharedMesh) Rebuild();
        }

        [ContextMenu("Rebuild Surface")]
        public void Rebuild()
        {
            if (!spline) spline = GetComponent<TrackSpline>();
            if (!spline || spline.SegmentCount == 0) return;
            if (!generatedMesh)
            {
                generatedMesh = new Mesh { name = $"{name} Surface (Generated)" };
                generatedMesh.MarkDynamic();
            }
            generatedMesh.Clear();

            var vertices = new List<Vector3>((segments + 1) * 4);
            var normals = new List<Vector3>((segments + 1) * 4);
            var uvs = new List<Vector2>((segments + 1) * 4);
            var triangles = new List<int>(segments * 18);
            float distance = 0f;
            Vector3 previous = Vector3.zero;
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                spline.Evaluate(t, out var worldPosition, out var tangent, out var worldUp, out var width);
                Vector3 center = transform.InverseTransformPoint(worldPosition);
                Vector3 up = transform.InverseTransformDirection(worldUp).normalized;
                Vector3 right = Vector3.Cross(up, transform.InverseTransformDirection(tangent)).normalized;
                if (i > 0) distance += Vector3.Distance(previous, center);
                previous = center;
                vertices.Add(center - right * width * .5f); vertices.Add(center + right * width * .5f);
                vertices.Add(center - right * width * .5f - up * thickness); vertices.Add(center + right * width * .5f - up * thickness);
                normals.Add(up); normals.Add(up); normals.Add(-up); normals.Add(-up);
                float v = distance / uvMetersPerTile;
                uvs.Add(new Vector2(0f, v)); uvs.Add(new Vector2(1f, v)); uvs.Add(new Vector2(0f, v)); uvs.Add(new Vector2(1f, v));
                if (i == 0) continue;
                int a = (i - 1) * 4, b = i * 4;
                AddQuad(triangles, a, b, b + 1, a + 1);
                AddQuad(triangles, a + 3, b + 3, b + 2, a + 2);
                AddQuad(triangles, a + 2, b + 2, b, a);
                AddQuad(triangles, a + 1, b + 1, b + 3, a + 3);
            }
            generatedMesh.SetVertices(vertices); generatedMesh.SetNormals(normals); generatedMesh.SetUVs(0, uvs);
            generatedMesh.SetTriangles(triangles, 0); generatedMesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = generatedMesh;
            var collider = GetComponent<MeshCollider>(); collider.sharedMesh = null; collider.sharedMesh = generatedMesh;
        }

        private static void AddQuad(List<int> indices, int a, int b, int c, int d)
        { indices.Add(a); indices.Add(b); indices.Add(c); indices.Add(a); indices.Add(c); indices.Add(d); }

        private void OnDestroy()
        {
            if (!generatedMesh) return;
            if (Application.isPlaying) Destroy(generatedMesh); else DestroyImmediate(generatedMesh);
        }
    }
}
