using RoomBreakers.Core;
using UnityEngine;

namespace RoomBreakers.UnityInput
{
    // Development input only. It never masquerades as a tracked Meta hand.
    [DefaultExecutionOrder(-40)]
    public sealed class DesktopEncounterHand : HandSampleSource
    {
        [SerializeField] private FirstEncounterRig rig;
        private Vector2 mouse;
        private Vector3 lastPoint;
        private Plane plane;
        private bool pressed, hasPlane, releasePending, focused = true;
        private long sequence;
        private HandSample latest;
        public void Configure(FirstEncounterRig target) { rig = target; }
        public override bool TryGetLatest(out HandSample sample) { sample = latest; return latest.Sequence > 0; }
        private Ray Ray() => rig.ViewCamera.ScreenPointToRay(new Vector3(mouse.x, Screen.height - mouse.y, 0));
        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (rig == null || rig.ViewCamera == null) return;
            Event e = Event.current; mouse = e.mousePosition;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (rig.TryPickDesktop(Ray(), out Vector3 point, out bool menu))
                {
                    lastPoint = point; plane = new Plane(menu ? rig.ViewCamera.transform.forward : Vector3.up, point); hasPlane = true;
                }
                pressed = true; releasePending = false;
            }
            else if (e.type == EventType.MouseUp && e.button == 0) { pressed = false; releasePending = true; }
            else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            { rig.Game?.Controls.CancelInteraction(); pressed = hasPlane = releasePending = false; }
#endif
        }
        private void LateUpdate()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (rig == null || rig.ViewCamera == null) return;
            Ray ray = Ray(); bool valid = focused;
            if (hasPlane)
            {
                if (plane.Raycast(ray, out float enter)) lastPoint = ray.GetPoint(enter); else valid = false;
            }
            else if (rig.TryPickDesktop(ray, out Vector3 hover, out _)) lastPoint = hover;
            else
            {
                Vector3 anchor = rig.UiFrame != null ? UnitySpatialFrame.Vector(rig.UiFrame.Origin.Position) : new Vector3(0, .9f, .45f);
                var hoverPlane = new Plane(Vector3.up, anchor);
                if (hoverPlane.Raycast(ray, out float enter)) lastPoint = ray.GetPoint(enter); else valid = false;
            }
            latest = new HandSample(HandId.Right, ++sequence, Time.realtimeSinceStartupAsDouble,
                UnitySpatialFrame.Pose(lastPoint, Quaternion.identity), pressed ? 1 : 0, valid);
            if (releasePending) { releasePending = false; hasPlane = false; }
#endif
        }
        private void OnApplicationFocus(bool value) { focused = value; if (!value) pressed = hasPlane = releasePending = false; }
        private void OnDisable() { latest = default; pressed = hasPlane = releasePending = false; }
    }
}
