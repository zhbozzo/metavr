using Oculus.Interaction.Input;
using RoomBreakers.UnityInput;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RoomBreakers.MetaHands.Editor
{
    // No SDK downloads, license changes, automatic camera replacement or private room data.
    public sealed class HandsHarnessWindow : EditorWindow
    {
        private Camera playerCamera;
        private MonoBehaviour leftHand, rightHand;
        [MenuItem("Tools/RoomBreakers/Add Meta Hands Harness")]
        public static void Open()
        {
            var window = GetWindow<HandsHarnessWindow>("RoomBreakers Hands");
            window.playerCamera = Camera.main;
        }
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Add a hand-input test to your existing XR scene. Uses a synthetic layout, not MRUK or a room scan. Requires SDK v207.x and an already configured camera/hand rig. This integration has not yet been run in Unity.", MessageType.Info);
            playerCamera = (Camera)EditorGUILayout.ObjectField("Existing XR camera", playerCamera, typeof(Camera), true);
            leftHand = (MonoBehaviour)EditorGUILayout.ObjectField("Left tracked IHand", leftHand, typeof(MonoBehaviour), true);
            rightHand = (MonoBehaviour)EditorGUILayout.ObjectField("Right tracked IHand", rightHand, typeof(MonoBehaviour), true);
            EditorGUILayout.LabelField("One hand is sufficient. Do not select controller-driven/synthetic hands.", EditorStyles.wordWrappedLabel);
            bool inputs = (leftHand != null || rightHand != null) &&
                (leftHand == null || leftHand is IHand) && (rightHand == null || rightHand is IHand) &&
                (leftHand == null || leftHand != rightHand);
            bool sceneObjects = playerCamera != null && !EditorUtility.IsPersistent(playerCamera) &&
                (leftHand == null || !EditorUtility.IsPersistent(leftHand)) &&
                (rightHand == null || !EditorUtility.IsPersistent(rightHand));
            using (new EditorGUI.DisabledScope(Application.isPlaying || !inputs || !sceneObjects))
            {
                if (GUILayout.Button("Add harness to current scene")) Build();
            }
            if (!inputs) EditorGUILayout.HelpBox("Assign distinct scene components implementing IHand. At least one side is required.", MessageType.Warning);
            if (Application.isPlaying) EditorGUILayout.HelpBox("Exit Play Mode before changing scene wiring.", MessageType.Warning);
        }
        private void Build()
        {
            var root = new GameObject("ROOMBREAKERS - Hands harness (synthetic)");
            Undo.RegisterCreatedObjectUndo(root, "Add RoomBreakers hands harness");
            var room = Child(root.transform, "Room reference frame");
            var mini = Child(root.transform, "Miniature frame"); mini.localScale = Vector3.one * .2f;
            // Provisional edit-time placement; re-positioned once fresh tracked input is received in Play Mode.
            Vector3 forward = Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up);
            if (forward.sqrMagnitude < .01f) forward = Vector3.forward;
            forward.Normalize(); var yaw = Quaternion.LookRotation(forward, Vector3.up);
            room.SetPositionAndRotation(playerCamera.transform.position + forward * 1.8f - Vector3.up * .55f, yaw);
            mini.SetPositionAndRotation(playerCamera.transform.position + forward * .45f - Vector3.up * .30f, yaw);
            MetaHandSampleSource left = AddSource(root.transform, leftHand, "Left source");
            MetaHandSampleSource right = AddSource(root.transform, rightHand, "Right source");
            var rig = root.AddComponent<HandsScaleRig>(); rig.Configure(room, mini, left, right, playerCamera);
            var placement = root.AddComponent<HandsHarnessPlacement>(); placement.Configure(rig, room, mini, left, right, playerCamera);
            EditorUtility.SetDirty(rig); EditorUtility.SetDirty(placement);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
        }
        private static Transform Child(Transform parent, string name)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false); return t;
        }
        private static MetaHandSampleSource AddSource(Transform parent, MonoBehaviour hand, string label)
        {
            if (hand == null) return null;
            var source = Child(parent, label).gameObject.AddComponent<MetaHandSampleSource>();
            source.Bind(hand); EditorUtility.SetDirty(source); return source;
        }
    }
}
