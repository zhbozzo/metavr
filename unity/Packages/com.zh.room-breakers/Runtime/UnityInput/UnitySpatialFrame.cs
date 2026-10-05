using System;
using RoomBreakers.Core;
using UnityEngine;
using NVector = System.Numerics.Vector3;
using NQuaternion = System.Numerics.Quaternion;

namespace RoomBreakers.UnityInput
{
    public static class UnitySpatialFrame
    {
        public static Pose3 Pose(Vector3 p, Quaternion q) => new Pose3(
            new NVector(p.x, p.y, p.z), new NQuaternion(q.x, q.y, q.z, q.w));
        public static Vector3 Vector(NVector p) => new Vector3(p.X, p.Y, p.Z);
        public static Quaternion Rotation(NQuaternion q) => new Quaternion(q.X, q.Y, q.Z, q.W);
        public static void ApplyPose(Transform view, Pose3 p) => view.SetPositionAndRotation(Vector(p.Position), Rotation(p.Rotation));

        public static bool TryRead(Transform source, bool unitScale, out SpatialFrame frame, out string error)
        {
            frame = null; error = null;
            if (source == null) { error = "Assign both room and miniature frames."; return false; }
            // Do not accept mirrored or nonuniform ancestors, even if lossyScale happens to look uniform.
            for (Transform t = source; t != null; t = t.parent)
            {
                Vector3 s = t.localScale;
                if (!Finite(s) || s.x <= 0 || Mathf.Abs(s.x - s.y) > .00001f || Mathf.Abs(s.x - s.z) > .00001f)
                { error = "Frames cannot have mirrored, zero, nonuniform or nonfinite parent scales."; return false; }
            }
            Matrix4x4 m = source.localToWorldMatrix;
            Vector3 x = m.MultiplyVector(Vector3.right), y = m.MultiplyVector(Vector3.up), z = m.MultiplyVector(Vector3.forward);
            float scale = x.magnitude;
            if (!Finite(x) || !Finite(y) || !Finite(z) || !SpatialMath.IsFinite(scale) || scale <= 0)
            { error = "Frame matrix is not finite."; return false; }
            if (Mathf.Abs(y.magnitude - scale) > .0001f || Mathf.Abs(z.magnitude - scale) > .0001f ||
                Mathf.Abs(Vector3.Dot(x.normalized, y.normalized)) > .0001f ||
                Mathf.Abs(Vector3.Dot(x.normalized, z.normalized)) > .0001f ||
                Vector3.Dot(Vector3.Cross(x.normalized, y.normalized), z.normalized) < .999f)
            { error = "Frame contains shear or an invalid basis."; return false; }
            if (unitScale && Mathf.Abs(scale - 1f) > .00001f)
            { error = "The room frame must use meters at unit scale."; return false; }
            try { frame = new SpatialFrame(Pose(source.position, source.rotation), unitScale ? 1f : scale); return true; }
            catch (ArgumentException e) { error = e.Message; return false; }
        }
        private static bool Finite(Vector3 v) => SpatialMath.IsFinite(v.x) && SpatialMath.IsFinite(v.y) && SpatialMath.IsFinite(v.z);
    }
}
