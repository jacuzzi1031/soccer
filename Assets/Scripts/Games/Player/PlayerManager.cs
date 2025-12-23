using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Ball ball;
    [SerializeField] private Goal goalHome;
    [SerializeField] private Goal goalAway;
    [SerializeField] private Player  playerPrefab;
    
    [Header("Components")]
    [SerializeField] private Transform spawns;
    [SerializeField] private Transform kickOffs;
    [SerializeField] private Transform training;

    private const float DURATION_WEIGHT_CACHE = 0.2f;
    private float timeSinceLastCacheRefresh = 0f;
    private List<Player> squadHome;
    private List<Player> squadAway;
    private Dictionary<int, Player> playersById = new Dictionary<int, Player>();
    public static PlayerManager Instance{get; private set;}
    private int nextPlayerId = 0;
    private bool isCheckingForKickoffReadiness=false;
    private void Awake() {
        Instance = this;
    }
    private void OnEnable() {
        GameInterface.Interface.EventSystem.Subscribe<PlayStyleShowEvent>(OnPlayStyleShowEvent);
        GameInterface.Interface.EventSystem.Subscribe<OnControlSwitchEvent>(OnPlayStyleShowEvent);
        GameInterface.Interface.EventSystem.Subscribe<OnTeamResetEvent>(OnTeamResetEvent);
    }

    private void OnDisable() {
        GameInterface.Interface.EventSystem.Unsubscribe<PlayStyleShowEvent>(OnPlayStyleShowEvent);
        GameInterface.Interface.EventSystem.Unsubscribe<OnControlSwitchEvent>(OnPlayStyleShowEvent);
        GameInterface.Interface.EventSystem.Unsubscribe<OnTeamResetEvent>(OnTeamResetEvent);
    }
    

    private void OnTeamResetEvent(OnTeamResetEvent obj) {
        isCheckingForKickoffReadiness = true;
    }

    void CheckForKickoffReadiness()
    {
        foreach (var squad in new[] { squadHome, squadAway })
        {
            foreach (Player player in squad)
            {
                if (!player.IsReadyForKickoff())
                {
                    return;
                }
            }
        }
        ResetControlSchemes();
        isCheckingForKickoffReadiness = false;
        GameInterface.Interface.EventSystem.Publish(new OnKickoffReadyEvent());
    }


    private void OnPlayStyleShowEvent(OnControlSwitchEvent e) {
        if (e.OldPlayerId != -1)
        {
            Player oldPlayer = playersById[e.OldPlayerId];
            if (oldPlayer != null)
                oldPlayer.SetControlScheme(Player.ControlScheme.CPU);
        }

        Player newPlayer = playersById[e.NewPlayerId];
        if (newPlayer != null)
            newPlayer.SetControlScheme(e.Scheme);
    }

    public void InitializeSquads() {
        squadHome = SpawnPlayers(GameInterface.Interface.GameManager.currentMatch.countryHome,goalHome,true);
        goalHome.initialize(GameInterface.Interface.GameManager.currentMatch.countryHome);
        spawns.rotation = Quaternion.Euler(0, 180, 0);
        kickOffs.rotation = Quaternion.Euler(0, 180, 0);
        GameManager.MatchType currentMatchType = GameInterface.Interface.GameManager.currentMatchType;
        if (currentMatchType != GameManager.MatchType.Training&&currentMatchType!=GameManager.MatchType.TrainingWithEnemy) {
            squadAway = SpawnPlayers(GameInterface.Interface.GameManager.currentMatch.countryAway,goalAway,false);
        }
        else if(currentMatchType==GameManager.MatchType.Training){
            squadAway=SpawnOpponent(GameInterface.Interface.GameManager.currentMatch.countryAway,goalAway,false,false);
        }
        else {
            squadAway=SpawnOpponent(GameInterface.Interface.GameManager.currentMatch.countryAway,goalAway,true,false);
        }
        goalAway.initialize(GameInterface.Interface.GameManager.currentMatch.countryAway);

        ResetControlSchemes();
    }

    public void Update() {
        float currentTime = Time.time;
        if (currentTime - timeSinceLastCacheRefresh > DURATION_WEIGHT_CACHE)
        {
            timeSinceLastCacheRefresh = currentTime;
            SetOnDutyWeights();
        }

        if (isCheckingForKickoffReadiness) {
            CheckForKickoffReadiness();
        }
    }

    public IReadOnlyList<Player> GetSquad(bool isHome) {
        return isHome?squadHome:squadAway;
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
        GameInterface.Interface.EventSystem.Publish(new OnSquadsReadyEvent());
    }
    private void OnPlayStyleShowEvent(PlayStyleShowEvent obj) {
        if (playersById.TryGetValue(obj.playerId, out var player))
        {   
            player.ShowPlayStyle(obj.sprite);
        }
    }
    public List<Player> SpawnPlayers(string country, Goal ownGoal,bool isHome)
    {
        List<Player> players = new List<Player>();
        List<PlayerResource> playerResources = DataLoader.Instance.GetSquad(country);

        Transform kickoffParent = null;
        int startIndex = 0;

        switch (GameInterface.Interface.GameManager.currentMatchType)
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
            GameManager.MatchType currentMatchType = GameInterface.Interface.GameManager.currentMatchType;
            Vector2 playerPosition;
            Vector2 kickoffPosition;
            if (currentMatchType != GameManager.MatchType.Training&&currentMatchType!=GameManager.MatchType.TrainingWithEnemy) {
                playerPosition= spawns.GetChild(i).position;
                kickoffPosition = (i > 3)
                    ? (Vector2)kickoffParent.GetChild(i - 4).position
                    : playerPosition;
            }
            else {
                playerPosition=training.GetChild(i-4).position;
                kickoffPosition=training.GetChild(i-4).position;
            }
            Goal targetGoal = (ownGoal == goalAway) ? goalHome : goalAway;
            PlayerResource playerData = playerResources[i];
            Player player = SpawnPlayer(
                playerPosition,
                kickoffPosition,
                ownGoal,
                targetGoal,
                playerData,
                country,
                isHome
            );

            players.Add(player);
        }

        return players;
    }

    private List<Player> SpawnOpponent(string country, Goal ownGoal,bool withopponent,bool isHome) {
        List<Player> players=new List<Player>();
        List<PlayerResource> playerResources = DataLoader.Instance.GetSquad(country);
        for (int i = 0; i < (withopponent ? 2 : 1); i++) {
            Vector2 spawnPosition = spawns.GetChild(i).position;
            Goal targetGoal = (ownGoal == goalAway) ? goalHome : goalAway;
            PlayerResource playerData = playerResources[i];
            Vector2 kickoffPosition = spawnPosition;
            Player player = SpawnPlayer(
                spawnPosition,
                kickoffPosition,
                ownGoal,
                targetGoal,
                playerData,
                country,isHome
            );
            players.Add(player);
        }
        return players;
    }



    public Player SpawnPlayer(
        Vector2 playerPosition,
        Vector2 kickoffPosition,
        Goal ownGoal,
        Goal targetGoal,
        PlayerResource playerData,
        string country,
        bool isHome)
    {
        Player player = Instantiate(playerPrefab,transform);
        player.Initialize(nextPlayerId,playerPosition, kickoffPosition, ball, ownGoal, targetGoal, playerData, country, isHome);
        playersById[nextPlayerId]=player;
        nextPlayerId++;
        return player;
    }

    public Player GetPlayerById(int playerId) {
        return playersById[playerId];
    }
}
