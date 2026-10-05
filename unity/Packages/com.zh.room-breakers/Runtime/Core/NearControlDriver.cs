using System;
using System.Collections.Generic;
using System.Numerics;

namespace RoomBreakers.Core
{
    [Flags]
    public enum ControlAction
    {
        None = 0, Pause = 1, Resume = 2, Restart = 4,
        Reposition = 8, Confirm = 16, Cancel = 32
    }

    // A pinchable control token, not an index-finger poke button.
    // Position and radius are in world meters, independent of miniature scale.
    public readonly struct ControlTarget
    {
        public ControlAction Action { get; }
        public Vector3 Center { get; }
        public float Radius { get; }
        public ControlTarget(ControlAction action, Vector3 center, float radius)
        {
            if (!IsSingleAction(action)) throw new ArgumentException("One control action is required.", nameof(action));
            if (!SpatialMath.IsFinite(center)) throw new ArgumentException("Control center must be finite.", nameof(center));
            if (!SpatialMath.IsFinite(radius) || radius < .005f || radius > .15f)
                throw new ArgumentOutOfRangeException(nameof(radius));
            Action = action;
            Center = center;
            Radius = radius;
        }
        public bool IsValid => IsSingleAction(Action) && SpatialMath.IsFinite(Center) &&
            SpatialMath.IsFinite(Radius) && Radius >= .005f && Radius <= .15f;
        public double DistanceSquared(Vector3 point)
        {
            if (!SpatialMath.IsFinite(point)) return double.PositiveInfinity;
            double x = (double)point.X - Center.X;
            double y = (double)point.Y - Center.Y;
            double z = (double)point.Z - Center.Z;
            return x * x + y * y + z * z;
        }
        public bool Contains(Vector3 point, float multiplier = 1f) =>
            DistanceSquared(point) <= (double)Radius * Radius * multiplier * multiplier;
        internal static bool IsSingleAction(ControlAction action)
        {
            int value = (int)action;
            return value > 0 && (value & ~63) == 0 && (value & (value - 1)) == 0;
        }
    }

    // One menu interaction at a time, with independent freshness checks for each hand.
    // Activation requires a NEW open sample over the same unmoved token after a pinch.
    // Cancellation never activates a token. No hand samples are logged or persisted.
    public sealed class NearControlDriver
    {
        private sealed class Slot
        {
            public long Sequence;
            public double LastAcceptedTime = double.NegativeInfinity;
            public HandSample Sample;
            public bool Accepted, NewThisStep, Armed;
        }
        private readonly Slot left = new Slot();
        private readonly Slot right = new Slot();
        private readonly HandCaptureSettings settings;
        private ControlTarget pressed;
        private double lastNow = double.NegativeInfinity;
        public HandId Owner { get; private set; }
        public ControlAction PressedAction => Owner == HandId.None ? ControlAction.None : pressed.Action;
        public ControlAction HoverAction { get; private set; }
        public bool IsEngaged => Owner != HandId.None;

        public NearControlDriver(HandCaptureSettings settings = null)
        {
            this.settings = settings ?? new HandCaptureSettings();
        }

