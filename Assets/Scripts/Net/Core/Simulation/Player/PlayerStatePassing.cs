using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatePassing: PlayerSimState
{
    private float _elapsedTicks;
    private float _durationTicks=0.03f;
    
    private Vector2 passDestination;
    private bool overground;
    public override void OnEnter() {
        _elapsedTicks = 0f;
        
        Vector2 headingOffset = playerSim.HeadingRight ? Vector2.right * playerSim.Speed : Vector2.left * playerSim.Speed;
        
        if (stateData.passTarget == null)
        {
            overground = true;
            passDestination = (Vector2)_ballSim.Position + headingOffset;
        }
        else {
            var passTargetPosition = stateData.passTarget.Position;
            var passTargetVelocity = stateData.passTarget.Velocity;
            switch (stateData.InputType)
            {
                case 0:
                    passDestination = passTargetPosition + passTargetVelocity * 0.8f;
                    overground = true;
                    _ballSim.passTo(passDestination,true);
                    break;
                    
                case 1:
                    passDestination = passTargetPosition + passTargetVelocity * 0.8f;
                    overground = false;
                    _ballSim.passTo(passDestination,false);
                    break;
                    
                case 2:
                    passDestination = passTargetPosition + passTargetVelocity * 1.8f;
                    overground = true;

                    break;
            }
        }
    }

    public override void _Update(float deltaTime) {
        _elapsedTicks+=deltaTime;

        if (_elapsedTicks >= _durationTicks)
        {
            OnAnimationComplete();
        }
    }

    private void OnAnimationComplete() {
        _ballSim.passTo(passDestination,overground);
        playerSim.SwitchState(PlayerState.MOVING);
    }
}
