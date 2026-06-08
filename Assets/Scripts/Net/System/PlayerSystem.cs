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
	public SimEventBus _eventBus;
    public CommandBuffer _commandBuffer;
    public BallSim _ballSim;
    const float EPS = 0.0001f;  
    private int matchPlayerCount;
    
    //for passing delay to swap / or by another scheduler
    private int _switchControlFrame = -1;
    private PlayerSim _fromPlayer;
    private PlayerSim _toPlayer;
    private ControlScheme _scheme;
	public PlayerSystem(SimEventBus eventBus,CommandBuffer commandBuffer,int MatchPlayerCount) {
        _eventBus = eventBus;
        _commandBuffer=commandBuffer;
        matchPlayerCount = MatchPlayerCount;
        weightCacheIntervalFrames=Mathf.RoundToInt(DURATION_WEIGHT_CACHE / SimulationClock.FRAME_DT);
    }

    public void RegisterTeams(
        List<PlayerSim> home,
        List<PlayerSim> away,
        BallSim ballSim,List<Vector2> goalHomePos,List<Vector2> goalAwayPos,
        Rect goalHomeArea,Rect goalAwayArea)
    {
        teamHome = home;
        teamAway = away;
        _ballSim=ballSim;
        foreach (var homePlayer in teamHome) {
            homePlayer.SetEventBusAndCommandBuffer(_eventBus,_commandBuffer,_ballSim,goalAwayPos,goalHomeArea,matchPlayerCount);
        }
        foreach (var awayPlayer in teamAway) {
            awayPlayer.SetEventBusAndCommandBuffer(_eventBus,_commandBuffer,_ballSim,goalHomePos,goalAwayArea,matchPlayerCount);
        }
        ResetControlSchemesSim();
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
                    HandleSwap(command.SeatIndex,context.BallCarrierId,context.BallPosition);
                    break;
                case SimulationCommandType.NoneInputCommand:
                    HandleNoneInputCommand(command.SeatIndex,command.Direction);
                    break;
                case SimulationCommandType.ShootRelease:
                    HandleShootRelease(command.SeatIndex,context.BallCarrierId,command.Direction,context.BallCanAirInteract);
                    break;
                case SimulationCommandType.ShootPress:
                    HandleShootPress(command.SeatIndex,context.BallCarrierId,context.BallCanAirInteract,command.Direction);
                    break;
                case SimulationCommandType.ShortPass:
                    HandlePass(command.SeatIndex,0,command.Direction,context.DeltaTime,context.Frame);
                    break;
                case SimulationCommandType.LongPass:
                    HandlePass(command.SeatIndex,1,command.Direction,context.DeltaTime,context.Frame);
                    break;
                case SimulationCommandType.IncisivePass:
                    HandlePass(command.SeatIndex,2,command.Direction,context.DeltaTime,context.Frame);
                    break;
                case SimulationCommandType.TeamScoring:
                case SimulationCommandType.GameOverWinner:
                    foreach (var player in teamHome) {
                        player.SwitchState(command.isHome?PlayerState.CELEBRATING:PlayerState.MOURNING);
                    }
                    foreach (var player in teamAway) {
                        player.SwitchState(command.isHome?PlayerState.MOURNING:PlayerState.CELEBRATING);
                    }
                    break;
            }
        }
        if (isCheckingForKickoffReadiness)
        {
            CheckForKickoffReadiness();
        }
        if (lastCacheRefreshFrame < 0 ||
            context.Frame - lastCacheRefreshFrame >= weightCacheIntervalFrames)
        {
            lastCacheRefreshFrame = context.Frame;
            SetOnDutyWeights(context.BallPosition);
            //主要这个力太大导致靠太近
        }
        int homeCount = 0;
        int awayCount = 0;

        foreach (var player in teamHome)
        {
            if (IsNearBall(player.Position))
            {
                    homeCount++;
            }
        }
        foreach (var player in teamAway)
        {
            if (IsNearBall(player.Position))
            {
                awayCount++;
            }
        }
        
        
        foreach (var homePlayer in teamHome) {
            homePlayer.Tick(context.Frame,context.DeltaTime,homeCount,awayCount);
        }
        foreach (var awayPlayer in teamAway) {
            awayPlayer.Tick(context.Frame,context.DeltaTime,homeCount,awayCount);
        }
        
        //for passing delay to swap
        if (_switchControlFrame >= 0 && context.Frame >= _switchControlFrame)
        {
            SwitchControlTo(_fromPlayer, _toPlayer, _scheme);
            _switchControlFrame = -1;
        }
    }

    public float playerProximitySqr = 55f * 55f;
    private bool IsNearBall(Vector2 playerPosition) {
        var ballPos = _ballSim.Position;
        float distSqr= (playerPosition-ballPos).sqrMagnitude;
        if (distSqr < playerProximitySqr) {
            return true;
        } 
        return false;
    }

    private void HandleSwap(int seatIndex,int ballCarrierId,Vector2 ballPosition) {
        PlayerSim currentPlayer = seatIndex == 0?currentHomePlayer:currentAwayPlayer;
        //fifa is when equal then call player pointed come
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

        if (closestDist < senderDist)
        {
            if (currentPlayer.isHome)
            {
                SwitchControlTo(currentHomePlayer,closestCpuToBall,ControlScheme.P1);
            }
            else
            {
                SwitchControlTo(currentAwayPlayer,closestCpuToBall,ControlScheme.P2);
            }
            
        }
    }

    private void HandlePass(int seatIndex,int inputType,Vector2 Direction,float deltaTime,int currentFrame) {
        
        PlayerSim currentPlayer = seatIndex ==0?currentHomePlayer:currentAwayPlayer;
        currentPlayer.currentState.SetMoveDirection(Direction);
        
        bool hasBall = currentPlayer._ballSim.BallCarrierId == currentPlayer.playerId;
        if (!hasBall) {
            //tackle
            currentPlayer.currentState.OnTackle(Direction);
            return;
        }
        //get passTarget
        PlayerSim passTarget = null;
        IReadOnlyList<PlayerSim> team = seatIndex ==0?teamHome:teamAway;
        
        switch (inputType) {
            case 0:
                passTarget = GetShortPassTarget(currentPlayer, team, Direction);
                break;
            case 1:
                passTarget  = GetLongPassTarget(currentPlayer, team, Direction);
                break;
            case 2:
                passTarget = GetShortPassTarget(currentPlayer, team, Direction);
                break;
        } 
        currentPlayer.currentState.OnPass(Direction,inputType,passTarget);

        if (passTarget != null) {
            int delayFrames = (int)(0.23f / deltaTime);
            _switchControlFrame = currentFrame + delayFrames;
            _fromPlayer = currentPlayer;
            _toPlayer = passTarget;
            _scheme = seatIndex == 0 ? ControlScheme.P1 : ControlScheme.P2;
        }
    }

    private void HandleShootPress(int seatIndex,int ballCarrierId,bool BallCanAirInteract,Vector2 Direction) {
        PlayerSim currentPlayer = seatIndex == 0?currentHomePlayer:currentAwayPlayer;
        var hasBall = currentPlayer.playerId == ballCarrierId;
        currentPlayer.currentState.SetMoveDirection(Direction);
        currentPlayer.currentState?.OnShootPress(false,hasBall,BallCanAirInteract);
    }

    private void HandleShootRelease(int seatIndex,int ballCarrierId,Vector2 Direction,bool BallCanAirInteract) {
        PlayerSim currentPlayer = seatIndex ==0?currentHomePlayer:currentAwayPlayer;
        var hasBall = currentPlayer.playerId == ballCarrierId;
        currentPlayer.currentState.SetMoveDirection(Direction);
        currentPlayer.currentState?.OnShootRelease(hasBall,BallCanAirInteract);
    }

    private void HandleNoneInputCommand(int seatIndex,Vector2 Direction) {
        PlayerSim currentPlayer = seatIndex ==0?currentHomePlayer:currentAwayPlayer;
        currentPlayer.currentState.SetMoveDirection(Direction);
        // if (Direction.x != 0) currentPlayer.HeadingRight = Direction.x > 0;
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
            // teamHome[i].OnTeamReset(isHomeKickoff);
            teamHome[i].currentState.OnTeamReset(isHomeKickoff);
        }
        for (int i = 0; i < teamAway.Count; i++)
        {
            // teamAway[i].OnTeamReset(!isHomeKickoff);
            teamAway[i].currentState.OnTeamReset(!isHomeKickoff);
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

        SwitchControlTo(currentHomePlayer,teamHome[^1], ControlScheme.P1);
        
        
        if (matchPlayerCount==2) {
            SwitchControlTo(currentAwayPlayer,teamAway[^1], ControlScheme.P2);
        }
    }

    private void SwitchControlTo(PlayerSim oldPlayer,PlayerSim newPlayer, ControlScheme controlScheme) {
        int oldId = -1;
        if (oldId == newPlayer.playerId) {
            return;
        }
        if (oldPlayer != null) {
            oldPlayer.controlScheme=ControlScheme.CPU;
            oldId = oldPlayer.playerId;
        }
        newPlayer.controlScheme=controlScheme;
        if (controlScheme == ControlScheme.P1)
        {
            currentHomePlayer = newPlayer;
            currentHomePlayer.controlScheme = ControlScheme.P1;
        }
        else
        {
            currentAwayPlayer = newPlayer;
            currentAwayPlayer.controlScheme = ControlScheme.P2;
        }
        _eventBus.Publish(new ControllerChangedSignal
        {
            OldPlayerId = oldId,
            NewPlayerId = newPlayer.playerId,
            Scheme = controlScheme
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
                float d1 = (p1.spawnPosition - ballPosition).sqrMagnitude;
                float d2 = (p2.spawnPosition - ballPosition).sqrMagnitude;

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
    public void OnPlayerBecomesCarrier(int playerPlayerId,bool isHome) {
        PlayerSim currentPlayer=isHome?currentHomePlayer:currentAwayPlayer;
        if (currentPlayer==null||currentPlayer.playerId == playerPlayerId) {
            return;
        }
        List<PlayerSim>team=isHome?teamHome:teamAway;
        ControlScheme controlScheme=isHome?ControlScheme.P1:ControlScheme.P2;
        foreach (var player in team) {
            if (player.playerId == playerPlayerId) {
                SwitchControlTo(currentPlayer,player,controlScheme);
                break;
            }
        }
    }
    public PlayerSim GetShortPassTarget(PlayerSim self, IReadOnlyList<PlayerSim> team, Vector2 moveDir=new Vector2())
    { 
        var list = GetEligibleTargets(self, team, moveDir);
        return list.Count > 0 ? list[0] : null;
    }
    public static PlayerSim GetLongPassTarget(PlayerSim self, IReadOnlyList<PlayerSim> team, Vector2 moveDir=new Vector2())
    {
        var list = GetEligibleTargets(self, team, moveDir);

        if (list.Count >= 2)
            return list[1];
        else if (list.Count == 1)
            return list[0];
        else
            return null;
    }
    static List<PlayerSim> GetEligibleTargets(
        PlayerSim self,
        IReadOnlyList<PlayerSim> team,
        Vector2 moveDir)
    {
        if (moveDir.sqrMagnitude <= 0.01f)
            moveDir = self.HeadingRight ? Vector2.right : Vector2.left;

        List<PlayerSim> list = new List<PlayerSim>();

        PlayerSim closest = null;
        float closestDist = float.MaxValue;

        for (int i = 0; i < team.Count; i++)
        {
            var p = team[i];
            if (p == self) continue;

            Vector2 toTarget = p.Position - self.Position;
            

            float dist = (p.Position - self.Position).sqrMagnitude;

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p;
            }

            if (!IsWithinAngle(toTarget, moveDir)) continue;

            list.Add(p);
        }

        if (list.Count == 0 && closest != null)
            list.Add(closest);
        
        list.Sort((a, b) =>
        {
            float da = (a.Position - self.Position).sqrMagnitude;
            float db = (b.Position - self.Position).sqrMagnitude;
            return da.CompareTo(db);
        });

        return list;
    }
    static bool IsWithinAngle(Vector2 toTarget, Vector2 moveDir)
    {
        float dot = Vector2.Dot(toTarget.normalized, moveDir.normalized);
        return dot >  0.7071f; 
    }
}
