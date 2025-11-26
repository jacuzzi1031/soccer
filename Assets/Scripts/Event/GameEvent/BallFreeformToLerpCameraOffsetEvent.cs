using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallFreeformToLerpCameraOffsetEvent : IEvent
{
    public bool IsFreeform;

    public BallFreeformToLerpCameraOffsetEvent(bool isFreeform)
    {
        IsFreeform = isFreeform;
    }
}
