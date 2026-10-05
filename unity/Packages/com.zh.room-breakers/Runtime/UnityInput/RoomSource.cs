using RoomBreakers.Core;
using UnityEngine;

namespace RoomBreakers.UnityInput
{
    public enum RoomSourceState { Idle, Loading, Ready, PermissionRequired, Unavailable, Invalid, Failed }

    // Holds only in-memory room data. Consumers pin Revision and stop when it changes.
    public abstract class RoomSource : MonoBehaviour
    {
        public RoomSourceState State { get; protected set; } = RoomSourceState.Idle;
        public string Status { get; protected set; } = "Load your room to begin.";
        public RoomSnapshot Snapshot { get; protected set; }
        public SpatialFrame WorldFrame { get; protected set; }
        public long Revision { get; protected set; }
        public abstract void Load();
        protected void Invalidate(string message)
        {
            State = RoomSourceState.Invalid; Status = message; Snapshot = null; WorldFrame = null; Revision++;
        }
    }
}
