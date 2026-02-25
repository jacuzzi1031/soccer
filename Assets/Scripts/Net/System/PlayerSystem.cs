using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// PlayerSystem 唯一可以推进帧的地方（Tick）
/// PlayerManager 可以在任意时间响应事件 / 表现
/// </summary>
public class PlayerSystem:ISimulationSystem
{
    private List<PlayerSim> teamHome;
    private List<PlayerSim> teamAway;
    private PlayerSim currentHomePlayer;
    private PlayerSim currentAwayPlayer;
    private int weightCacheIntervalFrames;
    private const float DURATION_WEIGHT_CACHE = 0.2f;
    private const int FP = 1000;
    private float lastCacheRefreshFrame = 0f;
    private bool isCheckingForKickoffReadiness=false;
    public ControlContext _control;
	public SimEventBus _eventBus;
    public CommandBuffer _commandBuffer;
    public List<LineSegment> lines;
    const float EPS = 0.0001f;  
	public PlayerSystem(SimEventBus eventBus,CommandBuffer commandBuffer,List<LineSegment> lineSegments) {
        _eventBus = eventBus;
        _commandBuffer=commandBuffer;
        lines=lineSegments;
    }

    public void RegisterTeams(
        List<PlayerSim> home,
        List<PlayerSim> away)
    {
        teamHome = home;
        teamAway = away;
        foreach (var homePlayer in teamHome) {
            homePlayer.SetEventBusAndCommandBuffer(_eventBus,_commandBuffer,lines);
        }
        foreach (var awayPlayer in teamAway) {
            awayPlayer.SetEventBusAndCommandBuffer(_eventBus,_commandBuffer,lines);
        }
        weightCacheIntervalFrames=Mathf.RoundToInt(SimulationDriver.FRAME_DT * DURATION_WEIGHT_CACHE);
    }
    public void Tick(SimulationContext context)
    {
        foreach (var command in context.Commands)
        {
            switch (command.Type)
            {
                case SimulationCommandType.ResetAndHomeKickoff:
                    HandleResetTeams(true);
                    break;
                case SimulationCommandType.ResetAndAwayKickoff:
                    HandleResetTeams(false);
                    break;
                case SimulationCommandType.KickoffStart:
                    OnKickoffStarted();
                    break;
                case SimulationCommandType.Swap:
                    HandleSwap(command.OwnerId,context.BallCarrierId,context.BallPosition);
                    break;
                case SimulationCommandType.NoneInputCommand:
                    HandleNoneInputCommand(command.OwnerId,command.Direction);
                    break;
                case SimulationCommandType.ShootRelease:
                    HandleShootRelease(command.OwnerId,context.BallCarrierId,context.BallCanAirInteract,command.Direction);
                    break;
                case SimulationCommandType.ShootPress:
                    HandleShootPress(command.OwnerId,context.BallCarrierId,context.BallCanAirInteract,command.Direction);
                    break;
                case SimulationCommandType.ShortPass:
                    HandleShortPass(command.OwnerId);
                    break;
                case SimulationCommandType.LongPass:
                    HandleLongPass(command.OwnerId);
                    break;
                case SimulationCommandType.IncisivePass:
                    HandleIncisivePass(command.OwnerId);
                    break;
            }
        }
        if (lastCacheRefreshFrame < 0 ||
            context.Frame - lastCacheRefreshFrame >= weightCacheIntervalFrames)
        {
            lastCacheRefreshFrame = context.Frame;
            SetOnDutyWeights(context.BallPosition);
        }
        if (isCheckingForKickoffReadiness)
        {
            CheckForKickoffReadiness();
        }


        foreach (var homePlayer in teamHome) {
            homePlayer.Tick(context.Frame,context.DeltaTime);
        }
        foreach (var awayPlayer in teamAway) {
            awayPlayer.Tick(context.Frame,context.DeltaTime);
        }
    }

