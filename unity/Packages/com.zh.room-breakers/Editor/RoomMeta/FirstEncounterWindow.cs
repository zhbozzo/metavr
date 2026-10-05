using Meta.XR.MRUtilityKit;
using Oculus.Interaction.Input;
using RoomBreakers.MetaHands;
using RoomBreakers.MetaRoom;
using RoomBreakers.UnityInput;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RoomBreakers.MetaEncounter.Editor
{
    public sealed class FirstEncounterWindow : EditorWindow
    {
        private MRUK manager;
        private Camera camera;
        private MonoBehaviour left, right;
        [MenuItem("Tools/RoomBreakers/Add Device Room Encounter")]
        public static void Open() => GetWindow<FirstEncounterWindow>("Room Encounter");
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Adds only ROOMBREAKERS content to an existing XR scene. Configure the camera rig, passthrough, scene permission and tracked-hand providers first. SDK compilation and Quest validation are still required.", MessageType.Info);
            manager = (MRUK)EditorGUILayout.ObjectField("MRUK", manager, typeof(MRUK), true);
            camera = (Camera)EditorGUILayout.ObjectField("Existing XR camera", camera, typeof(Camera), true);
            left = (MonoBehaviour)EditorGUILayout.ObjectField("Left tracked IHand", left, typeof(MonoBehaviour), true);
            right = (MonoBehaviour)EditorGUILayout.ObjectField("Right tracked IHand", right, typeof(MonoBehaviour), true);
            EditorGUILayout.HelpBox("MRUK: Device only; Load Scene On Startup OFF; World Lock ON. This tool does not change those settings or install packages. One tracked hand is sufficient.", MessageType.None);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying || manager == null || camera == null || (left == null && right == null)))
                if (GUILayout.Button("Add first room encounter")) Add();
        }
        private void Add()
        {
            if ((left != null && (!(left is IHand l) || l.Handedness != Handedness.Left)) ||
                (right != null && (!(right is IHand r) || r.Handedness != Handedness.Right)))
            { EditorUtility.DisplayDialog("ROOMBREAKERS", "Each hand reference must implement IHand with the matching side.", "OK"); return; }
            if (manager.SceneSettings.DataSource != MRUK.SceneDataSource.Device || manager.SceneSettings.LoadSceneOnStartup || !manager.EnableWorldLock)
            { EditorUtility.DisplayDialog("ROOMBREAKERS", "Configure MRUK as described before adding the encounter.", "OK"); return; }
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            if (shader == null) { EditorUtility.DisplayDialog("ROOMBREAKERS", "A compatible unlit shader is required.", "OK"); return; }
            var root = new GameObject("ROOMBREAKERS Device Room Encounter"); Undo.RegisterCreatedObjectUndo(root, "Add ROOMBREAKERS room encounter");
            var source = root.AddComponent<MetaRoomSource>(); source.Configure(manager, camera);
            HandSampleSource lSource = Bind(root.transform, left, "Left hand source");
            HandSampleSource rSource = Bind(root.transform, right, "Right hand source");
            var rig = root.AddComponent<FirstEncounterRig>(); rig.Configure(source, lSource, rSource, camera, shader);
            EditorUtility.SetDirty(source); EditorUtility.SetDirty(rig);
            Selection.activeGameObject = root; EditorSceneManager.MarkSceneDirty(root.scene);
        }
        private static HandSampleSource Bind(Transform parent, MonoBehaviour hand, string label)
        {
            if (hand == null) return null;
            var child = new GameObject(label); child.transform.SetParent(parent, false);
            var source = child.AddComponent<MetaHandSampleSource>(); source.Bind(hand); EditorUtility.SetDirty(source); return source;
        }
    }
}
