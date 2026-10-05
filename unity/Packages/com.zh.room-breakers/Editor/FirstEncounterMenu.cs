using RoomBreakers.UnityInput;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RoomBreakers.ScaleLab.Editor
{
    public static class FirstEncounterMenu
    {
        [MenuItem("Tools/RoomBreakers/Open First Encounter (Desktop)")]
        public static void Open()
        {
            if (EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            if (shader == null) { EditorUtility.DisplayDialog("ROOMBREAKERS", "A built-in or URP unlit shader is required.", "OK"); return; }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Synthetic seated desktop camera");
            var camera = cameraObject.AddComponent<Camera>(); cameraObject.AddComponent<AudioListener>();
            camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 1.2f, 0);
            camera.transform.rotation = Quaternion.Euler(15, 0, 0); camera.fieldOfView = 90;
            camera.nearClipPlane = .03f; camera.farClipPlane = 30;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.04f, .055f, .075f);
            var root = new GameObject("First Encounter - SYNTHETIC MOUSE DEVELOPMENT MODE");
            var source = root.AddComponent<SyntheticRoomSource>();
            var hand = root.AddComponent<DesktopEncounterHand>();
            var rig = root.AddComponent<FirstEncounterRig>();
            rig.Configure(source, null, hand, camera, shader); hand.Configure(rig);
            Selection.activeGameObject = root; EditorSceneManager.MarkSceneDirty(scene);
            // User saves a real scene, with real editor-generated metadata. No fabricated YAML/GUIDs.
        }
    }
}