        public ControlAction Step(double now, HandSample leftSample, HandSample rightSample,
            IReadOnlyList<ControlTarget> targets, ControlAction allowed, HandId blockedHand = HandId.None)
        {
            Validate(now, targets, allowed, blockedHand);
            lastNow = now;
            Consume(left, HandId.Left, leftSample, now);
            Consume(right, HandId.Right, rightSample, now);
            bool lf = Fresh(left, now), rf = Fresh(right, now);
            if (!lf || blockedHand == HandId.Left) left.Armed = false;
            if (!rf || blockedHand == HandId.Right) right.Armed = false;
            HoverAction = ControlAction.None;
            if (allowed == ControlAction.None) { Cancel(); return ControlAction.None; }

            if (IsEngaged)
            {
                Slot slot = Owner == HandId.Left ? left : right;
                bool fresh = Owner == HandId.Left ? lf : rf;
                if (!fresh || Owner == blockedHand || !StillAvailable(targets, allowed))
                { Cancel(); return ControlAction.None; }
                HoverAction = pressed.Action;
                if (!slot.NewThisStep) return ControlAction.None;
                // A small retention margin avoids cancelling on tiny motion while held.
                // Release must still be INSIDE the original target, not the margin.
                if (!pressed.Contains(slot.Sample.Grip.Position, 1.4f))
                { Cancel(); return ControlAction.None; }
                if (slot.Sample.PinchStrength > settings.PinchOff) return ControlAction.None;
                ControlAction result = pressed.Contains(slot.Sample.Grip.Position) ? pressed.Action : ControlAction.None;
                Cancel();
                return result;
            }

            int li = lf && blockedHand != HandId.Left ? Nearest(left.Sample.Grip.Position, targets, allowed) : -1;
            int ri = rf && blockedHand != HandId.Right ? Nearest(right.Sample.Grip.Position, targets, allowed) : -1;
            double ld = li < 0 ? double.PositiveInfinity : targets[li].DistanceSquared(left.Sample.Grip.Position);
            double rd = ri < 0 ? double.PositiveInfinity : targets[ri].DistanceSquared(right.Sample.Grip.Position);
            if (li >= 0 || ri >= 0) HoverAction = targets[ld <= rd ? li : ri].Action;
            // Consume even a missed pinch. Closing elsewhere and sweeping into a token cannot activate it.
            bool lp = Rising(left, lf && blockedHand != HandId.Left);
            bool rp = Rising(right, rf && blockedHand != HandId.Right);
            bool l = lp && li >= 0, r = rp && ri >= 0;
            if (!l && !r) return ControlAction.None;
            Owner = l && (!r || ld <= rd) ? HandId.Left : HandId.Right;
            pressed = targets[Owner == HandId.Left ? li : ri];
            left.Armed = right.Armed = false;
            HoverAction = pressed.Action;
            return ControlAction.None;
        }

        private void Validate(double now, IReadOnlyList<ControlTarget> targets, ControlAction allowed, HandId blocked)
        {
            if (!HandCaptureSettings.Finite(now) || now < 0 || now < lastNow)
                throw new ArgumentOutOfRangeException(nameof(now));
            if (targets == null || targets.Count > 6) throw new ArgumentException("Provide up to six control targets.", nameof(targets));
            if (((int)allowed & ~63) != 0) throw new ArgumentException("Unknown action mask.", nameof(allowed));
            if (blocked != HandId.None && blocked != HandId.Left && blocked != HandId.Right)
                throw new ArgumentException("Invalid blocked hand.", nameof(blocked));
            for (int i = 0; i < targets.Count; i++)
            {
                if (!targets[i].IsValid) throw new ArgumentException("Invalid control target.", nameof(targets));
                for (int j = 0; j < i; j++)
                    if (targets[i].Action == targets[j].Action)
                        throw new ArgumentException("Control actions must be unique.", nameof(targets));
            }
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
                sample.Timestamp >= slot.LastAcceptedTime && sample.Timestamp <= now && now - sample.Timestamp <= settings.TimeoutSeconds;
            if (slot.Accepted) slot.LastAcceptedTime = sample.Timestamp;
            else slot.Armed = false;
        }
        private bool Fresh(Slot slot, double now) => slot.Accepted && now - slot.Sample.Timestamp <= settings.TimeoutSeconds;
        private bool Rising(Slot slot, bool fresh)
        {
            if (!fresh || !slot.NewThisStep) return false;
            if (slot.Sample.PinchStrength <= settings.PinchOff) { slot.Armed = true; return false; }
            if (slot.Sample.PinchStrength < settings.PinchOn) return false;
            bool result = slot.Armed;
            slot.Armed = false;
            return result;
        }
        private static int Nearest(Vector3 point, IReadOnlyList<ControlTarget> targets, ControlAction allowed)
        {
            int best = -1;
            double distance = double.PositiveInfinity;
            for (int i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if ((allowed & target.Action) == 0 || !target.Contains(point)) continue;
                double candidate = target.DistanceSquared(point);
                if (candidate < distance || (candidate == distance && (best < 0 || (int)target.Action < (int)targets[best].Action)))
                { best = i; distance = candidate; }
            }
            return best;
        }
        private bool StillAvailable(IReadOnlyList<ControlTarget> targets, ControlAction allowed)
        {
            if ((allowed & pressed.Action) == 0) return false;
            for (int i = 0; i < targets.Count; i++)
                if (targets[i].Action == pressed.Action)
                    return targets[i].Center == pressed.Center && targets[i].Radius == pressed.Radius;
            return false;
        }
        public void Cancel()
        {
            Owner = HandId.None;
            HoverAction = ControlAction.None;
            left.Armed = right.Armed = false;
            // Preserve sequence watermarks: cached data cannot rearm a cancelled interaction.
        }
    }
}
