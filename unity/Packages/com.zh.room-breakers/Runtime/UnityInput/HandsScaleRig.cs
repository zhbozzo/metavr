using System;
using System.Collections.Generic;
using RoomBreakers.Core;
using UnityEngine;
using NVector = System.Numerics.Vector3;
using NQuaternion = System.Numerics.Quaternion;

namespace RoomBreakers.UnityInput
{
    // Input harness, not the final game. Layout is synthetic; this component does not scan a room.
    // Add it to an existing XR rig scene. Never creates or moves the headset camera.
    public sealed class HandsScaleRig : MonoBehaviour
    {
        [SerializeField] private Transform roomFrame;
        [SerializeField] private Transform miniatureFrame;
        [SerializeField] private HandSampleSource leftSource;
        [SerializeField] private HandSampleSource rightSource;
        [SerializeField] private Camera playerCamera;
        [SerializeField, Range(.01f, .15f)] private float selectionRadius = .06f;
        [SerializeField, Range(.05f, .5f)] private float sampleTimeout = .20f;
        private ScaleSession session;
        private HandCaptureDriver driver;
        private GameObject generated;
        private Transform roomProbe, miniProbe, proxy, miniRing;
        private Renderer miniRenderer;
        private TextMesh statusText;
        private Material gold, cyan, green, muted;
        private readonly List<Material> materials = new List<Material>();
        private bool appFocused = true, appPaused;
        private string status;
        public ScaleSession Session => session;

