using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ball ball;
    [SerializeField] private Goal goalHome;
    [SerializeField] private Goal goalAway;
    [SerializeField] private Player  playerPrefab;
    
    [Header("Components")]
    [SerializeField] private Transform spawns;
    [SerializeField] private Transform kickOffs;
    [SerializeField] private Transform training;

    private const float DURATION_WEIGHT_CACHE = 0.2f;
    private float timeSinceLastCacheRefresh = 0f;
    // private bool isCheckForKickOffReadiness = false;
    private List<Player> squadHome;
    private List<Player> squadAway;
    [HideInInspector]public List<Player> currentTeam;
    [HideInInspector]public List<Player> opponentTeam;
    [HideInInspector]public Player currentControlPlayer;
    private Dictionary<int, Player> playersById = new Dictionary<int, Player>();
    public static PlayerManager Instance{get; private set;}
    private int nextPlayerId = 0;
    private void Awake() {
        Instance = this;
    }

    private void Start() {
        squadHome = SpawnPlayers(GameManager.Instance.currentMatch.countryHome,goalHome);
        // spawns.localScale = new Vector3(-1, spawns.localScale.y, spawns.localScale.z);
        // kickOffs.localScale = new Vector3(-1, kickOffs.localScale.y, kickOffs.localScale.z);
        spawns.rotation = Quaternion.Euler(0, 180, 0);
        kickOffs.rotation = Quaternion.Euler(0, 180, 0);
        GameManager.MatchType currentMatchType = GameManager.Instance.currentMathType;
        if (currentMatchType != GameManager.MatchType.Training&&currentMatchType!=GameManager.MatchType.TrainingWithEnemy) {
            squadAway = SpawnPlayers(GameManager.Instance.currentMatch.countryAway,goalAway);
        }
        else if(currentMatchType==GameManager.MatchType.Training){
            squadAway=SpawnOpponent(GameManager.Instance.currentMatch.countryAway,goalAway,false);
        }
        else {
            squadAway=SpawnOpponent(GameManager.Instance.currentMatch.countryAway,goalAway,true);
        }

        SetupControlSchemes();
        
        SubscribeGlobalInputs();
                
        GameInterface.Interface.EventSystem.Subscribe<PlayStyleShowEvent>(OnPlayStyleShowEvent);
        
        GameInterface.Interface.EventSystem.Subscribe<SwitchControlEvent>(OnSwitchControl);
    }

    public void Update() {
        float currentTime = Time.time;
        if (currentTime - timeSinceLastCacheRefresh > DURATION_WEIGHT_CACHE)
        {
            timeSinceLastCacheRefresh = currentTime;
            SetOnDutyWeights();
        }
    }

    private void SetOnDutyWeights() {
        List<List<Player>> squads = new List<List<Player>> { squadAway, squadHome };

        foreach (var squad in squads)
        {
            List<Player> cpuPlayers = squad
                .Where(p => p.controlScheme == Player.ControlScheme.CPU &&
                            p.role != Player.Role.GOALIE)
                .ToList();

            // 按 spawn_position 到球的距离排序
            cpuPlayers.Sort((p1, p2) =>
                (p1.spawnPosition -(Vector2) ball.transform.position).sqrMagnitude
                .CompareTo((p2.spawnPosition - (Vector2) ball.transform.position).sqrMagnitude)
            );
            
            for (int i = 0; i < cpuPlayers.Count; i++)
            {
                cpuPlayers[i].weightOnDutySteering = 1f - Ease((float)i / 10f, 0.1f);
            }
        }
    }
    float Ease(float x, float bias)
    {
        return Mathf.Pow(x, bias);
    }

    private void ResetControlSchemes() {
        foreach (var player in squadHome) {
            player.SetControlScheme(Player.ControlScheme.CPU);
        }
        foreach (var player in squadAway) {
            player.SetControlScheme(Player.ControlScheme.CPU);
        }
    }
    private void SubscribeGlobalInputs()
    {
        GameInput.Instance.OnSwapAction += PlayerOnOnSwap;
        GameInput.Instance.OnShootAction += OnShootInput;
        GameInput.Instance.OnShortPassAction += OnPassInput;
        GameInput.Instance.OnLongPassAction += OnPassInput;
        GameInput.Instance.OnIncesivePassAction += OnPassInput;
        GameInput.Instance.OnShootCancelAction+= OnShootCancel;

    }

    private void OnPlayStyleShowEvent(PlayStyleShowEvent obj) {
        if (playersById.TryGetValue(obj.playerId, out var player))
        {   
            player.ShowPlayStyle(obj.sprite);
        }
    }

    private void OnShootCancel(object sender, EventArgs e) {
        currentControlPlayer?.currentState.OnShootCancel();
    }

    private void OnPassInput(object sender, EventArgs e) {
        var passType = GameInput.Instance.LocalPlayerInputType;
        currentControlPlayer?.currentState.OnPass(passType);
    }


    private void OnDestroy() {
        GameInput.Instance.OnSwapAction -= PlayerOnOnSwap;
        GameInput.Instance.OnShootAction -= OnShootInput;
        GameInput.Instance.OnShortPassAction -= OnPassInput;
        GameInput.Instance.OnLongPassAction -= OnPassInput;
        GameInput.Instance.OnIncesivePassAction -= OnPassInput;
        GameInput.Instance.OnShootCancelAction+= OnShootCancel;
    }
    public void SetupControlSchemes()
    {
        
        //之后修改为  GameManager或者本身控制   public Dictionary<string, Player.ControlScheme> userControlMap =
        // new Dictionary<string, Player.ControlScheme>();
        ResetControlSchemes();
        switch (GameManager.Instance.currentMathType) {
            default:
            case GameManager.MatchType.Single:
                currentTeam = squadHome;
                // currentControlPlayer = currentTeam[currentTeam.Count - 1];
                currentControlPlayer = currentTeam[^1];
                currentControlPlayer.SetControlScheme(Player.ControlScheme.P1);
                break;
            case GameManager.MatchType.Coop:
                break;
            case GameManager.MatchType.Versus:
                break;
        }
    }



    public List<Player> SpawnPlayers(string country, Goal ownGoal)
    {
        List<Player> players = new List<Player>();
        List<PlayerResource> playerResources = DataLoader.Instance.GetSquad(country);

        Transform kickoffParent = null;
        int startIndex = 0;

        switch (GameManager.Instance.currentMathType)
        {
            case GameManager.MatchType.Training:
                kickoffParent = training.transform;
                startIndex = 4;  
                break;
            case GameManager.MatchType.TrainingWithEnemy:
                kickoffParent = training.transform;
                startIndex = 5;  
                break;

            default: 
                kickoffParent = kickOffs.transform;
                startIndex = 0; 
                break;
        }
        
        for (int i = startIndex; i < playerResources.Count; i++)
        {
            Vector2 playerPosition = spawns.GetChild(i).position;
            Goal targetGoal = (ownGoal == goalAway) ? goalHome : goalAway;
            PlayerResource playerData = playerResources[i];

            Vector2 kickoffPosition = (i > 3)
                ? (Vector2)kickoffParent.GetChild(i - 4).position
                : playerPosition;

            Player player = SpawnPlayer(
                playerPosition,
                kickoffPosition,
                ownGoal,
                targetGoal,
                playerData,
                country
            );

            players.Add(player);
        }

        return players;
    }

    private List<Player> SpawnOpponent(string country, Goal ownGoal,bool withopponent) {
        List<Player> players=new List<Player>();
        List<PlayerResource> playerResources = DataLoader.Instance.GetSquad(country);
        for (int i = 0; i < (withopponent ? 2 : 1); i++) {
            Vector2 opponentGKPosition = spawns.GetChild(i).position;
            Goal targetGoal = (ownGoal == goalAway) ? goalHome : goalAway;
            PlayerResource playerData = playerResources[i];
            Vector2 kickoffPosition = opponentGKPosition;
            Player player = SpawnPlayer(
                opponentGKPosition,
                kickoffPosition,
                ownGoal,
                targetGoal,
                playerData,
                country
            );
            players.Add(player);
        }
        return players;
    }


    private void OnShootInput(object sender, EventArgs e)
    {   
        currentControlPlayer?.currentState?.OnShoot();
    }
    public Player SpawnPlayer(
        Vector2 playerPosition,
        Vector2 kickoffPosition,
        Goal ownGoal,
        Goal targetGoal,
        PlayerResource playerData,
        string country)
    {
        Player player = Instantiate(playerPrefab,transform);
        player.Initialize(nextPlayerId,playerPosition, kickoffPosition, ball, ownGoal, targetGoal, playerData, country);
        playersById[nextPlayerId]=player;
        nextPlayerId++;
        return player;
    }

    private void PlayerOnOnSwap(object sender, EventArgs e)
    {
        if (currentControlPlayer.HasBall()) return;
        Player closestCpuToBall = null;
        float closestDist = float.MaxValue;

        foreach (var p in currentTeam)
        {
            if (p.controlScheme != Player.ControlScheme.CPU || 
                p.role == Player.Role.GOALIE ||
                p == currentControlPlayer)
                continue;
            float dist = (p.transform.position - ball.transform.position).sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closestCpuToBall = p;
            }
        }

        if (closestCpuToBall == null) return;

        float senderDist = (currentControlPlayer.transform.position - ball.transform.position).sqrMagnitude;

        if (closestDist < senderDist) {
            SwitchControlEvent switchControlEvent = new SwitchControlEvent(
                currentControlPlayer.playerId,
                closestCpuToBall.playerId,
                currentControlPlayer.controlScheme
                );
            GameInterface.Interface.EventSystem.Publish(switchControlEvent);
        }
    }
    
    private void OnSwitchControl(SwitchControlEvent e)
    {
        if (!playersById.TryGetValue(e.FromPlayerId, out var fromPlayer)) return;
        if (!playersById.TryGetValue(e.ToPlayerId, out var toPlayer)) return;
        
        fromPlayer.SetControlScheme(Player.ControlScheme.CPU);
        toPlayer.SetControlScheme(e.NewControlScheme);
        currentControlPlayer = toPlayer;
    }

}
