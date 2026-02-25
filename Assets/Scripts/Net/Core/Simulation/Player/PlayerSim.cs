using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSim {
    public int playerId;
    public PlayerSimStateFactory stateFactory=new PlayerSimStateFactory();
    public ControlScheme controlScheme;
    public bool facingRight=true;
    public Vector2 teamResetPosition;
    public Vector2 kickoffPosition;
    public Vector2 Position;
    public Vector2 Velocity;
    public float Height;
    public bool HeadingRight;
    public PlayerSimState CurrentSimState;
    public PlayerState CurrentState;
    public bool isHome;
    public string fullName;
    public string country;
    public float Power;
    public float Speed;
    public Role role = Role.MIDFIELD;
    public float weightOnDutySteering;
    public bool initialFacingRight;
    public SimEventBus _eventBus;
    public CommandBuffer _commandBuffer;
    public List<LineSegment> lines;
    public float radius=6.5f;
    public int Frame { get;private set;}

    

    public PlayerSim(int nextPlayerId,PlayerResource contextPlayerData, Vector2 contextplayerPosition, Vector2 contextkickoffPosition, string contextcountry, bool contextisHome,bool ContextInitialFacingRight) {
        playerId = nextPlayerId;
        teamResetPosition= contextplayerPosition;
        kickoffPosition= contextkickoffPosition;
        country= contextcountry;
        isHome = contextisHome;
        Speed = contextPlayerData.speed;
        Power = contextPlayerData.power;
        role = contextPlayerData.role;
        fullName = contextPlayerData.fullName;
        controlScheme = ControlScheme.CPU;
        Position = isHome ? kickoffPosition : teamResetPosition;
        initialFacingRight= ContextInitialFacingRight;
        SwitchState(PlayerState.RESETING, PlayerStateData.Build().SetResetPosition(Position));
    }

    public void Tick(int frame,float deltaTime)
    {
        Frame=frame;
        CurrentSimState?._Update(deltaTime);
    }

    public void OnTakeTackleHit(Vector2 dir)
    {
        // if (!HasBall()) return; 只有carrierSnapshot
        SwitchState(PlayerState.HURT, PlayerStateData.Build().SetMoveDir(dir));
    }
    public void SwitchState(PlayerState id, PlayerStateData data = null) {
        if (CurrentState == id) return;
        CurrentState = id;
        CurrentSimState = stateFactory.GetFreshState(id);
        CurrentSimState.Setup(this, data ?? PlayerStateData.Build(),_eventBus,_commandBuffer);

        CurrentSimState.OnEnter();
    }
    public bool IsReadyForKickoff() {
        return CurrentSimState != null && CurrentSimState.IsReadyForKickoff();
    }

    public void SetControlScheme(ControlScheme ContextControlScheme) {
        controlScheme = ContextControlScheme;
    }
    public bool IsFacingTargetGoal() {

         return (facingRight && initialFacingRight) || (!initialFacingRight && !facingRight);
    }

    public void OnTeamReset(bool isKickoff) {
        SwitchState(PlayerState.RESETING,PlayerStateData.Build().SetResetPosition(isKickoff?kickoffPosition:teamResetPosition));
    }

    public void SetEventBusAndCommandBuffer(SimEventBus eventBus,CommandBuffer commandBuffer,List<LineSegment>lineSegments) {
        _eventBus=eventBus;
        _commandBuffer = commandBuffer;
        lines = lineSegments;
    }
}