    private void HandleSwap(int commandOwnerId,int ballCarrierId,Vector2 ballPosition) {
        PlayerSim currentPlayer = commandOwnerId == _control.HomeOwnerId?currentHomePlayer:currentAwayPlayer;
        if(currentPlayer.playerId==ballCarrierId) return;
        
        List<PlayerSim> currentTeam=currentPlayer.isHome?teamHome:teamAway;
        
        PlayerSim closestCpuToBall = null;
        float closestDist = float.MaxValue;

        foreach (var p in currentTeam)
        {
            if (p.controlScheme != ControlScheme.CPU || 
                p.role == Role.GOALIE ||
                p == currentPlayer)
                continue;
            float dist = (p.Position - ballPosition).sqrMagnitude;
            // if (dist < closestDist)
            // {
            //     closestDist = dist;
            //     closestCpuToBall = p;
            // }  
            if (dist + EPS < closestDist)
            {
                closestDist = dist;
                closestCpuToBall = p;
            }else if (Mathf.Abs(dist - closestDist) <= EPS)
            {
                if (p.playerId < closestCpuToBall.playerId)
                {
                    closestCpuToBall = p;
                    closestDist = dist;
                }
            }
        }

        if (closestCpuToBall == null) return;

        float senderDist = (currentPlayer.Position - ballPosition).sqrMagnitude;

        if (closestDist < senderDist&&currentPlayer.isHome) {
            currentHomePlayer=closestCpuToBall;
            _eventBus.Publish(new ControllerChangedSignal
            {
                HomePlayerId = closestCpuToBall.playerId,
                AwayPlayerId = -1
            });
        }else if (closestDist < senderDist && !currentPlayer.isHome) {
            currentAwayPlayer=closestCpuToBall;
            _eventBus.Publish(new ControllerChangedSignal
            {
                HomePlayerId = -1,
                AwayPlayerId = closestCpuToBall.playerId,
            });
        }
    }

    private void HandleIncisivePass(int commandOwnerId) {
        PlayerSim currentPlayer = commandOwnerId == _control.HomeOwnerId?currentHomePlayer:currentAwayPlayer;
    }

    private void HandleLongPass(int commandOwnerId) {
        PlayerSim currentPlayer = commandOwnerId == _control.HomeOwnerId?currentHomePlayer:currentAwayPlayer;
    }

    private void HandleShortPass(int commandOwnerId) {
        PlayerSim currentPlayer = commandOwnerId == _control.HomeOwnerId?currentHomePlayer:currentAwayPlayer;
    }

    private void HandleShootPress(int commandOwnerId,int ballCarrierId,bool BallCanAirInteract,Vector2 Direction) {
        PlayerSim currentPlayer = commandOwnerId == _control.HomeOwnerId?currentHomePlayer:currentAwayPlayer;
        var hasBall = currentPlayer.playerId == ballCarrierId;
        currentPlayer.CurrentSimState.SetMoveDirection(Direction);
        currentPlayer.CurrentSimState?.OnShootPress(false,hasBall,BallCanAirInteract);
    }

    private void HandleShootRelease(int commandOwnerId,int ballCarrierId,bool BallCanAirInteract,Vector2 Direction) {
        PlayerSim currentPlayer = commandOwnerId == _control.HomeOwnerId?currentHomePlayer:currentAwayPlayer;
        var hasBall = currentPlayer.playerId == ballCarrierId;
        currentPlayer.CurrentSimState.SetMoveDirection(Direction);
        currentPlayer.CurrentSimState?.OnShootRelease();
    }

    private void HandleNoneInputCommand(int commandOwnerId,Vector2 Direction) {
        PlayerSim currentPlayer = commandOwnerId == _control.HomeOwnerId?currentHomePlayer:currentAwayPlayer;
        currentPlayer.CurrentSimState.SetMoveDirection(Direction);
        if (Direction.x != 0) currentPlayer.facingRight = Direction.x > 0;
    }

    private void OnKickoffStarted() {
        foreach (var squad in new[] { teamHome, teamAway })
        {
            foreach (PlayerSim player in squad)
            {
                player.SwitchState(PlayerState.MOVING);
            }
        }
    }

