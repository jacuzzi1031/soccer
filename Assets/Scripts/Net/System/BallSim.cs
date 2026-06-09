using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSim:ISimulationSystem
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float height=0.0f;
    public float heightVelocity=0.0f;
    private const float KICKOFF_PASS_DISTANCE = 30f;
    private const float DURATION_PASS_LOCK = 0.1f;
    private const float DURATION_KICKOFF_LOCK = 0.6f;
    public const float GRAVITY = 20f;
    public Vector2 spawnPosition;
    public BallSimState currentState;
    public BallSimStateFactory stateFactory=new BallSimStateFactory();
    private bool _running;
    public SimEventBus _eventBus;
    public BallState ballState;
    public CommandBuffer _commandBuffer;
    public int Frame { get;private set;}
    [HideInInspector]public float frictionAir = 10f;
    [HideInInspector]public float frictionGround = 75;
    [HideInInspector]public PlayerSim carrier;
    public const int INVALID_PLAYER_ID = -1;
    public int BallCarrierId => carrier?.playerId ?? INVALID_PLAYER_ID;
    public float radius=4.68f;
    public bool firstPlayerCarryBall;
    public BallSim(Vector2 ContextSpawnPosition,SimEventBus eventBus,CommandBuffer commandBuffer)
    {
        spawnPosition = ContextSpawnPosition;
        _eventBus = eventBus;
        Position=spawnPosition;
        _commandBuffer = commandBuffer;
        SwitchState(BallState.RESET);
    }
    public void SwitchState(BallState type, BallStateData data = null) {

        currentState?.OnExit();
        ballState = type;
        currentState = stateFactory.GetFreshState(type);
        currentState.Setup(this, data ?? new BallStateData(),_eventBus);
        currentState.OnEnter();
    }

    public void Tick(SimulationContext context) {
        Frame=context.Frame;
        foreach (var command in context.Commands)
        {
            switch (command.Type)
            {
                case SimulationCommandType.KickoffStart:
                    kickoffPass(spawnPosition + Vector2.down * KICKOFF_PASS_DISTANCE);
                    break;
                case SimulationCommandType.ResetAndHomeKickoff:
                case SimulationCommandType.ResetAndAwayKickoff:
                    firstPlayerCarryBall = false;
                    Position = spawnPosition;
                    Velocity = Vector2.zero;
                    height=0.0f;
                    _eventBus.Publish(new BallBacktoSpawnPositionSignal());
                    SwitchState(BallState.RESET);
                    break;
            }
        }
        currentState?._Update(context.DeltaTime);
        
    }

    public void shoot(Vector2 ShotVelocity) {
        Velocity = ShotVelocity;
        carrier = null;
        SwitchState(BallState.SHOT);
    }

    public void passTo(Vector2 destination,bool overground, float lockDuration = DURATION_PASS_LOCK) {
        Vector2 direction = (destination - Position).normalized;
        float distance = Vector2.Distance(Position, destination);
        float intensity = Mathf.Sqrt(2f * distance * frictionGround);
        Velocity = intensity * direction;
        // 如果是高空的，视为水平是匀速，速度为原本速度intensity  x=Vx*t  t=x/Vx
        // 垂直方向终点y=0,Vy=gt/2  代入t Vy=gx/2Vx 高度增加 /2->/1.85 也会有更快速度，飞到球员脸上而不是脚下
        if (!overground) {
            heightVelocity = GRAVITY * distance / (2f * intensity);
        }
        else
        {
            heightVelocity = 0f;
        }
        carrier = null;
        SwitchState(BallState.FREEFORM, BallStateData.Build().SetLockDuration(lockDuration));
    }
    public void kickoffPass(Vector2 destination) {
        Vector2 direction = (destination - Position).normalized;
        float distance = Vector2.Distance(Position, destination);
        float intensity = Mathf.Sqrt(3f * distance * frictionGround);
        Velocity = intensity * direction;
        carrier = null;
        SwitchState(BallState.FREEFORM, BallStateData.Build().SetLockDuration(0.5f));
    }

    public void Stop()
    {
        if (!_running) return;
        _running = false;
    }
    public bool CanAirInteract() {
        return currentState != null && currentState.CanAirInteract();
    }
    private const float TUMBLE_HEIGHT_VELOCITY = 8f;
    private const float DURATION_TUMBLE_LOCK = 0.2f;
    public void Tumble(Vector2 tumbleVelocity) {
        Velocity = tumbleVelocity*3f;
        carrier = null;
        heightVelocity = TUMBLE_HEIGHT_VELOCITY;
        SwitchState(BallState.FREEFORM, BallStateData.Build().SetLockDuration(DURATION_TUMBLE_LOCK));
    }
}
