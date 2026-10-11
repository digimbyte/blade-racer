using BladeRacer.Track;
using BladeRacer.Vehicle;
using BladeRacer.Input;
using BladeRacer.Platform;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BladeRacer.Editor
{
    public static class DemoTrackBuilder
    {
        [MenuItem("Blade Racer/Create Demo Track")]
        public static void CreateDemoTrack()
        {
            var root = new GameObject("Demo Track", typeof(TrackAssembler));
            Undo.RegisterCreatedObjectUndo(root, "Create Demo Track");
            TrackTile previous = null;
            TrackShape[] showcase = { TrackShape.Straight, TrackShape.BankedTurn, TrackShape.Climb, TrackShape.Twist, TrackShape.Loop, TrackShape.Chicane };
            foreach (var shape in showcase)
            {
                var tile = CreateTile(root.transform, shape);
                if (previous && previous.Exit) tile.SnapEntryTo(previous.Exit);
                previous = tile;
            }
            root.GetComponent<TrackAssembler>().CollectChildTiles();

            var vehicle = new GameObject("Player Hovercraft", typeof(BoxCollider), typeof(Rigidbody));
            vehicle.name = "Player Hovercraft";
            vehicle.transform.SetPositionAndRotation(new Vector3(0f, 2.5f, 4f), Quaternion.identity);
            vehicle.GetComponent<BoxCollider>().size = new Vector3(2f, .6f, 4f);
            vehicle.GetComponent<Rigidbody>().mass = 2f;
            vehicle.AddComponent<HoverVehicleController>();
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual"; visual.transform.SetParent(vehicle.transform, false);
            visual.transform.localScale = new Vector3(2f, .6f, 4f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            Undo.RegisterCreatedObjectUndo(vehicle, "Create Demo Vehicle");
            CreateCrossPlatformRig(vehicle.GetComponent<VehicleInputSource>());
            Selection.activeGameObject = root;
        }

        [MenuItem("Blade Racer/Create Cross-Platform Controls")]
        private static void CreateControlsMenu()
        {
            var input = Object.FindFirstObjectByType<VehicleInputSource>();
            if (!input) { Debug.LogError("Create or select a vehicle with VehicleInputSource first."); return; }
            CreateCrossPlatformRig(input);
        }

        private static void CreateCrossPlatformRig(VehicleInputSource input)
        {
            var platform = new GameObject("Platform", typeof(PlatformBootstrap));
            var canvas = new GameObject("Mobile Controls", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(SafeAreaFitter));
            canvas.transform.SetParent(platform.transform, false);
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920f, 1080f); scaler.matchWidthOrHeight = .5f;

            CreateControl(canvas.transform, "Steer", new Vector2(60f, 60f), new Vector2(420f, 300f), new Color(.1f, .7f, 1f, .3f), input, TouchControlKind.SteeringPad);
            CreateControl(canvas.transform, "Throttle", new Vector2(-260f, 60f), new Vector2(200f, 280f), new Color(.2f, 1f, .3f, .35f), input, TouchControlKind.Throttle);
            CreateControl(canvas.transform, "Brake", new Vector2(-480f, 60f), new Vector2(180f, 220f), new Color(1f, .25f, .2f, .35f), input, TouchControlKind.Brake);
            CreateControl(canvas.transform, "Boost", new Vector2(-60f, 60f), new Vector2(180f, 180f), new Color(.8f, .2f, 1f, .4f), input, TouchControlKind.Boost);

            var bootstrap = platform.GetComponent<PlatformBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("mobileControlsRoot").objectReferenceValue = canvas;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (!Object.FindFirstObjectByType<EventSystem>())
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(platform, "Create Cross-Platform Controls");
        }

        private static void CreateControl(Transform parent, string label, Vector2 offset, Vector2 size, Color color, VehicleInputSource input, TouchControlKind kind)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(TouchControl));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = kind == TouchControlKind.SteeringPad ? Vector2.zero : new Vector2(1f, 0f);
            rect.anchorMax = rect.anchorMin; rect.pivot = rect.anchorMin; rect.anchoredPosition = offset; rect.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            go.GetComponent<TouchControl>().Configure(input, kind);
        }

        [MenuItem("GameObject/Blade Racer/Track/Straight", false, 10)] private static void Straight(MenuCommand c) => CreateSelected(c, TrackShape.Straight);
        [MenuItem("GameObject/Blade Racer/Track/Curve", false, 11)] private static void Curve(MenuCommand c) => CreateSelected(c, TrackShape.Curve);
        [MenuItem("GameObject/Blade Racer/Track/Banked Turn", false, 12)] private static void Banked(MenuCommand c) => CreateSelected(c, TrackShape.BankedTurn);
        [MenuItem("GameObject/Blade Racer/Track/Climb", false, 13)] private static void Climb(MenuCommand c) => CreateSelected(c, TrackShape.Climb);
        [MenuItem("GameObject/Blade Racer/Track/Chicane", false, 14)] private static void Chicane(MenuCommand c) => CreateSelected(c, TrackShape.Chicane);
        [MenuItem("GameObject/Blade Racer/Track/Twist", false, 15)] private static void Twist(MenuCommand c) => CreateSelected(c, TrackShape.Twist);
        [MenuItem("GameObject/Blade Racer/Track/Loop", false, 16)] private static void Loop(MenuCommand c) => CreateSelected(c, TrackShape.Loop);

        private static void CreateSelected(MenuCommand command, TrackShape shape)
        {
            var parentObject = command.context as GameObject;
            var tile = CreateTile(parentObject ? parentObject.transform : null, shape);
            Selection.activeGameObject = tile.gameObject;
        }

        private static TrackTile CreateTile(Transform parent, TrackShape shape)
        {
            var root = new GameObject($"{shape} Track Tile", typeof(TrackTile), typeof(TrackSpline), typeof(TrackSurface), typeof(ProceduralTrackPiece));
            Undo.RegisterCreatedObjectUndo(root, "Create Track Tile");
            if (parent) root.transform.SetParent(parent, false);
            var spline = root.GetComponent<TrackSpline>();

            var entry = CreateSocket(root.transform, "Entry", Vector3.zero, SocketDirection.Entry);
            var exit = CreateSocket(root.transform, "Exit", Vector3.forward * 50f, SocketDirection.Exit);
            root.GetComponent<TrackTile>().Configure(spline, entry, exit);
            var surface = root.GetComponent<TrackSurface>();
            surface.Configure(spline);
            root.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
            root.GetComponent<ProceduralTrackPiece>().Configure(shape);
            return root.GetComponent<TrackTile>();
        }

        private static TrackSocket CreateSocket(Transform parent, string name, Vector3 position, SocketDirection direction)
        {
            var socket = new GameObject(name, typeof(TrackSocket));
            socket.transform.SetParent(parent, false); socket.transform.localPosition = position;
            var component = socket.GetComponent<TrackSocket>();
            component.Configure(direction, "standard", 12f);
            return component;
        }
    }
}
