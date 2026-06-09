using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSim {
    public int playerId;
    public PlayerSimStateFactory stateFactory=new PlayerSimStateFactory();
    private AIBehaviorFactory aiBehaviorFactory=new AIBehaviorFactory();
    public AIBehavior aiBehavior;
    public ControlScheme controlScheme;
    public Vector2 spawnPosition;
    public Vector2 kickoffPosition;
    public Vector2 Position;
    public Vector2 Velocity;
    public float Height;
    public float HeightVelocity;
    public bool HeadingRight=true;
    public PlayerSimState currentState;
    public PlayerState playerState;
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
    private const float BALL_CONTROL_HEIGHT_MAX = 10f;
    [HideInInspector]public float GRAVITY = 160f;
    public int Frame { get;private set;}
    public BallSim _ballSim;
    public List<Vector2> targetGoalPosition;
    

    public PlayerSim(int nextPlayerId,PlayerResource contextPlayerData, Vector2 contextplayerPosition, Vector2 contextkickoffPosition, string contextcountry, bool contextisHome,bool ContextInitialFacingRight) {
        playerId = nextPlayerId;
        spawnPosition= contextplayerPosition;
        kickoffPosition= contextkickoffPosition;
        country= contextcountry;
        isHome = contextisHome;
        Speed = contextPlayerData.speed;
        Power = contextPlayerData.power;
        role = contextPlayerData.role;
        fullName = contextPlayerData.fullName;
        controlScheme = ControlScheme.CPU;
        Position = isHome ? kickoffPosition : spawnPosition;
        initialFacingRight= ContextInitialFacingRight;
        HeadingRight=initialFacingRight;
        SwitchState(PlayerState.RESETING, PlayerStateData.Build().SetResetPosition(Position));
    }
    private void SetupAIBehavior(Rect GoalArea,int matchPlayerCount) {
        aiBehavior = aiBehaviorFactory.GetFreshAIBehavior(role);
        aiBehavior.Setup(this, _ballSim,GoalArea,matchPlayerCount);
    }

    
    public void Tick(int frame,float deltaTime,int homeCount,int awayCount)
    {
        aiBehavior.aiTimer += deltaTime;
        aiBehavior.homeCount= homeCount;
        aiBehavior.awayCount= homeCount;
        
        Frame=frame;
        currentState?._Update(deltaTime);
        ApplyHeight(deltaTime);
    }

    private void ApplyHeight(float deltaTime) {
        if (Height > 0f) {
            HeightVelocity -= GRAVITY*deltaTime;
            Height += HeightVelocity*deltaTime;
            if (Height < 0)
            {
                Height = 0;
                HeightVelocity = 0;
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
        ,List<Vector2> TargetGoalPosition,Rect GoalArea,int playerCount) {
        _eventBus=eventBus;
        _commandBuffer = commandBuffer;
        _ballSim=ballSim;
        targetGoalPosition = TargetGoalPosition;

        SetupAIBehavior(GoalArea,playerCount);
    }

    public bool CanCarryBall() {
        return currentState.CanCarryBall();
    }

    public void SetHeadingRight(Vector2 moveDir) {
        if (moveDir.x > 0) {
            HeadingRight = true;
        }else if (moveDir.x < 0) {
            HeadingRight = false;
        }
    }
    public Vector2 GetFarTargetPosition()
    {
        float farDistance=float.MinValue;
        int farIndex=0;
        for(int i=0;i<targetGoalPosition.Count;i++) {
            float dist = (Position - targetGoalPosition[i]).sqrMagnitude;
            if (dist >= farDistance) {
                farDistance = dist;
                farIndex = i;
            }
        }
        return targetGoalPosition[farIndex];
    }

    public Vector2 GetTopTargetPosition() {
        return targetGoalPosition[0];
    }

    public Vector2 GetBottomTargetPosition() {
        return targetGoalPosition[^1];
    }
    public Vector2 GetCenterTargetPosition() {
        return targetGoalPosition[1];
    }

    public bool HasBall() {
        return _ballSim.carrier!=null&&_ballSim.carrier.playerId == playerId;
    }



}
