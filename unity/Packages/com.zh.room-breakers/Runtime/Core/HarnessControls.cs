using System;
using System.Collections.Generic;
using System.Numerics;

namespace RoomBreakers.Core
{
    public enum ControlPrompt { None, Restart, Reposition }
    public enum ControlEffect { None, RepositionRequested }

    // Composes the real capture driver and menu driver over the SAME session.
    // UI gets first refusal; the gesture that activates a control cannot also grab the probe.
    public sealed class HarnessControls
    {
        private readonly ScaleSession session;
        private bool awaitingReposition;
        public HandCaptureDriver Capture { get; }
        public NearControlDriver Menu { get; }
        public ControlPrompt Prompt { get; private set; }
        public bool AwaitingReposition => awaitingReposition;
        public ControlAction AllowedActions
        {
            get
            {
                if ((session.PauseReasons & PauseReason.FocusLost) != 0) return ControlAction.None;
                if (awaitingReposition) return ControlAction.Cancel;
                if (Prompt != ControlPrompt.None) return ControlAction.Confirm | ControlAction.Cancel;
                ControlAction normal = ControlAction.Restart | ControlAction.Reposition;
                if ((session.PauseReasons & PauseReason.User) == 0) return normal | ControlAction.Pause;
                // A user Resume never acknowledges unexpected world/frame movement.
                return (session.PauseReasons & PauseReason.Placement) == 0 ? normal | ControlAction.Resume : normal;
            }
        }

        public HarnessControls(ScaleSession session, HandCaptureSettings settings = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            Capture = new HandCaptureDriver(session, settings);
            Menu = new NearControlDriver(settings);
        }

        public ControlEffect Step(double now, HandSample left, HandSample right, IReadOnlyList<ControlTarget> targets)
        {
            // A hand already holding the probe cannot also activate a control.
            // The other hand may pause it; acquiring that UI token cancels capture without a reward.
            ControlAction allowed = AllowedActions;
            ControlAction action = Menu.Step(now, left, right, targets, allowed, session.Owner);
            if (Menu.IsEngaged) SetMenuPause(true);
            ControlEffect effect = ControlEffect.None;
            if (action != ControlAction.None && (action & allowed) != 0)
            {
                switch (action)
                {
                    case ControlAction.Pause:
                        Capture.SetExternalPause(PauseReason.User, true);
                        break;
                    case ControlAction.Resume:
                        Capture.SetExternalPause(PauseReason.User, false);
                        break;
                    case ControlAction.Restart:
                        Prompt = ControlPrompt.Restart;
                        break;
                    case ControlAction.Reposition:
                        Prompt = ControlPrompt.Reposition;
                        break;
                    case ControlAction.Cancel:
                        Prompt = ControlPrompt.None;
                        awaitingReposition = false;
                        break;
                    case ControlAction.Confirm:
                        if (Prompt == ControlPrompt.Restart)
                        {
                            Capture.Reset();
                            Prompt = ControlPrompt.None;
                        }
                        else if (Prompt == ControlPrompt.Reposition)
                        {
                            awaitingReposition = true;
                            effect = ControlEffect.RepositionRequested;
                        }
                        break;
                }
            }
            SyncMenuPause();
            Capture.Step(now, left, right);
            return effect;
        }

        // Called only after a confirmed in-headset Reposition request.
        // Keeps the physical room frame and canonical object exactly unchanged.
        // Passing null rejects the proposed move and leaves the confirmation available to retry/cancel.
        public bool CompleteReposition(SpatialFrame miniature)
        {
            if (!awaitingReposition || Prompt != ControlPrompt.Reposition ||
                (session.PauseReasons & PauseReason.FocusLost) != 0) return false;
            awaitingReposition = false;
            if (miniature == null || !Capture.Recalibrate(new DualScaleMap(session.Map.Room, miniature))) return false;
            Prompt = ControlPrompt.None;
            Menu.Cancel();
            SyncMenuPause();
            return true;
        }

        public void SetExternalPause(PauseReason reason, bool enabled)
        {
            if (reason != PauseReason.User && reason != PauseReason.FocusLost && reason != PauseReason.Placement)
                throw new ArgumentException("Only user, focus and placement pauses are external.", nameof(reason));
            if (((session.PauseReasons & reason) != 0) == enabled) return;
            Capture.SetExternalPause(reason, enabled);
            // Changing focus or placement must not leave a pending command ready to fire on return.
            ClearUi();
        }
        public bool Recalibrate(DualScaleMap map)
        {
            if (!Capture.Recalibrate(map)) return false;
            ClearUi();
            return true;
        }
        public void CancelInteraction() { Capture.Cancel(); ClearUi(); }
        public void ResetProbe() { Capture.Reset(); ClearUi(); }
        private void ClearUi()
        {
            Menu.Cancel();
            Prompt = ControlPrompt.None;
            awaitingReposition = false;
            SyncMenuPause();
        }
        private void SyncMenuPause() => SetMenuPause(Menu.IsEngaged || Prompt != ControlPrompt.None);
        private void SetMenuPause(bool enabled)
        {
            if (((session.PauseReasons & PauseReason.Menu) != 0) != enabled)
                Capture.SetExternalPause(PauseReason.Menu, enabled);
        }
    }

    public static class MiniaturePlacement
    {
        // Proposed seated layout, NOT an obstacle/safety detector. Only virtual miniature content moves.
        // Uses the headset pose at confirmation; never continuously follows or moves the camera.
        public static bool TryInFrontOf(Pose3 head, SpatialFrame current, out SpatialFrame proposed,
            float distance = .45f, float belowHead = .30f)
        {
            proposed = null;
            if (!head.IsValid || current == null || !SpatialMath.IsFinite(distance) ||
                !SpatialMath.IsFinite(belowHead) || distance < .25f || distance > .65f || belowHead < .10f || belowHead > .50f)
                return false;
            Vector3 forward = Vector3.Transform(Vector3.UnitZ, head.Rotation);
            forward.Y = 0;
            if (forward.LengthSquared() < .0001f) return false;
            forward = Vector3.Normalize(forward);
            try
            {
                Quaternion yaw = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.Atan2(forward.X, forward.Z));
                proposed = new SpatialFrame(new Pose3(head.Position + forward * distance - Vector3.UnitY * belowHead, yaw), current.Scale);
                return true;
            }
            catch (ArgumentException) { return false; }
        }
    }
}
