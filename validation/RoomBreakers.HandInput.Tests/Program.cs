using System;
using System.Numerics;
using RoomBreakers.Core;

internal static class Program
{
    private static int passed, failed;
    private static Pose3 P(float x = 0) => new Pose3(new Vector3(x, 0, 0), Quaternion.Identity);
    private static void Check(bool x) { if (!x) throw new Exception("Assertion failed"); }
    private static void Near(float x, float y) => Check(Math.Abs(x - y) < .0001f);
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
    private static void Test(string label, Action action)
    {
        try { action(); passed++; Console.WriteLine("PASS " + label); }
        catch (Exception e) { failed++; Console.Error.WriteLine("FAIL " + label + " :: " + e); }
    }
    private sealed class Rig
    {
        public readonly ScaleSession S;
        public readonly HandCaptureDriver D;
        public double Now;
        public long LS, RS;
        public HandSample Left, Right;
        public Rig()
        {
            var map = new DualScaleMap(new SpatialFrame(Pose3.Identity, 1),
                new SpatialFrame(new Pose3(new Vector3(2, 1, -2), Quaternion.CreateFromYawPitchRoll(.3f, -.2f, .4f)), .2f));
            S = new ScaleSession(map, P(), new InteractionBounds(new Vector3(-2), new Vector3(2)), new Vector3(1, 0, 0), .15f);
            D = new HandCaptureDriver(S);
        }
        public HandSample Make(HandId h, long seq, double time, float x, float strength, bool tracked = true)
            => new HandSample(h, seq, time, S.Map.MiniatureView(P(x)), strength, tracked);
        public void L(float x = 0, float p = 0, bool tracked = true)
        {
            Now += .01; Left = Make(HandId.Left, ++LS, Now, x, p, tracked); D.Step(Now, Left, Right);
        }
        public void R(float x = 0, float p = 0, bool tracked = true)
        {
            Now += .01; Right = Make(HandId.Right, ++RS, Now, x, p, tracked); D.Step(Now, Left, Right);
        }
        public void Both(float lx, float lp, float rx, float rp)
        {
            Now += .01;
            Left = Make(HandId.Left, ++LS, Now, lx, lp);
            Right = Make(HandId.Right, ++RS, Now, rx, rp);
            D.Step(Now, Left, Right);
        }
        public void Idle(double dt) { Now += dt; D.Step(Now, Left, Right); }
        public void RawLeft(HandSample sample) { Now += .01; Left = sample; D.Step(Now, Left, Right); }
        public void Grab() { L(); L(0, 1); Check(S.State == ProbeState.Held); }
    }
    private static int Main()
    {
        Test("starts paused until fresh open hand", () => { var r = new Rig(); Check(r.S.IsPaused && r.D.RequiresOpenHand); r.L(); Check(!r.S.IsPaused); });
        Test("closed hand on startup never grabs", () => { var r = new Rig(); r.L(0, 1); r.L(0, 1); Check(r.S.IsPaused && r.S.State == ProbeState.Available); });
        Test("open then near pinch acquires", () => { var r = new Rig(); r.Grab(); Check(r.S.Owner == HandId.Left); });
        Test("right hand works without left tracking", () => { var r = new Rig(); r.R(); r.R(0, 1); Check(r.S.Owner == HandId.Right && !r.S.IsPaused); });
        Test("open hand supplies hover but no capture", () => { var r = new Rig(); r.L(); r.L(); Check(r.D.HoverHand == HandId.Left && r.S.State == ProbeState.Available); });
        Test("far pinch cannot acquire", () => { var r = new Rig(); r.L(1); r.L(1, 1); Check(r.S.State == ProbeState.Available); });
        Test("missed pinch cannot sweep into object", () => { var r = new Rig(); r.L(1); r.L(1, 1); r.L(0, 1); Check(r.S.State == ProbeState.Available); r.L(); r.L(0, 1); Check(r.S.State == ProbeState.Held); });
        Test("pinch hysteresis retains capture", () => { var r = new Rig(); r.Grab(); r.L(.3f, .5f); Check(r.S.State == ProbeState.Held); Near(r.S.ObjectPose.Position.X, .3f); });
        Test("release uses current pose not previous pose", () => { var r = new Rig(); r.Grab(); r.L(1, 0); Check(r.S.State == ProbeState.Returned && r.S.ReturnCount == 1); });
        Test("release away from target cancels", () => { var r = new Rig(); r.Grab(); r.L(1, 1); r.L(0, 0); Check(r.S.State == ProbeState.Available && r.S.ReturnCount == 0); });
        Test("out of bounds release cannot use stale target", () => { var r = new Rig(); r.Grab(); r.L(1, 1); r.L(10, 0); Check(r.S.ReturnCount == 0 && r.S.State == ProbeState.Available); });
        Test("explicit tracking loss cancels and pauses", () => { var r = new Rig(); r.Grab(); r.L(1, 1); r.L(1, 0, false); Check(r.S.IsPaused && r.S.ReturnCount == 0 && r.S.Owner == HandId.None); Near(r.S.ObjectPose.Position.X, 0); });
        Test("absent callbacks expire held capture", () => { var r = new Rig(); r.Grab(); r.Idle(.201); Check(r.S.IsPaused && r.S.Owner == HandId.None); });
        Test("cached duplicate cannot refresh watchdog", () => { var r = new Rig(); r.Grab(); r.Idle(.1); r.Idle(.101); Check(r.S.IsPaused && r.D.RequiresOpenHand); });
        Test("same sequence with new timestamp is ignored", () => { var r = new Rig(); r.Grab(); r.Now += .21; r.Left = r.Make(HandId.Left, r.LS, r.Now, 1, 0); r.D.Step(r.Now, r.Left, r.Right); Check(r.S.IsPaused && r.S.ReturnCount == 0); });
        Test("out of order sequence cannot commit", () => { var r = new Rig(); r.Grab(); r.RawLeft(r.Make(HandId.Left, 1, r.Now, 1, 0)); Check(r.S.State == ProbeState.Held && r.S.ReturnCount == 0); });
        Test("closed hand after loss requires reopening", () => { var r = new Rig(); r.Grab(); r.Idle(.21); r.L(0, 1); Check(r.S.IsPaused); r.L(); Check(!r.S.IsPaused && r.S.State == ProbeState.Available); r.L(0, 1); Check(r.S.State == ProbeState.Held); });
        Test("cached open sample cannot clear pause", () => { var r = new Rig(); r.L(); r.D.SetExternalPause(PauseReason.User, true); r.D.SetExternalPause(PauseReason.User, false); r.Idle(.01); Check(r.S.IsPaused); r.L(); Check(!r.S.IsPaused); });
        Test("valid inactive hand does not mask owner loss", () => { var r = new Rig(); r.Both(0, 0, 1, 0); r.L(0, 1); r.R(1, 0); r.L(0, 0, false); Check(r.S.IsPaused && r.S.Owner == HandId.None); r.R(); Check(!r.S.IsPaused); });
        Test("missing idle hand does not interrupt owner", () => { var r = new Rig(); r.Grab(); for (int i = 0; i < 30; i++) r.L(0, .9f); Check(r.S.State == ProbeState.Held && !r.S.IsPaused); });
        Test("foreign invalid data cannot cancel owner", () => { var r = new Rig(); r.Grab(); r.R(0, float.NaN, false); Check(r.S.State == ProbeState.Held && r.S.Owner == HandId.Left); });
        Test("simultaneous tie deterministically selects left", () => { var r = new Rig(); r.Both(0, 0, 0, 0); r.Both(0, 1, 0, 1); Check(r.S.Owner == HandId.Left); });
        Test("nearest simultaneous pinch selects right", () => { var r = new Rig(); r.Both(.2f, 0, 0, 0); r.Both(.2f, 1, 0, 1); Check(r.S.Owner == HandId.Right); });
        Test("second hand cannot steal capture", () => { var r = new Rig(); r.Both(0, 0, 0, 0); r.L(0, 1); r.R(0, 1); Check(r.S.Owner == HandId.Left); });
        Test("inactive closed hand cannot capture after owner cancels", () => { var r = new Rig(); r.Both(0, 0, 0, 0); r.L(0, 1); r.R(0, 1); r.L(0, 0); r.R(0, 1); Check(r.S.State == ProbeState.Available); });
        Test("NaN strength cancels rather than releasing", () => { var r = new Rig(); r.Grab(); r.L(1, float.NaN); Check(r.S.IsPaused && r.S.ReturnCount == 0); });
        Test("out of range strength rejected", () => { var r = new Rig(); r.L(0, 2); Check(r.S.IsPaused); });
        Test("invalid pose cancels owner", () => { var r = new Rig(); r.Grab(); r.RawLeft(new HandSample(HandId.Left, ++r.LS, r.Now, default, 0, true)); Check(r.S.IsPaused && r.S.ReturnCount == 0); });
        Test("future timestamp never matures into valid sample", () => { var r = new Rig(); r.L(); r.RawLeft(r.Make(HandId.Left, ++r.LS, 1, 0, 0)); r.Idle(1); Check(r.S.IsPaused); });
        Test("late sample rejected", () => { var r = new Rig(); r.Grab(); r.Idle(.21); r.RawLeft(r.Make(HandId.Left, ++r.LS, 0, 0, 0)); Check(r.S.IsPaused); });
        Test("timestamp regression rejected", () => { var r = new Rig(); r.Grab(); r.RawLeft(r.Make(HandId.Left, ++r.LS, 0, 0, 0)); Check(r.S.IsPaused); });
        Test("negative timestamp rejected", () => { var r = new Rig(); r.RawLeft(r.Make(HandId.Left, 1, -1, 0, 0)); Check(r.S.IsPaused); });
        Test("NaN timestamp rejected", () => { var r = new Rig(); r.RawLeft(r.Make(HandId.Left, 1, double.NaN, 0, 0)); Check(r.S.IsPaused); });
        Test("wrong slot identity ignored", () => { var r = new Rig(); r.RawLeft(r.Make(HandId.Right, 1, 0, 0, 0)); Check(r.S.IsPaused); });
        Test("zero sequence is not a sample", () => { var r = new Rig(); r.RawLeft(r.Make(HandId.Left, 0, 0, 0, 0)); Check(r.S.IsPaused); });
        Test("invalid clock rejected before state change", () => { var r = new Rig(); r.Grab(); Throws<ArgumentOutOfRangeException>(() => r.D.Step(double.NaN, default, default)); Check(r.S.State == ProbeState.Held); });
        Test("clock regression rejected", () => { var r = new Rig(); r.L(); Throws<ArgumentOutOfRangeException>(() => r.D.Step(0, default, default)); });
        Test("external pause reasons remain independent", () => { var r = new Rig(); r.Grab(); r.D.SetExternalPause(PauseReason.User, true); r.D.SetExternalPause(PauseReason.FocusLost, true); r.D.SetExternalPause(PauseReason.FocusLost, false); r.L(); Check((r.S.PauseReasons & PauseReason.User) != 0); });
        Test("reset does not resume focus loss", () => { var r = new Rig(); r.Grab(); r.D.SetExternalPause(PauseReason.FocusLost, true); r.D.Reset(); r.L(); Check(r.S.IsPaused); });
        Test("reset with pinched hand cannot reacquire", () => { var r = new Rig(); r.Grab(); r.D.Reset(); r.L(0, 1); Check(r.S.IsPaused && r.S.State == ProbeState.Available); });
        Test("recalibration needs pause", () => { var r = new Rig(); r.L(); Check(!r.D.Recalibrate(r.S.Map)); });
        Test("paused recalibration preserves canonical object", () => { var r = new Rig(); r.L(); r.D.SetExternalPause(PauseReason.Placement, true); Check(r.D.Recalibrate(new DualScaleMap(new SpatialFrame(P(), 1), new SpatialFrame(P(3), .3f)))); Near(r.S.ObjectPose.Position.X, 0); });
        Test("tracking pause cannot be externally cleared", () => { var r = new Rig(); Throws<ArgumentException>(() => r.D.SetExternalPause(PauseReason.TrackingLost, false)); });
        Test("bad tuning values rejected", () => { Throws<ArgumentException>(() => new HandCaptureSettings(pinchOn: .2f, pinchOff: .4f)); Throws<ArgumentOutOfRangeException>(() => new HandCaptureSettings(timeoutSeconds: double.NaN)); Throws<ArgumentOutOfRangeException>(() => new HandCaptureSettings(selectionRadiusMeters: 0)); });
        Test("repeat release cannot duplicate reward", () => { var r = new Rig(); r.Grab(); r.L(1, 0); for (int i = 0; i < 5; i++) r.L(1, 0); Check(r.S.ReturnCount == 1); });
        Test("5000 seeded input frames preserve capture invariants", () =>
        {
            var random = new Random(4417); var r = new Rig();
            for (int i = 0; i < 5000; i++)
            {
                int op = random.Next(8);
                if (op < 3) r.L((float)random.NextDouble() * 1.1f, (float)random.NextDouble(), random.Next(12) != 0);
                else if (op < 6) r.R((float)random.NextDouble() * 1.1f, (float)random.NextDouble(), random.Next(12) != 0);
                else if (op == 6) r.Idle(.25);
                else r.D.Reset();
                Check(r.S.ObjectPose.IsValid && r.S.ReturnCount >= 0 && r.S.ReturnCount <= 1);
                Check(!r.S.IsPaused || r.S.State != ProbeState.Held);
                Check((r.S.State == ProbeState.Held) == (r.S.Owner != HandId.None));
            }
        });
        Console.WriteLine($"HAND INPUT: {passed} passed, {failed} failed. Synthetic input into real C# driver; not a sensor or Unity test.");
        return failed == 0 ? 0 : 1;
    }
}
