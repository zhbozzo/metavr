using RoomBreakers.Core;
using UnityEngine;

namespace RoomBreakers.UnityInput
{
    // Places virtual test content once, not the camera or real-world furniture.
    // This is a provisional test layout, NOT scene understanding or a safety boundary.
    [DefaultExecutionOrder(-20)]
    public sealed class HandsHarnessPlacement : MonoBehaviour
    {
        [SerializeField] private HandsScaleRig rig;
        [SerializeField] private Transform roomFrame, miniatureFrame;
        [SerializeField] private HandSampleSource leftSource, rightSource;
        [SerializeField] private Camera playerCamera;
        private bool attempted;
        public void Configure(HandsScaleRig target, Transform room, Transform mini, HandSampleSource left,
            HandSampleSource right, Camera camera)
        {
            rig = target; roomFrame = room; miniatureFrame = mini;
            leftSource = left; rightSource = right; playerCamera = camera;
        }
        private static bool Fresh(HandSampleSource source, double now)
        {
            return source != null && source.TryGetLatest(out HandSample sample) && sample.Tracked &&
                sample.Grip.IsValid && sample.Timestamp >= 0 && sample.Timestamp <= now && now - sample.Timestamp <= .20;
        }
        private void LateUpdate()
        {
            if (attempted || rig == null || !rig.IsReady) return;
            double now = Time.realtimeSinceStartupAsDouble;
            if (!Fresh(leftSource, now) && !Fresh(rightSource, now)) return;
            RecenterVirtualLayout();
        }
        [ContextMenu("Recenter virtual test layout")]
        public void RecenterVirtualLayout()
        {
            if (!Application.isPlaying || rig == null || !rig.IsReady || playerCamera == null ||
                roomFrame == null || miniatureFrame == null) return;
            Vector3 forward = Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up);
            if (forward.sqrMagnitude < .01f) return;
            forward.Normalize(); Quaternion yaw = Quaternion.LookRotation(forward, Vector3.up);
            Vector3 head = playerCamera.transform.position;
            roomFrame.SetPositionAndRotation(head + forward * 1.8f - Vector3.up * .55f, yaw);
            miniatureFrame.SetPositionAndRotation(head + forward * .45f - Vector3.up * .30f, yaw);
            // ConfirmPlacement cancels any capture before remapping. It owns only Placement,
            // so an existing User/FocusLost pause is never cleared by this operation.
            rig.ConfirmPlacement();
            attempted = true;
        }
    }
}
