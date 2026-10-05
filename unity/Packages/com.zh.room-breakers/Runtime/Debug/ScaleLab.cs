using System.Collections.Generic;
using RoomBreakers.Core;
using UnityEngine;
using NVector = System.Numerics.Vector3;
using NQuaternion = System.Numerics.Quaternion;

namespace RoomBreakers.ScaleLab
{
    // Synthetic mouse-only validation scene. Intentionally does NOT claim XR support.
    public sealed class ScaleLab : MonoBehaviour
    {
        private ScaleSession session;
        private Camera viewCamera;
        private Transform roomRoot;
        private Transform miniatureRoot;
        private Transform largeProbe;
        private Transform smallProbe;
        private Transform handProxy;
        private Collider pickCollider;
        private LineRenderer correspondence;
        private readonly List<Material> materials = new List<Material>();
        private Plane dragPlane;
        private Vector3 lastPoint;
        private float heightOffset;
        private float handYaw;
        private bool appFocused = true;
        private bool appPaused;
        private readonly Rect panel = new Rect(12, 12, 650, 200);

        private static NVector N(Vector3 v) => new NVector(v.x, v.y, v.z);
        private static Vector3 U(NVector v) => new Vector3(v.X, v.Y, v.Z);
        private static Quaternion U(NQuaternion q) => new Quaternion(q.X, q.Y, q.Z, q.W);
        private static NQuaternion N(Quaternion q) => new NQuaternion(q.x, q.y, q.z, q.w);

        private void Start()
        {
            var room = new SpatialFrame(Pose3.Identity, 1f);
            var mini = new SpatialFrame(new Pose3(new NVector(-2.4f, 1f, -2.1f), N(Quaternion.Euler(0, -15, 0))), .25f);
            session = new ScaleSession(new DualScaleMap(room, mini), new Pose3(new NVector(0, .5f, 0), NQuaternion.Identity),
                new InteractionBounds(new NVector(-1.8f, .15f, -1.3f), new NVector(1.8f, 1.8f, 1.3f)), new NVector(1.2f, .5f, .8f), .38f);
            roomRoot = new GameObject("Synthetic room (not a scan)").transform;
            roomRoot.SetParent(transform, false);
            miniatureRoot = new GameObject("Miniature view of same state").transform;
            miniatureRoot.SetParent(transform, false);
            ApplyFrames();
            BuildRoom(roomRoot);
            BuildRoom(miniatureRoot);
            largeProbe = MakePrimitive("Room probe view", PrimitiveType.Cube, roomRoot, Vector3.zero, Vector3.one * .34f, new Color(1f, .7f, .16f), false);
            smallProbe = MakePrimitive("Selectable miniature probe", PrimitiveType.Cube, miniatureRoot, Vector3.zero, Vector3.one * .34f, new Color(1f, .7f, .16f), true);
            pickCollider = smallProbe.GetComponent<Collider>();
            ((BoxCollider)pickCollider).size = Vector3.one * 1.5f;
            handProxy = new GameObject("Enlarged input proxy - NOT tracked hand joints").transform;
            handProxy.SetParent(transform, false);
            MakePrimitive("Palm proxy", PrimitiveType.Cube, handProxy, Vector3.zero, new Vector3(.38f, .06f, .26f), new Color(.25f, .72f, 1f), false);
            for (int i = 0; i < 3; i++)
                MakePrimitive("Finger proxy", PrimitiveType.Cube, handProxy, new Vector3(-.12f + i * .12f, 0, .26f), new Vector3(.06f, .06f, .26f), new Color(.25f, .72f, 1f), false);
            var lineObject = new GameObject("Selected correspondence");
            lineObject.transform.SetParent(transform, false);
            correspondence = lineObject.AddComponent<LineRenderer>();
            correspondence.sharedMaterial = MakeMaterial(new Color(.25f, .72f, 1f));
            correspondence.positionCount = 2;
            correspondence.startWidth = .012f;
            correspondence.endWidth = .012f;
            correspondence.useWorldSpace = true;
            var cameraObject = new GameObject("Scale lab camera");
            cameraObject.transform.SetParent(transform, false);
            viewCamera = cameraObject.AddComponent<Camera>();
            viewCamera.transform.position = new Vector3(4.2f, 4.8f, -7f);
            viewCamera.transform.LookAt(new Vector3(-.6f, .35f, -.2f));
            viewCamera.clearFlags = CameraClearFlags.SolidColor;
            viewCamera.backgroundColor = new Color(.045f, .06f, .085f);
            viewCamera.nearClipPlane = .05f;
            viewCamera.farClipPlane = 40;
            viewCamera.fieldOfView = 48;
            RefreshViews();
        }

