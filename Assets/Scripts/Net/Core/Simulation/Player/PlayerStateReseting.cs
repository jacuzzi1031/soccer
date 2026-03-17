using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateReseting: PlayerSimState
{
  public bool hasArrived = false;
  public float arriveDistance = 1f;
  public override void OnEnter() {
    hasArrived = false;
  }

  public override void _Update(float deltaTime) {
    if (hasArrived) return;
    Vector2 playerPosition = playerSim.Position;
    Vector2 dir = (stateData.ResetPosition - playerPosition).normalized;
    if ((stateData.ResetPosition - playerPosition).sqrMagnitude < arriveDistance * arriveDistance)
    {
      hasArrived = true;
      playerSim.HeadingRight = playerSim.initialFacingRight;
      playerSim.Velocity = Vector2.zero;
    }
    else
    {
      playerSim.Velocity = dir * playerSim.Speed;
    }
    playerPosition += playerSim.Velocity * deltaTime;
    playerSim.Position = playerPosition;
  }
  
  public override bool IsReadyForKickoff()
  {
    return hasArrived;
  }
}
