using System;
using System.Collections.Generic;
using System.Numerics;

namespace RoomBreakers.Core
{
    // Gameplay geometry only. It must NEVER be advertised as a physical safety system.
    public static class RoomGeometry
    {
        public static Vector2 XZ(Vector3 p) => new Vector2(p.X, p.Z);
        public static Vector3 AtHeight(Vector2 p, float y) => new Vector3(p.X, y, p.Y);
        public static bool Finite(Vector2 p) => SpatialMath.IsFinite(p.X) && SpatialMath.IsFinite(p.Y);
        public static double Cross(Vector2 a, Vector2 b, Vector2 c) =>
            ((double)b.X - a.X) * ((double)c.Y - a.Y) - ((double)b.Y - a.Y) * ((double)c.X - a.X);
        public static double PointSegmentDistanceSquared(Vector2 p, Vector2 a, Vector2 b)
        {
            double x = (double)b.X - a.X, y = (double)b.Y - a.Y;
            double length = x * x + y * y;
            double t = length < 1e-16 ? 0 : Math.Max(0, Math.Min(1, (((double)p.X - a.X) * x + ((double)p.Y - a.Y) * y) / length));
            double dx = p.X - (a.X + t * x), dy = p.Y - (a.Y + t * y);
            return dx * dx + dy * dy;
        }
        public static bool Intersects(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            double x = Cross(a, b, c), y = Cross(a, b, d), z = Cross(c, d, a), w = Cross(c, d, b);
            if (((x > 0 && y < 0) || (x < 0 && y > 0)) && ((z > 0 && w < 0) || (z < 0 && w > 0))) return true;
            const double epsilon = 1e-12;
            return PointSegmentDistanceSquared(a, c, d) <= epsilon || PointSegmentDistanceSquared(b, c, d) <= epsilon ||
                PointSegmentDistanceSquared(c, a, b) <= epsilon || PointSegmentDistanceSquared(d, a, b) <= epsilon;
        }
        public static double SegmentDistanceSquared(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            if (Intersects(a, b, c, d)) return 0;
            return Math.Min(Math.Min(PointSegmentDistanceSquared(a, c, d), PointSegmentDistanceSquared(b, c, d)),
                Math.Min(PointSegmentDistanceSquared(c, a, b), PointSegmentDistanceSquared(d, a, b)));
        }
    }

