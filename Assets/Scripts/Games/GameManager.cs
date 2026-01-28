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
    
    public MatchSystem MatchSystem { get; private set; }
    public PlayerSystem PlayerSystem { get; private set; }
    public BallSim BallSim { get; private set; }
    public void StartMatch(BallView ballView)
    {
        ControlContext controlContext =PrepareControlContext(GameInterface.Interface.RoomManager.RoomPlayerList);
        MatchSystem  = new MatchSystem(playerSetup[0], playerSetup[1]);

        BallSim = new BallSim(ballView.spawnPosition);
        ballView.InjectSim(BallSim);
        
        /* 正确的方式创建是GameManager
         * 先获取PlayerManager中inspector的引用（如Spawn/kickoffposition）
         *  然后PlayerSystem 注入依赖initDataList/List<PlayerResource> playerResources ，创建并返回HomeSims/AwaySims
         * 然后HomeSims/AwaySims作为PlayerManager的参数创建绑定
         */
        
        
        PlayerSystem = new PlayerSystem();

        PlayerManager.Instance.InitializeSquads(
            (home, away) => { PlayerSystem.RegisterTeams(home, away); }
        );  
        PlayerSystem.InjectAttribute(BallSim,MatchSystem,controlContext);


        var systems = new List<ISimulationSystem>
        {
            MatchSystem,
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

        MatchSystem  = null;
        PlayerSystem = null;
        BallSim   = null;
    }
}
