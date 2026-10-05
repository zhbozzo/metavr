using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoomBreakers.Core;
using RoomBreakers.UnityInput;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NVector = System.Numerics.Vector3;
using NQuaternion = System.Numerics.Quaternion;

namespace RoomBreakers.Tests
{
    // Synthetic providers are explicit test fixtures, never a replacement for Meta sensor validation.
    public sealed class IntegrationRoomSource : RoomSource
    {
        public override void Load()
        {
            Snapshot = SyntheticRooms.Rectangular(); WorldFrame = new SpatialFrame(Pose3.Identity, 1);
            Revision++; State = RoomSourceState.Ready; Status = "SYNTHETIC INTEGRATION FIXTURE";
        }
        public void BreakRoom() { Invalidate("Test invalidation"); }
    }
    [DefaultExecutionOrder(-100)]
    public sealed class IntegrationHandSource : HandSampleSource
    {
        public Pose3 WorldPose = new Pose3(new NVector(0, 2, -1), NQuaternion.Identity);
        public float Strength;
        private long sequence;
        private HandSample latest;
        private void Update()
        {
            latest = new HandSample(HandId.Right, ++sequence, Time.realtimeSinceStartupAsDouble, WorldPose, Strength, true);
        }
        public override bool TryGetLatest(out HandSample sample) { sample = latest; return latest.Sequence > 0; }
    }

    // These tests run in real Unity PlayMode, including Start/LateUpdate, views and disposal.
    // They do not read a Quest, grade images, or write to the player's persistent progress directory.
    public sealed class EncounterIntegrationTests
    {
        private Scene scene, previous;
        private GameObject root;
        private FirstEncounterRig rig;
        private IntegrationRoomSource room;
        private IntegrationHandSource hand;
        private Camera camera;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previous = SceneManager.GetActiveScene();
            scene = SceneManager.CreateScene("RB integration " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(scene);
            root = new GameObject("Integration fixture - no device data"); root.SetActive(false);
            camera = new GameObject("Fixture camera").AddComponent<Camera>();
            camera.transform.SetParent(root.transform); camera.transform.position = new Vector3(0, 1.2f, 0);
            camera.transform.rotation = Quaternion.Euler(15, 0, 0); camera.nearClipPlane = .03f;
            camera.gameObject.AddComponent<AudioListener>();
            room = root.AddComponent<IntegrationRoomSource>(); hand = root.AddComponent<IntegrationHandSource>();
            rig = root.AddComponent<FirstEncounterRig>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            Assert.That(shader, Is.Not.Null, "A real built-in/URP shader must be available.");
            rig.Configure(room, null, hand, camera, shader);
            // Install memory-only journals BEFORE activating any MonoBehaviour. Fail before Start if fields change.
            foreach (string name in new[] { "practiceProgress", "deviceProgress" })
            {
                FieldInfo field = typeof(FirstEncounterRig).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(field, Is.Not.Null, "Progress isolation requires an updated fixture.");
                field.SetValue(rig, ProgressJournal.InMemory());
            }
            root.SetActive(true); yield return null; yield return null;
            Assert.That(rig.enabled, Is.True, "Rig failed during initialization.");
            Assert.That(rig.UiFrame, Is.Not.Null);
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null) UnityEngine.Object.Destroy(root);
            yield return null; yield return null;
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            LogAssert.NoUnexpectedReceived();
        }
        private IEnumerator World(Pose3 pose, float strength)
        {
            hand.WorldPose = pose; hand.Strength = strength;
            yield return null; yield return null;
        }
        private IEnumerator Canonical(Pose3 pose, float strength = 0)
        { yield return World(rig.Game.Session.Map.MiniatureView(pose), strength); }
        private IEnumerator Load()
        {
            rig.LoadRoom();
            for (int i = 0; i < 20 && rig.Game == null; i++) yield return null;
            Assert.That(rig.Game, Is.Not.Null, "Room planning or view construction failed.");
            yield return null;
        }
        private IEnumerator ReturnCreature()
        {
            yield return Canonical(rig.Game.Session.ObjectPose);
            yield return Canonical(rig.Game.Session.ObjectPose);
            yield return Canonical(rig.Game.Session.ObjectPose, 1);
            Assert.That(rig.Game.Session.State, Is.EqualTo(ProbeState.Held));
            yield return Canonical(new Pose3(rig.Game.Session.ReturnCenter, rig.Game.Session.ObjectPose.Rotation));
        }
        private IEnumerator Control(float horizontal)
        {
            var frame = rig.UiFrame;
            var position = frame.Origin.Position + NVector.Transform(new NVector(horizontal, -.055f, -.035f), frame.Origin.Rotation);
            var pose = new Pose3(position, frame.Origin.Rotation);
            yield return World(pose, 0); yield return World(pose, 1); yield return World(pose, 0);
        }

