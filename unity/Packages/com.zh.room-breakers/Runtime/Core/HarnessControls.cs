using System;
using System.Collections.Generic;
using System.Numerics;

namespace RoomBreakers.Core
{
    public enum ControlPrompt { None, Restart, Reposition }
    public enum ControlEffect { None, RepositionRequested }

    // UI and capture share one authoritative session; a UI gesture never falls through into gameplay.
    public sealed class HarnessControls
    {
        private readonly ScaleSession session;
        private bool awaitingReposition;
        public HandCaptureDriver Capture { get; }
        public NearControlDriver Menu { get; }
        public ControlPrompt Prompt { get; private set; }
        public bool AwaitingReposition => awaitingReposition;
        public long RestartSerial { get; private set; }
        public ControlAction AllowedActions
        {
            get
            {
                if ((session.PauseReasons & PauseReason.FocusLost) != 0) return ControlAction.None;
                if (awaitingReposition) return ControlAction.Cancel;
                if (Prompt != ControlPrompt.None) return ControlAction.Confirm | ControlAction.Cancel;
                ControlAction normal = ControlAction.Restart | ControlAction.Reposition;
                if ((session.PauseReasons & PauseReason.User) == 0) return normal | ControlAction.Pause;
                return (session.PauseReasons & PauseReason.Placement) == 0 ? normal | ControlAction.Resume : normal;
            }
        }
        public HarnessControls(ScaleSession session, HandCaptureSettings settings = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            Capture = new HandCaptureDriver(session, settings); Menu = new NearControlDriver(settings);
        }
        public ControlEffect Step(double now, HandSample left, HandSample right, IReadOnlyList<ControlTarget> targets)
        {
            ControlAction allowed = AllowedActions;
            ControlAction action = Menu.Step(now, left, right, targets, allowed, session.Owner);
            if (Menu.IsEngaged) SetMenuPause(true);
            ControlEffect effect = ControlEffect.None;
            if (action != ControlAction.None && (action & allowed) != 0)
            {
                switch (action)
                {
                    case ControlAction.Pause: Capture.SetExternalPause(PauseReason.User, true); break;
                    case ControlAction.Resume: Capture.SetExternalPause(PauseReason.User, false); break;
                    case ControlAction.Restart: Prompt = ControlPrompt.Restart; break;
                    case ControlAction.Reposition: Prompt = ControlPrompt.Reposition; break;
                    case ControlAction.Cancel: Prompt = ControlPrompt.None; awaitingReposition = false; break;
                    case ControlAction.Confirm:
                        if (Prompt == ControlPrompt.Restart)
                        {
                            Capture.Reset(); RestartSerial++; Prompt = ControlPrompt.None;
                        }
                        else if (Prompt == ControlPrompt.Reposition)
                        {
                            awaitingReposition = true; effect = ControlEffect.RepositionRequested;
                        }
                        break;
                }
            }
            SyncMenuPause(); Capture.Step(now, left, right); return effect;
        }
        public bool CompleteReposition(SpatialFrame miniature)
        {
            if (!awaitingReposition || Prompt != ControlPrompt.Reposition || (session.PauseReasons & PauseReason.FocusLost) != 0) return false;
            awaitingReposition = false;
            if (miniature == null || !Capture.Recalibrate(new DualScaleMap(session.Map.Room, miniature))) return false;
            Prompt = ControlPrompt.None; Menu.Cancel(); SyncMenuPause(); return true;
        }
        public void SetExternalPause(PauseReason reason, bool enabled)
        {
            if (reason != PauseReason.User && reason != PauseReason.FocusLost && reason != PauseReason.Placement)
                throw new ArgumentException("Only user, focus and placement pauses are external.", nameof(reason));
            if (((session.PauseReasons & reason) != 0) == enabled) return;
            Capture.SetExternalPause(reason, enabled); ClearUi();
        }
        public bool Recalibrate(DualScaleMap map)
        {
            if (!Capture.Recalibrate(map)) return false;
            ClearUi(); return true;
        }
        public void CancelInteraction() { Capture.Cancel(); ClearUi(); }
        public void ResetProbe() { Capture.Reset(); RestartSerial++; ClearUi(); }
        private void ClearUi()
        {
            Menu.Cancel(); Prompt = ControlPrompt.None; awaitingReposition = false; SyncMenuPause();
        }
        private void SyncMenuPause() => SetMenuPause(Menu.IsEngaged || Prompt != ControlPrompt.None);
        private void SetMenuPause(bool enabled)
        {
            if (((session.PauseReasons & PauseReason.Menu) != 0) != enabled) Capture.SetExternalPause(PauseReason.Menu, enabled);
        }
    }

    public static class MiniaturePlacement
    {
        // Proposed seated layout, not physical boundary/obstacle detection. Never moves the camera.
        public static bool TryInFrontOf(Pose3 head, SpatialFrame current, out SpatialFrame proposed,
            float distance = .45f, float belowHead = .30f)
        {
            proposed = null;
            if (!head.IsValid || current == null || !SpatialMath.IsFinite(distance) || !SpatialMath.IsFinite(belowHead) ||
                distance < .25f || distance > .65f || belowHead < .10f || belowHead > .50f) return false;
            Vector3 forward = Vector3.Transform(Vector3.UnitZ, head.Rotation); forward.Y = 0;
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
