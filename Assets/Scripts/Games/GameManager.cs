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
    public BallSystem BallSystem { get; private set; }
    public void StartMatch()
    {
        MatchSystem  = new MatchSystem(playerSetup[0], playerSetup[1]);
        PlayerSystem = new PlayerSystem();
        BallSystem   = new BallSystem();

        var systems = new List<ISimulationSystem>
        {
            MatchSystem,
            PlayerSystem,
            BallSystem
        };
        SimulationDriver.Instance.SetSystems(systems);
        SimulationDriver.Instance.StartSimulation();
    }
    public void EndMatch()
    {
        SimulationDriver.Instance.StopMatch();

        GameInterface.Interface.SceneLoader
            .LoadScene(Scene.MainMenuScene);

        MatchSystem  = null;
        PlayerSystem = null;
        BallSystem   = null;
    }
}
