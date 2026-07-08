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
    //整个演算框架的初始化架构
    private void BuildSimulationWorld()
    {
        #region 构建静态场景数据（碰撞边界、球门区域）
        
        // 将Unity场景中的边界转换为演算系统使用的线段数据
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
    
        // 球门位置与得分区域
        List<FixedVector2> goalHomePos = new List<FixedVector2>();
        foreach (var transform in GoalHomePosition)
            goalHomePos.Add((FixedVector2)transform.position);
    
        List<FixedVector2> goalAwayPos = new List<FixedVector2>();
        foreach (var transform in GoalAwayPosition)
            goalAwayPos.Add((FixedVector2)transform.position);
    
        FixedRect goalHomeArea = CreateGoalRect(GoalHomeArea);
        FixedRect goalAwayArea = CreateGoalRect(GoalAwayArea);
    
        #endregion
    
        #region 创建演算基础设施（系统通信eventBus、命令缓存commandBuffer、输入缓存InputBuffer）
    
        // EventBus：演算系统和编排系统事件通信
        var eventBus = new SimEventBus();
    
        // CommandBuffer：缓存本帧产生的输入命令以及演算系统内部命令，由SimulationWorld统一执行
        var commandBuffer = new CommandBuffer();
    
        // InputBuffer：网络层输入缓冲区，保存帧同步输入
        var inputBuffer = GameInterface.Interface.GameFrameSyncManager.InputBuffer;
        if (inputBuffer == null)
        {
            Debug.LogError("Input buffer is null");
        }
    
        // 比赛基础信息
        var countryHome = GameInterface.Interface.GameManager.playerSetup[0];
        var countryAway = GameInterface.Interface.GameManager.playerSetup[1];
        int matchPlayerCount = GameInterface.Interface.GameFrameSyncManager.matchPlayerCount;
    
        #endregion
    
        #region 创建演算子系统及表现层的桥接
    
        var matchSystem = new MatchSystem(
            eventBus,
            commandBuffer,
            GameInterface.Interface.GameManager.currentMatchType,
            matchPlayerCount);
    
        MatchController = new MatchController(
            matchSystem,
            eventBus,
            countryHome,
            countryAway);
    
        var ballSim = new BallSim(
            (FixedVector2)ballView.spawnPosition,
            eventBus,
            commandBuffer);
    
        ballView.InjectSim(ballSim);
    
        var playerSystem = new PlayerSystem(
            eventBus,
            commandBuffer,
            matchPlayerCount);
    
        var simConfig = new SimulationConfig();
    
        var collisionSystem = new CollisionSystem(
            eventBus,
            commandBuffer);
    
        var boundarySystem = new BoundarySystem(
            eventBus);
    
        #endregion
    
        #region 创建PlayerSim，绑定PlayerView，并向各演算系统注册运行时数据
        
        PlayerManager.Instance.InitializeSquads((home, away) =>
        {
            playerSystem.RegisterTeams(
                home,
                away,
                ballSim,
                goalHomePos,
                goalAwayPos,
                goalHomeArea,
                goalAwayArea);
    
            collisionSystem.RegisterTeams(
                home,
                away,
                simConfig,
                ballSim,
                playerLines);
    
            boundarySystem.RegisterTeams(
                home,
                away,
                commandBuffer,
                simConfig,
                ballSim,
                playerLines,
                ballLines,
                scoreLines,
                stopballLines);
        });
    
        #endregion
    
        #region 创建SimulationWorld并启动演算
    
        // SimulationContext作为所有演算系统共享的运行时上下文
        var simModel = new SimulationModel(
            matchSystem,
            playerSystem,
            ballSim);
    
        var simulationContext = new SimulationContext(simModel);
    
        // SimulationWorld负责按固定Tick依次驱动所有SimulationSystem
        var world = new SimulationWorld(
            new List<ISimulationSystem>
            {
                matchSystem,
                playerSystem,
                ballSim,
                collisionSystem,
                boundarySystem
            },
            simulationContext,
            commandBuffer,
            eventBus,
            inputBuffer);
    
        // 将演算世界交给SimulationClock，以固定帧率推进整个确定性演算
        SimulationClock.Instance.SetWorld(world);
    
        #endregion
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