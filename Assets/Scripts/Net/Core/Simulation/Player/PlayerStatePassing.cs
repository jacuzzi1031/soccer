using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStatePassing: PlayerSimState
{
   private const int PASS_ANIMATION_FRAMES = 3;
    private const int RECOVER_FRAMES = 18;


    private bool _passTriggered;

    private FixedVector2 passDestination;
    private bool overground;

    public override void OnEnter()
    {
        stateFrame = 0;
        _passTriggered = false;

        FixedVector2 headingOffset =
            playerSim.HeadingRight
                ? FixedVector2.Right * playerSim.Speed
                : FixedVector2.Left * playerSim.Speed;

        if (stateData.passTarget == null)
        {
            overground = true;
            passDestination = _ballSim.Position + headingOffset;
        }
        else
        {
            FixedVector2 passTargetPosition =
                stateData.passTarget.Position;

            FixedVector2 passTargetVelocity =
                stateData.passTarget.Velocity;

            FixedFloat predictTime;
            bool? overground = null;
            PlayerState playStyle;

            switch (stateData.InputType)
            {
                case 0:
                    predictTime = (FixedFloat)0.2f;
                    overground = true;
                    playStyle = PlayerState.SHORTPASS;
                    break;

                case 1:
                    predictTime = (FixedFloat)0.2f;
                    overground = false;
                    playStyle = PlayerState.LONGPASS;
                    break;

                case 2:
                    predictTime = (FixedFloat)1.8f;
                    overground = true;
                    playStyle = PlayerState.INCISIVEPASS;
                    break;

                default:
                    return;
            }

            passDestination = passTargetPosition + passTargetVelocity * predictTime;

            if (overground.HasValue)
            {
                _ballSim.passTo(passDestination, overground.Value);
                _passTriggered = true;
            }

            _eventBus.Publish(
                new PlayStyleShowSignal(playerSim.playerId, playStyle)
            );
        }
    }

    public override void _Update()
    {
        stateFrame++;

        if (!_passTriggered &&
            stateFrame >= PASS_ANIMATION_FRAMES)
        {
            _ballSim.passTo(passDestination, overground);
            _passTriggered = true;
        }

        if (stateFrame >= RECOVER_FRAMES)
        {
            playerSim.SwitchState(PlayerState.MOVING);
        }
    }
    
}
