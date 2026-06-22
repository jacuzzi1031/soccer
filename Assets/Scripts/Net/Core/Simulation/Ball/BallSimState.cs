using System;
using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using Unity.VisualScripting;
using UnityEngine;

public class BallSimState
{
    protected BallSim ballSim=null;
    public PlayerView carrier=null;
    protected BallStateData stateData = new BallStateData();
    public static  FixedFloat BOUNCINESS = (FixedFloat)0.8f;
    protected int stateFrame;
    public void Setup(BallSim contextBallSim,BallStateData ContextBallStateData,SimEventBus eventBus){
        ballSim = contextBallSim;
        stateData = ContextBallStateData;
    }
    public virtual void _Update() {
    }
    public virtual void OnExit() {
    }
    public virtual void OnEnter() {}
    public virtual bool CanCarriedBall() {
        return false;
    }
    public virtual bool CanAirInteract() {return false;}
    protected void MoveHorizontal()
    {
        FixedFloat dt = SimulationConfig.DeltaTime;

        FixedVector2 velocity = FixedVector2.MoveTowards(
            ballSim.Velocity,
            FixedVector2.Zero,
            (ballSim.Height > 0 ? ballSim.frictionAir : ballSim.frictionGround) * dt
        );

        if (velocity == FixedVector2.Zero)
        {
            ballSim.Velocity = FixedVector2.Zero;
            return;
        }

        ballSim.Velocity = velocity;
        ballSim.Position += velocity * dt;
    }

    public void MoveVertical() {
        MoveVertical(FixedFloat.Zero);
    }
    public void MoveVertical(FixedFloat bounciness)
    {   
        FixedFloat height = ballSim.Height;
        FixedFloat heightVelocity = ballSim.HeightVelocity;
        FixedVector2 velocity = ballSim.Velocity;

        if (height > FixedFloat.Zero || heightVelocity > FixedFloat.Zero)
        {
            FixedFloat dt = SimulationConfig.DeltaTime;

            heightVelocity -= BallSim.GRAVITY * dt;
            height += heightVelocity * dt;

            if (height < FixedFloat.Zero)
            {
                height = FixedFloat.Zero;

                if (heightVelocity < FixedFloat.Zero)
                {
                    heightVelocity = -heightVelocity * bounciness;
                    velocity *= bounciness;
                }
            }
        }

        ballSim.Height = height;
        ballSim.HeightVelocity = heightVelocity;
        ballSim.Velocity = velocity;
    }
}
