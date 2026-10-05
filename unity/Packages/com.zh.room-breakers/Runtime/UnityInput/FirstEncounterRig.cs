using System;
using RoomBreakers.Core;
using UnityEngine;
using NVector = System.Numerics.Vector3;
using NQuaternion = System.Numerics.Quaternion;

namespace RoomBreakers.UnityInput
{
    // Uses an existing camera/hand rig. Does not configure XR providers or passthrough implicitly.
    public sealed class FirstEncounterRig : MonoBehaviour
    {
        [SerializeField] private RoomSource roomSource;
        [SerializeField] private HandSampleSource leftSource, rightSource;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Shader prototypeShader;
        private FirstEncounter game;
        private FirstEncounterView view;
        private GameObject visuals, boot;
        private Transform loadToken, bootLabel;
        private TextMesh bootText;
        private Material bootMaterial;
        private Font font;
        private SpatialFrame uiFrame;
        private NVector canonicalCenter;
        private long plannedRevision = -1, acceptedRevision = -1;
        private readonly NearControlDriver bootInput = new NearControlDriver();
        private readonly ControlTarget[] loadTarget = new ControlTarget[1];
        private readonly Transform[] returnZones = new Transform[2];
        private bool focused = true, suspended;
        private string planningError;
        public FirstEncounter Game => game;
        public Camera ViewCamera => playerCamera;
        public SpatialFrame UiFrame => uiFrame;
        public void Configure(RoomSource room, HandSampleSource left, HandSampleSource right, Camera camera, Shader shader)
        {
            roomSource = room; leftSource = left; rightSource = right; playerCamera = camera; prototypeShader = shader;
        }
        private void Start()
        {
            try
            {
                if (roomSource == null || playerCamera == null || (leftSource == null && rightSource == null))
                    throw new InvalidOperationException("Assign a room source, a camera and at least one explicit hand source.");
                if (prototypeShader == null) throw new InvalidOperationException("Assign a serialized unlit shader; do not rely on a stripped runtime Shader.Find.");
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font == null) throw new InvalidOperationException("Prototype font unavailable.");
                visuals = new GameObject("ROOMBREAKERS first encounter runtime"); // Deliberate world-space identity root.
                boot = new GameObject("Room loading controls"); boot.transform.SetParent(visuals.transform, false);
                loadToken = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform; loadToken.SetParent(boot.transform, false);
                loadToken.localScale = new Vector3(.12f, .07f, .035f); loadToken.GetComponent<Collider>().enabled = false;
                bootMaterial = new Material(prototypeShader);
                if (bootMaterial.HasProperty("_BaseColor")) bootMaterial.SetColor("_BaseColor", new Color(.25f, .55f, .8f));
                if (bootMaterial.HasProperty("_Color")) bootMaterial.SetColor("_Color", new Color(.25f, .55f, .8f));
                loadToken.GetComponent<Renderer>().sharedMaterial = bootMaterial;
                bootLabel = new GameObject("Room status").transform; bootLabel.SetParent(boot.transform, false);
                bootText = bootLabel.gameObject.AddComponent<TextMesh>(); bootText.font = font;
                bootText.fontSize = 40; bootText.characterSize = .0028f; bootText.anchor = TextAnchor.MiddleCenter;
                bootText.alignment = TextAlignment.Center; bootLabel.GetComponent<MeshRenderer>().sharedMaterial = font.material;
                PlaceBoot();
            }
            catch (Exception e) { Debug.LogException(e, this); enabled = false; }
        }
        private void PlaceBoot()
        {
            Pose3 head = UnitySpatialFrame.Pose(playerCamera.transform.position, playerCamera.transform.rotation);
            if (!MiniaturePlacement.TryInFrontOf(head, new SpatialFrame(Pose3.Identity, 1), out SpatialFrame placed)) return;
            uiFrame = placed; UnitySpatialFrame.ApplyPose(boot.transform, placed.Origin);
            loadToken.localPosition = Vector3.zero; bootLabel.localPosition = new Vector3(0, .11f, -.02f);
        }
        private static HandSample Read(HandSampleSource source) => source != null && source.TryGetLatest(out HandSample sample) ? sample : default;
        private void LateUpdate()
        {
            if (visuals == null || !isActiveAndEnabled) return;
            HandSample left = Read(leftSource), right = Read(rightSource); double now = Time.realtimeSinceStartupAsDouble;
            if (game != null && (roomSource.State != RoomSourceState.Ready || acceptedRevision != roomSource.Revision))
            {
                game.InvalidateRoom(); ClearEncounter(); planningError = "Room changed. Load it again for a new encounter."; PlaceBoot();
            }
            if (game == null)
            {
                boot.SetActive(true);
                if (roomSource.State == RoomSourceState.Ready && plannedRevision != roomSource.Revision)
                {
                    plannedRevision = roomSource.Revision;
                    TryStartEncounter();
                    if (game != null) return;
                }
                bootText.text = (planningError ?? roomSource.Status) + "\n\nLOAD ROOM\nPinch the blue token, then open.";
                loadTarget[0] = new ControlTarget(ControlAction.Confirm, UnitySpatialFrame.Pose(loadToken.position, Quaternion.identity).Position, .06f);
                ControlAction allowed = focused && !suspended && roomSource.State != RoomSourceState.Loading ? ControlAction.Confirm : ControlAction.None;
                if (bootInput.Step(now, left, right, loadTarget, allowed, HandId.None) == ControlAction.Confirm) LoadRoom();
                return;
            }
            view.Refresh(game, playerCamera, uiFrame);
            ControlEffect effect = game.Step(now, Time.unscaledDeltaTime, left, right, view.Panel.Targets);
            if (effect == ControlEffect.RepositionRequested) Reposition();
            view.Refresh(game, playerCamera, uiFrame); RefreshReturnZones();
        }
        public void LoadRoom()
        {
            if (roomSource == null || roomSource.State == RoomSourceState.Loading) return;
            ClearEncounter(); planningError = null; plannedRevision = -1; bootInput.Cancel(); roomSource.Load();
        }
        private void TryStartEncounter()
        {
            try
            {
                Pose3 headWorld = UnitySpatialFrame.Pose(playerCamera.transform.position, playerCamera.transform.rotation);
                Pose3 canonicalHead = roomSource.WorldFrame.ToLocal(headWorld);
                if (!RoomPlanner.TryPlan(roomSource.Snapshot, canonicalHead, out RoomPlan plan, out planningError)) return;
                if (!MiniaturePlacement.TryInFrontOf(headWorld, new SpatialFrame(Pose3.Identity, 1), out SpatialFrame table))
                { planningError = "Look forward and choose LOAD ROOM again."; return; }
                var min = plan.Room.Floor.Min; var max = plan.Room.Floor.Max;
                canonicalCenter = new NVector((min.X + max.X) * .5f, plan.ReturnCenter.Y, (min.Y + max.Y) * .5f);
                float scale = Mathf.Clamp(.60f / Math.Max(max.X - min.X, max.Y - min.Y), .04f, .25f);
                SpatialFrame miniature = CenteredFrame(table, scale);
                game = new FirstEncounter(plan, new DualScaleMap(roomSource.WorldFrame, miniature));
                game.Controls.SetExternalPause(PauseReason.FocusLost, !focused || suspended);
                acceptedRevision = roomSource.Revision; uiFrame = table;
                view = new FirstEncounterView(visuals.transform, game, font, prototypeShader);
                CreateReturnZones(); boot.SetActive(false); view.Refresh(game, playerCamera, uiFrame);
            }
            catch (Exception e)
            {
                ClearEncounter(); planningError = "Encounter setup failed. Check the local Unity console before retrying.";
                Debug.LogException(e, this);
            }
        }
        private SpatialFrame CenteredFrame(SpatialFrame table, float scale) => new SpatialFrame(new Pose3(
            table.Origin.Position - NVector.Transform(canonicalCenter * scale, table.Origin.Rotation), table.Origin.Rotation), scale);
        private void Reposition()
        {
            Pose3 head = UnitySpatialFrame.Pose(playerCamera.transform.position, playerCamera.transform.rotation);
            if (!MiniaturePlacement.TryInFrontOf(head, uiFrame, out SpatialFrame table)) { game.Controls.CompleteReposition(null); return; }
            if (game.Controls.CompleteReposition(CenteredFrame(table, game.Session.Map.Miniature.Scale))) uiFrame = table;
        }
        private void CreateReturnZones()
        {
            for (int v = 0; v < 2; v++)
            {
                var root = new GameObject("Visible return zone " + v).transform; root.SetParent(visuals.transform, false); returnZones[v] = root;
                for (int plane = 0; plane < 2; plane++)
                {
                    var line = new GameObject("Return circle").AddComponent<LineRenderer>(); line.transform.SetParent(root, false);
                    line.sharedMaterial = bootMaterial; line.useWorldSpace = false; line.loop = true; line.positionCount = 48;
                    line.startWidth = line.endWidth = .012f;
                    for (int i = 0; i < 48; i++)
                    {
                        float a = i * Mathf.PI * 2 / 48; float x = Mathf.Cos(a) * game.Session.ReturnRadius, y = Mathf.Sin(a) * game.Session.ReturnRadius;
                        line.SetPosition(i, plane == 0 ? new Vector3(x, y, 0) : new Vector3(x, 0, y));
                    }
                }
            }
            RefreshReturnZones();
        }
        private void RefreshReturnZones()
        {
            for (int i = 0; i < 2; i++)
            {
                SpatialFrame frame = i == 0 ? game.Session.Map.Room : game.Session.Map.Miniature;
                UnitySpatialFrame.ApplyPose(returnZones[i], frame.ToWorld(new Pose3(game.Plan.ReturnCenter, NQuaternion.Identity)));
                returnZones[i].localScale = Vector3.one * frame.Scale;
                returnZones[i].gameObject.SetActive(game.CreatureVisible);
            }
        }
        // Desktop-only adapter uses the same target coordinates and hand driver. No fake Meta data.
        public bool TryPickDesktop(Ray ray, out Vector3 point, out bool menu)
        {
            point = Vector3.zero; menu = false; float best = float.MaxValue;
            if (game == null && loadToken != null) Pick(ray, loadToken.position, .06f, true, ref best, ref point, ref menu);
            else if (view != null)
            {
                foreach (ControlTarget target in view.Panel.Targets)
                    Pick(ray, UnitySpatialFrame.Vector(target.Center), target.Radius, true, ref best, ref point, ref menu);
                if (game.CreatureVisible) Pick(ray, view.MiniatureMotePosition, .04f, false, ref best, ref point, ref menu);
            }
            return best < float.MaxValue;
        }
        private static void Pick(Ray ray, Vector3 center, float radius, bool isMenu, ref float best, ref Vector3 point, ref bool menu)
        {
            float t = Vector3.Dot(center - ray.origin, ray.direction);
            if (t > 0 && t < best && Vector3.Distance(ray.GetPoint(t), center) <= radius)
            { best = t; point = center; menu = isMenu; }
        }
        private void ClearEncounter()
        {
            view?.Dispose(); view = null; game = null;
            foreach (Transform zone in returnZones) if (zone != null) Destroy(zone.gameObject);
        }
        private void OnApplicationFocus(bool value) { focused = value; game?.Controls.SetExternalPause(PauseReason.FocusLost, !focused || suspended); if (!value) bootInput.Cancel(); }
        private void OnApplicationPause(bool value) { suspended = value; game?.Controls.SetExternalPause(PauseReason.FocusLost, !focused || suspended); if (value) bootInput.Cancel(); }
        private void OnDisable() { game?.Controls.SetExternalPause(PauseReason.FocusLost, true); bootInput.Cancel(); if (visuals != null) visuals.SetActive(false); }
        private void OnEnable() { game?.Controls.SetExternalPause(PauseReason.FocusLost, !focused || suspended); if (visuals != null) visuals.SetActive(true); }
        private void OnDestroy() { ClearEncounter(); if (visuals != null) Destroy(visuals); if (bootMaterial != null) Destroy(bootMaterial); }
    }
}
