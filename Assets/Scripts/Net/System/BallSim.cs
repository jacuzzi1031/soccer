using System;
using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class BallSim:ISimulationSystem
{
    public FixedVector2 Position;
    public FixedVector2 Velocity;
    public FixedFloat height=(FixedFloat) 0.0f;
    public FixedFloat heightVelocity=(FixedFloat)0.0f;
    private static FixedFloat KICKOFF_PASS_DISTANCE = (FixedFloat)30f;
    private static FixedFloat DURATION_PASS_LOCK =(FixedFloat) 0.1f;
    public static FixedFloat GRAVITY = (FixedFloat)20f;
    public FixedVector2 spawnPosition;
    public BallSimState currentState;
    public BallSimStateFactory stateFactory=new BallSimStateFactory();
    private bool _running;
    public SimEventBus _eventBus;
    public BallState ballState;
    public CommandBuffer _commandBuffer;
    public int Frame { get;private set;}
    [HideInInspector]public FixedFloat frictionAir = (FixedFloat)10f;
    [HideInInspector]public FixedFloat frictionGround = (FixedFloat)75f;
    [HideInInspector]public PlayerSim carrier;
    public const int INVALID_PLAYER_ID = -1;
    public int BallCarrierId => carrier?.playerId ?? INVALID_PLAYER_ID;
    public bool firstPlayerCarryBall;
    public BallSim(FixedVector2 ContextSpawnPosition,SimEventBus eventBus,CommandBuffer commandBuffer)
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
                    kickoffPass(spawnPosition + FixedVector2.Down * KICKOFF_PASS_DISTANCE);
                    break;
                case SimulationCommandType.ResetAndHomeKickoff:
                case SimulationCommandType.ResetAndAwayKickoff:
                    firstPlayerCarryBall = false;
                    Position = spawnPosition;
                    Velocity = FixedVector2.Zero;
                    height=FixedFloat.Zero;
                    _eventBus.Publish(new BallBacktoSpawnPositionSignal());
                    SwitchState(BallState.RESET);
                    break;
            }
        }
        currentState?._Update();
        
    }

    public void shoot(FixedVector2 ShotVelocity) {
        Velocity = ShotVelocity;
        carrier = null;
        SwitchState(BallState.SHOT);
    }
    public void passTo(FixedVector2 destination, bool overground)
        => passTo(destination, overground, DURATION_PASS_LOCK);
    public void passTo(FixedVector2 destination,bool overground, FixedFloat lockDuration) {
        FixedVector2 direction = (destination - Position).normalized;
        FixedFloat distance = FixedVector2.Distance(Position, destination);
        FixedFloat intensity = FixedMath.BetterSqrt(2 * distance * frictionGround);
        Velocity = intensity * direction;
        // 如果是高空的，视为水平是匀速，速度为原本速度intensity  x=Vx*t  t=x/Vx
        // 垂直方向终点y=0,Vy=gt/2  代入t Vy=gx/2Vx 高度增加 /2->/1.85 也会有更快速度，飞到球员脸上而不是脚下
        if (!overground) {
            heightVelocity = GRAVITY * distance / (2 * intensity);
        }
        else
        {
            heightVelocity = FixedFloat.Zero;
        }
        carrier = null;
        SwitchState(BallState.FREEFORM, BallStateData.Build().SetLockDuration(lockDuration));
    }
    public void kickoffPass(FixedVector2 destination) {
        FixedVector2 direction = (destination - Position).normalized;
        FixedFloat distance = FixedVector2.Distance(Position, destination);
        FixedFloat intensity = FixedMath.BetterSqrt(3 * distance * frictionGround);
        Velocity = intensity * direction;
        carrier = null;
        SwitchState(BallState.FREEFORM, BallStateData.Build().SetLockDuration((FixedFloat)0.5f));
    }

    public void Stop()
    {
        if (!_running) return;
        _running = false;
    }
    public bool CanAirInteract() {
        return currentState != null && currentState.CanAirInteract();
    }
    private static  FixedFloat TUMBLE_HEIGHT_VELOCITY =(FixedFloat) 8f;
    private static FixedFloat DURATION_TUMBLE_LOCK = (FixedFloat) 0.2f;
    public void Tumble(FixedVector2 tumbleVelocity) {
        Velocity = tumbleVelocity*3;
        carrier = null;
        heightVelocity = TUMBLE_HEIGHT_VELOCITY;
        SwitchState(BallState.FREEFORM, BallStateData.Build().SetLockDuration(DURATION_TUMBLE_LOCK));
    }
}
