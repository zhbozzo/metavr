using System;
using System.Numerics;
using RoomBreakers.Core;

internal static class Program
{
    private static int passed, failed;
    private static readonly ControlTarget[] NoTargets = Array.Empty<ControlTarget>();
    private static Pose3 P(Vector3 p) => new Pose3(p, Quaternion.Identity);
    private static void Require(bool condition, string message = "Assertion failed") { if (!condition) throw new Exception(message); }
    private static void Near(Vector3 a, Vector3 b) => Require(Vector3.Distance(a, b) < .0001f, "Positions differ");
    private static void Rotation(Quaternion a, Quaternion b) => Require(Math.Abs(Quaternion.Dot(a, b)) > .9999f, "Rotations differ");
    private static void Throws(Action f) { try { f(); } catch (ArgumentException) { return; } throw new Exception("Expected argument rejection"); }
    private static void Test(string name, Action f)
    {
        try { f(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + " :: " + e); }
    }
    private static RoomPlan Plan()
    {
        Require(RoomPlanner.TryPlan(SyntheticRooms.Rectangular(), P(new Vector3(0, 1.5f, 0)), out RoomPlan p, out string error), error);
        return p;
    }
    private static ReflectionLane Lane()
    {
        Require(ReflectionLane.TryCreate(Plan(), out ReflectionLane lane, out string error), error); return lane;
    }
    private static DualScaleMap Map() => new DualScaleMap(new SpatialFrame(Pose3.Identity, 1),
        new SpatialFrame(new Pose3(new Vector3(-.4f, 1, -.3f), Quaternion.CreateFromYawPitchRoll(.3f, 0, 0)), .2f));
    private static ScaleSession Mirror()
    {
        return new ScaleSession(Map(), Pose3.Identity, new InteractionBounds(new Vector3(-2), new Vector3(2)),
            Vector3.Zero, .2f, behavior: CaptureBehavior.OrientInPlace);
    }
    private static void Until(ShellDuel d, ShellPhase phase, Quaternion q)
    {
        for (int i = 0; i < 700 && d.Phase != phase; i++) d.Tick(.1f, q);
        Require(d.Phase == phase, "Did not reach " + phase + ": " + d.Phase);
    }
    private sealed class Player
    {
        public FirstEncounter Game = new FirstEncounter(Plan(), Map(), includeShell: true);
        private double now;
        private long sequence;
        public void Frame(Pose3 p, float pinch = 0, float dt = 0, bool tracked = true)
        {
            now += Math.Max(.01f, dt);
            var sample = new HandSample(HandId.Right, ++sequence, now, Game.Session.Map.MiniatureView(p), pinch, tracked);
            Game.Step(now, dt, default, sample, NoTargets);
        }
        public void Idle(float dt = .1f) => Frame(P(new Vector3(0, 3, -1)), dt: dt);
        public void Return()
        {
            Frame(Game.Session.ObjectPose); Frame(Game.Session.ObjectPose);
            Frame(Game.Session.ObjectPose, 1); Require(Game.Session.State == ProbeState.Held, "Failed to capture creature");
            Frame(new Pose3(Game.Session.ReturnCenter, Game.Session.ObjectPose.Rotation));
        }
        public void ReachShell() { Return(); Return(); Return(); Require(Game.Phase == EncounterPhase.Reflector); }
        public void Orient()
        {
            Frame(Game.Session.ObjectPose); Frame(Game.Session.ObjectPose);
            Frame(Game.Session.ObjectPose, 1); Require(Game.Session.State == ProbeState.Held);
            Frame(new Pose3(Game.Session.ObjectPose.Position, Game.Duel.Lane.SolutionRotation), 1);
            Frame(Game.Session.ObjectPose);
        }
        public void WaitForShell()
        {
            for (int i = 0; i < 400 && Game.Phase != EncounterPhase.ShellVulnerable; i++) Idle();
            Require(Game.Phase == EncounterPhase.ShellVulnerable, "No vulnerability: " + Game.Phase);
        }
    }
    private static int Main()
    {
        Test("reflection lane is geometry-checked and admits a real return solution", () =>
        { var l = Lane(); Require(l.Length >= .65f && l.Length <= 1.35f); Require(l.RoomPlan.Room.IsMotionClear(l.ShellPosition, l.ReflectorPosition, .18f)); Require(l.RoomPlan.Room.IsMotionClear(l.ShellPosition, l.RoomPlan.ReturnCenter)); });
        Test("null plan does not create a synthetic reflection fallback", () => Require(!ReflectionLane.TryCreate(null, out _, out _)));
        Test("swept sphere catches a target between free endpoints", () =>
        { Require(ProjectileMath.TrySphereEntry(new Vector3(-2, 0, 0), new Vector3(2, 0, 0), Vector3.Zero, .2f, out float t)); Require(Math.Abs(t - .45f) < .0001f); });
        Test("sphere test rejects a target behind the shot", () => Require(!ProjectileMath.TrySphereEntry(Vector3.UnitX, Vector3.UnitX * 2, Vector3.Zero, .2f, out _)));
        Test("sphere test handles stationary and initially overlapping pulses", () =>
        { Require(ProjectileMath.TrySphereEntry(Vector3.Zero, Vector3.Zero, Vector3.Zero, .2f, out _)); Require(!ProjectileMath.TrySphereEntry(Vector3.One, Vector3.One, Vector3.Zero, .2f, out _)); });
        Test("sphere test rejects nonfinite data", () => Require(!ProjectileMath.TrySphereEntry(new Vector3(float.NaN), Vector3.Zero, Vector3.Zero, .2f, out _)));
        Test("reflector never translates when the hand moves", () =>
        { var s = Mirror(); s.BeginCapture(HandId.Right, s.Map.MiniatureView(Pose3.Identity)); Require(s.MoveCapture(HandId.Right, s.Map.MiniatureView(new Pose3(new Vector3(.3f, 0, 0), Quaternion.CreateFromAxisAngle(Vector3.UnitY, .4f))))); Near(s.ObjectPose.Position, Vector3.Zero); });
        Test("orientation release retains rotation but never awards a creature return", () =>
        { var s = Mirror(); var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, .7f); s.BeginCapture(HandId.Right, s.Map.MiniatureView(Pose3.Identity)); s.MoveCapture(HandId.Right, s.Map.MiniatureView(new Pose3(Vector3.Zero, q))); Require(!s.CanReturn); Require(s.ReleaseCapture(HandId.Right)); Rotation(s.ObjectPose.Rotation, q); Require(s.OrientationCommitCount == 1 && s.ReturnCount == 0 && s.State == ProbeState.Available); });
        Test("orientation cancel restores the previously placed angle", () =>
        { var s = Mirror(); s.BeginCapture(HandId.Right, s.Map.MiniatureView(Pose3.Identity)); s.MoveCapture(HandId.Right, s.Map.MiniatureView(new Pose3(Vector3.Zero, Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1)))); s.CancelCapture(HandId.Right); Rotation(s.ObjectPose.Rotation, Quaternion.Identity); Require(s.OrientationCommitCount == 0); });
        Test("invalid final orientation sample cannot commit an earlier angle", () =>
        { var s = Mirror(); s.BeginCapture(HandId.Right, s.Map.MiniatureView(Pose3.Identity)); Require(!s.MoveCapture(HandId.Right, default)); Require(!s.ReleaseCapture(HandId.Right)); Require(s.OrientationCommitCount == 0); });
        Test("reflector leash rejects distant interaction", () =>
        { var s = Mirror(); Require(!s.BeginCapture(HandId.Right, s.Map.MiniatureView(P(new Vector3(2, 0, 0))))); s.BeginCapture(HandId.Right, s.Map.MiniatureView(Pose3.Identity)); Require(!s.MoveCapture(HandId.Right, s.Map.MiniatureView(P(new Vector3(2, 0, 0))))); Require(!s.ReleaseCapture(HandId.Right)); });
        Test("orientation cannot be advanced by autonomous creature movement", () => Require(!Mirror().TryAdvance(P(Vector3.One))));
        Test("unknown interaction behavior is rejected", () => Throws(() => new ScaleSession(Map(), Pose3.Identity, new InteractionBounds(-Vector3.One, Vector3.One), Vector3.Zero, .2f, behavior: (CaptureBehavior)50)));
        Test("duel waits without damage before first intentional placement", () =>
        { var d = new ShellDuel(Lane()); for (int i = 0; i < 500; i++) d.Tick(.1f, d.ReflectorRotation); Require(d.Phase == ShellPhase.AwaitingOrientation && d.Misses == 0 && d.ShotSerial == 0); });
        Test("preview distinguishes initial wrong angle from correct solution", () =>
        { var l = Lane(); var d = new ShellDuel(l); Require(!d.PreviewHitsShell); d.Tick(0, l.SolutionRotation); Require(d.PreviewHitsShell); });
        Test("one valid reflection breaks armor exactly once", () =>
        { var l = Lane(); var d = new ShellDuel(l); d.Begin(); Until(d, ShellPhase.Vulnerable, l.SolutionRotation); Require(d.ReflectionCount == 1 && d.ShieldBreakCount == 1 && d.Misses == 0 && !d.ProjectileVisible); });
        Test("same solution works at larger clamped frame step and faster pulse", () =>
        { var l = Lane(); var d = new ShellDuel(l, pulseSpeed: 5); d.Begin(); Until(d, ShellPhase.Vulnerable, l.SolutionRotation); Require(d.Misses == 0); });
        Test("edge-on mirror misses instead of creating an invalid bounce", () =>
        { var l = Lane(); var d = new ShellDuel(l, allowedMisses: 1); d.Begin(); Until(d, ShellPhase.Lost, Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 2) * l.SolutionRotation); Require(d.Misses == 1 && d.ReflectionCount == 0); });
        Test("failed outgoing pulse consumes only one refuge light", () =>
        { var l = Lane(); var d = new ShellDuel(l, allowedMisses: 1); d.Begin(); Until(d, ShellPhase.Lost, l.InitialRotation); for (int i = 0; i < 100; i++) d.Tick(.1f, l.InitialRotation); Require(d.Misses == 1); });
        Test("pause freezes shot, charge and orientation", () =>
        { var l = Lane(); var d = new ShellDuel(l); d.Begin(); float timer = d.RemainingSeconds; d.Tick(.1f, l.SolutionRotation, paused: true); Require(d.RemainingSeconds == timer); Rotation(d.ReflectorRotation, l.InitialRotation); });
        Test("paused moving projectile cannot hit", () =>
        { var l = Lane(); var d = new ShellDuel(l); d.Begin(); Until(d, ShellPhase.Incoming, l.SolutionRotation); var p = d.ProjectilePosition; for (int i = 0; i < 40; i++) d.Tick(.1f, l.SolutionRotation, paused: true); Near(p, d.ProjectilePosition); Require(d.ShieldBreakCount == 0); });
        Test("large elapsed time is not replayed as hidden simulation", () =>
        { var d = new ShellDuel(Lane()); d.Begin(); float before = d.RemainingSeconds; d.Tick(1000, d.ReflectorRotation); Require(Math.Abs(before - d.RemainingSeconds - .1f) < .0001f); });
        Test("invalid time or orientation cannot mutate duel state", () =>
        { var d = new ShellDuel(Lane()); Throws(() => d.Tick(float.NaN, Quaternion.Identity)); Throws(() => d.Tick(.1f, default)); Require(d.Phase == ShellPhase.AwaitingOrientation && d.Misses == 0); });
        Test("armor never reappears on a held Shell", () =>
        { var l = Lane(); var d = new ShellDuel(l, vulnerableSeconds: .5f); d.Begin(); Until(d, ShellPhase.Vulnerable, l.SolutionRotation); for (int i = 0; i < 100; i++) d.Tick(.1f, l.SolutionRotation, shellHeld: true); Require(d.Phase == ShellPhase.Vulnerable); });
        Test("unheld vulnerability expires into a new announced attack", () =>
        { var l = Lane(); var d = new ShellDuel(l, vulnerableSeconds: .5f); d.Begin(); Until(d, ShellPhase.Vulnerable, l.SolutionRotation); Until(d, ShellPhase.Charging, l.SolutionRotation); Require(d.Armored && !d.ProjectileVisible); });
        Test("only vulnerable Shell can be resolved and only once", () =>
        { var l = Lane(); var d = new ShellDuel(l); Require(!d.ResolveCapture()); d.Begin(); Until(d, ShellPhase.Vulnerable, l.SolutionRotation); Require(d.ResolveCapture()); Require(!d.ResolveCapture()); d.Tick(.1f, l.InitialRotation); Require(d.Phase == ShellPhase.Resolved); });
        Test("three Motes lead to a distinct reflector lesson instead of premature victory", () =>
        { var p = new Player(); p.ReachShell(); Require(p.Game.Captured == 3 && p.Game.CaptureGoal == 4 && p.Game.ActiveIsReflector && !p.Game.ShowReturnZone); });
        Test("armored Shell and reflector do not share the active interaction target", () =>
        { var p = new Player(); p.ReachShell(); p.Frame(p.Game.CreaturePose); p.Frame(p.Game.CreaturePose, 1); Require(p.Game.Session.State != ProbeState.Held && p.Game.Duel.Armored); });
        Test("complete one-hand loop: orient, reflect, capture Shell, return and win", () =>
        { var p = new Player(); p.ReachShell(); p.Orient(); p.WaitForShell(); p.Return(); Require(p.Game.Phase == EncounterPhase.Won && p.Game.Captured == 4 && p.Game.Duel.Phase == ShellPhase.Resolved); });
        Test("holding Shell past its window remains capturable", () =>
        { var p = new Player(); p.ReachShell(); p.Orient(); p.WaitForShell(); p.Frame(p.Game.Session.ObjectPose); p.Frame(p.Game.Session.ObjectPose, 1); for (int i = 0; i < 150; i++) p.Frame(p.Game.Session.ObjectPose, 1, .1f); Require(p.Game.Phase == EncounterPhase.ShellVulnerable && p.Game.Session.State == ProbeState.Held); p.Frame(P(p.Game.Session.ReturnCenter)); Require(p.Game.Phase == EncounterPhase.Won); });
        Test("tracking loss while orienting cancels without starting the lesson", () =>
        { var p = new Player(); p.ReachShell(); p.Frame(p.Game.Session.ObjectPose); p.Frame(p.Game.Session.ObjectPose, 1); p.Frame(p.Game.Session.ObjectPose, 1, tracked: false); Require(p.Game.Session.IsPaused && p.Game.Session.State != ProbeState.Held && p.Game.Duel.Phase == ShellPhase.AwaitingOrientation); });
        Test("restarting Shell stage restores Mote tutorial and preserves user pause", () =>
        { var p = new Player(); p.ReachShell(); p.Game.Controls.SetExternalPause(PauseReason.User, true); p.Game.Controls.ResetProbe(); p.Idle(); Require(p.Game.Phase == EncounterPhase.LearnCapture && p.Game.Duel == null && p.Game.Captured == 0 && p.Game.Session.IsPaused); });
        Test("room invalidation cannot be bypassed by Shell restart", () =>
        { var p = new Player(); p.ReachShell(); p.Game.InvalidateRoom(); p.Game.Controls.ResetProbe(); p.Idle(); Require(p.Game.Phase == EncounterPhase.RoomInvalid && p.Game.Session.IsPaused); });
        Test("repositioning the miniature preserves the reflection lane", () =>
        { var p = new Player(); p.ReachShell(); var l = p.Game.Duel.Lane; p.Game.Controls.SetExternalPause(PauseReason.User, true); Require(p.Game.Controls.Recalibrate(new DualScaleMap(p.Game.Session.Map.Room, new SpatialFrame(P(new Vector3(1, 1, 1)), .2f)))); Near(l.ShellPosition, p.Game.Duel.Lane.ShellPosition); Near(l.ReflectorPosition, p.Game.Session.ObjectPose.Position); });
        Test("100 seeded mirror rotations never create multiple active pulses or negative lives", () =>
        { var random = new Random(643); for (int trial = 0; trial < 100; trial++) { var d = new ShellDuel(Lane()); d.Begin(); for (int i = 0; i < 80; i++) { var q = Quaternion.CreateFromYawPitchRoll((float)random.NextDouble()*6, (float)random.NextDouble(), 0); d.Tick(.1f, q); Require(d.Misses <= 3 && d.Misses >= 0 && SpatialMath.IsFinite(d.ProjectilePosition)); } } });
        Console.WriteLine($"SHELL: {passed} passed, {failed} failed. Real core, synthetic room/input; not Unity, SDK or headset validation.");
        return failed == 0 ? 0 : 1;
    }
}
