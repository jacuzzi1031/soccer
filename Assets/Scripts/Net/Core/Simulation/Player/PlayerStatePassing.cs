using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStatePassing: PlayerSimState
{
   private const int PASS_ANIMATION_FRAMES = 3;
    private const int RECOVER_FRAMES = 18;

    private int _elapsedFrames;
    private bool _passTriggered;

    private FixedVector2 passDestination;
    private bool overground;

    public override void OnEnter()
    {
        _elapsedFrames = 0;
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

            switch (stateData.InputType)
            {
                case 0:
                    passDestination =
                        passTargetPosition +
                        passTargetVelocity * (FixedFloat)0.6f;

                    overground = true;

                    _ballSim.passTo(passDestination, true);
                    _passTriggered = true;
                    break;

                case 1:
                    passDestination =
                        passTargetPosition +
                        passTargetVelocity * (FixedFloat)0.6f;

                    overground = false;

                    _ballSim.passTo(passDestination, false);
                    _passTriggered = true;
                    break;

                case 2:
                    passDestination =
                        passTargetPosition +
                        passTargetVelocity * (FixedFloat)1.8f;

                    overground = true;
                    break;
            }
        }
    }

    public override void _Update()
    {
        _elapsedFrames++;

        if (!_passTriggered &&
            _elapsedFrames >= PASS_ANIMATION_FRAMES)
        {
            _ballSim.passTo(passDestination, overground);
            _passTriggered = true;
        }

        if (_elapsedFrames >= RECOVER_FRAMES)
        {
            playerSim.SwitchState(PlayerState.MOVING);
        }
    }
    
}
