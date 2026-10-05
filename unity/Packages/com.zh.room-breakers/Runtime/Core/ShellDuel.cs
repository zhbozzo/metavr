using System;
using System.Numerics;

namespace RoomBreakers.Core
{
    public enum ShellPhase { AwaitingOrientation, Charging, Incoming, Reflected, Vulnerable, Resolved, Lost }

    // A bounded optical lane in the already-validated room; never a replacement room.
    public sealed class ReflectionLane
    {
        public RoomPlan RoomPlan { get; }
        public Vector3 ShellPosition { get; }
        public Vector3 ReflectorPosition { get; }
        public Vector3 IncomingDirection { get; }
        public float Length { get; }
        public Quaternion SolutionRotation { get; }
        public Quaternion InitialRotation { get; }
        private ReflectionLane(RoomPlan plan, Vector3 shell, Vector3 reflector)
        {
            RoomPlan = plan; ShellPosition = shell; ReflectorPosition = reflector;
            Length = Vector3.Distance(shell, reflector);
            IncomingDirection = Vector3.Normalize(reflector - shell);
            float yaw = (float)Math.Atan2(IncomingDirection.X, IncomingDirection.Z);
            SolutionRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
            InitialRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw + (float)Math.PI / 4);
        }
        public static bool TryCreate(RoomPlan plan, out ReflectionLane lane, out string error)
        {
            lane = null; error = "No clear reflection lane. Face a different part of the room and load again.";
            if (plan == null) return false;
            Vector3 shell = plan.PositionAt(Math.Min(.30f, plan.RouteLength * .15f));
            if (!plan.Room.IsMotionClear(shell, plan.ReturnCenter)) return false;
            // Fixed 24 candidates, with disc clearance, and a direct return path for the captured Shell.
            for (int i = 1; i <= 24; i++)
            {
                Vector3 mirror = plan.PositionAt(plan.RouteLength * i / 24f);
                float distance = Vector3.Distance(shell, mirror);
                if (distance < .65f || distance > 1.35f || Math.Abs(shell.Y - mirror.Y) > .001f) continue;
                if (!plan.Room.IsMotionClear(mirror, mirror, .28f) || !plan.Room.IsMotionClear(shell, mirror, .18f)) continue;
                lane = new ReflectionLane(plan, shell, mirror); error = null; return true;
            }
            return false;
        }
    }

    public static class ProjectileMath
    {
        // Earliest entry into a sphere, not endpoint-only collision. Double intermediates avoid float overflow.
        public static bool TrySphereEntry(Vector3 from, Vector3 to, Vector3 center, float radius, out float fraction)
        {
            fraction = 0;
            if (!SpatialMath.IsFinite(from) || !SpatialMath.IsFinite(to) || !SpatialMath.IsFinite(center) ||
                !SpatialMath.IsFinite(radius) || radius <= 0) return false;
            double mx = (double)from.X - center.X, my = (double)from.Y - center.Y, mz = (double)from.Z - center.Z;
            double dx = (double)to.X - from.X, dy = (double)to.Y - from.Y, dz = (double)to.Z - from.Z;
            double c = mx * mx + my * my + mz * mz - (double)radius * radius;
            if (c <= 0) return true;
            double a = dx * dx + dy * dy + dz * dz, b = mx * dx + my * dy + mz * dz;
            if (a < 1e-20 || b >= 0) return false;
            double discriminant = b * b - a * c;
            if (discriminant < 0) return false;
            double t = (-b - Math.Sqrt(discriminant)) / a;
            if (t < 0 || t > 1) return false;
            fraction = (float)t; return true;
        }
    }

    // Single authoritative projectile. Both display scales read this object; no physics in either view.
    // The reflector stays on its plinth; only orientation changes. It cannot create extra shots.
    public sealed class ShellDuel
    {
        public const float HitRadius = .20f;
        private const float PulseRadius = .035f;
        private readonly int allowedMisses;
        private readonly float chargeSeconds, vulnerableSeconds, pulseSpeed;
        private float outgoingDistance;
        public ReflectionLane Lane { get; }
        public ShellPhase Phase { get; private set; } = ShellPhase.AwaitingOrientation;
        public Quaternion ReflectorRotation { get; private set; }
        public Vector3 ProjectilePosition { get; private set; }
        public Vector3 ReflectedDirection { get; private set; }
        public Vector3 PreviewEnd { get; private set; }
        public bool PreviewHitsShell { get; private set; }
        public int Misses { get; private set; }
        public int ReflectionCount { get; private set; }
        public int ShieldBreakCount { get; private set; }
        public int ShotSerial { get; private set; }
        public float RemainingSeconds { get; private set; }
        public float ChargeProgress => Phase == ShellPhase.Charging ? 1 - RemainingSeconds / chargeSeconds : 0;
        public bool ProjectileVisible => Phase == ShellPhase.Incoming || Phase == ShellPhase.Reflected;
        public bool Armored => Phase != ShellPhase.Vulnerable && Phase != ShellPhase.Resolved;
        private float MaxTravel => Lane.Length * 2 + 1;

        public ShellDuel(ReflectionLane lane, int allowedMisses = 3, float chargeSeconds = 1.25f,
            float vulnerableSeconds = 10, float pulseSpeed = .90f)
        {
            Lane = lane ?? throw new ArgumentNullException(nameof(lane));
            if (allowedMisses < 1 || allowedMisses > 3 || !SpatialMath.IsFinite(chargeSeconds) || chargeSeconds < .1f || chargeSeconds > 10 ||
                !SpatialMath.IsFinite(vulnerableSeconds) || vulnerableSeconds < .5f || vulnerableSeconds > 60 ||
                !SpatialMath.IsFinite(pulseSpeed) || pulseSpeed < .1f || pulseSpeed > 5)
                throw new ArgumentOutOfRangeException(nameof(allowedMisses), "Invalid duel tuning.");
            this.allowedMisses = allowedMisses; this.chargeSeconds = chargeSeconds;
            this.vulnerableSeconds = vulnerableSeconds; this.pulseSpeed = pulseSpeed;
            ReflectorRotation = lane.InitialRotation; ProjectilePosition = lane.ShellPosition;
            UpdatePreview();
        }
        // Called after a deliberate, valid orientation placement, not a closed hand on startup.
        public bool Begin()
        {
            if (Phase != ShellPhase.AwaitingOrientation) return false;
            Charge(); return true;
        }
        public void Tick(float deltaSeconds, Quaternion orientation, bool paused = false, bool shellHeld = false)
        {
            if (!SpatialMath.IsFinite(deltaSeconds) || deltaSeconds < 0 || !SpatialMath.IsValidRotation(orientation))
                throw new ArgumentException("Finite time and a valid reflector rotation are required.");
            if (paused || Phase == ShellPhase.Resolved || Phase == ShellPhase.Lost) return;
            ReflectorRotation = SpatialMath.Normalize(orientation); UpdatePreview();
            float dt = Math.Min(deltaSeconds, .1f);
            if (Phase == ShellPhase.AwaitingOrientation) return;
            if (Phase == ShellPhase.Vulnerable)
            {
                if (shellHeld) return; // Never reapply armor while the player holds Shell.
                RemainingSeconds = Math.Max(0, RemainingSeconds - dt);
                if (RemainingSeconds == 0) Charge();
                return;
            }
            if (Phase == ShellPhase.Charging)
            {
                RemainingSeconds = Math.Max(0, RemainingSeconds - dt);
                if (RemainingSeconds == 0)
                { Phase = ShellPhase.Incoming; ProjectilePosition = Lane.ShellPosition; ShotSerial++; }
                return;
            }
            float travel = pulseSpeed * dt;
            if (Phase == ShellPhase.Incoming)
            {
                float remaining = Vector3.Distance(ProjectilePosition, Lane.ReflectorPosition);
                Vector3 next = remaining <= travel ? Lane.ReflectorPosition : ProjectilePosition + Lane.IncomingDirection * travel;
                if (!Lane.RoomPlan.Room.IsMotionClear(ProjectilePosition, next, PulseRadius)) { Miss(); return; }
                ProjectilePosition = next;
                if (remaining > travel) return;
                Vector3 normal = Vector3.Transform(Vector3.UnitZ, ReflectorRotation);
                // Edge-on surface misses; no divide-by-zero or fabricated bounce.
                if (Math.Abs(Vector3.Dot(normal, Lane.IncomingDirection)) < .10f) { Miss(); return; }
                ReflectedDirection = SpatialMath.Reflect(Lane.IncomingDirection, normal);
                Phase = ShellPhase.Reflected; ReflectionCount++; outgoingDistance = 0;
                travel = Math.Max(0, travel - remaining);
            }
            if (Phase == ShellPhase.Reflected) AdvanceReflected(travel);
        }
        private void AdvanceReflected(float travel)
        {
            travel = Math.Min(travel, Math.Max(0, MaxTravel - outgoingDistance));
            Vector3 next = ProjectilePosition + ReflectedDirection * travel;
            bool hit = ProjectileMath.TrySphereEntry(ProjectilePosition, next, Lane.ShellPosition, HitRadius, out float fraction);
            Vector3 end = hit ? Vector3.Lerp(ProjectilePosition, next, fraction) : next;
            // A wall/furniture before the hit wins. An obstacle behind the hit does not erase it.
            if (!Lane.RoomPlan.Room.IsMotionClear(ProjectilePosition, end, PulseRadius)) { Miss(); return; }
            ProjectilePosition = end; outgoingDistance += travel * (hit ? fraction : 1);
            if (hit)
            { Phase = ShellPhase.Vulnerable; RemainingSeconds = vulnerableSeconds; ShieldBreakCount++; return; }
            if (outgoingDistance >= MaxTravel - .00001f) Miss();
        }
        private void Charge()
        {
            Phase = ShellPhase.Charging; RemainingSeconds = chargeSeconds;
            ProjectilePosition = Lane.ShellPosition; outgoingDistance = 0;
        }
        private void Miss()
        {
            Misses++; // One terminal outcome consumes the only active pulse.
            if (Misses >= allowedMisses) { Phase = ShellPhase.Lost; RemainingSeconds = 0; }
            else Charge();
        }
        public bool ResolveCapture()
        {
            if (Phase != ShellPhase.Vulnerable) return false;
            Phase = ShellPhase.Resolved; RemainingSeconds = 0; return true;
        }
        private void UpdatePreview()
        {
            Vector3 start = Lane.ReflectorPosition;
            Vector3 normal = Vector3.Transform(Vector3.UnitZ, ReflectorRotation);
            if (Math.Abs(Vector3.Dot(normal, Lane.IncomingDirection)) < .10f)
            { PreviewEnd = start; PreviewHitsShell = false; return; }
            Vector3 direction = SpatialMath.Reflect(Lane.IncomingDirection, normal);
            Vector3 end = start + direction * MaxTravel;
            bool hit = ProjectileMath.TrySphereEntry(start, end, Lane.ShellPosition, HitRadius, out float fraction);
            if (hit) end = Vector3.Lerp(start, end, fraction);
            PreviewHitsShell = hit && Lane.RoomPlan.Room.IsMotionClear(start, end, PulseRadius);
            if (!Lane.RoomPlan.Room.IsMotionClear(start, end, PulseRadius))
            {
                // Truncate the preview before geometry rather than drawing an apparently valid path through a wall.
                float lo = 0, hi = 1;
                for (int i = 0; i < 12; i++)
                {
                    float mid = (lo + hi) * .5f;
                    if (Lane.RoomPlan.Room.IsMotionClear(start, Vector3.Lerp(start, end, mid), PulseRadius)) lo = mid; else hi = mid;
                }
                end = Vector3.Lerp(start, end, lo);
            }
            PreviewEnd = end;
        }
    }
}
