using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    public static GameSceneBootstrap Instance { get;private set; }
    [SerializeField] BallView ballView;
    [SerializeField] Transform topLeft;
    [SerializeField] Transform topRight;
    [SerializeField] Transform downLeft;
    [SerializeField] Transform downRight;
    public MatchController MatchController;

    void Awake() {
        Instance = this;
    }

    public void Start() {
        BuildSimulationWorld();
    }
    private void BuildSimulationWorld() {
        List<LineSegment> lineSegments = new List<LineSegment>();
        lineSegments.Add(new LineSegment{Start=topLeft.position, End=topRight.position});
        lineSegments.Add(new LineSegment{Start=downLeft.position, End=downRight.position});
        lineSegments.Add(new LineSegment{Start=topLeft.position, End=downLeft.position});
        lineSegments.Add(new LineSegment{Start=topRight.position, End=downRight.position});
        
        var EventBus = new SimEventBus();
        
        var commandBuffer = new CommandBuffer();
        
        MatchController  = new MatchController(EventBus,
            GameInterface.Interface.GameManager.playerSetup[0], 
            GameInterface.Interface.GameManager.playerSetup[1]);
        int playerCount = GameInterface.Interface.GameFrameSyncManager.playerCount;

        var MatchSystem = new MatchSystem(EventBus,commandBuffer,GameInterface.Interface.GameManager.currentMatchType);
        var BallSim = new BallSim(ballView.spawnPosition,EventBus,commandBuffer,lineSegments);
        ballView.InjectSim(BallSim);
        
        var PlayerSystem = new PlayerSystem(EventBus,commandBuffer,lineSegments);

        PlayerManager.Instance.InitializeSquads((home, away) => {
                PlayerSystem.RegisterTeams(home, away);
            }
        );  
        
        PlayerSystem.setPlayerCount(playerCount);


        var simModel = new SimulationModel(MatchSystem,PlayerSystem,BallSim);
        var simulationContext = new SimulationContext(simModel);


        var InputBuffer = GameInterface.Interface.GameFrameSyncManager.InputBuffer;
        if (InputBuffer == null) {
            Debug.LogError("Input buffer is null");
        }
        var world = new SimulationWorld(
            new List<ISimulationSystem> { MatchSystem, PlayerSystem, BallSim },
            simulationContext,
            commandBuffer,
            EventBus,
            InputBuffer
        );
        SimulationClock.Instance.SetWorld(world);
    }
    
    public void EndMatch()
    {
        GameInterface.Interface.SceneLoader
            .LoadScene(Scene.MainMenuScene);

        MatchController  = null;
    }
}

