using System;
using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class GameSceneBootstrap : MonoBehaviour{
    public static GameSceneBootstrap Instance { get; private set; }
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
    Vector2 fieldCenter = new Vector2(0, 0);

    void Awake() {
        Instance = this;
    }

    public void Start() {
        BuildSimulationWorld();
    }

    LineSegment BuildLine(FixedVector2 a, FixedVector2 b) {
        LineSegment line = new LineSegment();

        line.Start = a;
        line.End = b;

        line.Edge = b - a;
        line.EdgeSqr = line.Edge.sqrMagnitude;
        return line;
    }

    List<LineSegment> LineaddRange(Transform[] boundaryPoints) {
        List<LineSegment> segments = new List<LineSegment>();

        for (int i = 0; i < boundaryPoints.Length - 1; i++) {
            FixedVector2 a =  (FixedVector2)boundaryPoints[i].position;
            FixedVector2 b = (FixedVector2)boundaryPoints[i + 1].position;

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

        List<FixedVector2> goalHomePos = new List<FixedVector2>();
        foreach (var transform in GoalHomePosition) {
            goalHomePos.Add((FixedVector2)transform.position);
        }

        List<FixedVector2> goalAwayPos = new List<FixedVector2>();
        foreach (var transform in GoalAwayPosition) {
            goalAwayPos.Add((FixedVector2)transform.position);
        }

        FixedRect  goalHomeArea = CreateGoalRect(GoalHomeArea);
        FixedRect goalAwayArea = CreateGoalRect(GoalAwayArea);


        var EventBus = new SimEventBus();

        var commandBuffer = new CommandBuffer();

        var countryHome = GameInterface.Interface.GameManager.playerSetup[0];
        var countryAway = GameInterface.Interface.GameManager.playerSetup[1];

        int matchPlayerCount = GameInterface.Interface.GameFrameSyncManager.matchPlayerCount;
        var MatchSystem =
            new MatchSystem(EventBus, commandBuffer, GameInterface.Interface.GameManager.currentMatchType,matchPlayerCount);
        MatchController = new MatchController(MatchSystem, EventBus, countryHome, countryAway);

        var BallSim = new BallSim((FixedVector2)ballView.spawnPosition, EventBus, commandBuffer);
        ballView.InjectSim(BallSim);



        var PlayerSystem = new PlayerSystem(EventBus, commandBuffer, matchPlayerCount);

        var simConfig = new SimulationConfig();
        var CollisionSystem = new CollisionSystem();
        var BoundarySystem = new BoundarySystem();
        PlayerManager.Instance.InitializeSquads((home, away) => {
                PlayerSystem.RegisterTeams(home, away, BallSim, goalHomePos, goalAwayPos, goalHomeArea, goalAwayArea);
                CollisionSystem.RegisterTeams(home, away, simConfig, BallSim, playerLines);
                BoundarySystem.RegisterTeams(home, away, commandBuffer, simConfig, BallSim, playerLines, ballLines,
                    scoreLines, stopballLines);
            }
        );


        var simModel = new SimulationModel(MatchSystem, PlayerSystem, BallSim);
        var simulationContext = new SimulationContext(simModel);


        var InputBuffer = GameInterface.Interface.GameFrameSyncManager.InputBuffer;
        if (InputBuffer == null) {
            Debug.LogError("Input buffer is null");
        }

        var world = new SimulationWorld(
            new List<ISimulationSystem> { MatchSystem, PlayerSystem, BallSim, CollisionSystem, BoundarySystem },
            simulationContext,
            commandBuffer,
            EventBus,
            InputBuffer
        );
        SimulationClock.Instance.SetWorld(world);
    }

    private FixedRect CreateGoalRect(
        Transform[] goalPoints) {
        if (goalPoints == null ||
            goalPoints.Length < 2) {
            Debug.LogError(
                "GoalHomeArea 至少需要两个点");

            return new FixedRect();
        }

        FixedVector2 p1 =
            new FixedVector2(goalPoints[0].position);

        FixedVector2 p2 =
            new FixedVector2(goalPoints[1].position);

        FixedFloat xMin =
            FixedFloat.Min(p1.x, p2.x);

        FixedFloat xMax =
            FixedFloat.Max(p1.x, p2.x);

        FixedFloat yMin =
            FixedFloat.Min(p1.y, p2.y);

        FixedFloat yMax =
            FixedFloat.Max(p1.y, p2.y);

        return new FixedRect(
            xMin,
            yMin,
            xMax,
            yMax);
    }

    public void EndMatch() {
        StartCoroutine(ReturnToMainMenu());
    }

    private IEnumerator ReturnToMainMenu() {
        yield return new WaitForSeconds(2.5f);
        MatchController = null;
        GameInterface.Interface.GameFrameSyncManager.ClearInputBuffer();
        SimulationClock.Instance.OnGameOver();
        QuitRoomRequest quitRoomRequest = GameInterface.Interface.RequestManager.GetRequest<QuitRoomRequest>();
        quitRoomRequest.SendQuitRoomRequest();
    }

    public void PauseThenQuitMatch() {
        MatchController = null;
        GameInterface.Interface.GameFrameSyncManager.ClearInputBuffer();
        SimulationClock.Instance.OnGameOver();
        QuitRoomRequest quitRoomRequest = GameInterface.Interface.RequestManager.GetRequest<QuitRoomRequest>();
        quitRoomRequest.SendQuitRoomRequest();
    }
}