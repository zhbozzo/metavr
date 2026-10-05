using System;
using System.Numerics;

namespace RoomBreakers.Core
{
    public enum GuideTarget { None, Creature, ReturnZone, Reflector }
    public enum PipMood { Curious, Guiding, Celebrating, Worried, Relieved }

    // Observes actual gameplay, never supplies input, moves entities or grants a win.
    // All hint/celebration clocks use active simulation time; pauses never age them.
    public sealed class EncounterCoach
    {
        private Guid run;
        private EncounterPhase phase;
        private int creature, captured, missed;
        private ProbeState probeState;
        private Pose3 lastGesturePose;
        private double previousTime, idleSeconds;
        public string Headline { get; private set; } = "Pip needs your help.";
        public string Instruction { get; private set; } = "Open a hand to begin.";
        public string Chapter { get; private set; } = "01 / YOUR ROOM, YOUR HANDS";
        public GuideTarget Target { get; private set; }
        public PipMood Mood { get; private set; }
        public int HintLevel { get; private set; }
        public double IdleSeconds => idleSeconds;
        public double BeatRemaining { get; private set; }
        public double ReturnPulseRemaining { get; private set; }
        public int ReturnPulseSerial { get; private set; }

        public void Observe(FirstEncounter game)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));
            bool newRun = run != game.RunId;
            if (newRun)
            {
                run = game.RunId; previousTime = game.ActiveSeconds; idleSeconds = 0;
                captured = game.Captured; missed = game.Missed; creature = game.CreatureSerial;
                phase = game.Phase; probeState = game.Session.State; lastGesturePose = game.Session.ObjectPose;
                BeatRemaining = 2; ReturnPulseRemaining = 0; ReturnPulseSerial = 0;
            }
            double dt = Math.Max(0, Math.Min(.1, game.ActiveSeconds - previousTime));
            previousTime = game.ActiveSeconds;
            BeatRemaining = Math.Max(0, BeatRemaining - dt);
            ReturnPulseRemaining = Math.Max(0, ReturnPulseRemaining - dt);
            bool transition = phase != game.Phase || creature != game.CreatureSerial;
            bool returned = game.Captured > captured;
            bool damaged = game.Missed > missed;
            // Autonomous walking is NOT player progress; only deliberate held movement resets the hint timer.
            bool gesture = game.Session.State == ProbeState.Held && game.Session.HasValidCaptureSample &&
                (Vector3.Distance(lastGesturePose.Position, game.Session.ObjectPose.Position) > .03f ||
                 Math.Abs(Quaternion.Dot(lastGesturePose.Rotation, game.Session.ObjectPose.Rotation)) < .998f);
            bool interactionChanged = probeState != game.Session.State;
            if (transition || returned || damaged || gesture || interactionChanged)
            { idleSeconds = 0; lastGesturePose = game.Session.ObjectPose; }
            else if (!game.Session.IsPaused) idleSeconds += dt;
            if (transition) BeatRemaining = 2;
            if (returned) { ReturnPulseRemaining = .8; ReturnPulseSerial++; }
            captured = game.Captured; missed = game.Missed; creature = game.CreatureSerial;
            phase = game.Phase; probeState = game.Session.State;
            HintLevel = idleSeconds >= 14 ? 2 : idleSeconds >= 6 ? 1 : 0;
            Target = GuideTarget.None; Mood = PipMood.Guiding;
            Chapter = game.Duel == null ? "01 / YOUR ROOM, YOUR HANDS" : "02 / TURN THE TABLES";

            if (game.RoomInvalidated)
            { Set("The room changed.", "Load the room again. No result was recorded."); return; }
            // Interruption instructions take priority over lessons and celebration.
            if ((game.Session.PauseReasons & (PauseReason.FocusLost | PauseReason.Placement)) != 0)
            { Set("Interaction paused.", "Restore tracking and confirm the play space."); return; }
            if (game.Controls.Prompt != ControlPrompt.None)
            { Set("Your choice.", "CONFIRM continues. CANCEL keeps this run."); return; }
            if ((game.Session.PauseReasons & PauseReason.User) != 0)
            { Set("Take your time.", "Choose RESUME when you are ready."); return; }
            if (game.Phase == EncounterPhase.Won)
            { Chapter = "RIFT CLOSED"; Mood = PipMood.Relieved; Set("Pip's refuge is safe.", "RESTART begins a new run. Your completed result stays."); return; }
            if (game.Phase == EncounterPhase.Lost)
            { Chapter = "TRY ANOTHER APPROACH"; Mood = PipMood.Worried; Set("The refuge went dark.", "RESTART to try again. The first lesson waits for you."); return; }
            if (game.Controls.Capture.RequiresOpenHand)
            { Mood = PipMood.Curious; Set("Let me see an open hand.", "Open your fingers in front of the headset."); return; }
            if ((game.Session.PauseReasons & PauseReason.Menu) != 0)
            { Set("Choose a control.", "Open over the same token, or move away to cancel."); return; }
            if (game.Session.State == ProbeState.Held && !game.Session.HasValidCaptureSample)
            { Mood = PipMood.Worried; Set("That move is blocked.", "Move back to clear space. No throw is required."); return; }
            if (ReturnPulseRemaining > 0) Mood = PipMood.Celebrating;
            if (game.ActiveIsReflector)
            {
                Target = GuideTarget.Reflector;
                bool held = game.Session.State == ProbeState.Held;
                bool aligned = game.Duel.PreviewHitsShell;
                Set("Shell's armor needs its own pulse.", held
                    ? (aligned ? "The path reaches Shell. Open to leave this angle." : "Turn your wrist until the preview reaches Shell.")
                    : "Pinch the reflector handle, turn it, then open.");
                if (HintLevel == 2 && !held) Instruction = "The small handle controls the big mirror. Pinch it before turning.";
                if (game.Duel.Phase == ShellPhase.Incoming || game.Duel.Phase == ShellPhase.Reflected)
                    Headline = "Watch the same pulse in both scales.";
                return;
            }
            if (game.Session.State == ProbeState.Held)
            {
                Target = GuideTarget.ReturnZone;
                Set(game.ShellVisible ? "You've got Shell." : "You control both versions.", game.Session.CanReturn
                    ? "Open your fingers now to send it through the rift."
                    : "Bring the tiny creature into the marked return zone.");
                if (HintLevel == 2 && !game.Session.CanReturn) Instruction = "Use the small rings beside the portal. Do not reach for the real wall.";
                return;
            }
            Target = GuideTarget.Creature;
            Set(game.ShellVisible ? "The armor is open. Now grab Shell!" : "Pinch the tiny creature.",
                HintLevel == 0 ? "Bring thumb and index together near its miniature."
                    : "Use the miniature in front of you. The big creature follows.");
            if (HintLevel == 2) Instruction = "Open first, move next to the tiny creature, then pinch. There is no throwing gesture.";
        }
        private void Set(string headline, string instruction) { Headline = headline; Instruction = instruction; }
    }

    // Bind once to an encounter and keep the journal across LOAD ROOM/restarts.
    public sealed class EncounterExperience
    {
        private readonly FirstEncounter game;
        private Guid recordedRun;
        public EncounterCoach Coach { get; } = new EncounterCoach();
        public ProgressJournal Journal { get; }
        public bool Practice { get; }
        public string ResultSummary { get; private set; } = "";
        public string SaveStatus => Journal.Dirty ? "This progress is in memory only. " + Journal.Status : Journal.Status;
        public EncounterExperience(FirstEncounter game, ProgressJournal journal, bool practice)
        {
            this.game = game ?? throw new ArgumentNullException(nameof(game));
            Journal = journal ?? throw new ArgumentNullException(nameof(journal)); Practice = practice;
        }
        public void Observe()
        {
            Coach.Observe(game);
            if (game.Phase != EncounterPhase.Won && game.Phase != EncounterPhase.Lost)
            { ResultSummary = ""; return; }
            if (recordedRun == game.RunId) return;
            RunResult result = RunResult.From(game);
            if (result == null) return;
            recordedRun = game.RunId;
            if (Journal.Record(result)) Journal.Flush(); // Once per outcome, never per rendered frame.
            ResultSummary = (result.Won && result.Integrity == 3 ? "PERFECT DEFENSE" : result.Won ? "REFUGE PROTECTED" : "RUN COMPLETE") +
                "  |  " + result.Returned + "/" + game.CaptureGoal + " returned  |  " + result.Integrity + "/3 lights";
        }
        public string ProgressLine
        {
            get
            {
                LocalProgress p = Journal.Current;
                string next = p.BeaconTier == 0 ? "First victory lights Pip's beacon." : p.BeaconTier == 1
                    ? (3 - p.Wins) + " more victories for beacon II." : p.BeaconTier == 2
                    ? (5 - p.Wins) + " more victories for beacon III." : "Pip's beacon is complete.";
                return (Practice ? "PRACTICE / " : "LOCAL / ") + p.Wins + " wins, " + p.PerfectWins + " perfect. " + next;
            }
        }
    }
}