    private void HandleResetTeams(bool isHomeKickoff) {

        isCheckingForKickoffReadiness = true;
        
        //for celebrating and mourning players
        for (int i = 0; i < teamHome.Count; i++)
        {
            teamHome[i].OnTeamReset(isHomeKickoff);
        }
        for (int i = 0; i < teamAway.Count; i++)
        {
            teamAway[i].OnTeamReset(!isHomeKickoff);
        }
    }
    /// <summary>
    ///  MatchSystem ->RESET state =>command.ResetAndHomeKickoff
    ///         PlayerSystem isCheckingForKickoffReadiness = true; could Tick for CheckForKickoffReadiness
    ///         all players arrive to assign position and 
    ///           set hasArrived = true;  for PlayerSystem CheckForKickoffReadiness IsReadyForKickoff 
    ///             =>command.AllPlayersReadyForKickoff=> MatchSystem KICKOFF state
    ///             and IsReadyForKickoff also ResetControlScheme
    /// </summary>
    void CheckForKickoffReadiness()
    {
        foreach (var squad in new[] { teamHome, teamAway })
        {
            foreach (PlayerSim player in squad)
            {
                if (!player.IsReadyForKickoff())
                {
                    return;
                }
            }
        }
        isCheckingForKickoffReadiness = false;
        //给MatchSystem写事实
        _commandBuffer.Enqueue(new SimulationCommand
        {
            Type = SimulationCommandType.AllPlayersReadyForKickoff
        });
        ResetControlSchemesSim();
    }



    private void ResetControlSchemesSim() {
        for (int i = 0; i < teamHome.Count; i++) {
            teamHome[i].controlScheme = ControlScheme.CPU;
        }
        for (int i = 0; i < teamAway.Count; i++) {
            teamAway[i].controlScheme = ControlScheme.CPU;
        }

        if (_control.HomeOwnerId != -1) {
            teamHome[^1].controlScheme = ControlScheme.P1;
            currentHomePlayer=teamHome[^1];
        }        
        if (_control.AwayOwnerId != -1) {
            teamAway[^1].controlScheme = ControlScheme.P2;
            currentAwayPlayer=teamAway[^1];
        }
        _eventBus.Publish(new ControllerChangedSignal
        {
            HomePlayerId = _control.HomeOwnerId != -1?teamHome[^1].playerId:-1,
            AwayPlayerId = _control.AwayOwnerId != -1?teamAway[^1].playerId:-1
        });
    }
    private void SetOnDutyWeights(Vector2 ballPosition) {
        List<List<PlayerSim>> squads = new List<List<PlayerSim>> { teamHome, teamAway };

        foreach (var squad in squads)
        {   
            List<PlayerSim> cpuPlayers= new List<PlayerSim>();
            for (int j = 0; j < squad.Count; j++)
            {
                var p = squad[j];
                if (p.controlScheme == ControlScheme.CPU &&
                    p.role != Role.GOALIE)
                {
                    cpuPlayers.Add(p);
                }
            }
            cpuPlayers.Sort((p1, p2) =>
            {
                float d1 = (p1.teamResetPosition - ballPosition).sqrMagnitude;
                float d2 = (p2.teamResetPosition - ballPosition).sqrMagnitude;

                int cmp = d1.CompareTo(d2);
                if (cmp != 0)
                    return cmp;
                //tie-breaker = 当主要排序条件“相等或几乎相等”时，用一个“永远一致的次级规则”来打破平局。
                return p1.playerId.CompareTo(p2.playerId);
            });


            for (int i = 0; i < cpuPlayers.Count; i++)
            {
                int x = i * FP / 10;              // 0 ~ 1000
                int eased = (x * x) / FP;         // bias = 2
                int weight = FP - eased;          // 1 - ease

                cpuPlayers[i].weightOnDutySteering = weight;
            }
        }
    }
    public void Stop()
    {
    }

    public void InjectControlContext(ControlContext controlContext) {
        _control=controlContext;
        ResetControlSchemesSim();
    }
}