        private Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) throw new System.InvalidOperationException("Scale Lab requires a built-in or URP unlit shader. Open in a compatible 3D project.");
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            materials.Add(material);
            return material;
        }

        private Transform MakePrimitive(string label, PrimitiveType kind, Transform parent, Vector3 position, Vector3 scale, Color color, bool selectable)
        {
            GameObject obj = GameObject.CreatePrimitive(kind);
            obj.name = label;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = MakeMaterial(color);
            obj.GetComponent<Collider>().enabled = selectable;
            return obj.transform;
        }

        private void BuildRoom(Transform parent)
        {
            MakePrimitive("Floor", PrimitiveType.Cube, parent, new Vector3(0, -.025f, 0), new Vector3(4, .05f, 3), new Color(.13f, .2f, .27f), false);
            MakePrimitive("Back wall", PrimitiveType.Cube, parent, new Vector3(0, .65f, 1.5f), new Vector3(4, 1.3f, .05f), new Color(.19f, .27f, .34f), false);
            MakePrimitive("Landmark", PrimitiveType.Cube, parent, new Vector3(-1.3f, .15f, .8f), new Vector3(.5f, .3f, .5f), new Color(.42f, .47f, .55f), false);
            var target = new GameObject("Valid return ring");
            target.transform.SetParent(parent, false);
            target.transform.localPosition = U(session.ReturnCenter);
            var line = target.AddComponent<LineRenderer>();
            line.sharedMaterial = MakeMaterial(new Color(.15f, .92f, .6f));
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 48;
            line.startWidth = .025f;
            line.endWidth = .025f;
            for (int i = 0; i < 48; i++)
            {
                float angle = i * 2f * Mathf.PI / 48;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * session.ReturnRadius, 0, Mathf.Sin(angle) * session.ReturnRadius));
            }
        }

        private static void SetFrame(Transform target, SpatialFrame frame)
        {
            target.SetPositionAndRotation(U(frame.Origin.Position), U(frame.Origin.Rotation));
            target.localScale = Vector3.one * frame.Scale;
        }
        private void ApplyFrames()
        {
            // ScaleLab root must remain identity; do not introduce parent nonuniform scale.
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
            SetFrame(roomRoot, session.Map.Room);
            SetFrame(miniatureRoot, session.Map.Miniature);
        }
        private static void ShowPose(Transform view, Pose3 pose) => view.SetPositionAndRotation(U(pose.Position), U(pose.Rotation));
        private void RefreshViews()
        {
            ShowPose(largeProbe, session.Map.RoomView(session.ObjectPose));
            ShowPose(smallProbe, session.Map.MiniatureView(session.ObjectPose));
            bool visible = session.State != ProbeState.Returned;
            largeProbe.gameObject.SetActive(visible);
            smallProbe.gameObject.SetActive(visible);
            bool held = session.State == ProbeState.Held;
            handProxy.gameObject.SetActive(held);
            correspondence.enabled = held;
            if (held)
            {
                ShowPose(handProxy, session.LastHandWorld);
                correspondence.SetPosition(0, smallProbe.position);
                correspondence.SetPosition(1, largeProbe.position);
            }
        }
        private void Update()
        {
            if (session == null) return;
            session.Tick(Time.unscaledDeltaTime);
            RefreshViews();
        }
        private Pose3 MousePose(Vector3 point)
        {
            Quaternion rotation = miniatureRoot.rotation * Quaternion.AngleAxis(handYaw, Vector3.up);
            return new Pose3(N(point + miniatureRoot.up * heightOffset), N(rotation));
        }
        private void OnGUI()
        {
            if (session == null || viewCamera == null) return;
            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Label("ROOMBREAKERS | SYNTHETIC DESKTOP SCALE LAB - NOT XR");
            GUILayout.Label("Mouse input; synthetic room; no passthrough or tracked hands.");
            GUILayout.Label("Drag the SMALL gold cube to its green ring. Q/E rotate. Scroll changes height. Esc cancels.");
            GUILayout.Label("State: " + session.State + " | Returned: " + session.ReturnCount + " | Pause: " + session.PauseReasons + (session.CanReturn ? " | RELEASE TO RETURN" : ""));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pause / resume")) session.SetPause(PauseReason.User, (session.PauseReasons & PauseReason.User) == 0);
            if (GUILayout.Button("Simulate tracking loss / restore")) session.SetPause(PauseReason.TrackingLost, (session.PauseReasons & PauseReason.TrackingLost) == 0);
            if (GUILayout.Button("Reset probe")) session.Reset();
            GUILayout.EndHorizontal();
            GUI.enabled = session.IsPaused;
            GUILayout.Label("Miniature scale (adjust while paused)");
            float scale = GUILayout.HorizontalSlider(session.Map.Miniature.Scale, .15f, .35f);
            if (Mathf.Abs(scale - session.Map.Miniature.Scale) > .0001f)
            {
                session.Recalibrate(new DualScaleMap(session.Map.Room, new SpatialFrame(session.Map.Miniature.Origin, scale)));
                ApplyFrames();
            }
            GUI.enabled = true;
            GUILayout.EndArea();
            Event e = Event.current;
            // Losing a mouse-up over the HUD must not leave an object captured.
            if (e.rawType == EventType.MouseUp && e.button == 0 && session.State == ProbeState.Held && panel.Contains(e.mousePosition))
            {
                session.CancelCapture(HandId.Left);
                RefreshViews();
                return;
            }
            if (e.type == EventType.Used || panel.Contains(e.mousePosition) || session.IsPaused) return;
            var ray = viewCamera.ScreenPointToRay(new Vector3(e.mousePosition.x, Screen.height - e.mousePosition.y, 0));
            if (e.type == EventType.MouseDown && e.button == 0 && pickCollider.enabled && smallProbe.gameObject.activeInHierarchy)
            {
                RaycastHit hit;
                if (pickCollider.Raycast(ray, out hit, 100))
                {
                    heightOffset = 0;
                    handYaw = 0;
                    lastPoint = hit.point;
                    dragPlane = new Plane(miniatureRoot.up, hit.point);
                    session.BeginCapture(HandId.Left, MousePose(hit.point));
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag && e.button == 0 && session.State == ProbeState.Held)
            {
                float enter;
                if (dragPlane.Raycast(ray, out enter))
                {
                    lastPoint = ray.GetPoint(enter);
                    session.MoveCapture(HandId.Left, MousePose(lastPoint));
                }
                e.Use();
            }
            else if (e.type == EventType.ScrollWheel && session.State == ProbeState.Held)
            {
                heightOffset = Mathf.Clamp(heightOffset - e.delta.y * .005f, -.25f, .25f);
                session.MoveCapture(HandId.Left, MousePose(lastPoint));
                e.Use();
            }
            else if (e.type == EventType.KeyDown && session.State == ProbeState.Held)
            {
                if (e.keyCode == KeyCode.Escape) { session.CancelCapture(HandId.Left); e.Use(); }
                else if (e.keyCode == KeyCode.Q || e.keyCode == KeyCode.E)
                {
                    handYaw += e.keyCode == KeyCode.Q ? -15 : 15;
                    session.MoveCapture(HandId.Left, MousePose(lastPoint));
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && session.State == ProbeState.Held)
            {
                session.ReleaseCapture(HandId.Left);
                e.Use();
            }
            RefreshViews();
        }
        private void OnApplicationFocus(bool hasFocus)
        {
            appFocused = hasFocus;
            if (session != null) session.SetPause(PauseReason.FocusLost, !appFocused || appPaused);
        }
        private void OnApplicationPause(bool paused)
        {
            appPaused = paused;
            if (session != null) session.SetPause(PauseReason.FocusLost, !appFocused || appPaused);
        }
        private void OnDisable()
        {
            if (session != null) session.SetPause(PauseReason.FocusLost, true);
        }
        private void OnEnable()
        {
            if (session != null) session.SetPause(PauseReason.FocusLost, !appFocused || appPaused);
        }
        private void OnDestroy()
        {
            foreach (Material material in materials) if (material != null) Destroy(material);
        }
    }
}
