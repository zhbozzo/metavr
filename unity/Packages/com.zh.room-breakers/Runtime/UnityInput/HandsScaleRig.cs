using System;
using System.Collections.Generic;
using RoomBreakers.Core;
using UnityEngine;
using NVector = System.Numerics.Vector3;
using NQuaternion = System.Numerics.Quaternion;

namespace RoomBreakers.UnityInput
{
    // Synthetic input harness, not the final game or a room scan. Never moves the headset camera.
    public sealed class HandsScaleRig : MonoBehaviour
    {
        [SerializeField] private Transform roomFrame, miniatureFrame;
        [SerializeField] private HandSampleSource leftSource, rightSource;
        [SerializeField] private Camera playerCamera;
        [SerializeField, Range(.01f, .15f)] private float selectionRadius = .06f;
        [SerializeField, Range(.05f, .5f)] private float sampleTimeout = .20f;
        private ScaleSession session;
        private HandCaptureDriver driver;
        private GameObject generated;
        private Transform roomProbe, miniProbe, proxy, roomRing, miniRing;
        private Renderer miniRenderer;
        private TextMesh statusText;
        private Material gold, cyan, green;
        private readonly List<Material> materials = new List<Material>();
        private bool appFocused = true, appPaused, ready;
        private string status;
        public ScaleSession Session => session;
        public bool IsReady => ready && isActiveAndEnabled;

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
            map = new DualScaleMap(room, mini); return true;
        }
        private void Start()
        {
            try { Initialize(); }
            catch (Exception e)
            {
                ready = false; enabled = false;
                if (generated != null) generated.SetActive(false);
                Debug.LogException(e, this); // Never turn an initialization failure into a running, partially wired harness.
            }
        }
        private void Initialize()
        {
            if (leftSource == null && rightSource == null)
                throw new InvalidOperationException("Assign at least one hand source. No simulated hands are substituted.");
            if (leftSource != null && leftSource == rightSource)
                throw new InvalidOperationException("Left and right sources cannot be the same component.");
            if (playerCamera == null) playerCamera = Camera.main;
            if (!TryMap(out DualScaleMap map)) throw new InvalidOperationException(status);
            session = new ScaleSession(map, new Pose3(new NVector(0, .3f, 0), NQuaternion.Identity),
                new InteractionBounds(new NVector(-1, .05f, -.6f), new NVector(1, 1.4f, .6f)), new NVector(.7f, .3f, .35f), .20f);
            driver = new HandCaptureDriver(session, new HandCaptureSettings(sampleTimeout, selectionRadiusMeters: selectionRadius));
            driver.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused);
            generated = new GameObject("RoomBreakers hand harness views (synthetic layout)");
            // Identity root prevents accidental scaling of already-mapped world poses.
            gold = MakeMaterial(new Color(1f, .68f, .16f)); cyan = MakeMaterial(new Color(.18f, .72f, 1f));
            green = MakeMaterial(new Color(.18f, .85f, .50f));
            roomProbe = Primitive("Room probe", PrimitiveType.Cube, gold);
            miniProbe = Primitive("Miniature probe", PrimitiveType.Cube, gold); miniRenderer = miniProbe.GetComponent<Renderer>();
            proxy = Primitive("Enlarged pinch-position proxy (not a hand mesh)", PrimitiveType.Sphere, cyan);
            proxy.localScale = Vector3.one * .10f;
            roomRing = CreateRing("Room return volume", session.Map.Room);
            miniRing = CreateRing("Miniature return volume", session.Map.Miniature);
            var label = new GameObject("Hands harness instructions"); label.transform.SetParent(generated.transform, false);
            statusText = label.AddComponent<TextMesh>(); statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (statusText.font == null) throw new InvalidOperationException("The harness font could not be loaded.");
            statusText.fontSize = 40; statusText.characterSize = .0035f; statusText.anchor = TextAnchor.MiddleCenter;
            statusText.alignment = TextAlignment.Center; statusText.color = Color.white;
            label.GetComponent<MeshRenderer>().sharedMaterial = statusText.font.material;
            ready = true; Refresh();
        }
        private Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) throw new InvalidOperationException("Use a supported built-in or URP renderer for this harness.");
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
            obj.GetComponent<Collider>().enabled = false; // Input and return authority are in the domain.
            return obj.transform;
        }
        private Transform CreateRing(string label, SpatialFrame frame)
        {
            var obj = new GameObject(label); obj.transform.SetParent(generated.transform, false);
            UnitySpatialFrame.ApplyPose(obj.transform, frame.ToWorld(new Pose3(session.ReturnCenter, NQuaternion.Identity)));
            obj.transform.localScale = Vector3.one * frame.Scale;
            var line = obj.AddComponent<LineRenderer>(); line.sharedMaterial = green;
            line.useWorldSpace = false; line.loop = true; line.positionCount = 48;
            line.startWidth = line.endWidth = .015f * frame.Scale;
            for (int i = 0; i < 48; i++)
            {
                float a = i * 2f * Mathf.PI / 48;
                line.SetPosition(i, new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * session.ReturnRadius);
            }
            return obj.transform;
        }
        private static HandSample Read(HandSampleSource source) =>
            source != null && source.TryGetLatest(out HandSample sample) ? sample : default;
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
            if (!IsReady) return;
            if (!FramesUnchanged()) driver.SetExternalPause(PauseReason.Placement, true);
            driver.Step(Time.realtimeSinceStartupAsDouble, Read(leftSource), Read(rightSource));
            session.Tick(Time.unscaledDeltaTime); Refresh();
        }
        private void Refresh()
        {
            if (!ready) return;
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
            statusText.transform.position = UnitySpatialFrame.Vector(session.Map.MiniatureView(
                new Pose3(new NVector(0, 1.2f, .15f), NQuaternion.Identity)).Position);
            if (playerCamera != null) statusText.transform.rotation = playerCamera.transform.rotation;
        }
        public void PauseInteraction() { driver?.SetExternalPause(PauseReason.User, true); }
        public void ResumeInteraction() { driver?.SetExternalPause(PauseReason.User, false); }
        public void RestartProbe() { driver?.Reset(); }
        public void CancelInteraction() { driver?.Cancel(); }
        public void ConfirmPlacement()
        {
            if (!IsReady) return;
            driver.SetExternalPause(PauseReason.Placement, true);
            if (!TryMap(out DualScaleMap map) || !driver.Recalibrate(map)) return;
            Destroy(roomRing.gameObject); Destroy(miniRing.gameObject);
            roomRing = CreateRing("Room return volume", session.Map.Room);
            miniRing = CreateRing("Miniature return volume", session.Map.Miniature);
            driver.SetExternalPause(PauseReason.Placement, false);
        }
        private void OnApplicationFocus(bool value) { appFocused = value; driver?.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused); }
        private void OnApplicationPause(bool value) { appPaused = value; driver?.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused); }
        private void OnDisable()
        {
            driver?.SetExternalPause(PauseReason.FocusLost, true);
            if (generated != null) generated.SetActive(false);
        }
        private void OnEnable()
        {
            driver?.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused);
            if (generated != null && ready) generated.SetActive(true);
        }
        private void OnDestroy()
        {
            if (generated != null) Destroy(generated);
            foreach (var material in materials) if (material != null) Destroy(material);
        }
    }
}
