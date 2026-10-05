using System;
using System.Numerics;
using RoomBreakers.Core;

internal static class Program
{
    private static int passed;
    private static int failed;
    private static Pose3 P(float x = 0, float y = 0, float z = 0) => new Pose3(new Vector3(x, y, z), Quaternion.Identity);
    private static DualScaleMap Map() => new DualScaleMap(new SpatialFrame(P(), 1f), new SpatialFrame(new Pose3(new Vector3(-2, 1, -2), Quaternion.CreateFromYawPitchRoll(.3f, -.2f, .1f)), .2f));
    private static ScaleSession Session() => new ScaleSession(Map(), P(1), new InteractionBounds(new Vector3(-10), new Vector3(10)), new Vector3(2, 0, 0), .25f);
    private static Pose3 Hand(ScaleSession s, Pose3 canonical) => s.Map.MiniatureView(canonical);
    private static void Require(bool condition) { if (!condition) throw new Exception("Assertion failed."); }
    private static void Near(float a, float b, float epsilon = .0001f) => Require(Math.Abs(a - b) <= epsilon);
    private static void Near(Vector3 a, Vector3 b, float epsilon = .0001f) => Require(Vector3.Distance(a, b) <= epsilon);
    private static void Near(Pose3 a, Pose3 b)
    {
        Near(a.Position, b.Position);
        Require(Math.Abs(Quaternion.Dot(a.Rotation, b.Rotation)) > .99999f);
    }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
    private static void Test(string name, Action action)
    {
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.Error.WriteLine("FAIL " + name + " :: " + e); }
    }
    private static void ReturnProbe(ScaleSession s)
    {
        Require(s.BeginCapture(HandId.Left, Hand(s, P())));
        Require(s.MoveCapture(HandId.Left, Hand(s, P(1))));
        Require(s.ReleaseCapture(HandId.Left));
    }
    private static int Main()
    {
        Test("pose rejects NaN", () => Throws<ArgumentException>(() => P(float.NaN)));
        Test("pose rejects infinity", () => Throws<ArgumentException>(() => P(float.PositiveInfinity)));
        Test("pose rejects zero quaternion", () => Throws<ArgumentException>(() => new Pose3(Vector3.Zero, default)));
        Test("pose rejects infinite quaternion", () => Throws<ArgumentException>(() => new Pose3(Vector3.Zero, new Quaternion(0, 0, 0, float.PositiveInfinity))));
        Test("pose normalizes quaternion", () => Near(new Pose3(Vector3.Zero, new Quaternion(0, 0, 0, 2)).Rotation.W, 1));
        Test("default pose is invalid", () => Require(!default(Pose3).IsValid));
        Test("frame rejects zero scale", () => Throws<ArgumentOutOfRangeException>(() => new SpatialFrame(P(), 0)));
        Test("frame rejects negative scale", () => Throws<ArgumentOutOfRangeException>(() => new SpatialFrame(P(), -1)));
        Test("frame rejects NaN scale", () => Throws<ArgumentOutOfRangeException>(() => new SpatialFrame(P(), float.NaN)));
        Test("frame rejects invalid origin", () => Throws<ArgumentException>(() => new SpatialFrame(default, 1)));
        Test("room must use meter scale", () => Throws<ArgumentException>(() => new DualScaleMap(new SpatialFrame(P(), 2), new SpatialFrame(P(), .2f))));
        Test("translated scaled frame", () => Near(new SpatialFrame(P(2, 3, 4), .2f).ToWorld(P(5, 5, 5)), P(3, 4, 5)));
        Test("quaternion rotates local X to Y", () => Near(new SpatialFrame(new Pose3(Vector3.Zero, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Math.PI / 2)), 1).ToWorld(P(1)).Position, Vector3.UnitY));
        Test("full 6DoF roundtrip, 300 deterministic cases", () =>
        {
            var random = new Random(1729);
            for (int i = 0; i < 300; i++)
            {
                var origin = new Pose3(new Vector3((float)random.NextDouble() * 5, 2, -3), Quaternion.CreateFromYawPitchRoll((float)random.NextDouble() * 3, (float)random.NextDouble() * 2, (float)random.NextDouble()));
                var frame = new SpatialFrame(origin, .1f + (float)random.NextDouble());
                var pose = new Pose3(new Vector3(1, (float)random.NextDouble() * 4, -2), Quaternion.CreateFromYawPitchRoll(.7f, .4f, -.8f));
                Near(frame.ToLocal(frame.ToWorld(pose)), pose);
            }
        });
        Test("dual scale maps hand across independent rotations", () =>
        {
            var map = new DualScaleMap(new SpatialFrame(new Pose3(new Vector3(5, 2, 3), Quaternion.CreateFromYawPitchRoll(.2f, .3f, -.5f)), 1), Map().Miniature);
            var canonical = new Pose3(new Vector3(1, 2, 3), Quaternion.CreateFromYawPitchRoll(-.4f, .7f, .3f));
            Near(map.EnlargedHand(map.MiniatureView(canonical)), map.RoomView(canonical));
        });
        Test("bounds reject inversion", () => Throws<ArgumentException>(() => new InteractionBounds(Vector3.One, Vector3.Zero)));
        Test("bounds reject NaN query", () => Require(!new InteractionBounds(Vector3.Zero, Vector3.One).Contains(new Vector3(float.NaN))));
        Test("initial entity outside bounds rejected", () => Throws<ArgumentException>(() => new ScaleSession(Map(), P(20), new InteractionBounds(new Vector3(-10), new Vector3(10)), Vector3.Zero, .2f)));
        Test("target radius rejected", () => Throws<ArgumentOutOfRangeException>(() => new ScaleSession(Map(), P(), new InteractionBounds(new Vector3(-10), new Vector3(10)), Vector3.Zero, 0)));
        Test("capture begins without jumping", () => { var s = Session(); Require(s.BeginCapture(HandId.Left, Hand(s, P()))); Near(s.ObjectPose, P(1)); });
        Test("capture retains positional offset", () => { var s = Session(); s.BeginCapture(HandId.Left, Hand(s, P())); Require(s.MoveCapture(HandId.Left, Hand(s, P(1)))); Near(s.ObjectPose, P(2)); });
        Test("capture rotates its full relative offset", () =>
        {
            var s = Session(); s.BeginCapture(HandId.Left, Hand(s, P()));
            var rotated = new Pose3(Vector3.Zero, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Math.PI / 2));
            Require(s.MoveCapture(HandId.Left, Hand(s, rotated)));
            Near(s.ObjectPose.Position, Vector3.UnitY);
            Require(Math.Abs(Quaternion.Dot(s.ObjectPose.Rotation, rotated.Rotation)) > .99999f);
        });
        Test("same hand duplicate begin is inert", () => { var s = Session(); s.BeginCapture(HandId.Left, Hand(s, P())); Require(!s.BeginCapture(HandId.Left, Hand(s, P(4)))); s.MoveCapture(HandId.Left, Hand(s, P(1))); Near(s.ObjectPose, P(2)); });
        Test("second hand cannot steal", () => { var s = Session(); s.BeginCapture(HandId.Left, Hand(s, P())); Require(!s.BeginCapture(HandId.Right, Hand(s, P()))); Require(s.Owner == HandId.Left); });
        Test("wrong hand cannot move", () => { var s = Session(); s.BeginCapture(HandId.Left, Hand(s, P())); Require(!s.MoveCapture(HandId.Right, Hand(s, P(1)))); Near(s.ObjectPose, P(1)); });
        Test("wrong hand cannot release", () => { var s = Session(); s.BeginCapture(HandId.Left, Hand(s, P())); Require(!s.ReleaseCapture(HandId.Right)); Require(s.State == ProbeState.Held); });
        Test("invalid hand rejected", () => { var s = Session(); Require(!s.BeginCapture(HandId.None, Hand(s, P()))); Require(!s.BeginCapture((HandId)42, Hand(s, P()))); });
        Test("invalid input rejected without state mutation", () => { var s = Session(); Require(!s.BeginCapture(HandId.Left, default)); Require(s.State == ProbeState.Available); });
        Test("out of bounds movement preserves last pose", () => { var s = Session(); s.BeginCapture(HandId.Left, Hand(s, P())); Require(!s.MoveCapture(HandId.Left, Hand(s, P(50)))); Near(s.ObjectPose, P(1)); });
        Test("cancel restores original position", () => { var s = Session(); s.BeginCapture(HandId.Left, Hand(s, P())); s.MoveCapture(HandId.Left, Hand(s, P(1))); Require(s.CancelCapture(HandId.Left)); Near(s.ObjectPose, P(1)); Require(s.Owner == HandId.None); });
        Test("invalid drop cancels instead of rewarding", () => { var s = Session(); s.BeginCapture(HandId.Left, Hand(s, P())); Require(!s.ReleaseCapture(HandId.Left)); Require(s.ReturnCount == 0 && s.State == ProbeState.Available); });
        Test("valid return resolves exactly once", () => { var s = Session(); ReturnProbe(s); Require(s.State == ProbeState.Returned && s.ReturnCount == 1); Require(!s.ReleaseCapture(HandId.Left)); Require(s.ReturnCount == 1); });
        Test("resolved object cannot be recaptured", () => { var s = Session(); ReturnProbe(s); Require(!s.BeginCapture(HandId.Left, Hand(s, P()))); });
        Test("tracking loss cancels held object", () => { var s = Session(); s.BeginCapture(HandId.Left, Hand(s, P())); s.MoveCapture(HandId.Left, Hand(s, P(1))); s.SetPause(PauseReason.TrackingLost, true); Near(s.ObjectPose, P(1)); Require(s.Owner == HandId.None && s.IsPaused); });
        Test("cannot capture while paused", () => { var s = Session(); s.SetPause(PauseReason.User, true); Require(!s.BeginCapture(HandId.Left, Hand(s, P()))); });
        Test("pause reasons do not override each other", () => { var s = Session(); s.SetPause(PauseReason.User, true); s.SetPause(PauseReason.FocusLost, true); s.SetPause(PauseReason.FocusLost, false); Require(s.IsPaused); s.SetPause(PauseReason.User, false); Require(!s.IsPaused); });
        Test("unknown pause reason rejected", () => { var s = Session(); Throws<ArgumentException>(() => s.SetPause((PauseReason)16, true)); });
        Test("combined pause reason rejected", () => { var s = Session(); Throws<ArgumentException>(() => s.SetPause(PauseReason.User | PauseReason.TrackingLost, true)); });
        Test("paused time never advances", () => { var s = Session(); s.SetPause(PauseReason.User, true); s.Tick(.05f); Require(s.ElapsedSeconds == 0); });
        Test("long delta does not catch up absent time", () => { var s = Session(); s.Tick(100); Near((float)s.ElapsedSeconds, .1f); });
        Test("negative delta rejected", () => { var s = Session(); Throws<ArgumentOutOfRangeException>(() => s.Tick(-1)); });
        Test("NaN delta rejected", () => { var s = Session(); Throws<ArgumentOutOfRangeException>(() => s.Tick(float.NaN)); });
        Test("recalibration requires pause", () => { var s = Session(); Require(!s.Recalibrate(Map())); });
        Test("paused recalibration preserves canonical entity", () => { var s = Session(); s.SetPause(PauseReason.Placement, true); Require(s.Recalibrate(new DualScaleMap(new SpatialFrame(P(), 1), new SpatialFrame(P(4, 3, 2), .3f)))); Near(s.ObjectPose, P(1)); });
        Test("reset preserves tracking pause", () => { var s = Session(); s.SetPause(PauseReason.TrackingLost, true); s.Reset(); Require(s.IsPaused && s.State == ProbeState.Available); });
        Test("reset clears return and elapsed state", () => { var s = Session(); ReturnProbe(s); s.Tick(.05f); s.Reset(); Require(s.ReturnCount == 0 && s.ElapsedSeconds == 0); Near(s.ObjectPose, P(1)); });
        Test("reflection returns expected direction", () => Near(SpatialMath.Reflect(new Vector3(1, -1, 0), Vector3.UnitY), Vector3.Normalize(new Vector3(1, 1, 0))));
        Test("reflection rejects zero normal", () => Throws<ArgumentException>(() => SpatialMath.Reflect(Vector3.UnitX, Vector3.Zero)));
        Test("reflection independent of normal magnitude", () => Near(SpatialMath.Reflect(new Vector3(1, -1, 0), Vector3.UnitY), SpatialMath.Reflect(new Vector3(1, -1, 0), Vector3.UnitY * 3)));
        Console.WriteLine($"RESULT: {passed} passed, {failed} failed. Core only; no Unity, XR, rendering or device tests were run.");
        return failed == 0 ? 0 : 1;
    }
}
