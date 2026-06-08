using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BallSimState
{
    protected BallSim ballSim=null;
    public PlayerView carrier=null;
    protected BallStateData stateData = new BallStateData();
    public const float BOUNCINESS = 0.8f;
    const float idleThreshold = 0.01f;
    public SimEventBus _eventBus;

    public void Setup(BallSim contextBallSim,BallStateData ContextBallStateData,SimEventBus eventBus){
        ballSim = contextBallSim;
        stateData = ContextBallStateData;
        _eventBus=eventBus;
    }
    public virtual void _Update(float deltaTime) {
    }
    public virtual void _FixedUpdate() {
        
    }
    public virtual void OnExit() {
    }
    public virtual void OnEnter() {}
    public virtual bool CanCarriedBall() {
        return false;
    }
    public virtual bool CanAirInteract() {return false;}
    protected void MoveHorizontal(float deltaTime)
    {
        Vector2 velocity = ballSim.Velocity;
        Vector2 position = ballSim.Position;

        float friction = ballSim.height > 0
            ? ballSim.frictionAir
            : ballSim.frictionGround;

        velocity = Vector2.MoveTowards(
            velocity,
            Vector2.zero,
            friction * deltaTime
        );
        if (velocity.sqrMagnitude < 0.0001f)
        {
            ballSim.Velocity = Vector2.zero;
            return;
        }

        Vector2 move = velocity * deltaTime;
        
        if (move.sqrMagnitude < 0.000001f)
        {
            ballSim.Velocity = velocity;
            return;
        }

        position += move;

        ballSim.Velocity = velocity;
        ballSim.Position = position;
    }
    public void MoveVertical(float deltaTime,float bounciness = 0.0f)
    {   
        float height = ballSim.height;
        float heightVelocity = ballSim.heightVelocity;
        Vector2 velocity = ballSim.Velocity;

        if (height > 0 || heightVelocity > 0)
        {
            float dt = deltaTime;

            heightVelocity -= BallSim.GRAVITY * dt;
            height += heightVelocity * dt;

            if (height < 0)
            {
                height = 0;

                if (heightVelocity < 0)
                {
                    heightVelocity = -heightVelocity * bounciness;
                    velocity *= bounciness;
                }
            }
        }

        ballSim.height = height;
        ballSim.heightVelocity = heightVelocity;
        ballSim.Velocity = velocity;
    }
    // public void MoveVertical(float deltaTime, float bounciness = 0.0f)
    // {
    //     if (ballSim.height > 0 || ballSim.heightVelocity > 0)
    //     {
    //         ballSim.heightVelocity -= GRAVITY * deltaTime;
    //
    //         ballSim.height += ballSim.heightVelocity * deltaTime;
    //
    //         if (ballSim.height < 0)
    //         {
    //             ballSim.height = 0;
    //
    //             if (ballSim.heightVelocity < 0)
    //             {
    //                 ballSim.heightVelocity = -ballSim.heightVelocity * bounciness;
    //                 ballSim.Velocity *= bounciness;
    //             }
    //         }
    //     }
    // }
}
