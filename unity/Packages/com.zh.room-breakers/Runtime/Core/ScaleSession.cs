using System;
using System.Numerics;

namespace RoomBreakers.Core
{
    public enum HandId { None, Left, Right }
    public enum ProbeState { Available, Held, Returned }
    public enum CaptureBehavior { ReturnToTarget, OrientInPlace }
    [Flags] public enum PauseReason { None = 0, User = 1, TrackingLost = 2, FocusLost = 4, Placement = 8, Menu = 16 }

    // One authoritative interactable. The miniature and large view never simulate independently.
    public sealed class ScaleSession
    {
        public const string EntityId = "scale-probe-001";
        private readonly Pose3 initialPose;
        private readonly InteractionBounds bounds;
        private readonly Func<Vector3, Vector3, bool> motionConstraint;
        private Pose3 captureStart, handToObject;
        public DualScaleMap Map { get; private set; }
        public CaptureBehavior Behavior { get; }
        public Pose3 ObjectPose { get; private set; }
        public Pose3 LastHandWorld { get; private set; }
        public Vector3 ReturnCenter { get; }
        public float ReturnRadius { get; }
        public ProbeState State { get; private set; }
        public HandId Owner { get; private set; }
        public PauseReason PauseReasons { get; private set; }
        public bool IsPaused => PauseReasons != PauseReason.None;
        public int ReturnCount { get; private set; }
        public int OrientationCommitCount { get; private set; }
        public double ElapsedSeconds { get; private set; }
        public bool HasValidCaptureSample { get; private set; }
        public bool CanReturn => Behavior == CaptureBehavior.ReturnToTarget && State == ProbeState.Held &&
            HasValidCaptureSample && !IsPaused && Vector3.DistanceSquared(ObjectPose.Position, ReturnCenter) <= ReturnRadius * ReturnRadius;

        public ScaleSession(DualScaleMap map, Pose3 initial, InteractionBounds interactionBounds, Vector3 returnCenter,
            float returnRadius, Func<Vector3, Vector3, bool> motionConstraint = null,
            CaptureBehavior behavior = CaptureBehavior.ReturnToTarget)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            bounds = interactionBounds ?? throw new ArgumentNullException(nameof(interactionBounds));
            this.motionConstraint = motionConstraint;
            if (behavior != CaptureBehavior.ReturnToTarget && behavior != CaptureBehavior.OrientInPlace)
                throw new ArgumentOutOfRangeException(nameof(behavior));
            Behavior = behavior;
            if (!initial.IsValid || !bounds.Contains(initial.Position)) throw new ArgumentException("Initial pose must be valid and within bounds.", nameof(initial));
            if (!bounds.Contains(returnCenter)) throw new ArgumentException("Return center must be within bounds.", nameof(returnCenter));
            if (!SpatialMath.IsFinite(returnRadius) || returnRadius <= 0f || returnRadius > 100f) throw new ArgumentOutOfRangeException(nameof(returnRadius));
            if (!CanRepresent(map, initial) || !CanRepresent(map, new Pose3(returnCenter, initial.Rotation)) ||
                !MotionAllowed(initial.Position, initial.Position) || !MotionAllowed(returnCenter, returnCenter))
                throw new ArgumentException("Initial pose and destination must be valid in both views and room geometry.", nameof(map));
            initialPose = initial; ObjectPose = initial; ReturnCenter = returnCenter; ReturnRadius = returnRadius;
            LastHandWorld = Pose3.Identity;
        }
        public bool BeginCapture(HandId hand, Pose3 miniatureHand)
        {
            if (IsPaused || State != ProbeState.Available || !ValidHand(hand) || !miniatureHand.IsValid) return false;
            Pose3 relative, world;
            try
            {
                if (!WithinOrientationReach(miniatureHand)) return false;
                Pose3 canonical = Map.FromMiniature(miniatureHand);
                Quaternion inverse = Quaternion.Conjugate(canonical.Rotation);
                relative = new Pose3(Vector3.Transform(ObjectPose.Position - canonical.Position, inverse), inverse * ObjectPose.Rotation);
                world = Map.RoomView(canonical);
            }
            catch (ArgumentException) { return false; }
            captureStart = ObjectPose; handToObject = relative; LastHandWorld = world;
            Owner = hand; State = ProbeState.Held; HasValidCaptureSample = true; return true;
        }
        public bool MoveCapture(HandId hand, Pose3 miniatureHand)
        {
            if (IsPaused || State != ProbeState.Held || Owner != hand || !ValidHand(hand)) return false;
            HasValidCaptureSample = false;
            if (!miniatureHand.IsValid) return false;
            Pose3 candidate, world;
            try
            {
                if (!WithinOrientationReach(miniatureHand)) return false;
                Pose3 canonical = Map.FromMiniature(miniatureHand);
                Vector3 position = Behavior == CaptureBehavior.OrientInPlace ? initialPose.Position :
                    canonical.Position + Vector3.Transform(handToObject.Position, canonical.Rotation);
                candidate = new Pose3(position, canonical.Rotation * handToObject.Rotation);
                world = Map.RoomView(canonical);
            }
            catch (ArgumentException) { return false; }
            if (!CanMoveTo(candidate)) return false;
            ObjectPose = candidate; LastHandWorld = world; HasValidCaptureSample = true; return true;
        }
        // Short interaction leash in miniature-world meters, not a certified comfort/safety limit.
        private bool WithinOrientationReach(Pose3 hand) => Behavior != CaptureBehavior.OrientInPlace ||
            Vector3.DistanceSquared(Map.MiniatureView(initialPose).Position, hand.Position) <= .15f * .15f;
        public bool TryAdvance(Pose3 next)
        {
            if (Behavior != CaptureBehavior.ReturnToTarget || IsPaused || State != ProbeState.Available || !CanMoveTo(next)) return false;
            ObjectPose = next; return true;
        }
        private bool CanMoveTo(Pose3 candidate) => candidate.IsValid && bounds.Contains(candidate.Position) &&
            CanRepresent(Map, candidate) && MotionAllowed(ObjectPose.Position, candidate.Position);
        private bool MotionAllowed(Vector3 from, Vector3 to) => motionConstraint == null || motionConstraint(from, to);

