using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using RoomBreakers.Core;

internal static class Program
{
    private static int passed, failed;
    private static void Require(bool b, string message = "Assertion failed") { if (!b) throw new Exception(message); }
    private static void Test(string name, Action test)
    {
        try { test(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + " :: " + e.Message); }
    }
    private static void Throws(Action a)
    { try { a(); } catch (ArgumentException) { return; } throw new Exception("Expected invalid input rejection"); }
    private static RunResult Win(int integrity = 3) => new RunResult(Guid.NewGuid(), true, 4, 4, integrity);
    private static RunResult Loss() => new RunResult(Guid.NewGuid(), false, 2, 4, 0);
    private static string Rehash(string body)
    {
        using (var sha = SHA256.Create()) return body + "|" + BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(body))).Replace("-", "").ToLowerInvariant();
    }
    private sealed class Temp : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "rb-experience-" + Guid.NewGuid().ToString("N"));
        public string PathFor(string name) => Path.Combine(Root, name);
        public Temp() { Directory.CreateDirectory(Root); }
        public void Dispose() { Directory.Delete(Root, true); }
    }
    private sealed class Player
    {
        public FirstEncounter Game { get; }
        public EncounterExperience Experience { get; }
        private double now;
        private long sequence;
        public Player(string path, bool shell = true)
        {
            Require(RoomPlanner.TryPlan(SyntheticRooms.Rectangular(), new Pose3(new Vector3(0, 1.5f, 0), Quaternion.Identity), out RoomPlan plan, out string error), error);
            var map = new DualScaleMap(new SpatialFrame(Pose3.Identity, 1), new SpatialFrame(new Pose3(new Vector3(0, 1, 0), Quaternion.Identity), .2f));
            Game = new FirstEncounter(plan, map, includeShell: shell);
            Experience = new EncounterExperience(Game, new ProgressJournal(path), true); Experience.Observe();
        }
        public void Frame(Pose3 p, float pinch = 0, float dt = 0, bool tracked = true)
        {
            now += Math.Max(.01, dt);
            var sample = new HandSample(HandId.Right, ++sequence, now, Game.Session.Map.MiniatureView(p), pinch, tracked);
            Game.Step(now, dt, default, sample, Array.Empty<ControlTarget>()); Experience.Observe();
        }
        public void Idle(int frames = 1)
        { for (int i = 0; i < frames; i++) Frame(new Pose3(new Vector3(0, 3, -1), Quaternion.Identity), dt: .1f); }
        public void Grab()
        { Frame(Game.Session.ObjectPose); Frame(Game.Session.ObjectPose); Frame(Game.Session.ObjectPose, 1); Require(Game.Session.State == ProbeState.Held); }
        public void Return()
        { Grab(); Frame(new Pose3(Game.Session.ReturnCenter, Game.Session.ObjectPose.Rotation)); }
        public void ReachShell() { Return(); Return(); Return(); Require(Game.ActiveIsReflector); }
        public void Orient()
        {
            Grab(); Frame(new Pose3(Game.Session.ObjectPose.Position, Game.Duel.Lane.SolutionRotation), 1);
            Frame(Game.Session.ObjectPose);
        }
        public void WaitVulnerable()
        { for (int i = 0; i < 400 && Game.Phase != EncounterPhase.ShellVulnerable; i++) Idle(); Require(Game.Phase == EncounterPhase.ShellVulnerable); }
        public void WinRun() { ReachShell(); Orient(); WaitVulnerable(); Return(); Require(Game.Phase == EncounterPhase.Won); }
    }
    private static int Main()
    {
        Test("terminal result validates IDs, goal, capture and integrity", () =>
        { Throws(() => new RunResult(Guid.Empty, true, 4, 4, 3)); Throws(() => new RunResult(Guid.NewGuid(), true, 3, 4, 3)); Throws(() => new RunResult(Guid.NewGuid(), false, 2, 4, 1)); Throws(() => new RunResult(Guid.NewGuid(), true, 4, 4, 0)); });
        Test("empty progress roundtrips", () =>
        { string s = LocalProgress.Empty.Encode(); Require(LocalProgress.Decode(s, out LocalProgress p) == ProgressDecode.Valid && p.Completed == 0 && p.Encode() == s); });
        Test("win loss and perfect defense update independent totals", () =>
        { var p = LocalProgress.Empty.With(Win(2)).With(Loss()).With(Win()); Require(p.Completed == 3 && p.Wins == 2 && p.PerfectWins == 1 && p.BestIntegrity == 3 && p.TotalReturned == 10); });
        Test("duplicate recent outcome leaves snapshot unchanged", () =>
        { var r = Win(); var p = LocalProgress.Empty.With(r); Require(ReferenceEquals(p, p.With(r))); });
        Test("beacon unlocks at one three and five wins, not losses", () =>
        { var p = LocalProgress.Empty.With(Loss()); Require(p.BeaconTier == 0); for (int i = 1; i <= 5; i++) { p = p.With(Win()); Require(p.BeaconTier == (i >= 5 ? 3 : i >= 3 ? 2 : 1)); } });
        Test("bounded recent results do not grow save files without limit", () =>
        { var p = LocalProgress.Empty; for (int i = 0; i < 100; i++) p = p.With(Win()); Require(p.Completed == 100 && p.RecentRuns.Count == 16 && p.Encode().Length < 1024); });
        Test("checksum detects changed totals", () =>
        { string s = LocalProgress.Empty.With(Win()).Encode().Replace("|1|1|1|3|4|", "|2|1|1|3|4|"); Require(LocalProgress.Decode(s, out _) == ProgressDecode.Corrupt); });
        Test("malformed empty oversized and truncated saves are rejected", () =>
        { foreach (string s in new[] { "", "hello", new string('a', 5000), "RBPROGRESS|1|0" }) Require(LocalProgress.Decode(s, out _) == ProgressDecode.Corrupt); });
        Test("future schema is distinguished from corruption", () => Require(LocalProgress.Decode("RBPROGRESS|2|future", out _) == ProgressDecode.Unsupported));
        Test("recomputed checksum does not bypass invalid counter invariants", () =>
        { foreach (string body in new[] { "RBPROGRESS|1|0|5|0|0|0|", "RBPROGRESS|1|-1|0|0|0|0|", "RBPROGRESS|1|1000001|0|0|0|0|", "RBPROGRESS|1|0|0|0|0|1|" }) Require(LocalProgress.Decode(Rehash(body), out _) == ProgressDecode.Corrupt); });
        Test("duplicate saved run IDs are rejected", () =>
        { string id = Guid.NewGuid().ToString("N"); Require(LocalProgress.Decode(Rehash("RBPROGRESS|1|2|2|2|3|8|" + id + "," + id), out _) == ProgressDecode.Corrupt); });
        Test("codec is independent of system number culture", () =>
        { var old = CultureInfo.CurrentCulture; try { CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-CL"); var p = LocalProgress.Empty.With(Win()); Require(LocalProgress.Decode(p.Encode(), out var q) == ProgressDecode.Valid && q.Wins == 1); } finally { CultureInfo.CurrentCulture = old; } });
        Test("first real filesystem save survives reopening", () =>
        { using (var t = new Temp()) { var j = new ProgressJournal(t.Root); j.Record(Win()); Require(j.Dirty && j.Flush() && !j.Dirty); Require(new ProgressJournal(t.Root).Current.Wins == 1); } });
        Test("writes alternate and preserve the previous readable checkpoint", () =>
        { using (var t = new Temp()) { var j = new ProgressJournal(t.Root); j.Record(Win()); Require(j.Flush()); string a = File.ReadAllText(t.PathFor("progress-a.txt")); j.Record(Win()); Require(j.Flush()); Require(File.ReadAllText(t.PathFor("progress-a.txt")) == a); Require(new ProgressJournal(t.Root).Current.Wins == 2); } });
        Test("truncated newest slot recovers the older checkpoint", () =>
        { using (var t = new Temp()) { var j = new ProgressJournal(t.Root); j.Record(Win()); j.Flush(); j.Record(Win()); j.Flush(); File.WriteAllText(t.PathFor("progress-b.txt"), "partial"); var reopened = new ProgressJournal(t.Root); Require(reopened.Recovered && reopened.Current.Wins == 1 && !reopened.IsReadOnly); reopened.Record(Win()); Require(reopened.Flush()); } });
        Test("corrupt older slot does not erase a newer good result", () =>
        { using (var t = new Temp()) { var j = new ProgressJournal(t.Root); j.Record(Win()); j.Flush(); j.Record(Win()); j.Flush(); File.WriteAllText(t.PathFor("progress-a.txt"), "broken"); Require(new ProgressJournal(t.Root).Current.Wins == 2); } });
        Test("two corrupt slots are preserved rather than silently reset on disk", () =>
        { using (var t = new Temp()) { File.WriteAllText(t.PathFor("progress-a.txt"), "broken-a"); File.WriteAllText(t.PathFor("progress-b.txt"), "broken-b"); var j = new ProgressJournal(t.Root); j.Record(Win()); Require(j.IsReadOnly && !j.Flush() && j.Current.Wins == 1); Require(File.ReadAllText(t.PathFor("progress-a.txt")) == "broken-a"); } });
        Test("future schema is never overwritten even beside a readable slot", () =>
        { using (var t = new Temp()) { var j = new ProgressJournal(t.Root); j.Record(Win()); j.Flush(); File.WriteAllText(t.PathFor("progress-b.txt"), "RBPROGRESS|2|future"); var r = new ProgressJournal(t.Root); Require(r.Current.Wins == 1 && r.IsReadOnly); r.Record(Win()); Require(!r.Flush() && File.ReadAllText(t.PathFor("progress-b.txt")) == "RBPROGRESS|2|future"); } });
        Test("filesystem failure keeps in-memory outcome and permits later retry", () =>
        { using (var t = new Temp()) { string p = t.PathFor("blocked"); File.WriteAllText(p, "not a directory"); var j = new ProgressJournal(p); j.Record(Win()); Require(!j.Flush() && j.Current.Wins == 1 && j.Dirty); File.Delete(p); Require(j.Flush() && new ProgressJournal(p).Current.Wins == 1); } });
        Test("unchanged or duplicate outcomes do not rewrite saved bytes", () =>
        { using (var t = new Temp()) { var j = new ProgressJournal(t.Root); var r = Win(); j.Record(r); j.Flush(); string s = File.ReadAllText(t.PathFor("progress-a.txt")); Require(!j.Record(r) && j.Flush()); Require(File.ReadAllText(t.PathFor("progress-a.txt")) == s && !File.Exists(t.PathFor("progress-b.txt"))); } });
        Test("newer results from another writer are not overwritten", () =>
        { using (var t = new Temp()) { var a = new ProgressJournal(t.Root); var b = new ProgressJournal(t.Root); a.Record(Win()); a.Flush(); b.Record(Loss()); Require(!b.Flush() && b.IsReadOnly && new ProgressJournal(t.Root).Current.Wins == 1); } });
        Test("practice and device directories remain separate", () =>
        { using (var t = new Temp()) { var p = new ProgressJournal(t.PathFor("practice")); p.Record(Win()); p.Flush(); Require(new ProgressJournal(t.PathFor("device")).Current.Wins == 0); } });
        Test("overlarge save is rejected without unbounded parsing", () =>
        { using (var t = new Temp()) { File.WriteAllText(t.PathFor("progress-a.txt"), new string('x', 5000)); Require(new ProgressJournal(t.Root).IsReadOnly); } });
        Test("active and room-invalid games never become completed results", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); Require(RunResult.From(p.Game) == null); p.Game.InvalidateRoom(); Require(RunResult.From(p.Game) == null); p.Experience.Observe(); Require(p.Experience.Journal.Current.Completed == 0); } });
        Test("run ID survives stage changes and changes only on restart", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); Guid id = p.Game.RunId; p.ReachShell(); Require(p.Game.RunId == id); p.Game.Controls.ResetProbe(); p.Idle(); Require(p.Game.RunId != id && p.Game.RunId != Guid.Empty); } });
        Test("full one-hand Mote-reflector-Shell victory saves exactly once", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.WinRun(); for (int i = 0; i < 100; i++) p.Experience.Observe(); var j = new ProgressJournal(t.Root); Require(j.Current.Completed == 1 && j.Current.Wins == 1 && j.Current.PerfectWins == 1 && j.Current.TotalReturned == 4); Require(p.Experience.ResultSummary.Contains("PERFECT")); } });
        Test("restarting after victory retains earlier progress and records a second run", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.WinRun(); p.Game.Controls.ResetProbe(); p.Idle(); Require(p.Experience.ResultSummary == "" && p.Experience.Journal.Current.Wins == 1); p.WinRun(); Require(new ProgressJournal(t.Root).Current.Wins == 2); } });
        Test("three Motes in extended mode do not prematurely save a victory", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.ReachShell(); Require(p.Experience.Journal.Current.Completed == 0 && p.Game.Phase == EncounterPhase.Reflector); } });
        Test("failed defense records a completed loss but grants no beacon", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root, false); p.Return(); for (int i = 0; i < 3000 && p.Game.Phase != EncounterPhase.Lost; i++) p.Idle(); Require(p.Game.Phase == EncounterPhase.Lost); Require(p.Experience.Journal.Current.Completed == 1 && p.Experience.Journal.Current.Wins == 0 && p.Experience.Journal.Current.BeaconTier == 0); } });
        Test("Pip asks for tracking recovery before giving gameplay hints", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); Require(p.Experience.Coach.Target == GuideTarget.None && p.Experience.Coach.Headline.Contains("open hand")); } });
        Test("idle lesson escalates hints at six and fourteen active seconds", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.Idle(65); Require(p.Experience.Coach.HintLevel == 1); p.Idle(80); Require(p.Experience.Coach.HintLevel == 2 && p.Game.Phase == EncounterPhase.LearnCapture && p.Game.Missed == 0); } });
        Test("user pause does not age guidance or consume simulation time", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.Idle(30); double idle = p.Experience.Coach.IdleSeconds; p.Game.Controls.SetExternalPause(PauseReason.User, true); p.Idle(200); Require(p.Experience.Coach.IdleSeconds == idle && p.Experience.Coach.Target == GuideTarget.None); } });
        Test("capture changes Pip's guide from creature to return zone", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.Idle(); Require(p.Experience.Coach.Target == GuideTarget.Creature); p.Grab(); Require(p.Experience.Coach.Target == GuideTarget.ReturnZone); } });
        Test("successful return creates a short feedback pulse, not another reward", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.Return(); Require(p.Experience.Coach.ReturnPulseRemaining > 0 && p.Experience.Coach.ReturnPulseSerial == 1 && p.Game.Captured == 1); p.Experience.Observe(); Require(p.Experience.Coach.ReturnPulseSerial == 1); } });
        Test("return feedback and phase beats freeze during pause", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.Return(); double pulse = p.Experience.Coach.ReturnPulseRemaining; p.Game.Controls.SetExternalPause(PauseReason.User, true); p.Idle(50); Require(p.Experience.Coach.ReturnPulseRemaining == pulse); } });
        Test("reflector guidance explains rotation and confirms an aligned solution", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.ReachShell(); p.Idle(2); Require(p.Experience.Coach.Target == GuideTarget.Reflector); p.Grab(); p.Frame(new Pose3(p.Game.Session.ObjectPose.Position, p.Game.Duel.Lane.SolutionRotation), 1, .01f); Require(p.Experience.Coach.Instruction.Contains("Open to leave")); } });
        Test("vulnerable Shell changes guidance to capture without auto-solving", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.ReachShell(); p.Orient(); p.WaitVulnerable(); p.Idle(2); Require(p.Experience.Coach.Target == GuideTarget.Creature && p.Game.Captured == 3); } });
        Test("reset clears old result message and guidance pulse", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.Return(); p.Game.Controls.ResetProbe(); p.Idle(); Require(p.Experience.Coach.ReturnPulseRemaining == 0 && p.Experience.Coach.HintLevel == 0 && p.Experience.ResultSummary == ""); } });
        Test("observer never mutates pose, health or reward totals", () =>
        { using (var t = new Temp()) { var p = new Player(t.Root); p.Idle(); var pose = p.Game.Session.ObjectPose; int hp = p.Game.Integrity; for (int i = 0; i < 1000; i++) p.Experience.Observe(); Require(p.Game.Session.ObjectPose.Position == pose.Position && p.Game.Integrity == hp && p.Game.Captured == 0); } });
        Console.WriteLine($"EXPERIENCE: {passed} passed, {failed} failed. Real C# and temporary filesystem; synthetic rooms/hands; no Unity or device test.");
        return failed == 0 ? 0 : 1;
    }
}
