using System;
using RoomBreakers.Core;
using UnityEngine;
using NVector = System.Numerics.Vector3;
using NQuaternion = System.Numerics.Quaternion;

namespace RoomBreakers.UnityInput
{
    // Existing camera/hand rig required. Never configures providers or passthrough implicitly.
    public sealed class FirstEncounterRig : MonoBehaviour
    {
        [SerializeField] private RoomSource roomSource;
        [SerializeField] private HandSampleSource leftSource, rightSource;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Shader prototypeShader;
        [SerializeField, Tooltip("Disable only for the original three-Mote practice encounter.")]
        private bool includeShell = true;
        private FirstEncounter game;
        private FirstEncounterView view;
        private EncounterExperience experience;
        private ProgressJournal practiceProgress, deviceProgress;
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
        { roomSource = room; leftSource = left; rightSource = right; playerCamera = camera; prototypeShader = shader; }
        private void Start()
        {
            try
            {
                if (roomSource == null || playerCamera == null || (leftSource == null && rightSource == null))
                    throw new InvalidOperationException("Assign a room source, a camera and at least one explicit hand source.");
                if (prototypeShader == null) throw new InvalidOperationException("Assign a serialized unlit shader; do not rely on a stripped runtime Shader.Find.");
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font == null) throw new InvalidOperationException("Prototype font unavailable.");
                visuals = new GameObject("ROOMBREAKERS encounter runtime");
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
            if (uiFrame == null)
            {
                PlaceBoot(); boot.SetActive(uiFrame != null);
                if (uiFrame == null) return;
            }
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
                    plannedRevision = roomSource.Revision; TryStartEncounter();
                    if (game != null) return;
                }
                bootText.text = (planningError ?? roomSource.Status) + "\n\nLOAD ROOM\nPinch the blue token, then open.";
                loadTarget[0] = new ControlTarget(ControlAction.Confirm, UnitySpatialFrame.Pose(loadToken.position, Quaternion.identity).Position, .06f);
                ControlAction allowed = focused && !suspended && roomSource.State != RoomSourceState.Loading ? ControlAction.Confirm : ControlAction.None;
                if (bootInput.Step(now, left, right, loadTarget, allowed, HandId.None) == ControlAction.Confirm) LoadRoom();
                return;
            }
            // Build input targets once; render/audio/feedback only once after the authoritative step.
            view.Panel.Refresh(uiFrame, game.Controls);
            ControlEffect effect = game.Step(now, Time.unscaledDeltaTime, left, right, view.Panel.Targets);
            if (effect == ControlEffect.RepositionRequested) Reposition();
            experience.Observe();
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
                if (includeShell && !ReflectionLane.TryCreate(plan, out _, out planningError)) return;
                if (!MiniaturePlacement.TryInFrontOf(headWorld, new SpatialFrame(Pose3.Identity, 1), out SpatialFrame table))
                { planningError = "Look forward and choose LOAD ROOM again."; return; }
                var min = plan.Room.Floor.Min; var max = plan.Room.Floor.Max;
                canonicalCenter = new NVector((min.X + max.X) * .5f, plan.ReturnCenter.Y, (min.Y + max.Y) * .5f);
                float scale = Mathf.Clamp(.60f / Math.Max(max.X - min.X, max.Y - min.Y), .04f, .25f);
                SpatialFrame miniature = CenteredFrame(table, scale);
                game = new FirstEncounter(plan, new DualScaleMap(roomSource.WorldFrame, miniature), includeShell: includeShell);
                game.Controls.SetExternalPause(PauseReason.FocusLost, !focused || suspended);
                acceptedRevision = roomSource.Revision; uiFrame = table;
                // Debug/synthetic/basic practice never unlocks release-device achievements.
                bool practice = Application.isEditor || Debug.isDebugBuild || plan.Room.IsSynthetic || !includeShell;
                experience = new EncounterExperience(game, GetProgress(practice), practice);
                experience.Observe();
                view = new FirstEncounterView(visuals.transform, game, font, prototypeShader, experience);
                CreateReturnZones(); boot.SetActive(false); view.Refresh(game, playerCamera, uiFrame);
            }
            catch (Exception e)
            {
                ClearEncounter(); planningError = "Encounter setup failed. Check the local Unity console before retrying.";
                Debug.LogException(e, this);
            }
        }
        private ProgressJournal GetProgress(bool practice)
        {
            ProgressJournal existing = practice ? practiceProgress : deviceProgress;
            if (existing != null) return existing;
            ProgressJournal journal;
            try
            {
                string path = Application.persistentDataPath;
                journal = string.IsNullOrWhiteSpace(path) ? ProgressJournal.InMemory() :
                    new ProgressJournal(System.IO.Path.Combine(path, "room-breakers", practice ? "practice-v1" : "device-v1"));
            }
            catch (Exception e) when (e is ArgumentException || e is System.IO.IOException || e is UnauthorizedAccessException ||
                e is System.Security.SecurityException || e is NotSupportedException)
            { journal = ProgressJournal.InMemory(); } // No machine paths or private data in logs.
            if (practice) practiceProgress = journal; else deviceProgress = journal;
            return journal;
        }
        private void FlushProgress()
        {
            // Retry pending writes only at explicit lifecycle boundaries, not on every frame.
            if (practiceProgress != null && practiceProgress.Dirty) practiceProgress.Flush();
            if (deviceProgress != null && deviceProgress.Dirty) deviceProgress.Flush();
        }
        private SpatialFrame CenteredFrame(SpatialFrame table, float scale)
        {
            NQuaternion rotation = roomSource.WorldFrame.Origin.Rotation;
            return new SpatialFrame(new Pose3(table.Origin.Position - NVector.Transform(canonicalCenter * scale, rotation), rotation), scale);
        }
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
                var zone = new GameObject("Visible return zone " + v).transform; zone.SetParent(visuals.transform, false); returnZones[v] = zone;
                for (int plane = 0; plane < 2; plane++)
                {
                    var line = new GameObject("Return circle").AddComponent<LineRenderer>(); line.transform.SetParent(zone, false);
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
                returnZones[i].gameObject.SetActive(game.ShowReturnZone);
            }
        }
        public bool TryPickDesktop(Ray ray, out Vector3 point, out bool menu)
        {
            point = Vector3.zero; menu = false; float best = float.MaxValue;
            if (game == null && loadToken != null && uiFrame != null) Pick(ray, loadToken.position, .06f, true, ref best, ref point, ref menu);
            else if (view != null)
            {
                foreach (ControlTarget target in view.Panel.Targets)
                    Pick(ray, UnitySpatialFrame.Vector(target.Center), target.Radius, true, ref best, ref point, ref menu);
                if (game.CreatureVisible)
                    Pick(ray, UnitySpatialFrame.Vector(game.Session.Map.MiniatureView(game.Session.ObjectPose).Position), .04f, false, ref best, ref point, ref menu);
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
            FlushProgress(); view?.Dispose(); view = null; experience = null; game = null;
            foreach (Transform zone in returnZones) if (zone != null) Destroy(zone.gameObject);
        }
        private void OnApplicationFocus(bool value)
        { focused = value; game?.Controls.SetExternalPause(PauseReason.FocusLost, !focused || suspended); if (!value) { bootInput.Cancel(); FlushProgress(); } }
        private void OnApplicationPause(bool value)
        { suspended = value; game?.Controls.SetExternalPause(PauseReason.FocusLost, !focused || suspended); if (value) { bootInput.Cancel(); FlushProgress(); } }
        private void OnDisable()
        { game?.Controls.SetExternalPause(PauseReason.FocusLost, true); bootInput.Cancel(); FlushProgress(); if (visuals != null) visuals.SetActive(false); }
        private void OnEnable()
        { game?.Controls.SetExternalPause(PauseReason.FocusLost, !focused || suspended); if (visuals != null) visuals.SetActive(true); }
        private void OnDestroy() { ClearEncounter(); if (visuals != null) Destroy(visuals); if (bootMaterial != null) Destroy(bootMaterial); }
    }
}
