using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public BallView ballView;
    [SerializeField] private Goal goalHome;
    [SerializeField] private Goal goalAway;
    [SerializeField] private PlayerView  playerViewPrefab;
    
    [Header("Components")]
    [SerializeField] private Transform spawns;
    [SerializeField] private Transform kickOffs;
    [SerializeField] private Transform training;
    

    private List<PlayerView> squadHome;
    private List<PlayerView> squadAway;
    private Dictionary<int, PlayerView> playersById = new Dictionary<int, PlayerView>();
    public static PlayerManager Instance{get; private set;}
    private int nextPlayerId = 0;
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
        GameInterface.Interface.GameManager.PlayerSystem.OnTeamResetEvent();
    }
    private void OnPlayStyleShowEvent(OnControlSwitchEvent e) {
        if (e.OldPlayerId != -1)
        {
            PlayerView oldPlayerView = playersById[e.OldPlayerId];
            if (oldPlayerView != null)
                oldPlayerView.SetControlScheme(ControlScheme.CPU);
        }

        PlayerView newPlayerView = playersById[e.NewPlayerId];
        if (newPlayerView != null)
            newPlayerView.SetControlScheme(e.Scheme);
    }

    public void InitializeSquads(Action<List<PlayerSim>, List<PlayerSim>> onTeamsReady)
    {
        // 创建 Home 和 Away 球员并初始化 PlayerView 和 PlayerSim
        List<PlayerSim> PlayerSimsHome = SpawnPlayerViewsAndSims(GameInterface.Interface.GameManager.MatchSystem.currentMatch.countryHome, goalHome, true);
        goalHome.initialize(GameInterface.Interface.GameManager.MatchSystem.currentMatch.countryHome);
        goalAway.initialize(GameInterface.Interface.GameManager.MatchSystem.currentMatch.countryAway);

        spawns.rotation = Quaternion.Euler(0, 180, 0);
        kickOffs.rotation = Quaternion.Euler(0, 180, 0);

        List<PlayerSim> PlayerSimsAway;
        GameManager.MatchType currentMatchType = GameInterface.Interface.GameManager.currentMatchType;
        if (currentMatchType != GameManager.MatchType.Training && currentMatchType != GameManager.MatchType.TrainingWithEnemy)
        {
            PlayerSimsAway = SpawnPlayerViewsAndSims(GameInterface.Interface.GameManager.MatchSystem.currentMatch.countryAway, goalAway, false);
        }
        else if (currentMatchType == GameManager.MatchType.Training)
        {
            PlayerSimsAway=SpawnOpponentViewsAndSims(GameInterface.Interface.GameManager.MatchSystem.currentMatch.countryAway, goalAway, false, false);
        }
        else
        {
            //TrainingWithEnemy
            PlayerSimsAway=SpawnOpponentViewsAndSims(GameInterface.Interface.GameManager.MatchSystem.currentMatch.countryAway, goalAway, true, false);
        }
        // 回调 PlayerSim 列表
        onTeamsReady?.Invoke(PlayerSimsHome, PlayerSimsAway);
    }
    public IReadOnlyList<PlayerView> GetSquad(bool isHome) {
        return isHome?squadHome:squadAway;
    }


    public void ResetControlSchemes() {
        foreach (var player in squadHome) {
            player.SetControlScheme(ControlScheme.CPU);
        }
        foreach (var player in squadAway) {
            player.SetControlScheme(ControlScheme.CPU);
        }
        GameInterface.Interface.EventSystem.Publish(new OnSquadsReadyEvent());
    }
    private void OnPlayStyleShowEvent(PlayStyleShowEvent obj) {
        if (playersById.TryGetValue(obj.playerId, out var player))
        {   
            player.ShowPlayStyle(obj.sprite);
        }
    }
    public List<PlayerSim> SpawnPlayerViewsAndSims(string country, Goal ownGoal, bool isHome)
    {
        List<PlayerView> playerViews = new List<PlayerView>();
        List<PlayerResource> playerResources = DataLoader.Instance.GetSquad(country);
        List<PlayerSim> playerSims = new List<PlayerSim>();

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
            
            if (currentMatchType != GameManager.MatchType.Training && currentMatchType != GameManager.MatchType.TrainingWithEnemy)
            {
                playerPosition = spawns.GetChild(i).position;
                kickoffPosition = (i > 3)
                    ? (Vector2)kickoffParent.GetChild(i - 4).position
                    : playerPosition;
            }
            else
            {
                playerPosition = training.GetChild(i - 4).position;
                kickoffPosition = training.GetChild(i - 4).position;
            }

            Goal targetGoal = (ownGoal == goalAway) ? goalHome : goalAway;
            PlayerResource playerData = playerResources[i];
            
            PlayerSim playerSim = new PlayerSim(
                nextPlayerId,
                playerData,
                playerPosition,
                kickoffPosition,
                country,
                isHome
            );
            
            PlayerView playerView = SpawnPlayer(
                playerPosition,
                kickoffPosition,
                ownGoal,
                targetGoal,
                playerData,
                country,
                isHome,
                playerSim
            );

            playerViews.Add(playerView); 
            playerSims.Add(playerSim); 
        }
        
        if (isHome)
        {
            squadHome = playerViews;
        }
        else
        {
            squadAway = playerViews;
        }
        
        return playerSims;
    }
    private List<PlayerSim> SpawnOpponentViewsAndSims(string country, Goal ownGoal,bool withopponent,bool isHome) {

        List<PlayerView> playerViews=new List<PlayerView>();
        List<PlayerSim> playerSims = new List<PlayerSim>();
        List<PlayerResource> playerResources = DataLoader.Instance.GetSquad(country);
        for (int i = 0; i < (withopponent ? 2 : 1); i++) {
            Vector2 spawnPosition = spawns.GetChild(i).position;
            Goal targetGoal = (ownGoal == goalAway) ? goalHome : goalAway;
            PlayerResource playerData = playerResources[i];
            Vector2 kickoffPosition = spawnPosition;
            PlayerSim playerSim = new PlayerSim(
                nextPlayerId,
                playerData,
                spawnPosition,
                kickoffPosition,
                country,
                isHome
            );
            PlayerView playerView = SpawnPlayer(
                spawnPosition,
                kickoffPosition,
                ownGoal,
                targetGoal,
                playerData,
                country,isHome,
                playerSim
            );
            playerViews.Add(playerView);
            playerSims.Add(playerSim);
        }
        squadAway=playerViews;
        return playerSims;
    }



    public PlayerView SpawnPlayer(
        Vector2 playerPosition,
        Vector2 kickoffPosition,
        Goal ownGoal,
        Goal targetGoal,
        PlayerResource playerData,
        string country,
        bool isHome,
        PlayerSim playerSim)
    {
        PlayerView playerView = Instantiate(playerViewPrefab, transform);
        playerView.Initialize(nextPlayerId, playerPosition, kickoffPosition, ballView, ownGoal, targetGoal, playerData, country, isHome);
        playerView.InjectSim(playerSim);

        playersById[nextPlayerId] = playerView;
        nextPlayerId++;

        return playerView;
    }

    public PlayerView GetPlayerById(int playerId) {
        return playersById[playerId];
    }
}
