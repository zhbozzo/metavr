using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meta.XR.MRUtilityKit;
using RoomBreakers.Core;
using RoomBreakers.UnityInput;
using UnityEngine;
using UnityEngine.Android;
using NVector = System.Numerics.Vector3;
using NVector2 = System.Numerics.Vector2;
using NQuaternion = System.Numerics.Quaternion;

namespace RoomBreakers.MetaRoom
{
    // Reviewed against MRUK v207 docs; still requires an actual SDK import/device test.
    // No raw camera frames, UUID logging, room JSON persistence or prefab fallback.
    public sealed class MetaRoomSource : RoomSource
    {
        [SerializeField] private MRUK manager;
        [SerializeField] private Camera playerCamera;
        private MRUKRoom loadedRoom;
        private int request;
        private bool subscribed;
        private double nextCheck;
        private readonly List<Transform> anchors = new List<Transform>();
        private readonly List<Vector3> anchorPositions = new List<Vector3>();
        private readonly List<Quaternion> anchorRotations = new List<Quaternion>();
        public void Configure(MRUK mruk, Camera camera) { manager = mruk; playerCamera = camera; }
        private void Subscribe()
        {
            if (subscribed) return;
            manager.RoomUpdatedEvent.AddListener(RoomChanged);
            manager.RoomRemovedEvent.AddListener(RoomChanged);
            subscribed = true;
        }
        private void RoomChanged(MRUKRoom room)
        {
            if (State == RoomSourceState.Ready && room == loadedRoom) Invalidate("Room changed. Load it again to plan a new encounter.");
        }
        public override async void Load()
        {
            if (!isActiveAndEnabled || State == RoomSourceState.Loading) return;
            int generation = ++request;
            Snapshot = null; WorldFrame = null; Revision++; loadedRoom = null;
            State = RoomSourceState.Loading; Status = "Checking spatial-data permission...";
            try
            {
                if (manager == null || playerCamera == null || !manager.isActiveAndEnabled)
                    throw new InvalidOperationException("Assign an active MRUK and the existing XR camera.");
                if (!manager.EnableWorldLock || manager.SceneSettings.DataSource != MRUK.SceneDataSource.Device ||
                    manager.SceneSettings.LoadSceneOnStartup)
                    throw new InvalidOperationException("Use Device only, world lock enabled and Load Scene On Startup disabled.");
                Subscribe();
#if UNITY_ANDROID && !UNITY_EDITOR
                string permission = OVRPermissionsRequester.ScenePermission;
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    var completion = new TaskCompletionSource<bool>();
                    var callbacks = new PermissionCallbacks();
                    callbacks.PermissionGranted += _ => completion.TrySetResult(true);
                    callbacks.PermissionDenied += _ => completion.TrySetResult(false);
                    Permission.RequestUserPermission(permission, callbacks);
                    await Task.WhenAny(completion.Task, Task.Delay(30000));
                    if (generation != request || this == null || !isActiveAndEnabled) return;
                    if (!Permission.HasUserAuthorizedPermission(permission))
                    {
                        State = RoomSourceState.PermissionRequired;
                        Status = "Spatial data was not granted. Enable it in app settings, then choose LOAD ROOM.";
                        return;
                    }
                }
#endif
                Status = "Loading the room stored on this device...";
                Task<MRUK.LoadDeviceResult> loading = manager.LoadSceneFromDevice(false, true);
                if (await Task.WhenAny(loading, Task.Delay(20000)) != loading)
                {
                    // The native discovery is not cancellable here; observe a later exception without using its data.
                    _ = Observe(loading);
                    if (generation == request) { State = RoomSourceState.Unavailable; Status = "Room load timed out. Wait before trying again."; }
                    return;
                }
                MRUK.LoadDeviceResult result = await loading;
                if (generation != request || this == null || !isActiveAndEnabled) return;
                if (result != MRUK.LoadDeviceResult.Success)
                {
                    State = result == MRUK.LoadDeviceResult.NoScenePermission ? RoomSourceState.PermissionRequired : RoomSourceState.Unavailable;
                    Status = result == MRUK.LoadDeviceResult.NoRoomsFound
                        ? "No stored room. Complete Space Setup in Quest settings, then choose LOAD ROOM."
                        : "Device room is not available. Check spatial permission and Space Setup, then retry.";
                    return;
                }
                loadedRoom = manager.GetCurrentRoom();
                if (loadedRoom == null || !loadedRoom.IsLocal || !manager.IsWorldLockActive ||
                    !loadedRoom.IsPositionInRoom(playerCamera.transform.position, false))
                    throw new InvalidOperationException("A localized device room containing the player is required.");
                BuildSnapshot();
                Revision++; State = RoomSourceState.Ready; Status = "Device room loaded. No room data leaves this device.";
            }
            catch (Exception e)
            {
                if (generation != request || this == null || !isActiveAndEnabled) return;
                Snapshot = null; WorldFrame = null; State = RoomSourceState.Failed;
                Status = "Room could not be prepared. Check MRUK setup and supported geometry; retry from LOAD ROOM.";
                Debug.LogError("ROOMBREAKERS room preparation failed (" + e.GetType().Name + "). No room identifiers or geometry logged.", this);
            }
        }
        private static async Task Observe(Task task) { try { await task; } catch (Exception) { /* Timed-out result is deliberately discarded. */ } }
        private NVector Canonical(Vector3 p) => WorldFrame.ToLocal(UnitySpatialFrame.Pose(p, Quaternion.identity)).Position;
        private void BuildSnapshot()
        {
            if (loadedRoom.FloorAnchors == null || loadedRoom.FloorAnchors.Count != 1)
                throw new ArgumentException("This slice supports one horizontal floor, not multi-level rooms.");
            MRUKAnchor floor = loadedRoom.FloorAnchors[0];
            if (floor == null || floor.PlaneBoundary2D == null || floor.PlaneBoundary2D.Count < 3 ||
                Mathf.Abs(Vector3.Dot(floor.transform.forward, Vector3.up)) < .98f)
                throw new ArgumentException("A horizontal floor boundary is required.");
            WorldFrame = new SpatialFrame(UnitySpatialFrame.Pose(floor.transform.position, Quaternion.identity), 1);
            var polygon = new List<NVector2>();
            foreach (Vector2 p in floor.PlaneBoundary2D)
            {
                NVector local = Canonical(floor.transform.TransformPoint(new Vector3(p.x, p.y, 0)));
                if (Math.Abs(local.Y) > .03f) throw new ArgumentException("Sloped floor is unsupported.");
                if (polygon.Count == 0 || NVector2.DistanceSquared(polygon[polygon.Count - 1], RoomGeometry.XZ(local)) > 1e-8f)
                    polygon.Add(RoomGeometry.XZ(local));
            }
            if (polygon.Count > 1 && NVector2.DistanceSquared(polygon[0], polygon[polygon.Count - 1]) < 1e-8f) polygon.RemoveAt(polygon.Count - 1);
            var walls = new List<WallSpan>(); var obstacles = new List<RoomObstacle>();
            anchors.Clear(); anchorPositions.Clear(); anchorRotations.Clear();
            foreach (MRUKAnchor anchor in loadedRoom.Anchors)
            {
                if (anchor == null) continue;
                if (!UnitySpatialFrame.TryRead(anchor.transform, true, out _, out _)) throw new ArgumentException("Anchor hierarchy must have unit, non-mirrored scale.");
                anchors.Add(anchor.transform); anchorPositions.Add(anchor.transform.position); anchorRotations.Add(anchor.transform.rotation);
                bool wall = anchor.HasAnyLabel(MRUKAnchor.SceneLabels.WALL_FACE | MRUKAnchor.SceneLabels.INNER_WALL_FACE);
                if (wall)
                {
                    if (!anchor.PlaneRect.HasValue || Mathf.Abs(Vector3.Dot(anchor.transform.up, Vector3.up)) < .98f)
                        throw new ArgumentException("Non-vertical wall is unsupported.");
                    Rect rect = anchor.PlaneRect.Value;
                    if (rect.width < .1f || rect.height < .1f) throw new ArgumentException("Degenerate wall.");
                    // Do not extrapolate a rectangular portal area outside an irregular wall boundary.
                    foreach (Vector2 p in RectCorners(rect, .01f))
                        if (!anchor.IsPositionInBoundary(p)) throw new ArgumentException("Irregular wall boundary needs a richer adapter.");
                    NVector a = Canonical(anchor.transform.TransformPoint(new Vector3(rect.xMin, rect.yMin, 0)));
                    NVector b = Canonical(anchor.transform.TransformPoint(new Vector3(rect.xMax, rect.yMin, 0)));
                    NVector top = Canonical(anchor.transform.TransformPoint(new Vector3(rect.xMin, rect.yMax, 0)));
                    walls.Add(new WallSpan(RoomGeometry.XZ(a), RoomGeometry.XZ(b), Math.Min(a.Y, top.Y), Math.Max(a.Y, top.Y)));
                }
                else if (!anchor.HasAnyLabel(MRUKAnchor.SceneLabels.FLOOR | MRUKAnchor.SceneLabels.CEILING | MRUKAnchor.SceneLabels.GLOBAL_MESH))
                {
                    if (anchor.VolumeBounds.HasValue) obstacles.Add(Box(anchor.transform, anchor.VolumeBounds.Value));
                    else if (anchor.PlaneRect.HasValue)
                    {
                        Rect r = anchor.PlaneRect.Value;
                        // Includes doors, windows, screens and wall art: no portal placed over them.
                        obstacles.Add(Box(anchor.transform, new Bounds(new Vector3(r.center.x, r.center.y, 0), new Vector3(r.width, r.height, .06f))));
                    }
                    else throw new ArgumentException("Unrepresented scene object cannot be silently ignored.");
                }
            }
            Snapshot = new RoomSnapshot(new FloorPolygon(polygon), walls, obstacles, false);
        }
        private static Vector2[] RectCorners(Rect r, float inset) => new[] {
            new Vector2(r.xMin + inset, r.yMin + inset), new Vector2(r.xMax - inset, r.yMin + inset),
            new Vector2(r.xMax - inset, r.yMax - inset), new Vector2(r.xMin + inset, r.yMax - inset) };
        private RoomObstacle Box(Transform t, Bounds bounds)
        {
            NVector min = new NVector(float.MaxValue), max = new NVector(float.MinValue);
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = bounds.center + Vector3.Scale(bounds.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                NVector c = Canonical(t.TransformPoint(p)); min = NVector.Min(min, c); max = NVector.Max(max, c);
            }
            return new RoomObstacle(min, max);
        }
        private void Update()
        {
            if (State != RoomSourceState.Ready || Time.realtimeSinceStartupAsDouble < nextCheck) return;
            nextCheck = Time.realtimeSinceStartupAsDouble + .1;
            if (loadedRoom == null || manager == null || !manager.IsWorldLockActive ||
                manager.GetCurrentRoom() != loadedRoom || !loadedRoom.IsPositionInRoom(playerCamera.transform.position, false))
            { Invalidate("Room localization changed. Load the room again before playing."); return; }
            for (int i = 0; i < anchors.Count; i++)
                if (anchors[i] == null || Vector3.Distance(anchors[i].position, anchorPositions[i]) > .02f ||
                    Quaternion.Angle(anchors[i].rotation, anchorRotations[i]) > .5f)
                { Invalidate("Anchor geometry moved. Load the room again."); return; }
        }
        private void OnDisable()
        {
            request++;
            if (manager != null && subscribed)
            {
                manager.RoomUpdatedEvent.RemoveListener(RoomChanged); manager.RoomRemovedEvent.RemoveListener(RoomChanged);
            }
            subscribed = false; Invalidate("Room source stopped. Load the room again.");
        }
    }
}
