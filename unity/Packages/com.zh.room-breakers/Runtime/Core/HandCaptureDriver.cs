using System;
using System.Numerics;

namespace RoomBreakers.Core
{
    // Timestamp is monotonic receipt time, not wall-clock time or a claimed sensor timestamp.
    // Sequence must advance only when the source receives genuinely new tracking data.
    public readonly struct HandSample
    {
        public HandId Hand { get; }
        public long Sequence { get; }
        public double Timestamp { get; }
        public Pose3 Grip { get; }
        public float PinchStrength { get; }
        public bool Tracked { get; }
        public HandSample(HandId hand, long sequence, double timestamp, Pose3 grip, float pinchStrength, bool tracked)
        {
            Hand = hand; Sequence = sequence; Timestamp = timestamp;
            Grip = grip; PinchStrength = pinchStrength; Tracked = tracked;
        }
    }

    public sealed class HandCaptureSettings
    {
        public double TimeoutSeconds { get; }
        public float PinchOn { get; }
        public float PinchOff { get; }
        public float SelectionRadiusMeters { get; }
        // Proposed tuning values; these are not measured Quest comfort/safety limits.
        public HandCaptureSettings(double timeoutSeconds = .20, float pinchOn = .80f,
            float pinchOff = .35f, float selectionRadiusMeters = .06f)
        {
            if (!Finite(timeoutSeconds) || timeoutSeconds < .01 || timeoutSeconds > 2)
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
            if (!SpatialMath.IsFinite(pinchOn) || !SpatialMath.IsFinite(pinchOff) ||
                pinchOff < 0 || pinchOn > 1 || pinchOn <= pinchOff)
                throw new ArgumentException("Pinch thresholds require 0 <= off < on <= 1.");
            if (!SpatialMath.IsFinite(selectionRadiusMeters) || selectionRadiusMeters < .005f || selectionRadiusMeters > .25f)
                throw new ArgumentOutOfRangeException(nameof(selectionRadiusMeters));
            TimeoutSeconds = timeoutSeconds; PinchOn = pinchOn; PinchOff = pinchOff;
            SelectionRadiusMeters = selectionRadiusMeters;
        }
        internal static bool Finite(double x) => !double.IsNaN(x) && !double.IsInfinity(x);
    }

    // Single-threaded input arbitration for the one-entity vertical slice.
    // Owns TrackingLost; other pause reasons must be changed through SetExternalPause.
    // Does not persist hand data, infer missing samples, or detect physical obstacles.
    public sealed class HandCaptureDriver
    {
        private sealed class Slot
        {
            public long Sequence;
            public HandSample Sample;
            public bool Accepted, NewThisStep, Armed;
            public double LastAcceptedTime = double.NegativeInfinity;
        }
        private readonly ScaleSession session;
        private readonly HandCaptureSettings settings;
        private readonly Slot left = new Slot();
        private readonly Slot right = new Slot();
        private double lastNow = double.NegativeInfinity;
        private bool recoveryRequired = true;
        public bool RequiresOpenHand => recoveryRequired;
        public HandId HoverHand { get; private set; }
        public bool LeftFresh { get; private set; }
        public bool RightFresh { get; private set; }

        public HandCaptureDriver(ScaleSession session, HandCaptureSettings settings = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.settings = settings ?? new HandCaptureSettings();
            RequireRearm();
        }