        [UnityTest]
        public IEnumerator BootPinchLoadsTheRealRig()
        {
            Assert.That(rig.Game, Is.Null);
            var target = rig.UiFrame.Origin;
            yield return World(target, 0); yield return World(target, 1); yield return World(target, 0);
            for (int i = 0; i < 20 && rig.Game == null; i++) yield return null;
            Assert.That(rig.Game, Is.Not.Null);
            Assert.That(rig.Game.Phase, Is.EqualTo(EncounterPhase.LearnCapture));
            Assert.That(rig.Game.Plan.Room.IsSynthetic, Is.True);
        }

        [UnityTest]
        public IEnumerator CaptureSynchronizesBothRenderedScales()
        {
            yield return Load();
            yield return Canonical(rig.Game.Session.ObjectPose);
            yield return Canonical(rig.Game.Session.ObjectPose, 1);
            Assert.That(rig.Game.Session.State, Is.EqualTo(ProbeState.Held));
            var pose = new Pose3(rig.Game.Session.ObjectPose.Position + new NVector(0, .12f, 0), rig.Game.Session.ObjectPose.Rotation);
            yield return Canonical(pose, 1);
            // Names identify presentation nodes only; gameplay identity comes from the authoritative session.
            var motes = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true))
                .Where(x => x.name == "Mote / Shell core").ToArray();
            Assert.That(motes.Length, Is.EqualTo(2));
            var large = motes.Single(x => x.parent.name == "Room-scale view");
            var miniature = motes.Single(x => x.parent.name == "Miniature view");
            Assert.That(Vector3.Distance(large.position, UnitySpatialFrame.Vector(rig.Game.Session.Map.RoomView(pose).Position)), Is.LessThan(.001f));
            Assert.That(Vector3.Distance(miniature.position, UnitySpatialFrame.Vector(rig.Game.Session.Map.MiniatureView(pose).Position)), Is.LessThan(.001f));
        }

        [UnityTest]
        public IEnumerator FullEncounterWinsAndRestartsThroughSceneInput()
        {
            yield return Load();
            Guid run = rig.Game.RunId;
            Vector3 initialCamera = camera.transform.position;
            yield return ReturnCreature(); yield return ReturnCreature(); yield return ReturnCreature();
            Assert.That(rig.Game.Phase, Is.EqualTo(EncounterPhase.Reflector));
            yield return Canonical(rig.Game.Session.ObjectPose);
            yield return Canonical(rig.Game.Session.ObjectPose, 1);
            yield return Canonical(new Pose3(rig.Game.Session.ObjectPose.Position, rig.Game.Duel.Lane.SolutionRotation), 1);
            yield return Canonical(rig.Game.Session.ObjectPose);
            double deadline = Time.realtimeSinceStartupAsDouble + 15;
            while (rig.Game.Phase == EncounterPhase.Reflector && Time.realtimeSinceStartupAsDouble < deadline)
                yield return null;
            Assert.That(rig.Game.Phase, Is.EqualTo(EncounterPhase.ShellVulnerable));
            yield return ReturnCreature();
            Assert.That(rig.Game.Phase, Is.EqualTo(EncounterPhase.Won));
            yield return Control(0); // RESTART token, followed by its separate CONFIRM token.
            Assert.That(rig.Game.Controls.Prompt, Is.EqualTo(ControlPrompt.Restart));
            yield return Control(-.095f);
            Assert.That(rig.Game.Phase, Is.EqualTo(EncounterPhase.LearnCapture));
            Assert.That(rig.Game.RunId, Is.Not.EqualTo(run));
            Assert.That(root.GetComponentsInChildren<Camera>(true).Length, Is.EqualTo(1));
            Assert.That(camera.transform.position, Is.EqualTo(initialCamera));
        }

        [UnityTest]
        public IEnumerator DisableAndInvalidationStopTheEncounter()
        {
            yield return Load();
            FirstEncounter original = rig.Game;
            rig.enabled = false; yield return null;
            Assert.That(original.Session.IsPaused, Is.True);
            Assert.That(original.Session.PauseReasons & PauseReason.FocusLost, Is.Not.EqualTo(PauseReason.None));
            rig.enabled = true; yield return null;
            room.BreakRoom(); yield return null; yield return null;
            Assert.That(original.RoomInvalidated, Is.True);
            Assert.That(rig.Game, Is.Null);
            Assert.That(rig.enabled, Is.True, "Recoverable room failure should retain the loading UI.");
        }
    }
}
