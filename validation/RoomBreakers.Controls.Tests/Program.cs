using System;
using System.Numerics;
using RoomBreakers.Core;

internal static class Program
{
    private static int passed, failed;
    private static Pose3 P(Vector3 p) => new Pose3(p, Quaternion.Identity);
    private static void Check(bool value, string message = "Assertion failed") { if (!value) throw new Exception(message); }
    private static void Near(Vector3 a, Vector3 b) => Check(Vector3.Distance(a, b) < .0001f);
    private static void Test(string name, Action test)
    {
        try { test(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception error) { failed++; Console.Error.WriteLine("FAIL " + name + " :: " + error); }
    }
    private static void Throws(Action action)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        throw new Exception("Expected ArgumentException");
    }
    private static HandSample Sample(HandId hand, long sequence, double time, Vector3 point, float pinch = 0, bool tracked = true)
        => new HandSample(hand, sequence, time, P(point), pinch, tracked);

    private sealed class Ui
    {
        public readonly NearControlDriver Driver = new NearControlDriver();
        public ControlTarget[] Targets = { new ControlTarget(ControlAction.Pause, Vector3.Zero, .04f) };
        public ControlAction Allowed = ControlAction.Pause;
        public double Now;
        public long Sequence;
        public HandSample Last;
        public ControlAction Send(Vector3 p, float pinch, bool tracked = true)
        {
            Now += .01;
            Last = Sample(HandId.Left, ++Sequence, Now, p, pinch, tracked);
            return Driver.Step(Now, Last, default, Targets, Allowed);
        }
        public void Press() { Send(Vector3.Zero, 0); Send(Vector3.Zero, 1); Check(Driver.IsEngaged); }
    }

    private sealed class Harness
    {
        public readonly ScaleSession Session;
        public readonly HarnessControls Controls;
        public double Now;
        private long leftSequence, rightSequence;
        private HandSample left, right;
        public Harness()
        {
            var map = new DualScaleMap(new SpatialFrame(Pose3.Identity, 1), new SpatialFrame(P(new Vector3(0, 1, 0)), .2f));
            Session = new ScaleSession(map, Pose3.Identity, new InteractionBounds(new Vector3(-2), new Vector3(2)), Vector3.UnitX, .15f);
            Controls = new HarnessControls(Session);
        }
        public ControlTarget[] Targets()
        {
            if (Controls.Prompt != ControlPrompt.None)
                return new[] { new ControlTarget(ControlAction.Confirm, new Vector3(-.2f, .8f, 0), .04f), new ControlTarget(ControlAction.Cancel, new Vector3(.2f, .8f, 0), .04f) };
            var pause = (Session.PauseReasons & PauseReason.User) == 0 ? ControlAction.Pause : ControlAction.Resume;
            return new[] { new ControlTarget(pause, new Vector3(-.2f, .8f, 0), .04f), new ControlTarget(ControlAction.Restart, new Vector3(0, .8f, 0), .04f), new ControlTarget(ControlAction.Reposition, new Vector3(.2f, .8f, 0), .04f) };
        }
        public ControlEffect Send(Vector3 point, float pinch, HandId hand = HandId.Left, ControlTarget[] overrideTargets = null)
        {
            Now += .01;
            if (hand == HandId.Left) left = Sample(hand, ++leftSequence, Now, point, pinch);
            else right = Sample(hand, ++rightSequence, Now, point, pinch);
            return Controls.Step(Now, left, right, overrideTargets ?? Targets());
        }
        public ControlEffect Click(ControlAction action, HandId hand = HandId.Left)
        {
            Vector3 point = default;
            bool found = false;
            foreach (var target in Targets()) if (target.Action == action) { found = true; point = target.Center; }
            Check(found, "Control is not in the current layout: " + action);
            Send(point, 0, hand);
            Send(point, 1, hand);
            return Send(point, 0, hand);
        }
        public void Grab()
        {
            var p = Session.Map.MiniatureView(Session.ObjectPose).Position;
            Send(p, 0); Send(p, 1);
            Check(Session.State == ProbeState.Held, "Probe was not captured");
        }
        public void Return()
        {
            Grab();
            Send(Session.Map.MiniatureView(P(Vector3.UnitX)).Position, 0);
            Check(Session.ReturnCount == 1);
        }
    }