        public void Configure(Transform room, Transform miniature, HandSampleSource left,
            HandSampleSource right, Camera camera)
        {
            if (Application.isPlaying && session != null) throw new InvalidOperationException("Configure before starting the harness.");
            roomFrame = room; miniatureFrame = miniature; leftSource = left; rightSource = right; playerCamera = camera;
        }
        private bool TryMap(out DualScaleMap map)
        {
            map = null;
            if (!UnitySpatialFrame.TryRead(roomFrame, true, out SpatialFrame room, out status)) return false;
            if (!UnitySpatialFrame.TryRead(miniatureFrame, false, out SpatialFrame mini, out status)) return false;
            if (mini.Scale < .10f || mini.Scale > .40f)
            { status = "Hands harness miniature scale must be between 0.10 and 0.40."; return false; }
            map = new DualScaleMap(room, mini);
            return true;
        }
        private void Start()
        {
            if (leftSource == null && rightSource == null)
            { status = "Assign at least one hand source. No simulated hands are substituted."; Debug.LogError(status, this); enabled = false; return; }
            if (leftSource != null && leftSource == rightSource)
            { status = "Left and right sources cannot be the same component."; Debug.LogError(status, this); enabled = false; return; }
            if (playerCamera == null) playerCamera = Camera.main;
            if (!TryMap(out DualScaleMap map)) { Debug.LogError(status, this); enabled = false; return; }
            session = new ScaleSession(map, new Pose3(new NVector(0, .3f, 0), NQuaternion.Identity),
                new InteractionBounds(new NVector(-1, .05f, -.6f), new NVector(1, 1.4f, .6f)), new NVector(.7f, .3f, .35f), .20f);
            driver = new HandCaptureDriver(session, new HandCaptureSettings(sampleTimeout, selectionRadiusMeters: selectionRadius));
            driver.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused);
            generated = new GameObject("RoomBreakers hand harness views (synthetic layout)");
            // Separate identity root prevents accidental scaling of the already-mapped world poses.
            gold = Material(new Color(1f, .68f, .16f)); cyan = Material(new Color(.18f, .72f, 1f));
            green = Material(new Color(.18f, .85f, .50f)); muted = Material(new Color(.2f, .25f, .30f));
            roomProbe = Primitive("Room probe", PrimitiveType.Cube, gold);
            miniProbe = Primitive("Miniature probe", PrimitiveType.Cube, gold);
            miniRenderer = miniProbe.GetComponent<Renderer>();
            proxy = Primitive("Enlarged pinch-position proxy (not a hand mesh)", PrimitiveType.Sphere, cyan);
            proxy.localScale = Vector3.one * .10f;
            CreateRing("Room return volume", session.Map.Room, green);
            miniRing = CreateRing("Miniature return volume", session.Map.Miniature, green);
            var label = new GameObject("Hands harness instructions"); label.transform.SetParent(generated.transform, false);
            statusText = label.AddComponent<TextMesh>(); statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 40; statusText.characterSize = .0035f; statusText.anchor = TextAnchor.MiddleCenter;
            statusText.alignment = TextAlignment.Center; statusText.color = Color.white;
            if (statusText.font != null) label.GetComponent<MeshRenderer>().sharedMaterial = statusText.font.material;
            Refresh();
        }
        private Material Material(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) throw new InvalidOperationException("Assign a supported built-in or URP renderer before running this harness.");
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            materials.Add(material); return material;
        }
        private Transform Primitive(string label, PrimitiveType type, Material material)
        {
            var obj = GameObject.CreatePrimitive(type); obj.name = label;
            obj.transform.SetParent(generated.transform, false);
            obj.GetComponent<Renderer>().sharedMaterial = material;
            obj.GetComponent<Collider>().enabled = false; // Selection and return authority are in the domain.
            return obj.transform;
        }
        private Transform CreateRing(string label, SpatialFrame frame, Material material)
        {
            var obj = new GameObject(label); obj.transform.SetParent(generated.transform, false);
            UnitySpatialFrame.ApplyPose(obj.transform, frame.ToWorld(new Pose3(session.ReturnCenter, NQuaternion.Identity)));
            obj.transform.localScale = Vector3.one * frame.Scale;
            var line = obj.AddComponent<LineRenderer>(); line.sharedMaterial = material;
            line.useWorldSpace = false; line.loop = true; line.positionCount = 48;
            line.startWidth = line.endWidth = .015f * frame.Scale;
            for (int i = 0; i < 48; i++)
            {
                float a = i * 2f * Mathf.PI / 48;
                line.SetPosition(i, new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * session.ReturnRadius);
            }
            return obj.transform;
        }
        private static HandSample Read(HandSampleSource source)
        {
            return source != null && source.TryGetLatest(out HandSample sample) ? sample : default;
        }
        private bool FramesUnchanged()
        {
            if (!TryMap(out DualScaleMap current)) return false;
            return Same(current.Room, session.Map.Room) && Same(current.Miniature, session.Map.Miniature);
        }
        private static bool Same(SpatialFrame a, SpatialFrame b) =>
            NVector.Distance(a.Origin.Position, b.Origin.Position) < .0001f &&
            Math.Abs(NQuaternion.Dot(a.Origin.Rotation, b.Origin.Rotation)) > .999999f && Math.Abs(a.Scale - b.Scale) < .00001f;
        private void LateUpdate()
        {
            if (driver == null) return;
            if (!FramesUnchanged()) driver.SetExternalPause(PauseReason.Placement, true);
            driver.Step(Time.realtimeSinceStartupAsDouble, Read(leftSource), Read(rightSource));
            session.Tick(Time.unscaledDeltaTime);
            Refresh();
        }
        private void Refresh()
        {
            if (generated == null) return;
            UnitySpatialFrame.ApplyPose(roomProbe, session.Map.RoomView(session.ObjectPose));
            UnitySpatialFrame.ApplyPose(miniProbe, session.Map.MiniatureView(session.ObjectPose));
            roomProbe.localScale = Vector3.one * .24f; miniProbe.localScale = Vector3.one * .24f * session.Map.Miniature.Scale;
            bool returned = session.State == ProbeState.Returned;
            roomProbe.gameObject.SetActive(!returned); miniProbe.gameObject.SetActive(!returned);
            bool held = session.State == ProbeState.Held;
            proxy.gameObject.SetActive(held && session.HasValidCaptureSample);
            if (held) UnitySpatialFrame.ApplyPose(proxy, session.LastHandWorld);
            miniRenderer.sharedMaterial = held || driver.HoverHand != HandId.None ? cyan : gold;
            miniRing.GetComponent<LineRenderer>().sharedMaterial = session.CanReturn ? cyan : green;
            if ((session.PauseReasons & PauseReason.Placement) != 0) status = "PLACEMENT PAUSED\nConfirm the new frames before continuing.";
            else if ((session.PauseReasons & (PauseReason.FocusLost | PauseReason.User)) != 0) status = "PAUSED";
            else if (driver.RequiresOpenHand) status = "Open a tracked hand to begin or recover.";
            else if (returned) status = "Returned! Use RestartProbe to repeat.";
            else if (session.CanReturn) status = "Open your fingers to return the probe.";
            else if (held) status = "Move the small probe into its ring.";
            else status = "Pinch the small gold probe.";
            statusText.text = "ROOMBREAKERS - HAND INPUT LAB\nSYNTHETIC LAYOUT / NO ROOM SCAN\n" + status;
            var labelPose = session.Map.MiniatureView(new Pose3(new NVector(0, 1.2f, .15f), NQuaternion.Identity));
            statusText.transform.position = UnitySpatialFrame.Vector(labelPose.Position);
            if (playerCamera != null) statusText.transform.rotation = playerCamera.transform.rotation;
        }
        public void PauseInteraction() { driver?.SetExternalPause(PauseReason.User, true); }
        public void ResumeInteraction() { driver?.SetExternalPause(PauseReason.User, false); }
        public void RestartProbe() { driver?.Reset(); }
        public void CancelInteraction() { driver?.Cancel(); }
        public void ConfirmPlacement()
        {
            if (driver == null) return;
            driver.SetExternalPause(PauseReason.Placement, true);
            if (!TryMap(out DualScaleMap map) || !driver.Recalibrate(map)) return;
            // Rebuild target visuals because their frames changed. No scene assets are modified.
            foreach (Transform t in generated.transform)
                if (t.name == "Room return volume" || t.name == "Miniature return volume") Destroy(t.gameObject);
            CreateRing("Room return volume", session.Map.Room, green);
            miniRing = CreateRing("Miniature return volume", session.Map.Miniature, green);
            driver.SetExternalPause(PauseReason.Placement, false);
        }
        private void OnApplicationFocus(bool value) { appFocused = value; driver?.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused); }
        private void OnApplicationPause(bool value) { appPaused = value; driver?.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused); }
        private void OnDisable() { driver?.SetExternalPause(PauseReason.FocusLost, true); }
        private void OnEnable() { driver?.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused); }
        private void OnDestroy()
        {
            if (generated != null) Destroy(generated);
            foreach (var material in materials) if (material != null) Destroy(material);
        }
    }
}
