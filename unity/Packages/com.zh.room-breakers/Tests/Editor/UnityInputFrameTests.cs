using NUnit.Framework;
using RoomBreakers.Core;
using RoomBreakers.UnityInput;
using UnityEngine;
using NVector = System.Numerics.Vector3;
using NQuaternion = System.Numerics.Quaternion;

namespace RoomBreakers.Tests
{
    // Requires the actual Unity engine. These tests are not executed by .NET CI.
    public sealed class UnityInputFrameTests
    {
        private GameObject root;
        private Transform child;
        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Transient frame validation");
            child = new GameObject("Frame").transform;
            child.SetParent(root.transform, false);
        }
        [TearDown]
        public void TearDown() { Object.DestroyImmediate(root); }

        [Test]
        public void MissingFrameRejected()
        {
            Assert.That(UnitySpatialFrame.TryRead(null, false, out _, out string error), Is.False);
            Assert.That(error, Is.Not.Empty);
        }
        [Test]
        public void UniformHierarchyMatchesTransform()
        {
            root.transform.SetPositionAndRotation(new Vector3(1, 2, -3), Quaternion.Euler(12, 38, -19));
            root.transform.localScale = Vector3.one * .5f;
            child.localPosition = new Vector3(.2f, .7f, -.3f);
            child.localRotation = Quaternion.Euler(-7, 21, 35);
            child.localScale = Vector3.one * .4f;
            Assert.That(UnitySpatialFrame.TryRead(child, false, out SpatialFrame frame, out string error), Is.True, error);
            Assert.That(frame.Scale, Is.EqualTo(.2f).Within(.00001f));
            var expected = child.TransformPoint(new Vector3(.3f, -.2f, 1));
            var actual = frame.ToWorld(new Pose3(new NVector(.3f, -.2f, 1), NQuaternion.Identity));
            Assert.That(Vector3.Distance(expected, UnitySpatialFrame.Vector(actual.Position)), Is.LessThan(.0001f));
        }
        [Test]
        public void NonUniformAncestorRejected()
        {
            root.transform.localScale = new Vector3(1, 2, 1);
            child.localScale = new Vector3(1, .5f, 1);
            Assert.That(UnitySpatialFrame.TryRead(child, false, out _, out _), Is.False);
        }
        [Test]
        public void MirroredAncestorRejected()
        {
            root.transform.localScale = -Vector3.one;
            child.localScale = -Vector3.one;
            Assert.That(UnitySpatialFrame.TryRead(child, false, out _, out _), Is.False);
        }
        [Test]
        public void ZeroScaleRejected()
        {
            child.localScale = Vector3.zero;
            Assert.That(UnitySpatialFrame.TryRead(child, false, out _, out _), Is.False);
        }
        [Test]
        public void PhysicalRoomRejectsScaledTransform()
        {
            child.localScale = Vector3.one * .2f;
            Assert.That(UnitySpatialFrame.TryRead(child, true, out _, out _), Is.False);
            child.localScale = Vector3.one;
            Assert.That(UnitySpatialFrame.TryRead(child, true, out SpatialFrame frame, out _), Is.True);
            Assert.That(frame.Scale, Is.EqualTo(1));
        }
        [Test]
        public void OutOfRangeScaleRejected()
        {
            child.localScale = Vector3.one * 101;
            Assert.That(UnitySpatialFrame.TryRead(child, false, out _, out _), Is.False);
        }
        [Test]
        public void DriverUsesUnityWorldPoseWithoutDoubleTransform()
        {
            child.SetPositionAndRotation(new Vector3(2, 1, -2), Quaternion.Euler(15, -30, 8));
            child.localScale = Vector3.one * .2f;
            Assert.That(UnitySpatialFrame.TryRead(child, false, out SpatialFrame frame, out _), Is.True);
            var map = new DualScaleMap(new SpatialFrame(Pose3.Identity, 1), frame);
            var session = new ScaleSession(map, Pose3.Identity,
                new InteractionBounds(new NVector(-2), new NVector(2)), NVector.UnitX, .15f);
            var driver = new HandCaptureDriver(session);
            Pose3 start = UnitySpatialFrame.Pose(child.TransformPoint(Vector3.zero), child.rotation);
            Pose3 target = UnitySpatialFrame.Pose(child.TransformPoint(Vector3.right), child.rotation);
            driver.Step(0, new HandSample(HandId.Left, 1, 0, start, 0, true), default);
            driver.Step(.01, new HandSample(HandId.Left, 2, .01, start, 1, true), default);
            Assert.That(session.State, Is.EqualTo(ProbeState.Held));
            driver.Step(.02, new HandSample(HandId.Left, 3, .02, target, 0, true), default);
            Assert.That(session.State, Is.EqualTo(ProbeState.Returned));
            Assert.That(session.ReturnCount, Is.EqualTo(1));
            Assert.That(Vector3.Distance(UnitySpatialFrame.Vector(session.ObjectPose.Position), Vector3.right), Is.LessThan(.0001f));
        }
    }
}
