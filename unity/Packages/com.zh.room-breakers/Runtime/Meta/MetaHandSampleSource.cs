using System;
using Oculus.Interaction.Input;
using RoomBreakers.Core;
using RoomBreakers.UnityInput;
using UnityEngine;

namespace RoomBreakers.MetaHands
{
    // API contract checked against official Interaction SDK v207 documentation.
    // Import/compile/device validation is still required. This is not a mock SDK.
    [DisallowMultipleComponent]
    public sealed class MetaHandSampleSource : HandSampleSource
    {
        [SerializeField, Tooltip("Assign an SDK Hand or HandRef backed by tracked hands, not controller-driven hands.")]
        private MonoBehaviour handComponent;
        private IHand hand;
        private HandSample latest;
        private bool hasSample, hasVersion;
        private int lastDataVersion;
        private long sequence;

        public void Bind(MonoBehaviour component)
        {
            if (!(component is IHand)) throw new ArgumentException("Component must implement Meta IHand.", nameof(component));
            PublishInvalid();
            handComponent = component; hand = (IHand)component;
            hasVersion = false;
        }
        public override bool TryGetLatest(out HandSample sample)
        {
            sample = latest;
            return hasSample;
        }
        private void OnEnable()
        {
            hand = handComponent as IHand;
            hasVersion = false;
            if (hand == null && Application.isPlaying)
                Debug.LogError("MetaHandSampleSource needs an explicitly assigned IHand component.", this);
        }
        private HandId Side()
        {
            if (hand == null) return HandId.None;
            return hand.Handedness == Handedness.Left ? HandId.Left :
                hand.Handedness == Handedness.Right ? HandId.Right : HandId.None;
        }
        private void PublishInvalid()
        {
            // One invalidation is enough. Repeating a stale provider never supplies valid freshness.
            if (hasSample && !latest.Tracked) return;
            HandId side = hasSample ? latest.Hand : Side();
            latest = new HandSample(side, ++sequence, Time.realtimeSinceStartupAsDouble, default, 0, false);
            hasSample = true;
        }
        private void Update()
        {
            if (handComponent == null || !handComponent.isActiveAndEnabled || hand == null)
            { PublishInvalid(); return; }
            int version = hand.CurrentDataVersion;
            if (!hasVersion)
            {
                // Establish a baseline; don't call an already-cached SDK pose a new observation.
                hasVersion = true; lastDataVersion = version; PublishInvalid(); return;
            }
            bool changed = version != lastDataVersion;
            if (changed) lastDataVersion = version;
            if (!hand.IsHighConfidence || !hand.IsTrackedDataValid ||
                !hand.GetFingerIsHighConfidence(HandFinger.Index) || !hand.GetFingerIsHighConfidence(HandFinger.Thumb))
            { PublishInvalid(); return; }
            if (!changed) return; // The driver watchdog handles an SDK that stopped producing data.

            if (!hand.GetJointPose(HandJointId.HandIndexTip, out Pose index) ||
                !hand.GetJointPose(HandJointId.HandThumbTip, out Pose thumb) ||
                !hand.GetRootPose(out Pose wrist))
            { PublishInvalid(); return; }
            try
            {
                // Meta's GetJointPose/GetRootPose already return world-space poses. Do not transform twice.
                Vector3 center = index.position * .5f + thumb.position * .5f;
                Pose3 grip = UnitySpatialFrame.Pose(center, wrist.rotation);
                float pinch = hand.GetFingerPinchStrength(HandFinger.Index);
                if (!SpatialMath.IsFinite(pinch) || pinch < 0 || pinch > 1 || Side() == HandId.None)
                { PublishInvalid(); return; }
                latest = new HandSample(Side(), ++sequence, Time.realtimeSinceStartupAsDouble, grip, pinch, true);
                hasSample = true;
            }
            catch (ArgumentException) { PublishInvalid(); }
        }
        private void OnDisable() { if (Application.isPlaying) PublishInvalid(); }
    }
}
