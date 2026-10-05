using System;
using System.Collections.Generic;
using System.IO;
using RoomBreakers.UnityInput;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoomBreakers.ScaleLab.Editor
{
    // Executed only by an actual Unity editor after its assemblies compile.
    // This prepares desktop development, not passthrough, Meta providers or an Android build.
    public static class DesktopProjectSetup
    {
        public const string ScenePath = "Assets/RoomBreakersGenerated/DesktopEncounter.unity";
        [Serializable]
        private sealed class Receipt
        {
            public int schema = 1;
            public string status = "PREPARED", unityVersion, nonce, scenePath = ScenePath;
            public int rigCount = 1, cameraCount = 1;
            public bool questTested = false;
        }

        public static void Prepare()
        {
            if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Use the setup script with a closed project.");
            string destination = Argument("-rbSetupReceipt"), nonce = Argument("-rbSetupNonce");
            if (!Path.IsPathRooted(destination) || !Guid.TryParseExact(nonce, "N", out _))
                throw new ArgumentException("A fresh absolute receipt path and nonce are required.");
            if (File.Exists(destination) || !Directory.Exists(Path.GetDirectoryName(destination)))
                throw new InvalidOperationException("Receipt directory must exist and its file must be new.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Modified scenes must be saved before batch setup.");

            Scene scene;
            if (File.Exists(ScenePath))
            {
                // Existing scenes are inspected, never overwritten or silently regenerated.
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Validate(scene);
            }
            else
            {
                if (!AssetDatabase.IsValidFolder("Assets/RoomBreakersGenerated"))
                {
                    if (Directory.Exists("Assets/RoomBreakersGenerated"))
                        throw new InvalidOperationException("Generated scene folder exists but is not an imported asset folder.");
                    AssetDatabase.CreateFolder("Assets", "RoomBreakersGenerated");
                }
                scene = FirstEncounterMenu.CreateScene();
                Validate(scene);
                if (!EditorSceneManager.SaveScene(scene, ScenePath, false))
                    throw new IOException("Unity could not save the desktop scene.");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            var receipt = new Receipt { unityVersion = Application.unityVersion, nonce = nonce };
            // CreateNew refuses old evidence. No report is written on validation/compiler failure.
            using (var stream = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream)) writer.Write(JsonUtility.ToJson(receipt, true));
            Debug.Log("ROOMBREAKERS desktop scene prepared. PlayMode and Quest validation are separate.");
        }

        private static void Validate(Scene scene)
        {
            var rigs = new List<FirstEncounterRig>();
            var cameras = new List<Camera>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                rigs.AddRange(root.GetComponentsInChildren<FirstEncounterRig>(true));
                cameras.AddRange(root.GetComponentsInChildren<Camera>(true));
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) != 0)
                        throw new InvalidOperationException("A scene object contains a missing script.");
            }
            if (rigs.Count != 1 || cameras.Count != 1 || !rigs[0].enabled || !cameras[0].enabled ||
                !rigs[0].gameObject.activeInHierarchy || !cameras[0].gameObject.activeInHierarchy)
                throw new InvalidOperationException("The desktop scene requires one active encounter rig and one camera.");
            var data = new SerializedObject(rigs[0]);
            var room = Reference(data, "roomSource") as SyntheticRoomSource;
            var hand = Reference(data, "rightSource") as DesktopEncounterHand;
            var camera = Reference(data, "playerCamera") as Camera;
            if (room == null || hand == null || camera != cameras[0] || Reference(data, "prototypeShader") == null ||
                !room.enabled || !hand.enabled || !room.gameObject.activeInHierarchy || !hand.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Desktop source, hand, camera and serialized shader references must be assigned.");
            var handData = new SerializedObject(hand);
            if (Reference(handData, "rig") != rigs[0])
                throw new InvalidOperationException("Desktop hand source points at a different encounter.");
        }
        private static UnityEngine.Object Reference(SerializedObject data, string property)
        {
            var value = data.FindProperty(property);
            if (value == null) throw new InvalidOperationException("Missing serialized property: " + property);
            return value.objectReferenceValue;
        }
        private static string Argument(string key)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++) if (arguments[i] == key) return arguments[i + 1];
            throw new ArgumentException("Missing setup argument: " + key);
        }
    }
}
