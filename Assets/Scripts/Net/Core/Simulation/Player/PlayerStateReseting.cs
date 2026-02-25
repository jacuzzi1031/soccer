using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateReseting: PlayerSimState
{
  public bool hasArrived = false;
  public float arriveDistance = 2f;
  public override void OnEnter() {
  }

  public override void _Update(float deltaTime) {
    if (hasArrived) return;

    Vector2 dir = (stateData.ResetPosition - playerSim.Position).normalized;
    if ((stateData.ResetPosition - playerSim.Position).sqrMagnitude < arriveDistance * arriveDistance)
    {
      hasArrived = true;
      playerSim.Velocity = Vector2.zero;
    }
    else
    {
      playerSim.Velocity = dir * playerSim.Speed;
    }
    playerSim.Position += playerSim.Velocity * deltaTime;
  }
  
  public override bool IsReadyForKickoff()
  {
    return hasArrived;
  }
}
