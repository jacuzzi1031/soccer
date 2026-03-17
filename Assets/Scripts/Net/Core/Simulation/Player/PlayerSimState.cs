using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSimState 
{   
    protected PlayerSim playerSim;
    private PlayerStateData _stateData;
    public PlayerStateData stateData => _stateData;
    protected Vector2 _moveDirection;
    protected SimEventBus _eventBus;
    protected CommandBuffer _commandBuffer;
    protected BallSim _ballSim;
    public const float BONUS_POWER =1.8f;
    protected Vector2 Direction;

    public void Setup(
        PlayerSim contextPlayerView,
        PlayerStateData contextData,
        SimEventBus eventBus,
        CommandBuffer commandBuffer,BallSim ballSim
    )
    {
        playerSim = contextPlayerView;
        _stateData = contextData;
        _eventBus = eventBus;
        _commandBuffer=commandBuffer;
        _ballSim=ballSim;
    }
    public virtual void _Update(float deltaTime) {
    }

    public void SetMoveDirection(Vector2 moveDirection) {
        _moveDirection = moveDirection;
    }


    public virtual void OnEnter() {
        
    }
    public virtual void OnExit() {
        
    }
    public virtual void OnShootPress(bool isReleased,bool hasBall,bool BallCanAirInteract) {
    }
    public virtual void OnShootRelease(bool hasBall,bool ballCanAirInteract){}
    public virtual void OnPass(Vector2 Direction,int passType=0,PlayerSim passTarget=null) {
        
    }
    public virtual bool CanCarryBall()=> false;

    public virtual bool IsReadyForKickoff() => false;

    public virtual bool VolleyShot() => false;

    public virtual void OnTeamReset(bool isHomeKickoff) {
    }
    public virtual bool IsDamageEmitter()=> false;

    public virtual bool CouldHurt()=> false;
    protected void MoveHorizontal(float deltaTime,float FRICTION) {
        Vector2 velocity = playerSim.Velocity;
        Vector2 position = playerSim.Position;
        
        velocity = Vector2.MoveTowards(
            velocity,
            Vector2.zero,
            FRICTION * deltaTime
        );
        if (velocity.sqrMagnitude < 0.0001f)
        {
            playerSim.Velocity = Vector2.zero;
            return;
        }

        Vector2 move = velocity * deltaTime;

        position += move;

        playerSim.Velocity = velocity;
        playerSim.Position = position;
    }

    public virtual void OnTackle(Vector2 direction) {
    }
}
