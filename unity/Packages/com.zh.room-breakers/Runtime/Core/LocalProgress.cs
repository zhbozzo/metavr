using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace RoomBreakers.Core
{
    // A terminal gameplay outcome only. No room, hand, account or device data.
    public sealed class RunResult
    {
        public Guid Id { get; }
        public bool Won { get; }
        public int Returned { get; }
        public int Integrity { get; }
        public RunResult(Guid id, bool won, int returned, int goal, int integrity)
        {
            if (id == Guid.Empty || (goal != 3 && goal != 4) || returned < 0 || returned > goal ||
                integrity < 0 || integrity > 3 || (won ? returned != goal || integrity == 0 : integrity != 0 || returned >= goal))
                throw new ArgumentException("A consistent terminal encounter outcome is required.");
            Id = id; Won = won; Returned = returned; Integrity = integrity;
        }
        public static RunResult From(FirstEncounter game)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));
            if (game.RoomInvalidated || (game.Phase != EncounterPhase.Won && game.Phase != EncounterPhase.Lost)) return null;
            return new RunResult(game.RunId, game.Phase == EncounterPhase.Won, game.Captured, game.CaptureGoal, game.Integrity);
        }
    }
    public enum ProgressDecode { Valid, Corrupt, Unsupported }

    public sealed class LocalProgress
    {
        public const int RecentLimit = 16;
        private const int MaximumRuns = 1000000;
        private readonly Guid[] recent;
        public int Completed { get; }
        public int Wins { get; }
        public int PerfectWins { get; }
        public int BestIntegrity { get; }
        public int TotalReturned { get; }
        public int BeaconTier => Wins >= 5 ? 3 : Wins >= 3 ? 2 : Wins >= 1 ? 1 : 0;
        public IReadOnlyList<Guid> RecentRuns { get; }
        public static LocalProgress Empty => new LocalProgress(0, 0, 0, 0, 0, Array.Empty<Guid>());
        private LocalProgress(int completed, int wins, int perfect, int best, int returned, Guid[] ids)
        {
            if (completed < 0 || completed > MaximumRuns || wins < 0 || wins > completed || perfect < 0 || perfect > wins ||
                best < 0 || best > 3 || (wins == 0 ? best != 0 : best == 0) ||
                (perfect > 0 && best != 3) || (best == 3 && perfect == 0) || returned < wins * 3 || returned > completed * 4 ||
                ids == null || ids.Length != Math.Min(completed, RecentLimit)) throw new ArgumentException("Invalid progress totals.");
            var unique = new HashSet<Guid>();
            foreach (Guid id in ids) if (id == Guid.Empty || !unique.Add(id)) throw new ArgumentException("Invalid recent outcome IDs.");
            Completed = completed; Wins = wins; PerfectWins = perfect; BestIntegrity = best; TotalReturned = returned;
            recent = (Guid[])ids.Clone(); RecentRuns = Array.AsReadOnly(recent);
        }
        public LocalProgress With(RunResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (Array.IndexOf(recent, result.Id) >= 0) return this;
            if (Completed == MaximumRuns) throw new InvalidOperationException("Progress limit reached.");
            var ids = new Guid[Math.Min(RecentLimit, recent.Length + 1)];
            int copied = ids.Length - 1;
            Array.Copy(recent, Math.Max(0, recent.Length - copied), ids, 0, copied); ids[copied] = result.Id;
            return new LocalProgress(Completed + 1, Wins + (result.Won ? 1 : 0),
                PerfectWins + (result.Won && result.Integrity == 3 ? 1 : 0),
                result.Won ? Math.Max(BestIntegrity, result.Integrity) : BestIntegrity,
                TotalReturned + result.Returned, ids);
        }
        public string Encode()
        {
            var ids = new string[recent.Length]; for (int i = 0; i < ids.Length; i++) ids[i] = recent[i].ToString("N");
            string body = string.Join("|", "RBPROGRESS", "1", Number(Completed), Number(Wins), Number(PerfectWins),
                Number(BestIntegrity), Number(TotalReturned), string.Join(",", ids));
            return body + "|" + Digest(body);
        }
        private static string Number(int n) => n.ToString(CultureInfo.InvariantCulture);
        private static string Digest(string text)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
        }
        public static ProgressDecode Decode(string text, out LocalProgress result)
        {
            result = null;
            if (string.IsNullOrEmpty(text) || text.Length > 4096) return ProgressDecode.Corrupt;
            string[] parts = text.Split('|');
            if (parts.Length < 2 || parts[0] != "RBPROGRESS") return ProgressDecode.Corrupt;
            if (parts[1] != "1") return ProgressDecode.Unsupported;
            if (parts.Length != 9 || parts[8] != Digest(text.Substring(0, text.LastIndexOf('|')))) return ProgressDecode.Corrupt;
            var numbers = new int[5];
            for (int i = 0; i < 5; i++)
                if (!int.TryParse(parts[i + 2], NumberStyles.None, CultureInfo.InvariantCulture, out numbers[i])) return ProgressDecode.Corrupt;
            string[] values = parts[7].Length == 0 ? Array.Empty<string>() : parts[7].Split(',');
            if (values.Length > RecentLimit) return ProgressDecode.Corrupt;
            var ids = new Guid[values.Length];
            for (int i = 0; i < values.Length; i++) if (!Guid.TryParseExact(values[i], "N", out ids[i])) return ProgressDecode.Corrupt;
            try { result = new LocalProgress(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4], ids); }
            catch (ArgumentException) { return ProgressDecode.Corrupt; }
            return ProgressDecode.Valid;
        }
    }

    // Local single-writer journal. Writes only the inactive checkpoint and verifies it.
    // Checksums detect corruption, not cheating. Not a power-loss-proof database or cloud save.
    public sealed class ProgressJournal
    {
        private readonly string directory;
        private int active = -1, persistedRevision;
        private string persistedPayload = LocalProgress.Empty.Encode();
        private bool readOnly;
        public LocalProgress Current { get; private set; } = LocalProgress.Empty;
        public bool Dirty { get; private set; }
        public bool Recovered { get; private set; }
        public bool IsReadOnly => readOnly;
        public string Status { get; private set; } = "Progress stays on this device.";
        private ProgressJournal()
        { directory = ""; readOnly = true; Status = "Storage is unavailable. Progress lasts for this session only."; }
        public static ProgressJournal InMemory() => new ProgressJournal();
        public ProgressJournal(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("A local progress directory is required.", nameof(directory));
            this.directory = Path.GetFullPath(directory); Load();
        }
        private string Slot(int index) => Path.Combine(directory, index == 0 ? "progress-a.txt" : "progress-b.txt");
        // -1 missing, 0 valid, 1 corrupt, 2 future, 3 unavailable.
        private int Read(int index, out LocalProgress value)
        {
            value = null;
            try
            {
                using (var file = new FileStream(Slot(index), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (file.Length > 4096) return 1;
                    using (var reader = new StreamReader(file, new UTF8Encoding(false, true)))
                        return (int)LocalProgress.Decode(reader.ReadToEnd(), out value);
                }
            }
            catch (FileNotFoundException) { return -1; }
            catch (DirectoryNotFoundException) { return -1; }
            catch (DecoderFallbackException) { return 1; }
            catch (Exception e) when (StorageFailure(e)) { return 3; }
        }
        private void Load()
        {
            int a = Read(0, out LocalProgress av), b = Read(1, out LocalProgress bv);
            if (a == 0 || b == 0)
            {
                active = a == 0 && (b != 0 || av.Completed >= bv.Completed) ? 0 : 1;
                Current = active == 0 ? av : bv; persistedRevision = Current.Completed; persistedPayload = Current.Encode();
            }
            bool ambiguous = a == 0 && b == 0 && av.Completed == bv.Completed && av.Encode() != bv.Encode();
            if (a == 2 || b == 2 || a == 3 || b == 3 || ambiguous || (active < 0 && (a != -1 || b != -1)))
            { readOnly = true; Status = "Saved progress unavailable. Playing without overwriting it."; return; }
            if (active >= 0 && (a == 1 || b == 1))
            { Recovered = true; Status = "Recovered the last readable progress checkpoint."; }
        }
        public bool Record(RunResult result)
        {
            LocalProgress next;
            try { next = Current.With(result); }
            catch (InvalidOperationException)
            { readOnly = true; Status = "Local progress limit reached. The game can continue."; return false; }
            if (ReferenceEquals(next, Current)) return false;
            Current = next; Dirty = true; return true;
        }
        public bool Flush()
        {
            if (!Dirty) return !readOnly;
            if (readOnly) return false;
            int a = Read(0, out LocalProgress av), b = Read(1, out LocalProgress bv);
            string desired = Current.Encode();
            if (a == 2 || b == 2 || a == 3 || b == 3)
            { readOnly = true; Status = "Progress changed elsewhere. This session has not overwritten it."; return false; }
            // An earlier write may have reached disk before verification was interrupted.
            if (a == 0 && av.Encode() == desired && (b != 0 || bv.Completed <= av.Completed)) { Accept(0); return true; }
            if (b == 0 && bv.Encode() == desired && (a != 0 || av.Completed <= bv.Completed)) { Accept(1); return true; }
            if (Conflicts(av, a) || Conflicts(bv, b))
            { readOnly = true; Status = "Progress changed elsewhere. This session has not overwritten it."; return false; }
            int target = active == 0 ? 1 : 0;
            try
            {
                Directory.CreateDirectory(directory);
                byte[] data = Encoding.UTF8.GetBytes(desired);
                using (var file = new FileStream(Slot(target), FileMode.Create, FileAccess.Write, FileShare.None))
                { file.Write(data, 0, data.Length); file.Flush(true); }
                if (Read(target, out LocalProgress verified) != 0 || verified.Encode() != desired)
                { Status = "Progress not saved. You can keep playing."; return false; }
                Accept(target); return true;
            }
            catch (Exception e) when (StorageFailure(e))
            { Status = "Progress not saved. You can keep playing."; return false; }
        }
        private bool Conflicts(LocalProgress value, int state) => state == 0 &&
            (value.Completed > persistedRevision || (value.Completed == persistedRevision && value.Encode() != persistedPayload));
        private void Accept(int slot)
        {
            active = slot; persistedRevision = Current.Completed; persistedPayload = Current.Encode();
            Dirty = false; Status = "Saved on this device.";
        }
        private static bool StorageFailure(Exception e) => e is IOException || e is UnauthorizedAccessException ||
            e is SecurityException || e is NotSupportedException;
    }
}
