using System;
using System.Numerics;
using RoomBreakers.Core;

internal static class Program
{
    private static int passed, failed;
    private static Pose3 P(float x = 0, float y = 0, float z = 0) => new Pose3(new Vector3(x, y, z), Quaternion.Identity);
    private static DualScaleMap Map(float scale = .2f) => new DualScaleMap(new SpatialFrame(P(), 1), new SpatialFrame(P(), scale));
    private static ScaleSession New() => new ScaleSession(Map(), P(1), new InteractionBounds(new Vector3(-10), new Vector3(10)), new Vector3(2, 0, 0), .25f);
    private static Pose3 Hand(ScaleSession s, Pose3 canonical) => s.Map.MiniatureView(canonical);
    private static void Require(bool condition, string detail = "Assertion failed") { if (!condition) throw new Exception(detail); }
    private static void Near(Pose3 a, Pose3 b)
    {
        Require(Vector3.Distance(a.Position, b.Position) < .0001f, "Position mismatch");
        Require(Math.Abs(Quaternion.Dot(a.Rotation, b.Rotation)) > .99999f, "Rotation mismatch");
    }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
    private static void Test(string name, Action action)
    {
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.Error.WriteLine("FAIL " + name + " :: " + e.Message); }
    }
    private static ScaleSession AtTarget()
    {
        var s = New();
        Require(s.BeginCapture(HandId.Left, Hand(s, P())));
        Require(s.MoveCapture(HandId.Left, Hand(s, P(1))));
        Require(s.CanReturn);
        return s;
    }
    private static void AssertSafe(ScaleSession s, int step)
    {
        string detail = "State invariant at seeded step " + step;
        Require(s.ObjectPose.IsValid && s.LastHandWorld.IsValid, detail);
        Require(s.ObjectPose.Position.X >= -10 && s.ObjectPose.Position.X <= 10, detail);
        Require(s.ObjectPose.Position.Y >= -10 && s.ObjectPose.Position.Y <= 10, detail);
        Require(s.ObjectPose.Position.Z >= -10 && s.ObjectPose.Position.Z <= 10, detail);
        Require((s.State == ProbeState.Held) == (s.Owner == HandId.Left || s.Owner == HandId.Right), detail);
        Require(!s.IsPaused || s.State != ProbeState.Held, detail);
        Require(s.ReturnCount == (s.State == ProbeState.Returned ? 1 : 0), detail);
        Require(!s.CanReturn || (s.State == ProbeState.Held && !s.IsPaused), detail);
        Require(!double.IsNaN(s.ElapsedSeconds) && !double.IsInfinity(s.ElapsedSeconds) && s.ElapsedSeconds >= 0, detail);
        Near(s.Map.FromMiniature(s.Map.MiniatureView(s.ObjectPose)), s.ObjectPose);
        Near(s.Map.Room.ToLocal(s.Map.RoomView(s.ObjectPose)), s.ObjectPose);
    }
    private static int Main()
    {
        Test("out-of-bounds latest sample cannot commit a stale target", () =>
        {
            var s = AtTarget(); Require(!s.MoveCapture(HandId.Left, Hand(s, P(50))));
            Require(!s.CanReturn); Require(!s.ReleaseCapture(HandId.Left));
            Require(s.ReturnCount == 0 && s.State == ProbeState.Available); Near(s.ObjectPose, P(1));
        });
        Test("invalid latest pose cannot commit a stale target", () =>
        {
            var s = AtTarget(); Require(!s.MoveCapture(HandId.Left, default));
            Require(!s.ReleaseCapture(HandId.Left)); Require(s.ReturnCount == 0); Near(s.ObjectPose, P(1));
        });
        Test("overflowing latest sample cannot commit a stale target", () =>
        {
            var s = AtTarget(); Require(!s.MoveCapture(HandId.Left, P(float.MaxValue)));
            Require(!s.ReleaseCapture(HandId.Left)); Require(s.ReturnCount == 0);
        });
        Test("a fresh valid sample recovers after rejected movement", () =>
        {
            var s = AtTarget(); s.MoveCapture(HandId.Left, Hand(s, P(50)));
            Require(s.MoveCapture(HandId.Left, Hand(s, P(1)))); Require(s.CanReturn);
            Require(s.ReleaseCapture(HandId.Left)); Require(s.ReturnCount == 1);
        });
        Test("foreign invalid samples cannot sabotage current owner", () =>
        {
            var s = AtTarget(); Require(!s.MoveCapture(HandId.Right, default));
            Require(s.CanReturn && s.ReleaseCapture(HandId.Left));
        });
        Test("cancellation clears target eligibility", () =>
        {
            var s = AtTarget(); Require(s.CancelCapture(HandId.Left));
            Require(!s.CanReturn && !s.ReleaseCapture(HandId.Left) && s.ReturnCount == 0);
        });
        Test("pause clears capture and cannot award a return", () =>
        {
            var s = AtTarget(); s.SetPause(PauseReason.TrackingLost, true);
            Require(!s.CanReturn && !s.ReleaseCapture(HandId.Left));
            s.SetPause(PauseReason.TrackingLost, false); Require(!s.ReleaseCapture(HandId.Left));
        });
        Test("constructor rejects poses that overflow a rendered view", () =>
        {
            var bounds = new InteractionBounds(new Vector3(-float.MaxValue), new Vector3(float.MaxValue));
            Throws<ArgumentException>(() => new ScaleSession(Map(100), P(float.MaxValue / 2), bounds, Vector3.Zero, .2f));
        });
        Test("recalibration rejects unrepresentable reset pose atomically", () =>
        {
            var bounds = new InteractionBounds(new Vector3(-float.MaxValue), new Vector3(float.MaxValue));
            var s = new ScaleSession(Map(), P(float.MaxValue / 10), bounds, Vector3.Zero, .2f);
            var previous = s.Map; s.SetPause(PauseReason.Placement, true);
            Require(!s.Recalibrate(Map(100))); Require(ReferenceEquals(previous, s.Map));
            s.Reset(); Require(s.Map.MiniatureView(s.ObjectPose).IsValid);
        });
        Test("movement rejects projection overflow before mutating state", () =>
        {
            var bounds = new InteractionBounds(new Vector3(-float.MaxValue), new Vector3(float.MaxValue));
            var s = new ScaleSession(Map(100), P(float.MaxValue / 150), bounds, Vector3.Zero, .2f);
            Require(s.BeginCapture(HandId.Left, P())); Pose3 before = s.ObjectPose;
            Require(!s.MoveCapture(HandId.Left, P(float.MaxValue / 2)));
            Require(s.ObjectPose.Position == before.Position); Require(s.Map.MiniatureView(s.ObjectPose).IsValid);
        });
        Test("invalid move then reset keeps independent pause reasons", () =>
        {
            var s = AtTarget(); s.MoveCapture(HandId.Left, default);
            s.SetPause(PauseReason.FocusLost, true); s.SetPause(PauseReason.User, true); s.Reset();
            s.SetPause(PauseReason.FocusLost, false);
            Require(s.IsPaused && !s.CanReturn && s.Owner == HandId.None);
        });
        Test("zero-time tick and repeated release cannot generate rewards", () =>
        {
            var s = AtTarget(); Require(s.ReleaseCapture(HandId.Left));
            for (int i = 0; i < 100; i++) { s.Tick(0); Require(!s.ReleaseCapture(HandId.Left)); }
            Require(s.ReturnCount == 1 && s.ElapsedSeconds == 0);
        });
        Test("extreme finite quaternion normalizes without overflow", () =>
        {
            var p = new Pose3(Vector3.Zero, new Quaternion(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue));
            Require(p.IsValid && Math.Abs(p.Rotation.LengthSquared() - 1) < .00001f);
        });
        Test("5000 seeded 6DoF round trips", () =>
        {
            var random = new Random(62173);
            for (int i = 0; i < 5000; i++)
            {
                float A() => (float)random.NextDouble() * 6 - 3;
                var pose = new Pose3(new Vector3(A(), A(), A()), Quaternion.CreateFromYawPitchRoll(A(), A(), A()));
                var origin = new Pose3(new Vector3(A(), A(), A()), Quaternion.CreateFromYawPitchRoll(A(), A(), A()));
                var frame = new SpatialFrame(origin, .1f + (float)random.NextDouble() * .3f);
                Near(pose, frame.ToLocal(frame.ToWorld(pose)));
            }
        });
        Test("20000 seeded interleaved commands preserve session invariants", () =>
        {
            var random = new Random(880301); var s = New();
            for (int i = 0; i < 20000; i++)
            {
                switch (random.Next(14))
                {
                    case 0: s.BeginCapture(HandId.Left, Hand(s, P())); break;
                    case 1: s.BeginCapture(HandId.Right, Hand(s, P())); break;
                    case 2: s.MoveCapture(s.Owner, Hand(s, P(random.Next(-5, 6)))); break;
                    case 3: s.MoveCapture(s.Owner, Hand(s, P(1000))); break;
                    case 4: s.ReleaseCapture(HandId.Left); break;
                    case 5: s.ReleaseCapture(HandId.Right); break;
                    case 6: s.CancelCapture(s.Owner); break;
                    case 7: s.SetPause(PauseReason.User, random.Next(2) == 0); break;
                    case 8: s.SetPause(PauseReason.FocusLost, random.Next(2) == 0); break;
                    case 9:
                        double before = s.ElapsedSeconds; s.Tick(.05f);
                        Require(!s.IsPaused || s.ElapsedSeconds == before, "Paused timer advanced"); break;
                    case 10: s.Reset(); break;
                    case 11: s.MoveCapture(s.Owner, default); break;
                    case 12:
                        var old = s.Map; bool changed = s.Recalibrate(Map(.15f + (float)random.NextDouble() * .2f));
                        Require(changed == s.IsPaused); Require(changed || ReferenceEquals(old, s.Map)); break;
                    case 13: s.BeginCapture(HandId.Left, default); break;
                }
                AssertSafe(s, i);
            }
        });
        Console.WriteLine($"HARDENING: {passed} passed, {failed} failed. Seeded iterations are within tests, not separate device trials.");
        return failed == 0 ? 0 : 1;
    }
}
