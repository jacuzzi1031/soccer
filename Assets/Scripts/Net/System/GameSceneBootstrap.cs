using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class GameSceneBootstrap : MonoBehaviour
{
    public static GameSceneBootstrap Instance { get;private set; }
    [SerializeField] BallView ballView;
    
    [SerializeField] Transform[] ballBoundaryUpPoints;
    [SerializeField] Transform[] ballBoundaryDownPoints;
    [SerializeField] Transform[] playerBoundaryPoints;
    [SerializeField] Transform[] goalleftBoundaryPoints;
    [SerializeField] Transform[] goalrightBoundaryPoints;
    [SerializeField] Transform[] stopballleftPoints;
    [SerializeField] Transform[] stopballrightPoints;
    
    [SerializeField] Transform[] GoalHomePosition;
    [SerializeField] Transform[] GoalAwayPosition;
    [SerializeField] Transform[] GoalHomeArea;
    [SerializeField] Transform[] GoalAwayArea;
    
    public MatchController MatchController;
    Vector2 fieldCenter=new Vector2(0,0);
    void Awake() {
        Instance = this;
    }

    public void Start() {
        BuildSimulationWorld();
    }
    LineSegment BuildLine(Vector2 a, Vector2 b)
    {
        Vector2 edge = b - a;


        return new LineSegment
        {
            Start = a,
            End = b,
            Edge = edge,
            EdgeSqr = Vector2.Dot(edge, edge),
        };
    }

    List<LineSegment> LineaddRange(Transform[] boundaryPoints) {
        List<LineSegment> segments=new List<LineSegment>();
        for (int i = 0; i < boundaryPoints.Length-1; i++)
        {
            Vector2 a = boundaryPoints[i].position;
            Vector2 b = boundaryPoints[i + 1].position;
            
            segments.Add(BuildLine(a, b));
        }

        return segments;
    }
    private void BuildSimulationWorld() {
        List<LineSegment> ballLines = new List<LineSegment>();
        ballLines.AddRange(LineaddRange(ballBoundaryUpPoints));
        ballLines.AddRange(LineaddRange(ballBoundaryDownPoints));
        
        List<LineSegment> playerLines = LineaddRange(playerBoundaryPoints);
        
        List<LineSegment> stopballLines = new List<LineSegment>();
        stopballLines.AddRange(LineaddRange(stopballleftPoints));
        stopballLines.AddRange(LineaddRange(stopballrightPoints));
        
        List<LineSegment> scoreLines = new List<LineSegment>();
        scoreLines.AddRange(LineaddRange(goalleftBoundaryPoints));
        scoreLines.AddRange(LineaddRange(goalrightBoundaryPoints));
        
        List<Vector2> goalHomePos = new List<Vector2>();
        foreach (var transform in GoalHomePosition) {
            goalHomePos.Add(transform.position);
        }
        List<Vector2> goalAwayPos = new List<Vector2>();
        foreach (var transform in GoalAwayPosition) {
            goalAwayPos.Add(transform.position);
        }
        Rect goalHomeArea = CreateGoalRect(GoalHomeArea);
        Rect goalAwayArea = CreateGoalRect(GoalAwayArea);
        
        
        
        var EventBus = new SimEventBus();
        
        var commandBuffer = new CommandBuffer();
        
        var countryHome = GameInterface.Interface.GameManager.playerSetup[0];
        var countryAway = GameInterface.Interface.GameManager.playerSetup[1];



        var MatchSystem = new MatchSystem(EventBus,commandBuffer,GameInterface.Interface.GameManager.currentMatchType);
        MatchController  = new MatchController(MatchSystem,EventBus, countryHome, countryAway);
        
        var BallSim = new BallSim(ballView.spawnPosition,EventBus,commandBuffer);
        ballView.InjectSim(BallSim);
        
        
        int matchPlayerCount = GameInterface.Interface.GameFrameSyncManager.matchPlayerCount;
        var PlayerSystem = new PlayerSystem(EventBus,commandBuffer,matchPlayerCount);
        
        var simConfig = new SimulationConfig();
        var CollisionSystem = new CollisionSystem();
        var BoundarySystem = new BoundarySystem();
        PlayerManager.Instance.InitializeSquads((home, away) => {
                PlayerSystem.RegisterTeams(home, away,BallSim,goalHomePos,goalAwayPos,goalHomeArea,goalAwayArea);
                CollisionSystem.RegisterTeams(home, away,simConfig,BallSim,playerLines);
                BoundarySystem.RegisterTeams(home, away,commandBuffer,simConfig,BallSim,playerLines,ballLines,scoreLines,stopballLines);
            }
        );  


        var simModel = new SimulationModel(MatchSystem,PlayerSystem,BallSim);
        var simulationContext = new SimulationContext(simModel);


        var InputBuffer = GameInterface.Interface.GameFrameSyncManager.InputBuffer;
        if (InputBuffer == null) {
            Debug.LogError("Input buffer is null");
        }
        var world = new SimulationWorld(
            new List<ISimulationSystem> { MatchSystem, PlayerSystem, BallSim,CollisionSystem, BoundarySystem },
            simulationContext,
            commandBuffer,
            EventBus,
            InputBuffer
        );
        SimulationClock.Instance.SetWorld(world);
    }

    private Rect CreateGoalRect(Transform[] goalPoints) {
        if (goalPoints == null || goalPoints.Length < 2)
        {
            Debug.LogError("GoalHomeArea 至少需要两个点");
            return new Rect();
        }

        Vector2 p1 = goalPoints[0].position;
        Vector2 p2 = goalPoints[1].position;

        float xMin = Mathf.Min(p1.x, p2.x);
        float xMax = Mathf.Max(p1.x, p2.x);
        float yMin = Mathf.Min(p1.y, p2.y);
        float yMax = Mathf.Max(p1.y, p2.y);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    public void EndMatch() {
        StartCoroutine(ReturnToMainMenu());
    }
    private IEnumerator ReturnToMainMenu() {
        yield return new WaitForSeconds(2.5f);
        MatchController  = null;
        GameInterface.Interface.GameFrameSyncManager.ClearInputBuffer();
        SimulationClock.Instance.OnGameOver();
        QuitRoomRequest quitRoomRequest = GameInterface.Interface.RequestManager.GetRequest<QuitRoomRequest>();
        quitRoomRequest.SendQuitRoomRequest();
    }

    public void PauseThenQuitMatch() {
        MatchController  = null;
        GameInterface.Interface.GameFrameSyncManager.ClearInputBuffer();
        SimulationClock.Instance.OnGameOver();
        QuitRoomRequest quitRoomRequest = GameInterface.Interface.RequestManager.GetRequest<QuitRoomRequest>();
        quitRoomRequest.SendQuitRoomRequest();
    }
}

