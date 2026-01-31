using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    public void OnInit()
    {
    }
    public enum MatchType {
        Training,          //自由训练
        TrainingWithEnemy,//对抗训练
        UltimateTeam     //正式比赛
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
    }
    public void SetGameMode(GameMode gameMode) {
        currentGameMode = gameMode;
    }
    
    public MatchController MatchController { get; private set; }
    public PlayerSystem PlayerSystem { get; private set; }
    public BallSim BallSim { get; private set; }
    public SimEventBus EventBus;
    public SimulationFacade SimulationFacade;

    public void StartMatch(BallView ballView)
    {
        EventBus = new SimEventBus();
        
        var simState = new SimulationState();


        BallSim = new BallSim(ballView.spawnPosition,EventBus);
        ballView.InjectSim(BallSim);
        simState.Ball = BallSim;
        /* 正确的方式创建是GameManager
         * 先获取PlayerManager中inspector的引用（如Spawn/kickoffposition）
         *  然后PlayerSystem 注入依赖initDataList/List<PlayerResource> playerResources ，创建并返回HomeSims/AwaySims
         * 然后HomeSims/AwaySims作为PlayerManager的参数创建绑定
         */
        
        
        PlayerSystem = new PlayerSystem(EventBus);

        PlayerManager.Instance.InitializeSquads((home, away) => {
                PlayerSystem.RegisterTeams(home, away);
                simState.Players.AddRange(home);
                simState.Players.AddRange(away);
            }
        );  
        ControlContext controlContext =PrepareControlContext(GameInterface.Interface.RoomManager.RoomPlayerList);
        PlayerSystem.InjectControlContext(controlContext);

        SimulationFacade = new SimulationFacade(PlayerSystem, BallSim);
        MatchController  = new MatchController(SimulationFacade,EventBus,playerSetup[0], playerSetup[1]);
        
        var simulationContext = new SimulationContext();
        SimulationDriver.Instance.SetState(simState);
        SimulationDriver.Instance.SetContext(simulationContext);
        var systems = new List<ISimulationSystem>
        {
            PlayerSystem,
            BallSim
        };
        SimulationDriver.Instance.SetSystems(systems);

        SimulationDriver.Instance.StartSimulation();
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

    private void Start() {
        
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