    public sealed class FloorPolygon
    {
        private readonly Vector2[] points;
        public IReadOnlyList<Vector2> Points { get; }
        public Vector2 Min { get; }
        public Vector2 Max { get; }
        public FloorPolygon(IReadOnlyList<Vector2> vertices)
        {
            if (vertices == null || vertices.Count < 3 || vertices.Count > 128)
                throw new ArgumentException("A floor needs 3 to 128 vertices.", nameof(vertices));
            points = new Vector2[vertices.Count];
            Vector2 min = new Vector2(float.MaxValue), max = new Vector2(float.MinValue);
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 p = vertices[i];
                if (!RoomGeometry.Finite(p) || Math.Abs(p.X) > 64 || Math.Abs(p.Y) > 64)
                    throw new ArgumentException("Floor coordinates must be finite and within the supported range.");
                points[i] = p; min = Vector2.Min(min, p); max = Vector2.Max(max, p);
            }
            double area = 0;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 a = points[i], b = points[(i + 1) % points.Length];
                if (Vector2.DistanceSquared(a, b) < 1e-8f) throw new ArgumentException("Duplicate floor vertex.");
                area += (double)a.X * b.Y - (double)b.X * a.Y;
                for (int j = i + 1; j < points.Length; j++)
                {
                    if (j == i + 1 || (i == 0 && j == points.Length - 1)) continue;
                    if (RoomGeometry.Intersects(a, b, points[j], points[(j + 1) % points.Length]))
                        throw new ArgumentException("Self-intersecting floor polygon.");
                }
            }
            if (Math.Abs(area) < .10 || max.X - min.X > 16 || max.Y - min.Y > 16)
                throw new ArgumentException("Floor is degenerate or exceeds the 16m prototype limit.");
            Min = min; Max = max; Points = Array.AsReadOnly(points);
        }
        public bool Contains(Vector2 p)
        {
            if (!RoomGeometry.Finite(p)) return false;
            bool inside = false;
            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
            {
                Vector2 a = points[j], b = points[i];
                if (RoomGeometry.PointSegmentDistanceSquared(p, a, b) < 1e-12) return true;
                if ((a.Y > p.Y) != (b.Y > p.Y) && p.X < ((double)b.X - a.X) * (p.Y - a.Y) / (b.Y - a.Y) + a.X)
                    inside = !inside;
            }
            return inside;
        }
        public bool SegmentInside(Vector2 a, Vector2 b, float clearance)
        {
            if (!SpatialMath.IsFinite(clearance) || clearance < 0 || !Contains(a) || !Contains(b)) return false;
            double radius2 = Math.Max(1e-12, (double)clearance * clearance);
            for (int i = 0; i < points.Length; i++)
                if (RoomGeometry.SegmentDistanceSquared(a, b, points[i], points[(i + 1) % points.Length]) < radius2) return false;
            return true;
        }
    }

    public sealed class RoomObstacle
    {
        public Vector3 Min { get; }
        public Vector3 Max { get; }
        // Conservative canonical AABB; a rotated piece of furniture may occupy less space.
        public RoomObstacle(Vector3 min, Vector3 max)
        {
            if (!SpatialMath.IsFinite(min) || !SpatialMath.IsFinite(max) || min.X > max.X || min.Y > max.Y || min.Z > max.Z ||
                min.LengthSquared() > 16384 || max.LengthSquared() > 16384)
                throw new ArgumentException("Invalid obstacle bounds.");
            Min = min; Max = max;
        }
        public bool IntersectsSegment(Vector3 a, Vector3 b, float radius)
        {
            double lo = 0, hi = 1;
            return Slab(a.X, b.X - a.X, Min.X - radius, Max.X + radius, ref lo, ref hi) &&
                Slab(a.Y, b.Y - a.Y, Min.Y - radius, Max.Y + radius, ref lo, ref hi) &&
                Slab(a.Z, b.Z - a.Z, Min.Z - radius, Max.Z + radius, ref lo, ref hi);
        }
        private static bool Slab(double p, double delta, double min, double max, ref double lo, ref double hi)
        {
            if (Math.Abs(delta) < 1e-12) return p >= min && p <= max;
            double a = (min - p) / delta, b = (max - p) / delta;
            if (a > b) { double temp = a; a = b; b = temp; }
            lo = Math.Max(lo, a); hi = Math.Min(hi, b); return lo <= hi;
        }
    }

    public sealed class WallSpan
    {
        public Vector2 A { get; }
        public Vector2 B { get; }
        public float Bottom { get; }
        public float Top { get; }
        public WallSpan(Vector2 a, Vector2 b, float bottom, float top)
        {
            if (!RoomGeometry.Finite(a) || !RoomGeometry.Finite(b) || !SpatialMath.IsFinite(bottom) || !SpatialMath.IsFinite(top) ||
                Vector2.Distance(a, b) < .10f || bottom >= top || bottom < -.1f || top > 10 ||
                a.LengthSquared() > 8192 || b.LengthSquared() > 8192) throw new ArgumentException("Invalid vertical wall.");
            A = a; B = b; Bottom = bottom; Top = top;
        }
    }

    public sealed class RoomSnapshot
    {
        public FloorPolygon Floor { get; }
        public IReadOnlyList<WallSpan> Walls { get; }
        public IReadOnlyList<RoomObstacle> Obstacles { get; }
        public bool IsSynthetic { get; }
        public RoomSnapshot(FloorPolygon floor, IReadOnlyList<WallSpan> walls, IReadOnlyList<RoomObstacle> obstacles, bool isSynthetic)
        {
            Floor = floor ?? throw new ArgumentNullException(nameof(floor));
            if (walls == null || walls.Count == 0 || walls.Count > 64 || obstacles == null || obstacles.Count > 128)
                throw new ArgumentException("Room requires bounded wall and obstacle collections.");
            var ws = new WallSpan[walls.Count]; var os = new RoomObstacle[obstacles.Count];
            for (int i = 0; i < ws.Length; i++) ws[i] = walls[i] ?? throw new ArgumentException("Null wall.");
            for (int i = 0; i < os.Length; i++) os[i] = obstacles[i] ?? throw new ArgumentException("Null obstacle.");
            Walls = Array.AsReadOnly(ws); Obstacles = Array.AsReadOnly(os); IsSynthetic = isSynthetic;
        }
        public bool IsMotionClear(Vector3 a, Vector3 b, float radius = .16f)
        {
            if (!SpatialMath.IsFinite(a) || !SpatialMath.IsFinite(b) || !SpatialMath.IsFinite(radius) || radius < .01f || radius > 1 ||
                Math.Min(a.Y, b.Y) < radius || Math.Max(a.Y, b.Y) > 4 || !Floor.SegmentInside(RoomGeometry.XZ(a), RoomGeometry.XZ(b), radius)) return false;
            foreach (RoomObstacle obstacle in Obstacles) if (obstacle.IntersectsSegment(a, b, radius)) return false;
            foreach (WallSpan wall in Walls)
                if (Math.Max(a.Y, b.Y) + radius >= wall.Bottom && Math.Min(a.Y, b.Y) - radius <= wall.Top &&
                    RoomGeometry.SegmentDistanceSquared(RoomGeometry.XZ(a), RoomGeometry.XZ(b), wall.A, wall.B) < (double)radius * radius) return false;
            return true;
        }
    }

    public sealed class RoomPlan
    {
        public RoomSnapshot Room { get; }
        public Pose3 Portal { get; }
        public Vector3 ReturnCenter { get; }
        public Vector3 Refuge { get; }
        public IReadOnlyList<Vector3> Route { get; }
        public float RouteLength { get; }
        internal RoomPlan(RoomSnapshot room, Pose3 portal, Vector3 returnCenter, IList<Vector3> path)
        {
            Room = room; Portal = portal; ReturnCenter = returnCenter; Refuge = path[path.Count - 1];
            var copy = new Vector3[path.Count]; float length = 0;
            for (int i = 0; i < copy.Length; i++) { copy[i] = path[i]; if (i > 0) length += Vector3.Distance(copy[i - 1], copy[i]); }
            Route = Array.AsReadOnly(copy); RouteLength = length;
        }
        public Vector3 PositionAt(float distance)
        {
            if (!SpatialMath.IsFinite(distance) || distance < 0) throw new ArgumentOutOfRangeException(nameof(distance));
            for (int i = 1; i < Route.Count; i++)
            {
                float length = Vector3.Distance(Route[i - 1], Route[i]);
                if (distance <= length) return Vector3.Lerp(Route[i - 1], Route[i], length > 1e-6f ? distance / length : 0);
                distance -= length;
            }
            return Refuge;
        }
    }

    public static class RoomPlanner
    {
        public const float Clearance = .18f;
        private const float Cell = .20f;
        // One flood fill per plan, reused by all portal candidates. No unbounded search or random retries.
        public static bool TryPlan(RoomSnapshot room, Pose3 canonicalHead, out RoomPlan plan, out string error)
        {
            plan = null; error = "Room data or a valid seated head pose is missing.";
            if (room == null || !canonicalHead.IsValid || !room.Floor.Contains(RoomGeometry.XZ(canonicalHead.Position))) return false;
            Vector3 f3 = Vector3.Transform(Vector3.UnitZ, canonicalHead.Rotation);
            Vector2 forward = new Vector2(f3.X, f3.Z), head = RoomGeometry.XZ(canonicalHead.Position);
            if (forward.LengthSquared() < .01f) { error = "Look forward, rather than straight up or down."; return false; }
            forward = Vector2.Normalize(forward);
            float height = Math.Max(.55f, Math.Min(1.4f, canonicalHead.Position.Y - .45f));
            var min = room.Floor.Min; var size = room.Floor.Max - min;
            int width = (int)Math.Ceiling(size.X / Cell), depth = (int)Math.Ceiling(size.Y / Cell), count = width * depth;
            if (width < 2 || depth < 2 || count > 6400) { error = "Room exceeds the bounded navigation grid."; return false; }
            var positions = new Vector3[count]; var free = new bool[count]; var previous = new int[count];
            int root = -1; float closest = float.MaxValue;
            Vector2 desired = head + forward * .85f;
            for (int i = 0; i < count; i++)
            {
                Vector2 point = min + new Vector2((i % width + .5f) * Cell, (i / width + .5f) * Cell);
                Vector2 relative = point - head;
                positions[i] = RoomGeometry.AtHeight(point, height); previous[i] = -2;
                float distance = relative.Length();
                free[i] = distance >= .55f && distance <= 5 && Vector2.Dot(relative, forward) >= distance * .64f &&
                    room.IsMotionClear(positions[i], positions[i], Clearance);
                float cost = Vector2.DistanceSquared(point, desired);
                if (free[i] && distance <= 1.4f && cost < closest) { closest = cost; root = i; }
            }
            if (root < 0) { error = "No clear refuge in the forward sector. Choose a different seated direction."; return false; }
            var queue = new int[count]; int read = 0, write = 0; queue[write++] = root; previous[root] = -1;
            int[] dx = { 1, -1, 0, 0 }, dz = { 0, 0, 1, -1 };
            while (read < write)
            {
                int index = queue[read++], x = index % width, z = index / width;
                for (int n = 0; n < 4; n++)
                {
                    int nx = x + dx[n], nz = z + dz[n]; if (nx < 0 || nx >= width || nz < 0 || nz >= depth) continue;
                    int next = nz * width + nx;
                    if (!free[next] || previous[next] != -2 || !room.IsMotionClear(positions[index], positions[next], Clearance)) continue;
                    previous[next] = index; queue[write++] = next;
                }
            }
            double best = double.PositiveInfinity;
            foreach (WallSpan wall in room.Walls)
            {
                if (height - wall.Bottom < .30f || wall.Top - height < .30f) continue;
                Vector2 delta = wall.B - wall.A; float wallLength = delta.Length(); if (wallLength < .9f) continue;
                for (int k = 1; k <= 3; k++)
                {
                    float fraction = k * .25f;
                    if (Math.Min(fraction, 1 - fraction) * wallLength < .35f) continue;
                    Vector2 face = Vector2.Lerp(wall.A, wall.B, fraction);
                    Vector2 normal = Vector2.Normalize(new Vector2(-delta.Y, delta.X));
                    if (Vector2.Dot(normal, head - face) < 0) normal = -normal;
                    Vector2 start2 = face + normal * .40f, rel = start2 - head;
                    float dist = rel.Length();
                    if (dist < 1.1f || dist > 5 || Vector2.Dot(rel, forward) < dist * .64f) continue;
                    Vector3 start = RoomGeometry.AtHeight(start2, height);
                    if (!room.IsMotionClear(start, start, Clearance)) continue;
                    // Check the portal aperture against furniture/door/window exclusions, not only its center.
                    Vector3 aperture = RoomGeometry.AtHeight(face + normal * .035f, height);
                    bool blocked = false;
                    foreach (RoomObstacle obstacle in room.Obstacles)
                        if (obstacle.IntersectsSegment(aperture, aperture, .31f)) { blocked = true; break; }
                    if (blocked) continue;
                    int attach = -1; float attachDistance = float.MaxValue;
                    for (int i = 0; i < count; i++)
                    {
                        if (previous[i] == -2) continue;
                        float d = Vector3.DistanceSquared(start, positions[i]);
                        if (d < attachDistance && d < .25f && room.IsMotionClear(start, positions[i], Clearance)) { attach = i; attachDistance = d; }
                    }
                    if (attach < 0) continue;
                    var raw = new List<Vector3> { start };
                    for (int at = attach; at >= 0; at = previous[at]) raw.Add(positions[at]);
                    var smooth = new List<Vector3> { start }; int current = 0;
                    while (current < raw.Count - 1)
                    {
                        int next = raw.Count - 1;
                        while (next > current + 1 && !room.IsMotionClear(raw[current], raw[next], Clearance)) next--;
                        smooth.Add(raw[next]); current = next;
                    }
                    float length = 0; for (int i = 1; i < smooth.Count; i++) length += Vector3.Distance(smooth[i - 1], smooth[i]);
                    if (length < .8f || length > 12) continue;
                    double score = length + 2 * (1 - Vector2.Dot(Vector2.Normalize(rel), forward));
                    if (score >= best) continue;
                    Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.Atan2(normal.X, normal.Y));
                    plan = new RoomPlan(room, new Pose3(aperture, rotation), start, smooth); best = score;
                }
            }
            error = plan == null ? "No supported wall has a clear route. Face another sector or update Space Setup." : null;
            return plan != null;
        }
    }
}
