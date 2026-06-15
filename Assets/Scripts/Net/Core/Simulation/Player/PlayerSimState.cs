using System;
using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerSimState 
{   
    protected PlayerSim playerSim;
    private PlayerStateData _stateData;
    public PlayerStateData stateData => _stateData;
    protected FixedVector2 _moveDirection;
    protected SimEventBus _eventBus;
    protected CommandBuffer _commandBuffer;
    protected BallSim _ballSim;
    public static FixedFloat BONUS_POWER =(FixedFloat)1.4f;
    protected FixedVector2 Direction;
    protected static FixedFloat AIR_FRICTION = (FixedFloat)25f;
    public FixedFloat diveDirY;
    private static readonly FixedFloat STOP_THRESHOLD = (FixedFloat)0.0001f;
    public int HashRandom(int seed)
    {
        uint x = (uint)seed;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;

        return (int)(x & 0x7fffffff);
    }
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
    public virtual void _Update() {
    }

    public void SetMoveDirection(FixedVector2 moveDirection) {
        _moveDirection = moveDirection;
    }


    public virtual void OnEnter() {
        
    }
    public virtual void OnExit() {
        
    }
    public virtual void OnShootPress(bool isReleased,bool hasBall,bool BallCanAirInteract) {
    }
    public virtual void OnShootRelease(bool hasBall,bool ballCanAirInteract){}
    public virtual void OnPass(FixedVector2 Direction,int passType=0,PlayerSim passTarget=null) {
        
    }
    public virtual bool CanCarryBall()=> false;

    public virtual bool IsReadyForKickoff() => false;

    public virtual bool VolleyShot() => false;

    public virtual void OnTeamReset(bool isHomeKickoff) {
    }
    public virtual bool IsDamageEmitter()=> false;

    public virtual bool CouldHurt()=> false;
    protected void MoveHorizontal(FixedFloat FRICTION) {
        FixedVector2 velocity = playerSim.Velocity;
        FixedVector2 position = playerSim.Position;
        
        velocity = FixedVector2.MoveTowards(
            velocity,
            FixedVector2.Zero,
            FRICTION * SimulationConfig.DeltaTime
        );
        if (velocity.sqrMagnitude < STOP_THRESHOLD)
        {
            playerSim.Velocity = FixedVector2.Zero;
            return;
        }

        FixedVector2 move = velocity * SimulationConfig.DeltaTime;

        position += move;

        playerSim.Velocity = velocity;
        playerSim.Position = position;
    }

    public virtual void OnTackle(FixedVector2 direction) {
    }
}
