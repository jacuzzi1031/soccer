using System;
using UnityEngine;

public class PlayerState
{
    public event Action<Player.State, PlayerStateData> StateTransitionRequested;

    protected Rigidbody2D rb;
    protected AIBehavior aiBehavior;
    protected Animator animator; 
    protected Ball ball;
    protected TriggerDetection ballDetectionArea;
    protected Goal ownGoal;
    protected Player player;
    protected PlayerStateData stateData = new PlayerStateData();
    protected Goal targetGoal;
    // protected TriggerDetection tackleDamageEmitterArea;
    protected TriggerDetection teammateDetectionArea;
    protected ParticleSystem runParticles;

    
    public void Setup(
        Player contextPlayer,
        PlayerStateData contextData,
        Rigidbody2D contextRb,
        Animator contextAnimationPlayer,
        Ball contextBall,
        ParticleSystem contextParticles,
        // TriggerDetection contextTeammateDetectionArea,
        // TriggerDetection contextBallDetectionArea,
        // Goal contextOwnGoal,
        // Goal contextTargetGoal,
        // TriggerDetection contextTackleDamageEmitterArea,
        AIBehavior contextAiBehavior
    )
    {
        player = contextPlayer;
        rb = contextRb;
        animator = contextAnimationPlayer;
        stateData = contextData;
        ball = contextBall;
        runParticles=contextParticles;
        // teammateDetectionArea = contextTeammateDetectionArea;
        // ballDetectionArea = contextBallDetectionArea;
        // ownGoal = contextOwnGoal;
        // targetGoal = contextTargetGoal;
        aiBehavior = contextAiBehavior;
        // tackleDamageEmitterArea = contextTackleDamageEmitterArea;

    }
    
    public void TransitionState(Player.State newState, PlayerStateData data = null)
    {
        StateTransitionRequested?.Invoke(newState, data ?? PlayerStateData.Build());
    }

    public virtual void _Update() {
        
    }
    public virtual void _FixedUpdate() {
        
    }

    public virtual void OnEnter() {
        
    }
    public virtual void OnExit() {
        
    }

    public virtual void OnShoot() {
    }

    public virtual void OnPass(GameInput.PlayerInputType passType) {
        
    }
    
    public virtual void OnAnimationComplete() { }

    public virtual bool CanCarryBall() => false;
    public virtual bool CanPass() => false;
    public virtual bool IsReadyForKickoff() => false;
}
