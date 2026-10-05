using System;
using System.IO;
using RoomBreakers.Core;

internal static class Program
{
    private static int passed, failed;
    private static void Require(bool condition) { if (!condition) throw new Exception("Assertion failed"); }
    private static RunResult Win() => new RunResult(Guid.NewGuid(), true, 4, 4, 3);
    private static void Test(string name, Action body)
    {
        try { body(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + " :: " + e.Message); }
    }
    private sealed class Temp : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "rb-recovery-" + Guid.NewGuid().ToString("N"));
        public string A => Path.Combine(Root, "progress-a.txt");
        public string B => Path.Combine(Root, "progress-b.txt");
        public Temp() { Directory.CreateDirectory(Root); }
        public void Dispose() { Directory.Delete(Root, true); }
    }
    private static int Main()
    {
        Test("memory-only progress remains usable without claiming a save", () =>
        { var j = ProgressJournal.InMemory(); j.Record(Win()); Require(j.Current.Wins == 1 && j.Dirty && j.IsReadOnly && !j.Flush()); });
        Test("ambiguous same-revision checkpoints are read-only on load", () =>
        { using (var t = new Temp()) { string a = LocalProgress.Empty.With(Win()).Encode(), b = LocalProgress.Empty.With(Win()).Encode(); File.WriteAllText(t.A, a); File.WriteAllText(t.B, b); var j = new ProgressJournal(t.Root); j.Record(Win()); Require(j.IsReadOnly && !j.Flush() && File.ReadAllText(t.A) == a && File.ReadAllText(t.B) == b); } });
        Test("a fully written outcome can recover a lost acknowledgment", () =>
        { using (var t = new Temp()) { var j = new ProgressJournal(t.Root); j.Record(Win()); File.WriteAllText(t.A, j.Current.Encode()); Require(j.Flush() && !j.Dirty && !j.IsReadOnly && !File.Exists(t.B)); } });
        Test("ambiguous history appearing before flush is never acknowledged as success", () =>
        { using (var t = new Temp()) { var j = new ProgressJournal(t.Root); j.Record(Win()); string a = j.Current.Encode(), b = LocalProgress.Empty.With(Win()).Encode(); File.WriteAllText(t.A, a); File.WriteAllText(t.B, b); Require(!j.Flush() && j.IsReadOnly && File.ReadAllText(t.A) == a && File.ReadAllText(t.B) == b); } });
        Test("unsupported format arriving after load is preserved", () =>
        { using (var t = new Temp()) { var j = new ProgressJournal(t.Root); j.Record(Win()); File.WriteAllText(t.A, "RBPROGRESS|9|future"); Require(!j.Flush() && j.IsReadOnly && File.ReadAllText(t.A) == "RBPROGRESS|9|future"); } });
        Test("several unsaved results survive a recoverable filesystem failure", () =>
        { using (var t = new Temp()) { string root = Path.Combine(t.Root, "unavailable"); File.WriteAllText(root, "file"); var j = new ProgressJournal(root); j.Record(Win()); Require(!j.Flush()); j.Record(Win()); Require(!j.Flush() && j.Current.Wins == 2); File.Delete(root); Require(j.Flush() && new ProgressJournal(root).Current.Wins == 2); } });
        Test("invalid UTF8 preserves the corrupt checkpoint", () =>
        { using (var t = new Temp()) { byte[] corrupt = { 0xff, 0xff, 0xff }; File.WriteAllBytes(t.A, corrupt); var j = new ProgressJournal(t.Root); j.Record(Win()); Require(j.IsReadOnly && !j.Flush() && File.ReadAllBytes(t.A)[0] == 0xff); } });
        Test("deduplication survives reopening the same recent result", () =>
        { using (var t = new Temp()) { var r = Win(); var first = new ProgressJournal(t.Root); first.Record(r); Require(first.Flush()); var second = new ProgressJournal(t.Root); Require(!second.Record(r) && !second.Dirty && second.Current.Completed == 1); } });
        Console.WriteLine($"PROGRESS RECOVERY: {passed} passed, {failed} failed. Real temporary filesystem, not Android or Unity storage validation.");
        return failed == 0 ? 0 : 1;
    }
}
