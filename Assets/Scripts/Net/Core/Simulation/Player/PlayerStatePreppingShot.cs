using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStatePreppingShot: PlayerSimState
{
    private FixedVector2 shotDirection = FixedVector2.Zero;

    private const int MAX_BONUS_FRAMES = 90;
    private const int BONUS_EVENT_FRAMES = 45;

    private static readonly FixedFloat EASE_REWARD_FACTOR = (FixedFloat)2f;

    private bool hasTriggeredBonusEvent;
    public override void OnEnter()
    {
        hasTriggeredBonusEvent = false;
        stateFrame = 0;

        playerSim.Velocity = FixedVector2.Zero;

        shotDirection =
            _moveDirection.x >= FixedFloat.Zero
                ? FixedVector2.Right
                : FixedVector2.Left;
    }
    public override void _Update()
    {
        stateFrame++;

        shotDirection += _moveDirection;

        if (!hasTriggeredBonusEvent &&
            stateFrame >= BONUS_EVENT_FRAMES)
        {
            _eventBus.Publish(
                new PlayStyleShowSignal(
                    playerSim.playerId,
                    PlayerState.PREPPING_SHOT)
            );

            hasTriggeredBonusEvent = true;
        }
    }
    public override void OnShootRelease(
        bool hasBall,
        bool ballCanAirInteract)
    {
        if (!hasBall)
            return;

        FixedFloat clampedFrames =
            FixedFloat.Min(stateFrame, MAX_BONUS_FRAMES);

        FixedFloat easeTime = clampedFrames / (FixedFloat)MAX_BONUS_FRAMES;

        FixedFloat bonus =easeTime * easeTime;

        FixedFloat shotPower =
            playerSim.Power *
            (FixedFloat.One + (FixedFloat)0.7f * bonus);

        if (hasTriggeredBonusEvent)
        {
            if (shotDirection.y > (FixedFloat)0.1f)
            {
                FixedVector2 dir =
                    playerSim.GetTopTargetPosition()
                    - playerSim.Position;

                shotDirection = dir.normalized;
            }
            else if (shotDirection.y < (FixedFloat)(-0.1f))
            {
                FixedVector2 dir =
                    playerSim.GetBottomTargetPosition()
                    - playerSim.Position;

                shotDirection = dir.normalized;
            }
            else
            {
                shotDirection =
                    shotDirection.normalized;
            }
        }
        else
        {
            shotDirection =
                shotDirection.normalized;
        }

        PlayerStateData data =
            PlayerStateData.Build()
                .SetShotPower(shotPower)
                .SetShotDirection(shotDirection)
                .SetIsInstant(false);

        playerSim.SwitchState(
            PlayerState.SHOOTING,
            data);
    }

    public override bool CouldHurt() {
        if (_ballSim.carrier.playerId == playerSim.playerId) {
            return true;
        }
        return false;
    }
}


