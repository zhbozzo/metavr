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
        private HarnessControls controls;
        private HandsControlPanel panel;
        private GameObject generated;
        private Transform roomProbe, miniProbe, proxy, roomRing, miniRing;
        private Renderer miniRenderer;
        private TextMesh statusText;
        private Material gold, cyan, green;
        private readonly List<Material> materials = new List<Material>();
        private bool appFocused = true, appPaused, ready;
        private string status, placementError;
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
            map = new DualScaleMap(room, mini);
            return true;
        }
        private void Start()
        {
            try { Initialize(); }
            catch (Exception error) { Fail(error); }
        }
        private void Fail(Exception error)
        {
            ready = false;
            enabled = false;
            if (generated != null) generated.SetActive(false);
            Debug.LogException(error, this);
        }
        private void Initialize()
        {
            if (leftSource == null && rightSource == null)
                throw new InvalidOperationException("Assign at least one hand source. No simulated hands are substituted.");
            if (leftSource != null && leftSource == rightSource)
                throw new InvalidOperationException("Left and right sources cannot be the same component.");
            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera == null) throw new InvalidOperationException("Assign the existing XR headset camera.");
            if (!TryMap(out DualScaleMap map)) throw new InvalidOperationException(status);
            session = new ScaleSession(map, new Pose3(new NVector(0, .3f, 0), NQuaternion.Identity),
                new InteractionBounds(new NVector(-1, .05f, -.6f), new NVector(1, 1.4f, .6f)), new NVector(.7f, .3f, .35f), .20f);
            controls = new HarnessControls(session, new HandCaptureSettings(sampleTimeout, selectionRadiusMeters: selectionRadius));
            controls.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused);
            generated = new GameObject("RoomBreakers hand harness views (synthetic layout)");
            gold = MakeMaterial(new Color(1f, .68f, .16f));
            cyan = MakeMaterial(new Color(.18f, .72f, 1f));
            green = MakeMaterial(new Color(.18f, .85f, .50f));
            roomProbe = Primitive("Room probe", PrimitiveType.Cube, gold);
            miniProbe = Primitive("Miniature probe", PrimitiveType.Cube, gold);
            miniRenderer = miniProbe.GetComponent<Renderer>();
            proxy = Primitive("Enlarged pinch-position proxy (not a hand mesh)", PrimitiveType.Sphere, cyan);
            proxy.localScale = Vector3.one * .10f;
            roomRing = CreateRing("Room return volume", session.Map.Room);
            miniRing = CreateRing("Miniature return volume", session.Map.Miniature);
            var label = new GameObject("Hands harness instructions");
            label.transform.SetParent(generated.transform, false);
            statusText = label.AddComponent<TextMesh>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (statusText.font == null) throw new InvalidOperationException("The harness font could not be loaded.");
            statusText.fontSize = 40;
            statusText.characterSize = .0035f;
            statusText.anchor = TextAnchor.MiddleCenter;
            statusText.alignment = TextAlignment.Center;
            statusText.color = Color.white;
            label.GetComponent<MeshRenderer>().sharedMaterial = statusText.font.material;
            panel = new HandsControlPanel(generated.transform, statusText.font, gold.shader);
            ready = true;
            Refresh();
        }
        private Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) throw new InvalidOperationException("Use a supported built-in or URP renderer for this harness.");
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            materials.Add(material);
            return material;
        }
        private Transform Primitive(string label, PrimitiveType type, Material material)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = label;
            obj.transform.SetParent(generated.transform, false);
            obj.GetComponent<Renderer>().sharedMaterial = material;
            obj.GetComponent<Collider>().enabled = false;
            return obj.transform;
        }
        private Transform CreateRing(string label, SpatialFrame frame)
        {
            var obj = new GameObject(label);
            obj.transform.SetParent(generated.transform, false);
            var line = obj.AddComponent<LineRenderer>();
            line.sharedMaterial = green;
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 48;
            for (int i = 0; i < 48; i++)
            {
                float a = i * 2f * Mathf.PI / 48;
                line.SetPosition(i, new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * session.ReturnRadius);
            }
            UpdateRing(obj.transform, frame);
            return obj.transform;
        }
        private void UpdateRing(Transform ring, SpatialFrame frame)
        {
            UnitySpatialFrame.ApplyPose(ring, frame.ToWorld(new Pose3(session.ReturnCenter, NQuaternion.Identity)));
            ring.localScale = Vector3.one * frame.Scale;
            var line = ring.GetComponent<LineRenderer>();
            line.startWidth = line.endWidth = .015f * frame.Scale;
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
            try
            {
                if (!FramesUnchanged()) controls.SetExternalPause(PauseReason.Placement, true);
                panel.Refresh(session.Map.Miniature, controls);
                ControlEffect effect = controls.Step(Time.realtimeSinceStartupAsDouble, Read(leftSource), Read(rightSource), panel.Targets);
                if (effect == ControlEffect.RepositionRequested) RepositionMiniature();
                session.Tick(Time.unscaledDeltaTime);
                Refresh();
            }
            catch (Exception error) { Fail(error); }
        }
        private void Refresh()
        {
            if (!ready) return;
            UnitySpatialFrame.ApplyPose(roomProbe, session.Map.RoomView(session.ObjectPose));
            UnitySpatialFrame.ApplyPose(miniProbe, session.Map.MiniatureView(session.ObjectPose));
            roomProbe.localScale = Vector3.one * .24f;
            miniProbe.localScale = Vector3.one * .24f * session.Map.Miniature.Scale;
            bool returned = session.State == ProbeState.Returned;
            roomProbe.gameObject.SetActive(!returned);
            miniProbe.gameObject.SetActive(!returned);
            bool held = session.State == ProbeState.Held;
            proxy.gameObject.SetActive(held && session.HasValidCaptureSample);
            if (held) UnitySpatialFrame.ApplyPose(proxy, session.LastHandWorld);
            miniRenderer.sharedMaterial = held || controls.Capture.HoverHand != HandId.None ? cyan : gold;
            miniRing.GetComponent<LineRenderer>().sharedMaterial = session.CanReturn ? cyan : green;
            if (controls.Prompt == ControlPrompt.None) placementError = null;
            if ((session.PauseReasons & PauseReason.FocusLost) != 0) status = "PAUSED - return to the app.";
            else if (!string.IsNullOrEmpty(placementError)) status = placementError;
            else if (controls.Prompt == ControlPrompt.Restart) status = "Choose CONFIRM or CANCEL below.";
            else if (controls.Prompt == ControlPrompt.Reposition) status = "Look forward; CONFIRM moves only the miniature.";
            else if ((session.PauseReasons & PauseReason.Placement) != 0) status = "PLACEMENT PAUSED\nFrames changed; resolve placement before resuming.";
            else if ((session.PauseReasons & PauseReason.User) != 0) status = "PAUSED - pinch RESUME below.";
            else if (controls.Menu.IsEngaged) status = "Open fingers over the token to choose.\nMove away to cancel.";
            else if (controls.Capture.RequiresOpenHand) status = "Open a tracked hand to begin or recover.";
            else if (returned) status = "Returned! Pinch RESTART to repeat.";
            else if (session.CanReturn) status = "Open your fingers to return the probe.";
            else if (held) status = "Move the small probe into its ring.";
            else status = "Pinch the small gold probe.";
            statusText.text = "ROOMBREAKERS - HAND INPUT LAB\nSYNTHETIC LAYOUT / NO ROOM SCAN\n" + status;
            statusText.transform.position = UnitySpatialFrame.Vector(session.Map.MiniatureView(
                new Pose3(new NVector(0, 1.2f, .15f), NQuaternion.Identity)).Position);
            statusText.transform.rotation = playerCamera.transform.rotation;
            panel.Refresh(session.Map.Miniature, controls);
        }

        private void RepositionMiniature()
        {
            // Fail before changing any scene pose if its parent hierarchy or room reference is invalid.
            if (!TryMap(out DualScaleMap before) || !Same(before.Room, session.Map.Room) ||
                !MiniaturePlacement.TryInFrontOf(UnitySpatialFrame.Pose(playerCamera.transform.position, playerCamera.transform.rotation),
                    session.Map.Miniature, out SpatialFrame proposed))
            {
                controls.CompleteReposition(null);
                placementError = "Move not applied. Look forward and retry,\nor cancel. Invalid room frames need setup recovery.";
                return;
            }
            Vector3 oldPosition = miniatureFrame.position;
            Quaternion oldRotation = miniatureFrame.rotation;
            Vector3 oldScale = miniatureFrame.localScale;
            float parentScale = miniatureFrame.parent == null ? 1f : miniatureFrame.parent.lossyScale.x;
            UnitySpatialFrame.ApplyPose(miniatureFrame, proposed.Origin);
            miniatureFrame.localScale = Vector3.one * (proposed.Scale / parentScale);
            bool valid = TryMap(out DualScaleMap actual) && Same(actual.Room, session.Map.Room) && Same(actual.Miniature, proposed);
            if (!valid || !controls.CompleteReposition(actual.Miniature))
            {
                miniatureFrame.SetPositionAndRotation(oldPosition, oldRotation);
                miniatureFrame.localScale = oldScale;
                if (controls.AwaitingReposition) controls.CompleteReposition(null);
                placementError = "Move not applied. Previous layout preserved.\nRetry or CANCEL.";
                return;
            }
            controls.SetExternalPause(PauseReason.Placement, false);
            UpdateRing(roomRing, session.Map.Room);
            UpdateRing(miniRing, session.Map.Miniature);
            placementError = null;
        }
        public void PauseInteraction() { controls?.SetExternalPause(PauseReason.User, true); }
        public void ResumeInteraction() { controls?.SetExternalPause(PauseReason.User, false); }
        public void RestartProbe() { controls?.ResetProbe(); }
        public void CancelInteraction() { controls?.CancelInteraction(); }
        public void ConfirmPlacement()
        {
            if (!IsReady) return;
            controls.SetExternalPause(PauseReason.Placement, true);
            if (!TryMap(out DualScaleMap map) || !controls.Recalibrate(map)) return;
            UpdateRing(roomRing, session.Map.Room);
            UpdateRing(miniRing, session.Map.Miniature);
            controls.SetExternalPause(PauseReason.Placement, false);
        }
        private void OnApplicationFocus(bool value)
        { appFocused = value; controls?.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused); }
        private void OnApplicationPause(bool value)
        { appPaused = value; controls?.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused); }
        private void OnDisable()
        {
            controls?.SetExternalPause(PauseReason.FocusLost, true);
            if (generated != null) generated.SetActive(false);
        }
        private void OnEnable()
        {
            controls?.SetExternalPause(PauseReason.FocusLost, !appFocused || appPaused);
            if (generated != null && ready) generated.SetActive(true);
        }
        private void OnDestroy()
        {
            panel?.Dispose();
            if (generated != null) Destroy(generated);
            foreach (var material in materials) if (material != null) Destroy(material);
        }
    }
}
