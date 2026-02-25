using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager
{
    public GameManager() {
    }
    public void OnInit() {
        currentMatchType = MatchType.Training;
    }
    public MatchType currentMatchType;
    public enum GameMode {
        Single,
        Versus,
        Coop
    }
    public GameMode currentGameMode;
    public string[] playerSetup = { "FRANCE", "" };
    
    
    public void SetCurrentMatchType(MatchType matchType) {
        currentMatchType=matchType;
    }
    public void SetMatchCountry(int RoomIndex, string Country) {
        playerSetup[RoomIndex] = Country;
        Debug.Log("Country:"+Country);
    }
    public void SetGameMode(GameMode gameMode) {
        currentGameMode = gameMode;
    }
    
    public MatchController MatchController { get; private set; }
    public PlayerSystem PlayerSystem { get; private set; }
    public BallSim BallSim { get; private set; }
    public MatchSystem MatchSystem { get; private set; }
    public SimEventBus EventBus;
    public SimulationFacade SimulationFacade;
    public event Action OnStartMatch;

    public void StartMatch(BallView ballView,List<LineSegment> lineSegments)
    {
        EventBus = new SimEventBus();
        
        var commandBuffer = new CommandBuffer();
        SimulationFacade = new SimulationFacade(commandBuffer);
        MatchController  = new MatchController(SimulationFacade,EventBus,playerSetup[0], playerSetup[1]);
        
        ControlContext controlContext =PrepareControlContext(GameInterface.Interface.RoomManager.RoomPlayerList);
        
        MatchSystem = new MatchSystem(EventBus,commandBuffer,currentMatchType,controlContext);
        BallSim = new BallSim(ballView.spawnPosition,EventBus,commandBuffer,lineSegments);
        ballView.InjectSim(BallSim);
        
        PlayerSystem = new PlayerSystem(EventBus,commandBuffer,lineSegments);

        PlayerManager.Instance.InitializeSquads((home, away) => {
                PlayerSystem.RegisterTeams(home, away);
            }
        );  
        
        PlayerSystem.InjectControlContext(controlContext);


        var simModel = new SimulationModel(MatchSystem,PlayerSystem,BallSim);
        var simulationContext = new SimulationContext(simModel);
        SimulationDriver.Instance.SetAttribute(simModel,commandBuffer,EventBus,simulationContext,controlContext,
            new List<ISimulationSystem> { MatchSystem,PlayerSystem, BallSim });
        SimulationDriver.Instance.StartSimulation();
        
        OnStartMatch?.Invoke();
    }

    public ControlContext PrepareControlContext(List<RoomPlayerInfo> roomPlayerInfoList)
    {
        var controlContext = new ControlContext
        {
            HomeOwnerId = -1,
            HomeControlledPlayerId = -1,
            AwayOwnerId = -1,
            AwayControlledPlayerId = -1
        };
        foreach (var info in roomPlayerInfoList)
        {
            if (info.isHome)
            {
                controlContext.HomeOwnerId = info.id;
            }
            else
            {
                controlContext.AwayOwnerId = info.id;
            }
        }
        return controlContext;
    }
    
    public void EndMatch()
    {
        SimulationDriver.Instance.StopMatch();

        GameInterface.Interface.SceneLoader
            .LoadScene(Scene.MainMenuScene);

        MatchController  = null;
        PlayerSystem = null;
        BallSim   = null;
    }
}
