using System;
using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSim {
    public int playerId;
    public PlayerSimStateFactory stateFactory=new PlayerSimStateFactory();
    private AIBehaviorFactory aiBehaviorFactory=new AIBehaviorFactory();
    public AIBehavior aiBehavior;
    public ControlScheme controlScheme;
    public FixedVector2 spawnPosition;
    public FixedVector2 kickoffPosition;
    public FixedVector2 Position;
    public FixedVector2 Velocity;
    public FixedFloat Height;
    public FixedFloat HeightVelocity;
    public bool HeadingRight=true;
    public PlayerSimState currentState;
    public PlayerState playerState;
    public bool isHome;
    public string fullName;
    public string country;
    public FixedFloat Power;
    public FixedFloat Speed;
    public Role role = Role.MIDFIELD;
    public FixedFloat weightOnDutySteering;
    public bool initialFacingRight;
    public SimEventBus _eventBus;
    public CommandBuffer _commandBuffer;
    private const float BALL_CONTROL_HEIGHT_MAX = 10f;
    [HideInInspector]public FixedFloat GRAVITY = (FixedFloat)160f;
    public int Frame { get;private set;}
    public BallSim _ballSim;
    public List<FixedVector2> targetGoalPosition;
    

    public PlayerSim(int nextPlayerId,PlayerResource contextPlayerData, FixedVector2 contextplayerPosition, FixedVector2 contextkickoffPosition, string contextcountry, bool contextisHome,bool ContextInitialFacingRight) {
        playerId = nextPlayerId;
        spawnPosition= contextplayerPosition;
        kickoffPosition= contextkickoffPosition;
        country= contextcountry;
        isHome = contextisHome;
        Speed = (FixedFloat)contextPlayerData.speed;
        Power = (FixedFloat)contextPlayerData.power;
        role = contextPlayerData.role;
        fullName = contextPlayerData.fullName;
        controlScheme = ControlScheme.CPU;
        Position = isHome ? kickoffPosition : spawnPosition;
        initialFacingRight= ContextInitialFacingRight;
        HeadingRight=initialFacingRight;
        SwitchState(PlayerState.RESETING, PlayerStateData.Build().SetResetPosition(Position));
    }
    private void SetupAIBehavior(FixedRect GoalArea,int matchPlayerCount) {
        aiBehavior = aiBehaviorFactory.GetFreshAIBehavior(role);
        aiBehavior.Setup(this, _ballSim,GoalArea,matchPlayerCount);
    }

    
    public void Tick(int frame,int homeCount,int awayCount)
    {
        aiBehavior.homeCount= homeCount;
        aiBehavior.awayCount= awayCount;
        
        Frame=frame;
        currentState?._Update();
        ApplyHeight();
    }

    private void ApplyHeight() {
        if (Height > FixedFloat.Zero) {
            HeightVelocity -= GRAVITY*SimulationConfig.DeltaTime;
            Height += HeightVelocity*SimulationConfig.DeltaTime;
            if (Height < FixedFloat.Zero)
            {
                Height = FixedFloat.Zero;
                HeightVelocity = FixedFloat.Zero;
            }
        }
    }

    public void SwitchState(PlayerState id, PlayerStateData data = null) {
        if (playerState == id) return;
        if (currentState != null) {
            currentState.OnExit();
        }
        playerState = id;
        currentState = stateFactory.GetFreshState(id);
        currentState.Setup(this, data ?? PlayerStateData.Build(),_eventBus,_commandBuffer,_ballSim);
        currentState.OnEnter();
    }
    public bool IsReadyForKickoff() {
        return currentState != null && currentState.IsReadyForKickoff();
    }

    public void SetControlScheme(ControlScheme ContextControlScheme) {
        controlScheme = ContextControlScheme;
    }
    public bool IsFacingTargetGoal() {

         return (HeadingRight && initialFacingRight) || (!initialFacingRight && !HeadingRight);
    }
    public void FaceTowardsBall() {
        if (!IsFacingTargetGoal()) {
            HeadingRight = !HeadingRight;
        }
    }

    public void OnTeamReset(bool isKickoff) {
        SwitchState(PlayerState.RESETING,PlayerStateData.Build().SetResetPosition(isKickoff?kickoffPosition:spawnPosition));
    }

    public void SetEventBusAndCommandBuffer(SimEventBus eventBus,CommandBuffer commandBuffer,BallSim ballSim
        ,List<FixedVector2> TargetGoalPosition,FixedRect GoalArea,int playerCount) {
        _eventBus=eventBus;
        _commandBuffer = commandBuffer;
        _ballSim=ballSim;
        targetGoalPosition = TargetGoalPosition;

        SetupAIBehavior(GoalArea,playerCount);
    }

    public bool CanCarryBall() {
        return currentState.CanCarryBall();
    }

    public void SetHeadingRight(FixedVector2 moveDir) {
        if (moveDir.x > 0) {
            HeadingRight = true;
        }else if (moveDir.x < 0) {
            HeadingRight = false;
        }
    }
    public FixedVector2 GetFarTargetPosition()
    {
        FixedFloat farDistance=FixedFloat.MinValue;
        int farIndex=0;
        for(int i=0;i<targetGoalPosition.Count;i++) {
            FixedFloat dist = (Position - targetGoalPosition[i]).sqrMagnitude;
            if (dist >= farDistance) {
                farDistance = dist;
                farIndex = i;
            }
        }
        return targetGoalPosition[farIndex];
    }

    public FixedVector2 GetTopTargetPosition() {
        return targetGoalPosition[0];
    }

    public FixedVector2 GetBottomTargetPosition() {
        return targetGoalPosition[^1];
    }
    public FixedVector2 GetCenterTargetPosition() {
        return targetGoalPosition[1];
    }

    public bool HasBall() {
        return _ballSim.carrier!=null&&_ballSim.carrier.playerId == playerId;
    }



}
