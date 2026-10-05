using System;
using System.Collections.Generic;
using System.Numerics;

namespace RoomBreakers.Core
{
    public enum EncounterPhase { LearnCapture, Defending, Won, Lost, RoomInvalid }

    // A deliberately small complete loop: learn one capture, then close one rift by returning three Motes.
    // One creature at a time reuses the proven capture/menu drivers; neither rendered scale owns gameplay.
    public sealed class FirstEncounter
    {
        private long observedRestart;
        private float distance;
        private readonly float spawnDistance, speed;
        public RoomPlan Plan { get; }
        public ScaleSession Session { get; }
        public HarnessControls Controls { get; }
        public EncounterPhase Phase { get; private set; } = EncounterPhase.LearnCapture;
        public int Captured { get; private set; }
        public int Missed { get; private set; }
        public int Integrity => Math.Max(0, 3 - Missed);
        public int CreatureSerial { get; private set; } = 1;
        public double ActiveSeconds { get; private set; }
        public bool RoomInvalidated { get; private set; }
        public bool CreatureVisible => Phase == EncounterPhase.LearnCapture || Phase == EncounterPhase.Defending;
        public float DistanceTravelled => distance;
        public FirstEncounter(RoomPlan plan, DualScaleMap map, float metersPerSecond = .18f)
        {
            Plan = plan ?? throw new ArgumentNullException(nameof(plan));
            if (!SpatialMath.IsFinite(metersPerSecond) || metersPerSecond < .02f || metersPerSecond > 1)
                throw new ArgumentOutOfRangeException(nameof(metersPerSecond));
            speed = metersPerSecond;
            spawnDistance = Math.Min(.65f, plan.RouteLength * .45f); distance = spawnDistance;
            Vector2 min = plan.Room.Floor.Min, max = plan.Room.Floor.Max;
            Session = new ScaleSession(map, new Pose3(plan.PositionAt(spawnDistance), Quaternion.Identity),
                new InteractionBounds(new Vector3(min.X, .16f, min.Y), new Vector3(max.X, 4, max.Y)),
                plan.ReturnCenter, .26f, (a, b) => plan.Room.IsMotionClear(a, b));
            Controls = new HarnessControls(Session);
        }
        public ControlEffect Step(double now, float deltaSeconds, HandSample left, HandSample right,
            IReadOnlyList<ControlTarget> targets)
        {
            if (!SpatialMath.IsFinite(deltaSeconds) || deltaSeconds < 0) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            ControlEffect effect = Controls.Step(now, left, right, targets);
            if (observedRestart != Controls.RestartSerial)
            {
                observedRestart = Controls.RestartSerial;
                Captured = Missed = 0; ActiveSeconds = 0; distance = spawnDistance; CreatureSerial++;
                Phase = RoomInvalidated ? EncounterPhase.RoomInvalid : EncounterPhase.LearnCapture;
                return effect;
            }
            if (!CreatureVisible)
            {
                // Keep the menu operable, but do not rearm gameplay on an invisible/final creature.
                Controls.Capture.Cancel(); return effect;
            }
            if (Session.IsPaused) return effect;
            float dt = Math.Min(deltaSeconds, .1f);
            Session.Tick(dt); ActiveSeconds += dt;
            if (Session.State == ProbeState.Returned)
            {
                Captured++;
                if (Captured >= 3) { Phase = EncounterPhase.Won; return effect; }
                Phase = EncounterPhase.Defending;
                Respawn(); return effect;
            }
            if (Phase == EncounterPhase.LearnCapture || Session.State == ProbeState.Held) return effect;
            float nextDistance = Math.Min(Plan.RouteLength, distance + speed * dt);
            Vector3 next = Plan.PositionAt(nextDistance);
            // Follow each segment rather than cutting across a corner when dt straddles it.
            float travelled = 0;
            for (int i = 1; i < Plan.Route.Count; i++)
            {
                travelled += Vector3.Distance(Plan.Route[i - 1], Plan.Route[i]);
                if (travelled > distance && travelled < nextDistance && !Session.TryAdvance(new Pose3(Plan.Route[i], Quaternion.Identity)))
                { InvalidateRoom(); return effect; }
            }
            if (!Session.TryAdvance(new Pose3(next, Quaternion.Identity))) { InvalidateRoom(); return effect; }
            distance = nextDistance;
            if (distance >= Plan.RouteLength)
            {
                Missed++;
                if (Missed >= 3) { Phase = EncounterPhase.Lost; Controls.Capture.Cancel(); }
                else Respawn();
            }
            return effect;
        }
        private void Respawn()
        {
            // Reset requires a fresh open hand. Holding the previous pinch cannot grab the next Mote.
            Controls.Capture.Reset(); distance = spawnDistance; CreatureSerial++;
        }
        public void InvalidateRoom()
        {
            RoomInvalidated = true; Phase = EncounterPhase.RoomInvalid;
            Controls.SetExternalPause(PauseReason.Placement, true);
        }
    }

    public static class SyntheticRooms
    {
        // Entirely original fixtures. Never substitute these silently for failed device data.
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
