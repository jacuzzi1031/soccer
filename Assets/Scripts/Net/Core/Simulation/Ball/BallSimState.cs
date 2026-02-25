using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BallSimState
{
    public const float GRAVITY = 8f;
    protected BallSim ballSim=null;
    public PlayerView carrier=null;
    protected BallStateData stateData = new BallStateData();
    public event Action<BallState, BallStateData> StateTransitionRequested;
    public const float BOUNCINESS = 0.8f;
    const float idleThreshold = 0.01f;
    public SimEventBus _eventBus;

    public virtual void Setup(BallSim contextBallSim,BallStateData ContextBallStateData,SimEventBus eventBus){
        ballSim = contextBallSim;
        stateData = ContextBallStateData;
        _eventBus=eventBus;
    }
    public void TransitionState(BallState newBallState, BallStateData data = null)
    {
        StateTransitionRequested?.Invoke(newBallState, data ?? BallStateData.Build());
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
        float friction = ballSim.height > 0 
            ? ballSim.frictionAir 
            : ballSim.frictionGround;

        ballSim.Velocity = Vector2.MoveTowards(
            ballSim.Velocity,
            Vector2.zero,
            friction * deltaTime
        );
        Vector2 move = ballSim.Velocity * deltaTime;

        if (move.sqrMagnitude < 0.000001f)
            return;

        Vector2 nextPos = ballSim.Position + move;

        ResolveLineCollisions(ref nextPos, ref ballSim.Velocity);

        ballSim.Position = nextPos;
    }
    private void ResolveLineCollisions(ref Vector2 position, ref Vector2 velocity)
    {
        float radius = ballSim.radius;

        foreach (var line in ballSim.lines)
        {
            Vector2 closest = ClosestPointOnSegment(position, line);

            Vector2 diff = position - closest;
            float sqrDist = diff.sqrMagnitude;

            if (sqrDist < radius * radius)
            {
                float dist = Mathf.Sqrt(sqrDist);
                Vector2 normal = dist > 0.00001f 
                    ? diff / dist
                    : (position - line.Start).normalized;
                
                position = closest + normal * radius;

                velocity = Vector2.Reflect(velocity, normal) * BOUNCINESS;
            }
        }
    }
    private Vector2 ClosestPointOnSegment(Vector2 point, LineSegment line)
    {
        Vector2 ab = line.End - line.Start;

        float abSqr = Vector2.Dot(ab, ab);

        if (abSqr < 0.000001f)
            return line.Start;

        float t = Vector2.Dot(point - line.Start, ab) / abSqr;

        t = Mathf.Clamp01(t);

        return line.Start + ab * t;
    }
}