        // Feed both latest samples once per frame, even when neither source changed.
        // default(HandSample) means no sample. Duplicate frames never refresh the watchdog.
        public void Step(double now, HandSample leftSample, HandSample rightSample)
        {
            if (!HandCaptureSettings.Finite(now) || now < 0 || now < lastNow)
                throw new ArgumentOutOfRangeException(nameof(now), "Use a nondecreasing monotonic clock.");
            lastNow = now;
            Consume(left, HandId.Left, leftSample, now);
            Consume(right, HandId.Right, rightSample, now);
            LeftFresh = Fresh(left, now); RightFresh = Fresh(right, now);
            HoverHand = HandId.None;
            if (!LeftFresh) left.Armed = false;
            if (!RightFresh) right.Armed = false;

            bool ownerLost = (session.Owner == HandId.Left && !LeftFresh) ||
                (session.Owner == HandId.Right && !RightFresh);
            if (ownerLost || (!LeftFresh && !RightFresh))
            {
                RequireRearm();
                return;
            }
            if ((session.PauseReasons & ~PauseReason.TrackingLost) != PauseReason.None)
            {
                RequireRearm();
                return;
            }
            if (recoveryRequired)
            {
                // Recovery requires a NEW, fresh, open sample. Cached open poses are insufficient.
                bool openLeft = LeftFresh && left.NewThisStep && left.Sample.PinchStrength <= settings.PinchOff;
                bool openRight = RightFresh && right.NewThisStep && right.Sample.PinchStrength <= settings.PinchOff;
                if (!openLeft && !openRight) return;
                recoveryRequired = false;
                session.SetPause(PauseReason.TrackingLost, false);
                left.Armed = openLeft; right.Armed = openRight;
                return;
            }
            if (session.IsPaused) { RequireRearm(); return; }
            if (session.State == ProbeState.Returned)
            {
                left.Armed = right.Armed = false;
                return;
            }
            if (session.State == ProbeState.Held)
            {
                Slot owner = session.Owner == HandId.Left ? left : right;
                Slot other = session.Owner == HandId.Left ? right : left;
                other.Armed = false;
                if (!owner.NewThisStep) return;
                HandId hand = session.Owner;
                // Always process the release-frame pose BEFORE deciding to commit.
                session.MoveCapture(hand, owner.Sample.Grip);
                if (owner.Sample.PinchStrength <= settings.PinchOff)
                {
                    session.ReleaseCapture(hand);
                    owner.Armed = true;
                }
                return;
            }

            double ld = LeftFresh ? DistanceSquared(left.Sample.Grip.Position) : double.PositiveInfinity;
            double rd = RightFresh ? DistanceSquared(right.Sample.Grip.Position) : double.PositiveInfinity;
            double radius2 = (double)settings.SelectionRadiusMeters * settings.SelectionRadiusMeters;
            if (ld <= radius2 || rd <= radius2) HoverHand = ld <= rd ? HandId.Left : HandId.Right;
            bool l = RisingPinch(left, LeftFresh) && ld <= radius2;
            bool r = RisingPinch(right, RightFresh) && rd <= radius2;
            if (!l && !r) return;
            // Nearest wins; a tie goes to left. Callback order cannot steal an existing capture.
            HandId winner = l && (!r || ld <= rd) ? HandId.Left : HandId.Right;
            Slot chosen = winner == HandId.Left ? left : right;
            if (session.BeginCapture(winner, chosen.Sample.Grip))
                left.Armed = right.Armed = false;
        }

        private void Consume(Slot slot, HandId expected, HandSample sample, double now)
        {
            slot.NewThisStep = false;
            if (sample.Hand != expected || sample.Sequence <= slot.Sequence) return;
            slot.Sequence = sample.Sequence;
            slot.Sample = sample;
            slot.NewThisStep = true;
            slot.Accepted = sample.Tracked && sample.Grip.IsValid &&
                SpatialMath.IsFinite(sample.PinchStrength) && sample.PinchStrength >= 0 && sample.PinchStrength <= 1 &&
                HandCaptureSettings.Finite(sample.Timestamp) && sample.Timestamp >= 0 &&
                sample.Timestamp >= slot.LastAcceptedTime && sample.Timestamp <= now &&
                now - sample.Timestamp <= settings.TimeoutSeconds;
            if (slot.Accepted) slot.LastAcceptedTime = sample.Timestamp;
            else slot.Armed = false;
        }
        private bool Fresh(Slot slot, double now) => slot.Accepted &&
            now - slot.Sample.Timestamp <= settings.TimeoutSeconds;
        private bool RisingPinch(Slot slot, bool fresh)
        {
            if (!fresh || !slot.NewThisStep) return false;
            if (slot.Sample.PinchStrength <= settings.PinchOff) { slot.Armed = true; return false; }
            if (slot.Sample.PinchStrength < settings.PinchOn) return false;
            bool triggered = slot.Armed;
            slot.Armed = false; // Even a missed grab consumes the pinch; no closed-hand sweeping.
            return triggered;
        }
        private double DistanceSquared(Vector3 point)
        {
            Vector3 target = session.Map.MiniatureView(session.ObjectPose).Position;
            double x = (double)point.X - target.X, y = (double)point.Y - target.Y, z = (double)point.Z - target.Z;
            return x * x + y * y + z * z;
        }
        private void RequireRearm()
        {
            recoveryRequired = true;
            left.Armed = right.Armed = false;
            HoverHand = HandId.None;
            session.SetPause(PauseReason.TrackingLost, true);
        }
        public void SetExternalPause(PauseReason reason, bool enabled)
        {
            if (reason == PauseReason.TrackingLost)
                throw new ArgumentException("TrackingLost is owned by the hand driver.", nameof(reason));
            session.SetPause(reason, enabled);
            RequireRearm();
        }
        public void Cancel()
        {
            if (session.State == ProbeState.Held) session.CancelCapture(session.Owner);
            RequireRearm();
        }
        public void Reset() { session.Reset(); RequireRearm(); }
        public bool Recalibrate(DualScaleMap map)
        {
            if (!session.Recalibrate(map)) return false;
            RequireRearm();
            return true;
        }
    }
}
