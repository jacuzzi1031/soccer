using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityForGameKickoffEvent : IEvent
{
    public int entityId;

    public EntityForGameKickoffEvent(int entityId) {
        this.entityId = entityId;
    }
}