    private static int Main()
    {
        Test("control target rejects invalid action, radius and position", () =>
        {
            Throws(() => new ControlTarget(ControlAction.None, Vector3.Zero, .04f));
            Throws(() => new ControlTarget(ControlAction.Pause | ControlAction.Resume, Vector3.Zero, .04f));
            Throws(() => new ControlTarget(ControlAction.Pause, new Vector3(float.NaN), .04f));
            Throws(() => new ControlTarget(ControlAction.Pause, Vector3.Zero, 0));
            Check(!default(ControlTarget).IsValid);
        });
        Test("open then pinch does not activate before release", () => { var u = new Ui(); u.Press(); Check(u.Driver.PressedAction == ControlAction.Pause); });
        Test("release at the same token activates exactly once", () =>
        {
            var u = new Ui(); u.Press(); Check(u.Send(Vector3.Zero, 0) == ControlAction.Pause);
            Check(u.Send(Vector3.Zero, 0) == ControlAction.None);
        });
        Test("closed hand on startup cannot press", () => { var u = new Ui(); u.Send(Vector3.Zero, 1); Check(!u.Driver.IsEngaged); });
        Test("missed pinch cannot sweep into a token", () =>
        { var u = new Ui(); u.Send(Vector3.One, 0); u.Send(Vector3.One, 1); u.Send(Vector3.Zero, 1); Check(!u.Driver.IsEngaged); });
        Test("moving away cancels without activation", () =>
        { var u = new Ui(); u.Press(); Check(u.Send(Vector3.One, 1) == ControlAction.None); Check(!u.Driver.IsEngaged); Check(u.Send(Vector3.Zero, 0) == ControlAction.None); });
        Test("release in retention margin does not activate", () =>
        { var u = new Ui(); u.Press(); Check(u.Send(new Vector3(.05f, 0, 0), 0) == ControlAction.None); Check(!u.Driver.IsEngaged); });
        Test("pinch hysteresis retains a pressed token", () =>
        { var u = new Ui(); u.Press(); Check(u.Send(Vector3.Zero, .5f) == ControlAction.None); Check(u.Driver.IsEngaged); });
        Test("explicit tracking loss cancels menu", () =>
        { var u = new Ui(); u.Press(); u.Send(Vector3.Zero, 0, false); Check(!u.Driver.IsEngaged); });
        Test("missing callbacks and duplicate sequence cannot activate", () =>
        {
            var u = new Ui(); u.Press();
            Check(u.Driver.Step(u.Now + .3, u.Last, default, u.Targets, u.Allowed) == ControlAction.None);
            Check(!u.Driver.IsEngaged);
        });
        Test("invalid release pose cancels instead of activating stale target", () =>
        {
            var u = new Ui(); u.Press(); double t = u.Now + .01;
            Check(u.Driver.Step(t, new HandSample(HandId.Left, ++u.Sequence, t, default, 0, true), default, u.Targets, u.Allowed) == ControlAction.None);
            Check(!u.Driver.IsEngaged);
        });
        Test("NaN pinch cannot act like release", () =>
        { var u = new Ui(); u.Press(); Check(u.Send(Vector3.Zero, float.NaN) == ControlAction.None); Check(!u.Driver.IsEngaged); });
        Test("future timestamp is rejected and cannot mature", () =>
        {
            var u = new Ui(); u.Press();
            var future = Sample(HandId.Left, ++u.Sequence, 5, Vector3.Zero, 0);
            Check(u.Driver.Step(.04, future, default, u.Targets, u.Allowed) == ControlAction.None);
            Check(u.Driver.Step(5, future, default, u.Targets, u.Allowed) == ControlAction.None);
        });
        Test("moving a pressed target cancels it", () =>
        { var u = new Ui(); u.Press(); u.Targets[0] = new ControlTarget(ControlAction.Pause, new Vector3(.001f, 0, 0), .04f); Check(u.Send(Vector3.Zero, 0) == ControlAction.None); });
        Test("disabling a pressed action cancels it", () =>
        { var u = new Ui(); u.Press(); u.Allowed = ControlAction.None; Check(u.Send(Vector3.Zero, 0) == ControlAction.None); });
        Test("blocked gameplay hand cannot press menu", () =>
        {
            var u = new Ui(); u.Send(Vector3.Zero, 0);
            u.Driver.Step(.02, Sample(HandId.Left, 2, .02, Vector3.Zero, 1), default, u.Targets, u.Allowed, HandId.Left);
            Check(!u.Driver.IsEngaged);
        });
        Test("both hands pressing simultaneously have one deterministic owner", () =>
        {
            var u = new Ui();
            u.Driver.Step(0, Sample(HandId.Left, 1, 0, Vector3.Zero), Sample(HandId.Right, 1, 0, Vector3.Zero), u.Targets, u.Allowed);
            u.Driver.Step(.01, Sample(HandId.Left, 2, .01, Vector3.Zero, 1), Sample(HandId.Right, 2, .01, Vector3.Zero, 1), u.Targets, u.Allowed);
            Check(u.Driver.Owner == HandId.Left);
        });
        Test("right hand alone can activate menu", () =>
        {
            var u = new Ui();
            u.Driver.Step(0, default, Sample(HandId.Right, 1, 0, Vector3.Zero), u.Targets, u.Allowed);
            u.Driver.Step(.01, default, Sample(HandId.Right, 2, .01, Vector3.Zero, 1), u.Targets, u.Allowed);
            Check(u.Driver.Step(.02, default, Sample(HandId.Right, 3, .02, Vector3.Zero, 0), u.Targets, u.Allowed) == ControlAction.Pause);
        });
        Test("invalid clock or duplicate targets fail before changing ownership", () =>
        {
            var u = new Ui(); u.Press();
            Throws(() => u.Driver.Step(double.NaN, default, default, u.Targets, u.Allowed));
            Throws(() => u.Driver.Step(0, default, default, u.Targets, u.Allowed));
            Throws(() => u.Driver.Step(.03, default, default, new[] { u.Targets[0], u.Targets[0] }, u.Allowed));
            Check(u.Driver.IsEngaged);
        });
        Test("Pause and Resume operate through hand samples", () =>
        {
            var h = new Harness(); h.Click(ControlAction.Pause);
            Check((h.Session.PauseReasons & PauseReason.User) != 0);
            h.Click(ControlAction.Resume); Check((h.Session.PauseReasons & PauseReason.User) == 0);
            h.Grab();
        });
        Test("right-hand-only paused startup can resume and restart", () =>
        {
            var h = new Harness(); h.Controls.SetExternalPause(PauseReason.User, true);
            h.Click(ControlAction.Resume, HandId.Right);
            h.Click(ControlAction.Restart, HandId.Right); Check(h.Controls.Prompt == ControlPrompt.Restart);
            h.Click(ControlAction.Confirm, HandId.Right); Check(h.Controls.Prompt == ControlPrompt.None);
        });
        Test("confirmation pauses simulation clock", () =>
        {
            var h = new Harness(); h.Click(ControlAction.Restart); double before = h.Session.ElapsedSeconds;
            h.Session.Tick(.1f); Check(h.Session.ElapsedSeconds == before && (h.Session.PauseReasons & PauseReason.Menu) != 0);
        });
        Test("restart does not reset until fresh confirmation gesture", () =>
        {
            var h = new Harness(); h.Return(); h.Click(ControlAction.Restart);
            Check(h.Session.ReturnCount == 1 && h.Controls.Prompt == ControlPrompt.Restart);
            h.Click(ControlAction.Confirm); Check(h.Session.ReturnCount == 0 && h.Session.State == ProbeState.Available);
        });
        Test("cancel restart preserves probe and prior user pause", () =>
        {
            var h = new Harness(); h.Return(); h.Controls.SetExternalPause(PauseReason.User, true);
            h.Click(ControlAction.Restart); h.Click(ControlAction.Cancel);
            Check(h.Session.ReturnCount == 1 && (h.Session.PauseReasons & PauseReason.User) != 0);
            Check((h.Session.PauseReasons & PauseReason.Menu) == 0);
        });
        Test("confirmed restart preserves prior user pause", () =>
        {
            var h = new Harness(); h.Controls.SetExternalPause(PauseReason.User, true);
            h.Click(ControlAction.Restart); h.Click(ControlAction.Confirm);
            Check((h.Session.PauseReasons & PauseReason.User) != 0);
        });
        Test("menu wins over an overlapping probe and cannot click through", () =>
        {
            var h = new Harness(); Vector3 p = h.Session.Map.MiniatureView(h.Session.ObjectPose).Position;
            var targets = new[] { new ControlTarget(ControlAction.Pause, p, .04f) };
            h.Send(p, 0, overrideTargets: targets); h.Send(p, 1, overrideTargets: targets);
            Check(h.Controls.Menu.IsEngaged && h.Session.State == ProbeState.Available);
            h.Send(p, 0, overrideTargets: targets); Check(h.Session.State == ProbeState.Available);
        });
        Test("other hand may pause an active capture without a reward", () =>
        {
            var h = new Harness(); h.Grab(); h.Click(ControlAction.Pause, HandId.Right);
            Check(h.Session.State == ProbeState.Available && h.Session.ReturnCount == 0);
            Check((h.Session.PauseReasons & PauseReason.User) != 0);
        });
        Test("held probe hand cannot simultaneously activate a token", () =>
        {
            var h = new Harness(); h.Grab(); var p = h.Targets()[0].Center;
            h.Send(p, 1); h.Send(p, 0);
            Check((h.Session.PauseReasons & PauseReason.User) == 0 && !h.Controls.Menu.IsEngaged);
        });
        Test("focus loss cancels prompt without resetting progress", () =>
        {
            var h = new Harness(); h.Return(); h.Click(ControlAction.Restart);
            h.Controls.SetExternalPause(PauseReason.FocusLost, true);
            Check(h.Controls.Prompt == ControlPrompt.None && h.Session.ReturnCount == 1);
            Check(h.Controls.AllowedActions == ControlAction.None && (h.Session.PauseReasons & PauseReason.Menu) == 0);
        });
        Test("external callers cannot clear menu or tracking pause", () =>
        {
            var h = new Harness();
            Throws(() => h.Controls.SetExternalPause(PauseReason.Menu, false));
            Throws(() => h.Controls.SetExternalPause(PauseReason.TrackingLost, false));
        });
        Test("reposition has no effect until confirmation", () =>
        {
            var h = new Harness(); var original = h.Session.Map;
            h.Click(ControlAction.Reposition); Check(ReferenceEquals(original, h.Session.Map));
            Check(!h.Controls.CompleteReposition(new SpatialFrame(P(new Vector3(3)), .2f)));
            h.Click(ControlAction.Cancel); Check(ReferenceEquals(original, h.Session.Map));
        });
        Test("confirmed reposition preserves room frame and canonical pose", () =>
        {
            var h = new Harness(); var oldRoom = h.Session.Map.Room; var oldPose = h.Session.ObjectPose;
            h.Click(ControlAction.Reposition); Check(h.Click(ControlAction.Confirm) == ControlEffect.RepositionRequested);
            var mini = new SpatialFrame(P(new Vector3(1, 2, 3)), .25f);
            Check(h.Controls.CompleteReposition(mini));
            Check(ReferenceEquals(oldRoom, h.Session.Map.Room)); Near(oldPose.Position, h.Session.ObjectPose.Position);
            Check(ReferenceEquals(mini, h.Session.Map.Miniature) && h.Controls.Prompt == ControlPrompt.None);
        });
        Test("failed reposition keeps old map and offers retry or cancel", () =>
        {
            var h = new Harness(); var old = h.Session.Map;
            h.Click(ControlAction.Reposition); h.Click(ControlAction.Confirm);
            Check(!h.Controls.CompleteReposition(null)); Check(ReferenceEquals(old, h.Session.Map));
            Check(h.Controls.Prompt == ControlPrompt.Reposition);
            h.Click(ControlAction.Cancel); Check(h.Controls.Prompt == ControlPrompt.None);
        });
        Test("focus interruption rejects a late reposition completion", () =>
        {
            var h = new Harness(); var old = h.Session.Map;
            h.Click(ControlAction.Reposition); h.Click(ControlAction.Confirm);
            h.Controls.SetExternalPause(PauseReason.FocusLost, true);
            Check(!h.Controls.CompleteReposition(new SpatialFrame(P(Vector3.One), .2f)));
            Check(ReferenceEquals(old, h.Session.Map));
        });
        Test("Resume cannot acknowledge an unexpected placement fault", () =>
        {
            var h = new Harness(); h.Controls.SetExternalPause(PauseReason.User, true);
            h.Controls.SetExternalPause(PauseReason.Placement, true);
            Check((h.Controls.AllowedActions & ControlAction.Resume) == 0);
        });
        Test("placement helper preserves scale and uses horizontal head direction", () =>
        {
            var current = new SpatialFrame(Pose3.Identity, .25f);
            var head = new Pose3(new Vector3(2, 1.5f, 3), Quaternion.CreateFromYawPitchRoll((float)Math.PI / 2, .3f, .2f));
            Check(MiniaturePlacement.TryInFrontOf(head, current, out SpatialFrame result));
            Near(result.Origin.Position, new Vector3(2.45f, 1.2f, 3));
            Check(result.Scale == .25f); Near(Vector3.Transform(Vector3.UnitZ, result.Origin.Rotation), Vector3.UnitX);
        });
        Test("placement helper rejects vertical gaze and invalid tuning", () =>
        {
            var current = new SpatialFrame(Pose3.Identity, .2f);
            var up = new Pose3(Vector3.Zero, Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2));
            Check(!MiniaturePlacement.TryInFrontOf(up, current, out _));
            Check(!MiniaturePlacement.TryInFrontOf(default, current, out _));
            Check(!MiniaturePlacement.TryInFrontOf(Pose3.Identity, current, out _, float.NaN));
            Check(!MiniaturePlacement.TryInFrontOf(Pose3.Identity, current, out _, .8f));
        });
        Test("5000 seeded frames keep menu and probe ownership exclusive", () =>
        {
            var h = new Harness(); var random = new Random(48021);
            for (int i = 0; i < 5000; i++)
            {
                Vector3 point = i % 5 == 0 ? h.Session.Map.MiniatureView(h.Session.ObjectPose).Position
                    : h.Targets()[random.Next(h.Targets().Length)].Center;
                h.Send(point, (float)random.NextDouble(), random.Next(2) == 0 ? HandId.Left : HandId.Right);
                Check(!(h.Controls.Menu.IsEngaged && h.Session.State == ProbeState.Held));
                Check(h.Session.ReturnCount >= 0 && h.Session.ReturnCount <= 1);
                if (h.Controls.Prompt != ControlPrompt.None) Check((h.Session.PauseReasons & PauseReason.Menu) != 0);
                if (h.Controls.AwaitingReposition) h.Controls.CompleteReposition(null);
            }
        });
        Console.WriteLine($"CONTROLS: {passed} passed, {failed} failed. Synthetic input into real core; not Unity, rendering or Quest tests.");
        return failed == 0 ? 0 : 1;
    }
}
