using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatePreppingShot: PlayerSimState
{
    Vector2 shotDirection = Vector2.zero;
    private float timeStartShot;
    private const float DURATION_MAX_BONUS=1.5f;
    private const float EASE_REWARD_FACTOR = 2.0f;
    private float pressRaw;
    private bool hasTriggeredBonusEvent = false;
    private float pressTime = 0f;
    public override void OnEnter() {
        hasTriggeredBonusEvent = false;
        pressTime = 0f;
        playerSim.Velocity = Vector2.zero;
        shotDirection=_moveDirection.x>=0? Vector2.right:Vector2.left;
    }

    public override void OnExit() {
    }
    public override void _Update(float deltaTime) {
        pressTime += deltaTime;
        shotDirection += _moveDirection;

        if (!hasTriggeredBonusEvent && pressTime >= DURATION_MAX_BONUS * 0.5f)
        {
            _eventBus.Publish(
                new PlayStyleShowSignal(playerSim.playerId, PlayerState.PREPPING_SHOT)
            );
            hasTriggeredBonusEvent = true;
        }
    }
    public override void OnShootRelease(bool hasBall,bool ballCanAirInteract)
    {
        if (!hasBall) {
            return;
        }
        float durationPress = Mathf.Clamp(pressTime, 0f, DURATION_MAX_BONUS);
        
        float easeTime = durationPress / DURATION_MAX_BONUS;
        float bonus = Mathf.Pow(easeTime, EASE_REWARD_FACTOR);
        
        float shotPower = playerSim.Power * (1f + 0.7f * bonus);
        if (hasTriggeredBonusEvent) {
            if (shotDirection.y > 0.1f) {
                var Dir = playerSim.GetTopTargetPosition() - playerSim.Position;
                shotDirection = Dir.normalized;
            }
            else if (shotDirection.y < -0.1f) {
                var Dir = playerSim.GetBottomTargetPosition() - playerSim.Position;
                shotDirection = Dir.normalized;
            }
            else {
                shotDirection = shotDirection.normalized;
            }
        }
        else {
            shotDirection = shotDirection.normalized;
        }
        
        PlayerStateData data = PlayerStateData.Build()
            .SetShotPower(shotPower)
            .SetShotDirection(shotDirection)
            .SetIsInstant(false);
        playerSim.SwitchState(PlayerState.SHOOTING, data);
    }

    public override bool CouldHurt() {
        if (_ballSim.carrier.playerId == playerSim.playerId) {
            return true;
        }
        return false;
    }
}


