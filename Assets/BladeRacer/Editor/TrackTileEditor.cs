using BladeRacer.Track;
using UnityEditor;
using UnityEngine;

namespace BladeRacer.Editor
{
    [CustomEditor(typeof(TrackTile))]
    public sealed class TrackTileEditor : UnityEditor.Editor
    {
        private TrackSocket targetSocket;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Snapping", EditorStyles.boldLabel);
            targetSocket = (TrackSocket)EditorGUILayout.ObjectField("Target Exit", targetSocket, typeof(TrackSocket), true);
            using (new EditorGUI.DisabledScope(!targetSocket))
            {
                if (GUILayout.Button("Snap Entry To Target"))
                {
                    var tile = (TrackTile)target;
                    Undo.RecordObject(tile.transform, "Snap Track Tile");
                    tile.SnapEntryTo(targetSocket);
                    EditorUtility.SetDirty(tile.transform);
                }
            }
        }
    }
}
