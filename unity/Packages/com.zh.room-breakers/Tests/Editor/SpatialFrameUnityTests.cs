using NUnit.Framework;
using RoomBreakers.Core;
using UnityEngine;
using NVector = System.Numerics.Vector3;
using NQuaternion = System.Numerics.Quaternion;

namespace RoomBreakers.Tests
{
    // These tests require the real Unity Test Framework. They are NOT run by .NET CI.
    public sealed class SpatialFrameUnityTests
    {
        private static NVector N(Vector3 v) => new NVector(v.x, v.y, v.z);
        private static NQuaternion N(Quaternion q) => new NQuaternion(q.x, q.y, q.z, q.w);
        private static Vector3 U(NVector v) => new Vector3(v.X, v.Y, v.Z);
        private static Quaternion U(NQuaternion q) => new Quaternion(q.X, q.Y, q.Z, q.W);

        [TestCase(.15f)]
        [TestCase(.25f)]
        [TestCase(.35f)]
        public void NumericFrameMatchesActualUnityTransform(float scale)
        {
            var obj = new GameObject("Transient frame test");
            try
            {
                Transform t = obj.transform;
                t.SetPositionAndRotation(new Vector3(2, 1, -3), Quaternion.Euler(33, -47, 19));
                t.localScale = Vector3.one * scale;
                var frame = new SpatialFrame(new Pose3(N(t.position), N(t.rotation)), scale);
                var local = new Pose3(new NVector(.7f, .5f, -1.2f), N(Quaternion.Euler(-20, 39, 71)));
                var result = frame.ToWorld(local);
                Assert.That(Vector3.Distance(t.TransformPoint(U(local.Position)), U(result.Position)), Is.LessThan(.0001f));
                Assert.That(Quaternion.Angle(t.rotation * U(local.Rotation), U(result.Rotation)), Is.LessThan(.03f));
                var restored = frame.ToLocal(result);
                Assert.That(Vector3.Distance(U(restored.Position), U(local.Position)), Is.LessThan(.0001f));
            }
            finally { Object.DestroyImmediate(obj); }
        }

        [Test]
        public void EnlargedHandPreservesRoomPoseAcrossTwoRotatedFrames()
        {
            var room = new SpatialFrame(new Pose3(new NVector(1, 2, 3), N(Quaternion.Euler(10, 45, -30))), 1);
            var miniature = new SpatialFrame(new Pose3(new NVector(-2, 1, -1), N(Quaternion.Euler(-20, 15, 5))), .2f);
            var map = new DualScaleMap(room, miniature);
            var canonical = new Pose3(new NVector(1, .5f, -1), N(Quaternion.Euler(70, 20, 30)));
            Pose3 result = map.EnlargedHand(map.MiniatureView(canonical));
            Pose3 expected = map.RoomView(canonical);
            Assert.That(Vector3.Distance(U(result.Position), U(expected.Position)), Is.LessThan(.0001f));
            Assert.That(Quaternion.Angle(U(result.Rotation), U(expected.Rotation)), Is.LessThan(.03f));
        }
    }
}
