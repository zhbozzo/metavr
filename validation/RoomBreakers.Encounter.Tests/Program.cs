using System;
using System.Collections.Generic;
using System.Numerics;
using RoomBreakers.Core;

internal static class Program
{
    private static int passed, failed;
    private static readonly ControlTarget[] NoControls = Array.Empty<ControlTarget>();
    private static Pose3 P(float x = 0, float y = 1.2f, float z = 0) => new Pose3(new Vector3(x, y, z), Quaternion.Identity);
    private static void Assert(bool condition) { if (!condition) throw new Exception("Assertion failed"); }
    private static void Near(Vector3 a, Vector3 b) => Assert(Vector3.Distance(a, b) < .0001f);
    private static void Throws(Action action)
    {
        try { action(); } catch (ArgumentException) { return; }
        throw new Exception("Expected an argument exception");
    }
    private static void Test(string name, Action action)
    {
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + " :: " + e); }
    }
    private static RoomPlan Plan(RoomSnapshot room = null)
    {
        Assert(RoomPlanner.TryPlan(room ?? SyntheticRooms.Rectangular(), P(), out RoomPlan plan, out string error));
        Assert(error == null); return plan;
    }
    private static FirstEncounter Game(float speed = .18f) => new FirstEncounter(Plan(),
        new DualScaleMap(new SpatialFrame(Pose3.Identity, 1), new SpatialFrame(P(-1, .8f, -.5f), .2f)), speed);
    private sealed class Player
    {
        public readonly FirstEncounter Game;
        public double Now;
        private long sequence;
        public Player(FirstEncounter game) { Game = game; }
        public void Frame(Vector3 canonicalPosition, float pinch, float dt = .01f, bool tracked = true)
        {
            Now += dt;
            var sample = new HandSample(HandId.Right, ++sequence, Now,
                Game.Session.Map.MiniatureView(new Pose3(canonicalPosition, Quaternion.Identity)), pinch, tracked);
            Game.Step(Now, dt, default, sample, NoControls);
        }
        public void Open() => Frame(Game.Session.ObjectPose.Position, 0);
        public void Return()
        {
            Open(); Frame(Game.Session.ObjectPose.Position, 1);
            Assert(Game.Session.State == ProbeState.Held);
            Frame(Game.Plan.ReturnCenter, 1);
            Frame(Game.Plan.ReturnCenter, 0);
        }
    }
    private static void CheckRoute(RoomPlan p)
    {
        Assert(p.Route.Count >= 2 && p.RouteLength >= .8f);
        for (int i = 1; i < p.Route.Count; i++) Assert(p.Room.IsMotionClear(p.Route[i - 1], p.Route[i], RoomPlanner.Clearance));
        Near(p.PositionAt(0), p.ReturnCenter); Near(p.PositionAt(1000), p.Refuge);
    }
    private static int Main()
    {
        Test("floor rejects null, too few and non-finite vertices", () =>
        {
            Throws(() => new FloorPolygon(null)); Throws(() => new FloorPolygon(new Vector2[2]));
            Throws(() => new FloorPolygon(new[] { Vector2.Zero, Vector2.One, new Vector2(float.NaN, 0) }));
        });
        Test("floor rejects duplicate and crossing edges", () =>
        {
            Throws(() => new FloorPolygon(new[] { Vector2.Zero, Vector2.UnitX, Vector2.UnitX, Vector2.UnitY }));
            Throws(() => new FloorPolygon(new[] { Vector2.Zero, Vector2.One, Vector2.UnitY, Vector2.UnitX }));
        });
        Test("floor rejects excessive dimensions", () => Throws(() => new FloorPolygon(new[] { Vector2.Zero, new Vector2(20, 0), new Vector2(0, 20) })));
        Test("floor includes boundary and excludes outside", () =>
        {
            var f = SyntheticRooms.Rectangular().Floor;
            Assert(f.Contains(Vector2.Zero) && f.Contains(new Vector2(2, 0)) && !f.Contains(new Vector2(3, 0)));
        });
        Test("floor winding does not change containment", () =>
        {
            var a = new[] { Vector2.Zero, new Vector2(2, 0), new Vector2(2, 2), new Vector2(0, 2) };
            var f = new FloorPolygon(a); Array.Reverse(a); var r = new FloorPolygon(a);
            Assert(f.Contains(Vector2.One) && r.Contains(Vector2.One));
        });
        Test("room snapshots copy input collections", () =>
        {
            var vertices = new[] { Vector2.Zero, new Vector2(2, 0), new Vector2(2, 2), new Vector2(0, 2) };
            var f = new FloorPolygon(vertices); vertices[0] = new Vector2(99);
            Assert(f.Points[0] == Vector2.Zero);
        });
        Test("concave floor rejects segment across missing corner", () =>
        {
            var f = new FloorPolygon(new[] { Vector2.Zero, new Vector2(3, 0), new Vector2(3, 1), new Vector2(1, 1), new Vector2(1, 3), new Vector2(0, 3) });
            Assert(f.Contains(new Vector2(.5f, 2)) && !f.Contains(new Vector2(2, 2)));
            Assert(!f.SegmentInside(new Vector2(.5f, 2.5f), new Vector2(2.5f, .5f), .1f));
        });
        Test("clearance rejects a route grazing a wall", () => Assert(!SyntheticRooms.Rectangular().IsMotionClear(new Vector3(1.95f, .8f, 0), new Vector3(1.95f, .8f, 1))));
        Test("swept volume catches an obstacle between endpoints", () =>
        {
            var b = new RoomObstacle(new Vector3(-.2f, 0, -.2f), new Vector3(.2f, 1, .2f));
            Assert(b.IntersectsSegment(new Vector3(-2, .5f, 0), new Vector3(2, .5f, 0), .1f));
            Assert(!b.IntersectsSegment(new Vector3(-2, 2, 0), new Vector3(2, 2, 0), .1f));
        });
        Test("invalid obstacles and walls are rejected", () =>
        {
            Throws(() => new RoomObstacle(Vector3.One, Vector3.Zero));
            Throws(() => new WallSpan(Vector2.Zero, Vector2.Zero, 0, 2));
            Throws(() => new WallSpan(Vector2.Zero, Vector2.One, 2, 0));
        });
        Test("room requires walls and rejects null obstacle entries", () =>
        {
            var r = SyntheticRooms.Rectangular();
            Throws(() => new RoomSnapshot(r.Floor, Array.Empty<WallSpan>(), Array.Empty<RoomObstacle>(), true));
            Throws(() => new RoomSnapshot(r.Floor, r.Walls, new RoomObstacle[] { null }, true));
        });
        Test("empty rectangular room produces a verified route", () => CheckRoute(Plan()));
        Test("obstacle changes portal or route rather than acting as background", () =>
        {
            RoomPlan a = Plan(), b = Plan(SyntheticRooms.Rectangular(true)); CheckRoute(b);
            Assert(Vector3.Distance(a.Portal.Position, b.Portal.Position) > .01f || Math.Abs(a.RouteLength - b.RouteLength) > .01f);
        });
        Test("planner is deterministic", () =>
        {
            var a = Plan(SyntheticRooms.Rectangular(true)); var b = Plan(SyntheticRooms.Rectangular(true));
            Near(a.Portal.Position, b.Portal.Position); Assert(a.Route.Count == b.Route.Count);
            for (int i = 0; i < a.Route.Count; i++) Near(a.Route[i], b.Route[i]);
        });
        Test("planner rejects head outside room", () => Assert(!RoomPlanner.TryPlan(SyntheticRooms.Rectangular(), P(30), out _, out _)));
        Test("planner rejects vertical viewing direction", () => Assert(!RoomPlanner.TryPlan(SyntheticRooms.Rectangular(),
            new Pose3(new Vector3(0, 1.2f, 0), Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2)), out _, out _)));
        Test("full obstruction produces recoverable failure, no synthetic substitution", () =>
        {
            var r = SyntheticRooms.Rectangular(); var blocked = new RoomSnapshot(r.Floor, r.Walls,
                new[] { new RoomObstacle(new Vector3(-2, 0, -1), new Vector3(2, 3, 3)) }, false);
            Assert(!RoomPlanner.TryPlan(blocked, P(), out RoomPlan p, out string error) && p == null && !string.IsNullOrWhiteSpace(error));
        });
        Test("plan interpolation rejects invalid distance", () => { var p = Plan(); Throws(() => p.PositionAt(-1)); Throws(() => p.PositionAt(float.NaN)); });
        Test("100 synthetic layouts either reject clearly or return only unobstructed routes", () =>
        {
            var random = new Random(921); var baseRoom = SyntheticRooms.Rectangular(); int valid = 0;
            for (int i = 0; i < 100; i++)
            {
                float x = (float)random.NextDouble() * 2 - 1, z = 1.2f + (float)random.NextDouble();
                var room = new RoomSnapshot(baseRoom.Floor, baseRoom.Walls, new[] {
                    new RoomObstacle(new Vector3(x, 0, z), new Vector3(x + .3f, 1.6f, z + .3f)) }, true);
                if (RoomPlanner.TryPlan(room, P(), out RoomPlan plan, out string error)) { CheckRoute(plan); valid++; }
                else Assert(!string.IsNullOrWhiteSpace(error));
            }
            Assert(valid > 0);
        });
        Test("tutorial waits for intentional capture", () =>
        {
            var p = new Player(Game()); p.Open(); Vector3 initial = p.Game.Session.ObjectPose.Position;
            for (int i = 0; i < 100; i++) p.Open();
            Near(initial, p.Game.Session.ObjectPose.Position); Assert(p.Game.Missed == 0);
        });
        Test("first return teaches and starts defense", () =>
        {
            var p = new Player(Game()); p.Return(); Assert(p.Game.Captured == 1 && p.Game.Phase == EncounterPhase.Defending && p.Game.CreatureSerial == 2);
        });
        Test("three intentional returns produce one victory", () =>
        {
            var p = new Player(Game()); p.Return(); p.Return(); p.Return();
            Assert(p.Game.Captured == 3 && p.Game.Phase == EncounterPhase.Won && !p.Game.CreatureVisible);
            for (int i = 0; i < 10; i++) p.Open(); Assert(p.Game.Captured == 3);
        });
        Test("three refuge hits produce defeat without rewards", () =>
        {
            var p = new Player(Game(1)); p.Return();
            for (int i = 0; i < 1500 && p.Game.Phase != EncounterPhase.Lost; i++) p.Frame(p.Game.Session.ObjectPose.Position, 0, .05f);
            Assert(p.Game.Phase == EncounterPhase.Lost && p.Game.Integrity == 0 && p.Game.Captured == 1);
        });
        Test("pause freezes creature, health and elapsed time", () =>
        {
            var p = new Player(Game()); p.Return(); p.Open();
            p.Game.Controls.SetExternalPause(PauseReason.User, true);
            Vector3 position = p.Game.Session.ObjectPose.Position; double time = p.Game.ActiveSeconds;
            for (int i = 0; i < 40; i++) p.Open();
            Near(position, p.Game.Session.ObjectPose.Position); Assert(time == p.Game.ActiveSeconds && p.Game.Integrity == 3);
        });
        Test("tracking loss cancels capture and prevents damage", () =>
        {
            var p = new Player(Game()); p.Open(); p.Frame(p.Game.Session.ObjectPose.Position, 1);
            p.Frame(p.Game.Session.ObjectPose.Position, 1, .01f, false);
            Assert(p.Game.Session.Owner == HandId.None && p.Game.Session.IsPaused && p.Game.Captured == 0);
        });
        Test("moving creature stops while held", () =>
        {
            var p = new Player(Game()); p.Return(); p.Open(); p.Frame(p.Game.Session.ObjectPose.Position, 1);
            float distance = p.Game.DistanceTravelled;
            for (int i = 0; i < 50; i++) p.Frame(p.Game.Session.ObjectPose.Position, 1);
            Assert(distance == p.Game.DistanceTravelled);
        });
        Test("invalid release restores captured position without a reward", () =>
        {
            var p = new Player(Game()); p.Open(); Vector3 start = p.Game.Session.ObjectPose.Position;
            p.Frame(start, 1); p.Frame(new Vector3(50, .8f, 50), 0);
            Near(start, p.Game.Session.ObjectPose.Position); Assert(p.Game.Captured == 0);
        });
        Test("restart after victory restores tutorial and preserves user pause", () =>
        {
            var p = new Player(Game()); p.Return(); p.Return(); p.Return();
            p.Game.Controls.SetExternalPause(PauseReason.User, true); p.Game.Controls.ResetProbe(); p.Open();
            Assert(p.Game.Phase == EncounterPhase.LearnCapture && p.Game.Captured == 0 && p.Game.Integrity == 3);
            Assert((p.Game.Session.PauseReasons & PauseReason.User) != 0);
        });
        Test("room invalidation cannot be cleared by restarting or resuming", () =>
        {
            var p = new Player(Game()); p.Open(); p.Game.InvalidateRoom(); p.Game.Controls.ResetProbe(); p.Open();
            p.Game.Controls.SetExternalPause(PauseReason.User, false); p.Open();
            Assert(p.Game.Phase == EncounterPhase.RoomInvalid && p.Game.Session.IsPaused && !p.Game.CreatureVisible);
        });
        Test("long frame clamps movement instead of catching up", () =>
        {
            var p = new Player(Game()); p.Return(); p.Open(); float before = p.Game.DistanceTravelled;
            p.Frame(p.Game.Session.ObjectPose.Position, 0, 30);
            Assert(p.Game.DistanceTravelled - before <= .019f);
        });
        Test("invalid dt is rejected before input mutates the state", () =>
        {
            var game = Game(); Throws(() => game.Step(0, float.NaN, default, default, NoControls)); Assert(game.Captured == 0);
        });
        Test("autonomous movement is forbidden while paused or held", () =>
        {
            var p = new Player(Game()); Assert(!p.Game.Session.TryAdvance(P())); p.Open();
            p.Frame(p.Game.Session.ObjectPose.Position, 1); Assert(!p.Game.Session.TryAdvance(P()));
        });
        Test("motion constraint blocks a carry through furniture and stale return", () =>
        {
            var box = new RoomObstacle(new Vector3(.4f, 0, -.2f), new Vector3(.6f, 2, .2f));
            var s = new ScaleSession(new DualScaleMap(new SpatialFrame(Pose3.Identity, 1), new SpatialFrame(Pose3.Identity, .2f)),
                P(0, .8f), new InteractionBounds(new Vector3(-2, 0, -2), new Vector3(2, 3, 2)), new Vector3(1, .8f, 0), .2f,
                (a, b) => !box.IntersectsSegment(a, b, .1f));
            Assert(s.BeginCapture(HandId.Left, s.Map.MiniatureView(s.ObjectPose)));
            Assert(!s.MoveCapture(HandId.Left, s.Map.MiniatureView(P(1, .8f)))); Assert(!s.ReleaseCapture(HandId.Left)); Assert(s.ReturnCount == 0);
        });
        Test("constructor rejects a spawn inside an obstacle", () => Throws(() => new ScaleSession(
            new DualScaleMap(new SpatialFrame(Pose3.Identity, 1), new SpatialFrame(Pose3.Identity, .2f)), P(),
            new InteractionBounds(new Vector3(-3), new Vector3(3)), Vector3.One, .2f, (a, b) => false)));
        Test("invalid encounter speed is rejected", () => { Throws(() => Game(float.NaN)); Throws(() => Game(0)); });
        Console.WriteLine($"ENCOUNTER: {passed} passed, {failed} failed. Actual C#; synthetic rooms/input; no Unity/Meta/device tests.");
        return failed == 0 ? 0 : 1;
    }
}
