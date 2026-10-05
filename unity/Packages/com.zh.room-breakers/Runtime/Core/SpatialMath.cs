using System;
using System.Numerics;

namespace RoomBreakers.Core
{
    public static class SpatialMath
    {
        public static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public static bool IsFinite(Vector3 v) => IsFinite(v.X) && IsFinite(v.Y) && IsFinite(v.Z);
        public static bool IsValidRotation(Quaternion q)
        {
            if (!IsFinite(q.X) || !IsFinite(q.Y) || !IsFinite(q.Z) || !IsFinite(q.W)) return false;
            double lengthSquared = (double)q.X * q.X + (double)q.Y * q.Y + (double)q.Z * q.Z + (double)q.W * q.W;
            return lengthSquared > 1e-12;
        }
        public static Quaternion Normalize(Quaternion q)
        {
            if (!IsValidRotation(q)) throw new ArgumentException("A finite, nonzero rotation is required.", nameof(q));
            double length = Math.Sqrt((double)q.X * q.X + (double)q.Y * q.Y + (double)q.Z * q.Z + (double)q.W * q.W);
            return new Quaternion((float)(q.X / length), (float)(q.Y / length), (float)(q.Z / length), (float)(q.W / length));
        }
        public static Vector3 RequireFinite(Vector3 v)
        {
            if (!IsFinite(v)) throw new ArgumentException("Position/vector must be finite.", nameof(v));
            return v;
        }
        public static Vector3 Reflect(Vector3 direction, Vector3 normal)
        {
            RequireFinite(direction);
            RequireFinite(normal);
            if (!IsFinite(direction.LengthSquared()) || !IsFinite(normal.LengthSquared()) || direction.LengthSquared() < 1e-12f || normal.LengthSquared() < 1e-12f)
                throw new ArgumentException("Direction and normal must have finite nonzero length.");
            Vector3 d = Vector3.Normalize(direction);
            Vector3 n = Vector3.Normalize(normal);
            return Vector3.Normalize(d - 2f * Vector3.Dot(d, n) * n);
        }
    }

    public readonly struct Pose3
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public bool IsValid => SpatialMath.IsFinite(Position) && SpatialMath.IsValidRotation(Rotation);
        public Pose3(Vector3 position, Quaternion rotation)
        {
            Position = SpatialMath.RequireFinite(position);
            Rotation = SpatialMath.Normalize(rotation);
        }
        public static Pose3 Identity => new Pose3(Vector3.Zero, Quaternion.Identity);
    }

    // Immutable frame. Scale is uniform; physical room frames must have scale 1.
    public sealed class SpatialFrame
    {
        public Pose3 Origin { get; }
        public float Scale { get; }
        public SpatialFrame(Pose3 origin, float scale)
        {
            if (!origin.IsValid) throw new ArgumentException("Invalid frame pose.", nameof(origin));
            // Explicit engineering range, not a device capability claim.
            if (!SpatialMath.IsFinite(scale) || scale < 0.0001f || scale > 100f)
                throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be between 0.0001 and 100.");
            Origin = new Pose3(origin.Position, origin.Rotation);
            Scale = scale;
        }
        public Pose3 ToWorld(Pose3 local)
        {
            if (!local.IsValid) throw new ArgumentException("Invalid local pose.", nameof(local));
            return new Pose3(Origin.Position + Vector3.Transform(local.Position * Scale, Origin.Rotation), Origin.Rotation * local.Rotation);
        }
        public Pose3 ToLocal(Pose3 world)
        {
            if (!world.IsValid) throw new ArgumentException("Invalid world pose.", nameof(world));
            Quaternion inverse = Quaternion.Conjugate(Origin.Rotation);
            return new Pose3(Vector3.Transform(world.Position - Origin.Position, inverse) / Scale, inverse * world.Rotation);
        }
    }

    public sealed class DualScaleMap
    {
        public SpatialFrame Room { get; }
        public SpatialFrame Miniature { get; }
        public DualScaleMap(SpatialFrame room, SpatialFrame miniature)
        {
            Room = room ?? throw new ArgumentNullException(nameof(room));
            Miniature = miniature ?? throw new ArgumentNullException(nameof(miniature));
            if (Math.Abs(room.Scale - 1f) > 1e-6f) throw new ArgumentException("Room coordinates must be meters at unit scale.", nameof(room));
        }
        public Pose3 RoomView(Pose3 canonical) => Room.ToWorld(canonical);
        public Pose3 MiniatureView(Pose3 canonical) => Miniature.ToWorld(canonical);
        public Pose3 FromMiniature(Pose3 miniatureWorld) => Miniature.ToLocal(miniatureWorld);
        public Pose3 EnlargedHand(Pose3 miniatureWorld) => RoomView(FromMiniature(miniatureWorld));
    }

    // Interaction limits only; this is NOT a physical safety/boundary detector.
    public sealed class InteractionBounds
    {
        public Vector3 Min { get; }
        public Vector3 Max { get; }
        public InteractionBounds(Vector3 min, Vector3 max)
        {
            Min = SpatialMath.RequireFinite(min);
            Max = SpatialMath.RequireFinite(max);
            if (min.X > max.X || min.Y > max.Y || min.Z > max.Z) throw new ArgumentException("Bounds are inverted.");
        }
        public bool Contains(Vector3 p) => SpatialMath.IsFinite(p) && p.X >= Min.X && p.Y >= Min.Y && p.Z >= Min.Z && p.X <= Max.X && p.Y <= Max.Y && p.Z <= Max.Z;
    }
}
