using RoomBreakers.Core;
using UnityEngine;

namespace RoomBreakers.UnityInput
{
    public sealed class SyntheticRoomSource : RoomSource
    {
        [SerializeField] private bool obstruction = true;
        public override void Load()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Snapshot = SyntheticRooms.Rectangular(obstruction);
            WorldFrame = new SpatialFrame(Pose3.Identity, 1);
            Revision++; State = RoomSourceState.Ready; Status = "SYNTHETIC DEVELOPMENT ROOM - not device geometry.";
#else
            State = RoomSourceState.Unavailable; Status = "Synthetic room source is disabled in release builds.";
#endif
        }
        private void OnDisable() { Invalidate("Synthetic source stopped."); }
    }
}
