using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RoomBreakers.ScaleLab.Editor
{
    public static class ScaleLabMenu
    {
        [MenuItem("Tools/RoomBreakers/Open Scale Lab")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Exit Play Mode before creating a Scale Lab scene.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("ROOMBREAKERS - Synthetic Scale Lab");
            root.AddComponent<global::RoomBreakers.ScaleLab.ScaleLab>();
            Selection.activeGameObject = root;
            Debug.Log("Press Play, then use the Game view. This scene is synthetic and mouse-only; it does not integrate Meta XR. Save it under a development-only scene path if needed.");
        }
    }
}
