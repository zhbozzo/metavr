using System;
using System.Collections.Generic;
using System.Numerics;

namespace RoomBreakers.Core
{
    public enum EncounterPhase { LearnCapture, Defending, Won, Lost, RoomInvalid, Reflector, ShellVulnerable }

    // One encounter authority. Its active interactable changes from Mote to reflector to vulnerable Shell.
    // This is not a separate simulation for each rendered scale. Old input ownership never crosses stages.
    public sealed class FirstEncounter
    {
        private long observedRestart;
        private float distance;
        private readonly float spawnDistance, speed;
        private readonly ReflectionLane reflectionLane;
        private int moteMisses;
        public RoomPlan Plan { get; }
        public ScaleSession Session { get; private set; }
        public HarnessControls Controls { get; private set; }
        public ShellDuel Duel { get; private set; }
        public EncounterPhase Phase { get; private set; } = EncounterPhase.LearnCapture;
        public int Captured { get; private set; }
        public int Missed { get; private set; }
        public int CaptureGoal => reflectionLane == null ? 3 : 4;
        public int Integrity => Math.Max(0, 3 - Missed);
        public int CreatureSerial { get; private set; } = 1;
        public double ActiveSeconds { get; private set; }
        public bool RoomInvalidated { get; private set; }
        public bool CreatureVisible => Phase == EncounterPhase.LearnCapture || Phase == EncounterPhase.Defending ||
            Phase == EncounterPhase.Reflector || Phase == EncounterPhase.ShellVulnerable;
        public bool ActiveIsReflector => Phase == EncounterPhase.Reflector;
        public bool ShellVisible => Duel != null && CreatureVisible;
        public bool ShowReturnZone => CreatureVisible && !ActiveIsReflector;
        public Pose3 CreaturePose => ActiveIsReflector ? new Pose3(Duel.Lane.ShellPosition, Quaternion.Identity) : Session.ObjectPose;
        public Pose3 ReflectorPose => Duel == null ? Pose3.Identity : ActiveIsReflector ? Session.ObjectPose :
            new Pose3(Duel.Lane.ReflectorPosition, Duel.ReflectorRotation);
        public float DistanceTravelled => distance;

