using System;
using System.Numerics;

namespace RoomBreakers.Core
{
    public enum HandId { None, Left, Right }
    public enum ProbeState { Available, Held, Returned }
    [Flags] public enum PauseReason { None = 0, User = 1, TrackingLost = 2, FocusLost = 4, Placement = 8 }

    // One entity for the first slice. Both views read this state; neither view simulates it.
    public sealed class ScaleSession
    {
        public const string EntityId = "scale-probe-001";
        private readonly Pose3 initialPose;
        private readonly InteractionBounds bounds;
        private Pose3 captureStart;
        private Pose3 handToObject;
        public DualScaleMap Map { get; private set; }
        public Pose3 ObjectPose { get; private set; }
        public Pose3 LastHandWorld { get; private set; }
        public Vector3 ReturnCenter { get; }
        public float ReturnRadius { get; }
        public ProbeState State { get; private set; }
        public HandId Owner { get; private set; }
        public PauseReason PauseReasons { get; private set; }
        public bool IsPaused => PauseReasons != PauseReason.None;
        public int ReturnCount { get; private set; }
        public double ElapsedSeconds { get; private set; }
        public bool HasValidCaptureSample { get; private set; }
        public bool CanReturn => State == ProbeState.Held && HasValidCaptureSample && !IsPaused && Vector3.DistanceSquared(ObjectPose.Position, ReturnCenter) <= ReturnRadius * ReturnRadius;

        public ScaleSession(DualScaleMap map, Pose3 initial, InteractionBounds interactionBounds, Vector3 returnCenter, float returnRadius)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            bounds = interactionBounds ?? throw new ArgumentNullException(nameof(interactionBounds));
            if (!initial.IsValid || !bounds.Contains(initial.Position)) throw new ArgumentException("Initial pose must be valid and within bounds.", nameof(initial));
            if (!bounds.Contains(returnCenter)) throw new ArgumentException("Return center must be within bounds.", nameof(returnCenter));
            if (!SpatialMath.IsFinite(returnRadius) || returnRadius <= 0f || returnRadius > 100f) throw new ArgumentOutOfRangeException(nameof(returnRadius));
            var returnPose = new Pose3(returnCenter, initial.Rotation);
            if (!CanRepresent(map, initial) || !CanRepresent(map, returnPose))
                throw new ArgumentException("Initial pose and return target must be finite in both views.", nameof(map));
            initialPose = initial;
            ObjectPose = initial;
            ReturnCenter = returnCenter;
            ReturnRadius = returnRadius;
            LastHandWorld = Pose3.Identity;
        }

        public bool BeginCapture(HandId hand, Pose3 miniatureHand)
        {
            if (IsPaused || State != ProbeState.Available || !ValidHand(hand) || !miniatureHand.IsValid) return false;
            Pose3 canonical;
            Pose3 relative;
            Pose3 world;
            try
            {
                canonical = Map.FromMiniature(miniatureHand);
                Quaternion inverse = Quaternion.Conjugate(canonical.Rotation);
                relative = new Pose3(Vector3.Transform(ObjectPose.Position - canonical.Position, inverse), inverse * ObjectPose.Rotation);
                world = Map.RoomView(canonical);
            }
            catch (ArgumentException) { return false; }
            captureStart = ObjectPose;
            handToObject = relative;
            LastHandWorld = world;
            Owner = hand;
            State = ProbeState.Held;
            HasValidCaptureSample = true;
            return true;
        }

        public bool MoveCapture(HandId hand, Pose3 miniatureHand)
        {
            // A foreign input must not invalidate the current owner's last sample.
            if (IsPaused || State != ProbeState.Held || Owner != hand || !ValidHand(hand)) return false;
            // Keep the last renderable pose, but never reward a release based on stale input.
            // A later valid sample can recover the current capture without a teleport.
            HasValidCaptureSample = false;
            if (!miniatureHand.IsValid) return false;
            Pose3 candidate;
            Pose3 world;
            try
            {
                Pose3 canonical = Map.FromMiniature(miniatureHand);
                candidate = new Pose3(canonical.Position + Vector3.Transform(handToObject.Position, canonical.Rotation), canonical.Rotation * handToObject.Rotation);
                world = Map.RoomView(canonical);
            }
            catch (ArgumentException) { return false; }
            if (!bounds.Contains(candidate.Position) || !CanRepresent(Map, candidate)) return false;
            ObjectPose = candidate;
            LastHandWorld = world;
            HasValidCaptureSample = true;
            return true;
        }

        // Adapters must deliver a current valid sample before release, or cancel when
        // tracking is stale. This domain guard handles explicitly rejected samples;
        // it cannot detect missing sensor callbacks without an adapter watchdog.
        public bool ReleaseCapture(HandId hand)
        {
            if (IsPaused || State != ProbeState.Held || Owner != hand || !ValidHand(hand)) return false;
            if (!CanReturn) { CancelCapture(hand); return false; }
            var returnedPose = new Pose3(ReturnCenter, ObjectPose.Rotation);
            if (!CanRepresent(Map, returnedPose)) { CancelCapture(hand); return false; }
            ObjectPose = returnedPose;
            Owner = HandId.None;
            State = ProbeState.Returned;
            HasValidCaptureSample = false;
            ReturnCount++;
            return true;
        }

        public bool CancelCapture(HandId hand)
        {
            if (State != ProbeState.Held || Owner != hand || !ValidHand(hand)) return false;
            ObjectPose = captureStart;
            Owner = HandId.None;
            State = ProbeState.Available;
            HasValidCaptureSample = false;
            return true;
        }

        public void SetPause(PauseReason reason, bool enabled)
        {
            int value = (int)reason;
            if (value <= 0 || (value & ~15) != 0 || (value & (value - 1)) != 0) throw new ArgumentException("Set one known pause reason at a time.", nameof(reason));
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
            // Validate all restorable poses before replacing either frame.
            if (!CanRepresent(map, initialPose) || !CanRepresent(map, ObjectPose) || !CanRepresent(map, new Pose3(ReturnCenter, Quaternion.Identity))) return false;
            Map = map;
            return true;
        }

        public void Tick(float deltaSeconds)
        {
            if (!SpatialMath.IsFinite(deltaSeconds) || deltaSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (!IsPaused) ElapsedSeconds += Math.Min(deltaSeconds, 0.1f);
        }

        public void Reset()
        {
            ObjectPose = initialPose;
            Owner = HandId.None;
            State = ProbeState.Available;
            ReturnCount = 0;
            ElapsedSeconds = 0;
            LastHandWorld = Pose3.Identity;
            HasValidCaptureSample = false;
            // Preserve pause reasons. Reset must not resume a tracking/focus fault.
        }

        private static bool CanRepresent(DualScaleMap map, Pose3 canonical)
        {
            try { return map.RoomView(canonical).IsValid && map.MiniatureView(canonical).IsValid; }
            catch (ArgumentException) { return false; }
        }
        private static bool ValidHand(HandId hand) => hand == HandId.Left || hand == HandId.Right;
    }
}
