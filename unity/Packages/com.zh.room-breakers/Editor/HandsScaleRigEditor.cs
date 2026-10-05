using RoomBreakers.UnityInput;
using UnityEditor;
using UnityEngine;

namespace RoomBreakers.ScaleLab.Editor
{
    [CustomEditor(typeof(HandsScaleRig))]
    public sealed class HandsScaleRigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("Development harness. These Inspector buttons are NOT a hands-first in-headset menu. Connect the public methods to real XR UI before claiming end-to-end hands support.", MessageType.Info);
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                var rig = (HandsScaleRig)target;
                if (GUILayout.Button("Pause interaction")) rig.PauseInteraction();
                if (GUILayout.Button("Resume (then open a tracked hand)")) rig.ResumeInteraction();
                if (GUILayout.Button("Restart probe")) rig.RestartProbe();
                if (GUILayout.Button("Cancel current capture")) rig.CancelInteraction();
                if (GUILayout.Button("Confirm edited frame placement")) rig.ConfirmPlacement();
                if (GUILayout.Button("Recenter virtual test layout"))
                    rig.GetComponent<HandsHarnessPlacement>()?.RecenterVirtualLayout();
            }
        }
    }
}
