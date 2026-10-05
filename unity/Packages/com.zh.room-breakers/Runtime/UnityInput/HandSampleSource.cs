using RoomBreakers.Core;
using UnityEngine;

namespace RoomBreakers.UnityInput
{
    // A provider returns its latest sample without manufacturing a new timestamp every frame.
    // Source identity and monotonically increasing sequence must survive temporary disable/enable.
    public abstract class HandSampleSource : MonoBehaviour
    {
        public abstract bool TryGetLatest(out HandSample sample);
    }
}