        // Basic three-Mote mode remains available for regression/practice; device and desktop demo opt into Shell.
        public FirstEncounter(RoomPlan plan, DualScaleMap map, float metersPerSecond = .18f, bool includeShell = false)
        {
            Plan = plan ?? throw new ArgumentNullException(nameof(plan));
            if (!SpatialMath.IsFinite(metersPerSecond) || metersPerSecond < .02f || metersPerSecond > 1)
                throw new ArgumentOutOfRangeException(nameof(metersPerSecond));
            speed = metersPerSecond;
            spawnDistance = Math.Min(.65f, plan.RouteLength * .45f); distance = spawnDistance;
            if (includeShell)
            {
                if (!ReflectionLane.TryCreate(plan, out ReflectionLane lane, out string error)) throw new ArgumentException(error, nameof(plan));
                reflectionLane = lane;
            }
            BindSession(map, new Pose3(plan.PositionAt(spawnDistance), Quaternion.Identity), CaptureBehavior.ReturnToTarget, PauseReason.None);
        }
        private void BindSession(DualScaleMap map, Pose3 pose, CaptureBehavior behavior, PauseReason preserve)
        {
            Vector2 min = Plan.Room.Floor.Min, max = Plan.Room.Floor.Max;
            Session = new ScaleSession(map, pose,
                new InteractionBounds(new Vector3(min.X, .16f, min.Y), new Vector3(max.X, 4, max.Y)),
                behavior == CaptureBehavior.OrientInPlace ? pose.Position : Plan.ReturnCenter,
                .26f, (a, b) => Plan.Room.IsMotionClear(a, b), behavior);
            float radius = Duel == null ? .06f : Math.Min(.055f, reflectionLane.Length * map.Miniature.Scale * .4f);
            Controls = new HarnessControls(Session, new HandCaptureSettings(selectionRadiusMeters: Math.Max(.005f, radius)));
            foreach (PauseReason reason in new[] { PauseReason.User, PauseReason.FocusLost, PauseReason.Placement })
                if ((preserve & reason) != 0) Controls.SetExternalPause(reason, true);
            observedRestart = Controls.RestartSerial;
        }
        public ControlEffect Step(double now, float deltaSeconds, HandSample left, HandSample right,
            IReadOnlyList<ControlTarget> targets)
        {
            if (!SpatialMath.IsFinite(deltaSeconds) || deltaSeconds < 0) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            ControlEffect effect = Controls.Step(now, left, right, targets);
            if (observedRestart != Controls.RestartSerial)
            {
                DualScaleMap map = Session.Map; PauseReason pauses = Session.PauseReasons;
                Captured = Missed = moteMisses = 0; ActiveSeconds = 0; distance = spawnDistance; CreatureSerial++; Duel = null;
                Phase = RoomInvalidated ? EncounterPhase.RoomInvalid : EncounterPhase.LearnCapture;
                BindSession(map, new Pose3(Plan.PositionAt(spawnDistance), Quaternion.Identity), CaptureBehavior.ReturnToTarget, pauses);
                return effect;
            }
            if (!CreatureVisible) { Controls.Capture.Cancel(); return effect; }
            if (Session.IsPaused) return effect;
            // Rejected orientation samples cannot continue combat using a stale orientation.
            if (Session.State == ProbeState.Held && !Session.HasValidCaptureSample) return effect;
            float dt = Math.Min(deltaSeconds, .1f);
            Session.Tick(dt); ActiveSeconds += dt;
            if (Duel != null) { StepShell(dt); return effect; }
            if (Session.State == ProbeState.Returned)
            {
                Captured++;
                if (Captured >= 3)
                {
                    if (reflectionLane == null) Phase = EncounterPhase.Won;
                    else
                    {
                        moteMisses = Missed; Duel = new ShellDuel(reflectionLane, 3 - Missed);
                        BeginReflector();
                    }
                    return effect;
                }
                Phase = EncounterPhase.Defending; Respawn(); return effect;
            }
            if (Phase == EncounterPhase.LearnCapture || Session.State == ProbeState.Held) return effect;
            float nextDistance = Math.Min(Plan.RouteLength, distance + speed * dt);
            float travelled = 0;
            for (int i = 1; i < Plan.Route.Count; i++)
            {
                travelled += Vector3.Distance(Plan.Route[i - 1], Plan.Route[i]);
                if (travelled > distance && travelled < nextDistance && !Session.TryAdvance(new Pose3(Plan.Route[i], Quaternion.Identity)))
                { InvalidateRoom(); return effect; }
            }
            if (!Session.TryAdvance(new Pose3(Plan.PositionAt(nextDistance), Quaternion.Identity))) { InvalidateRoom(); return effect; }
            distance = nextDistance;
            if (distance >= Plan.RouteLength)
            {
                Missed++;
                if (Missed >= 3) { Phase = EncounterPhase.Lost; Controls.Capture.Cancel(); }
                else Respawn();
            }
            return effect;
        }
        private void StepShell(float dt)
        {
            if (Phase == EncounterPhase.ShellVulnerable && Session.State == ProbeState.Returned)
            {
                if (Duel.ResolveCapture()) { Captured++; Phase = EncounterPhase.Won; }
                return;
            }
            if (ActiveIsReflector && Session.OrientationCommitCount > 0 && Duel.Phase == ShellPhase.AwaitingOrientation) Duel.Begin();
            Duel.Tick(dt, ActiveIsReflector ? Session.ObjectPose.Rotation : Duel.ReflectorRotation,
                shellHeld: Phase == EncounterPhase.ShellVulnerable && Session.State == ProbeState.Held);
            Missed = moteMisses + Duel.Misses;
            if (Duel.Phase == ShellPhase.Lost) { Phase = EncounterPhase.Lost; Controls.Capture.Cancel(); return; }
            if (Phase == EncounterPhase.Reflector && Duel.Phase == ShellPhase.Vulnerable)
            {
                Phase = EncounterPhase.ShellVulnerable; CreatureSerial++;
                BindSession(Session.Map, new Pose3(reflectionLane.ShellPosition, Quaternion.Identity),
                    CaptureBehavior.ReturnToTarget, Session.PauseReasons);
            }
            else if (Phase == EncounterPhase.ShellVulnerable && Duel.Phase != ShellPhase.Vulnerable) BeginReflector();
        }
        private void BeginReflector()
        {
            DualScaleMap map = Session.Map; PauseReason pauses = Session.PauseReasons;
            Phase = EncounterPhase.Reflector; CreatureSerial++;
            BindSession(map, new Pose3(reflectionLane.ReflectorPosition, Duel.ReflectorRotation), CaptureBehavior.OrientInPlace, pauses);
        }
        private void Respawn() { Controls.Capture.Reset(); distance = spawnDistance; CreatureSerial++; }
        public void InvalidateRoom()
        {
            RoomInvalidated = true; Phase = EncounterPhase.RoomInvalid;
            Controls.SetExternalPause(PauseReason.Placement, true);
        }
    }

    public static class SyntheticRooms
    {
        public static RoomSnapshot Rectangular(bool obstruction = false)
        {
            var floor = new FloorPolygon(new[] { new Vector2(-2, -1), new Vector2(2, -1), new Vector2(2, 3), new Vector2(-2, 3) });
            var walls = new[] { new WallSpan(new Vector2(-2, 3), new Vector2(2, 3), 0, 2.7f),
                new WallSpan(new Vector2(-2, -1), new Vector2(-2, 3), 0, 2.7f),
                new WallSpan(new Vector2(2, 3), new Vector2(2, -1), 0, 2.7f),
                new WallSpan(new Vector2(2, -1), new Vector2(-2, -1), 0, 2.7f) };
            RoomObstacle[] boxes = obstruction
                ? new[] { new RoomObstacle(new Vector3(-.35f, 0, 1.35f), new Vector3(.35f, 1.5f, 2.15f)) }
                : Array.Empty<RoomObstacle>();
            return new RoomSnapshot(floor, walls, boxes, true);
        }
    }
}
