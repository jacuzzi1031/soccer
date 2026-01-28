using System;
using GameFrameSync;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerViewState
{
    public event Action<PlayerView.State, PlayerStateData> StateTransitionRequested;

    protected Rigidbody2D rb;
    protected AIBehavior aiBehavior;
    protected Animator animator; 
    protected BallView BallView;
    protected TriggerDetection ballDetectionArea;
    protected Goal ownGoal;
    protected PlayerView PlayerView;
    protected PlayerStateData stateData = new PlayerStateData();
    protected Goal targetGoal;
    protected Collider2D tackleDamageEmitterArea;
    protected TriggerDetection teammateDetectionArea;
    protected ParticleSystem runParticles;
    public Sprite playStyleSprite;
    protected static readonly int SpeedHash =
        Animator.StringToHash("Speed");
    private float runThreshold = 45f;
    
    public void Setup(
        PlayerView contextPlayerView,
        PlayerStateData contextData,
        Rigidbody2D contextRb,
        Animator contextAnimationPlayer,
        BallView contextBallView,
        ParticleSystem contextParticles,
        Goal contextOwnGoal,
        Goal contextTargetGoal,
        AIBehavior contextAiBehavior,
        Collider2D contextTackleDamageEmitterArea
    )
    {
        PlayerView = contextPlayerView;
        rb = contextRb;
        animator = contextAnimationPlayer;
        stateData = contextData;
        BallView = contextBallView;
        runParticles=contextParticles;
        ownGoal = contextOwnGoal;
        targetGoal = contextTargetGoal;
        aiBehavior = contextAiBehavior;
        tackleDamageEmitterArea = contextTackleDamageEmitterArea;
    }
    
    public void TransitionState(PlayerView.State newState, PlayerStateData data = null)
    {
        StateTransitionRequested?.Invoke(newState, data ?? PlayerStateData.Build());
    }
    public void SetMovementAnimation() {
        animator.SetFloat(SpeedHash, rb.velocity.magnitude);
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        if (!info.IsName("movement"))
        {
            if (runParticles.isPlaying)
                runParticles.Stop();
            return;
        }
        float speed = animator.GetFloat(SpeedHash);
        if (speed >= runThreshold)
        {
            if (!runParticles.isPlaying)
                runParticles.Play();
        }
        else
        {
            if (runParticles.isPlaying)
                runParticles.Stop();
        }
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

    public virtual void OnPass(InputType passType) {
        
    }
    
    public virtual void OnAnimationComplete() { }

    public virtual bool CanCarryBall() => false;
    public virtual bool CanPass() => false;
    public virtual bool IsReadyForKickoff() => false;

    public virtual bool CanVolleyKickOrHeader() {
        return false;
    }

    public virtual void VolleyShot(BallView body) {
        
    }

    public virtual void OnShootCancel() {
    }
}