        public bool ReleaseCapture(HandId hand)
        {
            if (IsPaused || State != ProbeState.Held || Owner != hand || !ValidHand(hand)) return false;
            if (Behavior == CaptureBehavior.OrientInPlace)
            {
                if (!HasValidCaptureSample) { CancelCapture(hand); return false; }
                // A placed orientation is not a returned enemy and cannot award score.
                State = ProbeState.Available; Owner = HandId.None; HasValidCaptureSample = false;
                OrientationCommitCount++; return true;
            }
            if (!CanReturn) { CancelCapture(hand); return false; }
            var returnedPose = new Pose3(ReturnCenter, ObjectPose.Rotation);
            if (!CanMoveTo(returnedPose)) { CancelCapture(hand); return false; }
            ObjectPose = returnedPose; Owner = HandId.None; State = ProbeState.Returned;
            HasValidCaptureSample = false; ReturnCount++; return true;
        }
        public bool CancelCapture(HandId hand)
        {
            if (State != ProbeState.Held || Owner != hand || !ValidHand(hand)) return false;
            ObjectPose = captureStart; Owner = HandId.None; State = ProbeState.Available;
            HasValidCaptureSample = false; return true;
        }
        public void SetPause(PauseReason reason, bool enabled)
        {
            const PauseReason known = PauseReason.User | PauseReason.TrackingLost | PauseReason.FocusLost | PauseReason.Placement | PauseReason.Menu;
            int value = (int)reason;
            if (value <= 0 || (value & ~(int)known) != 0 || (value & (value - 1)) != 0) throw new ArgumentException("Set one known pause reason at a time.", nameof(reason));
            if (enabled)
            {
                if (State == ProbeState.Held) CancelCapture(Owner);
                PauseReasons |= reason;
            }
            else PauseReasons &= ~reason;
        }
        public bool Recalibrate(DualScaleMap map)
        {
            if (map == null || !IsPaused || State == ProbeState.Held) return false;
            if (!CanRepresent(map, initialPose) || !CanRepresent(map, ObjectPose) || !CanRepresent(map, new Pose3(ReturnCenter, Quaternion.Identity))) return false;
            Map = map; return true;
        }
        public void Tick(float deltaSeconds)
        {
            if (!SpatialMath.IsFinite(deltaSeconds) || deltaSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (!IsPaused) ElapsedSeconds += Math.Min(deltaSeconds, .1f);
        }
        public void Reset()
        {
            ObjectPose = initialPose; Owner = HandId.None; State = ProbeState.Available;
            ReturnCount = OrientationCommitCount = 0; ElapsedSeconds = 0; LastHandWorld = Pose3.Identity; HasValidCaptureSample = false;
        }
        private static bool CanRepresent(DualScaleMap map, Pose3 canonical)
        {
            try { return map.RoomView(canonical).IsValid && map.MiniatureView(canonical).IsValid; }
            catch (ArgumentException) { return false; }
        }
        private static bool ValidHand(HandId hand) => hand == HandId.Left || hand == HandId.Right;
    }
}
