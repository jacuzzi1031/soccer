using System;
using System.Collections;
using System.Collections.Generic;
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

    private const float DURATION_WEIGHT_CACHE = 0.2f;
    // private bool isCheckForKickOffReadiness = false;
    private List<Player> squadHome;
    private List<Player> squadAway;
    [HideInInspector]public List<Player> currentTeam;
    [HideInInspector]public List<Player> opponentTeam;
    public Player currentControlPlayer;
    
    public static PlayerManager Instance{get; private set;}

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        squadHome = SpawnPlayers(GameManager.Instance.currentMatch.countryHome,goalHome);
        spawns.localScale = new Vector3(-1, spawns.localScale.y, spawns.localScale.z);
        kickOffs.localScale = new Vector3(-1, kickOffs.localScale.y, kickOffs.localScale.z);
        squadAway = SpawnPlayers(GameManager.Instance.currentMatch.countryAway,goalAway);
        SetupControlSchemes();
        
        SubscribeGlobalInputs();
        GameInterface.Interface.EventSystem.Subscribe<PassBallToSwapPlayer>(obj =>SwapTo(obj.ToSwapPlayer));
    }
    private void SubscribeGlobalInputs()
    {
        GameInput.Instance.OnSwapAction += PlayerOnOnSwap;
        GameInput.Instance.OnShootAction += OnShootInput;
        GameInput.Instance.OnShortPassAction += OnPassInput;
        GameInput.Instance.OnLongPassAction += OnPassInput;
        GameInput.Instance.OnIncesivePassAction += OnPassInput;
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
    }
    public void SetupControlSchemes()
    {
        ResetControlSchemes();
        switch (GameManager.Instance.currentMathType) {
            default:
            case GameManager.MatchType.Single:
                currentTeam = squadHome;
                currentControlPlayer = currentTeam[5];
                currentControlPlayer.SetControlScheme(Player.ControlScheme.P1);
                break;
            case GameManager.MatchType.Coop:
                break;
            case GameManager.MatchType.Versus:
                break;
        }
    }

    private void ResetControlSchemes() {
        foreach (var player in squadHome) {
            player.SetControlScheme(Player.ControlScheme.CPU);
        }
        foreach (var player in squadAway) {
            player.SetControlScheme(Player.ControlScheme.CPU);
        }
    }

    public List<Player> SpawnPlayers(string country, Goal ownGoal) {
        List<Player> players=new List<Player>();
        List<PlayerResource> playerResources = DataLoader.Instance.GetSquad(country);
        for (int i = 0; i < playerResources.Count; i++) {
            Vector2 playerPosition = spawns.GetChild(i).position;
            Goal targetGoal = (ownGoal == goalAway) ? goalHome : goalAway;
            PlayerResource playerData = playerResources[i];
            Vector2 kickoffPosition = playerPosition;
            if (i > 3)
            {
                kickoffPosition = kickOffs.transform.GetChild(i - 4).position;
            }
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
        player.Initialize(playerPosition, kickoffPosition, ball, ownGoal, targetGoal, playerData, country);

        // GameInput.Instance.OnSwapAction+= InstanceOnOnSwapAction;
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

        if (closestDist < senderDist)
        {
            var playerControlScheme = currentControlPlayer.controlScheme;
            currentControlPlayer.SetControlScheme(Player.ControlScheme.CPU);
            closestCpuToBall.SetControlScheme(playerControlScheme);
            currentControlPlayer=closestCpuToBall;
        }
    }

    public void SwapTo(Player player) {
        var playerControlScheme = currentControlPlayer.controlScheme;
        currentControlPlayer.SetControlScheme(Player.ControlScheme.CPU);
        player.SetControlScheme(playerControlScheme);
        currentControlPlayer=player;
    }

}
