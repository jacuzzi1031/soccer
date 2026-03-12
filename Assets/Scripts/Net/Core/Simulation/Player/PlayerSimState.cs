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
    public virtual void OnPass(int passType,PlayerSim passTarget) {
        
    }
    public virtual bool CanCarryBall()=> false;

    public virtual bool IsReadyForKickoff() => false;

    public virtual void VolleyShot() {
    }

    public virtual void OnTeamReset(bool isHomeKickoff) {
    }
}
