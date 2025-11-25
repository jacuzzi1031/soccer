using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ActorsContainer : MonoBehaviour
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
    private bool isCheckForKickOffReadiness = false;
    private Player[] squadHome;
    private Player[] squadAway;

    private void Start() {
        squadHome = SpawnPlayers(GameManager.Instance.currentMatch.countryHome,goalHome);
        goalHome.Initialize(GameManager.Instance.currentMatch.countryHome);
        spawns.localScale = new Vector3(-1, spawns.localScale.y, spawns.localScale.z);
        kickOffs.localScale = new Vector3(-1, kickOffs.localScale.y, kickOffs.localScale.z);
        squadAway = SpawnPlayers(GameManager.Instance.currentMatch.countryAway,goalAway);
        goalAway.Initialize(GameManager.Instance.currentMatch.countryAway);
        SetupControlSchemes();
    }

    public void SetupControlSchemes() {
        ResetControlSchemes();
        string p1Country=GameManager.Instance.playerSetup[0];
        if (GameManager.Instance.IsCoop())
        {
            Player[] playerSquad = squadHome[0].country == p1Country ? squadHome : squadAway;
            playerSquad[4].SetControlScheme(Player.ControlScheme.P1);
            playerSquad[5].SetControlScheme(Player.ControlScheme.P2);
        }
        else if (GameManager.Instance.IsSinglePlayer())
        {
            Player[] playerSquad = squadHome[0].country == p1Country ? squadHome : squadAway;
            playerSquad[5].SetControlScheme(Player.ControlScheme.P1);
        }
        else // Versus
        {
            Player[] p1Squad = squadHome[0].country == p1Country ? squadHome : squadAway;
            Player[] p2Squad = p1Squad == squadAway ? squadHome : squadAway;

            p1Squad[5].SetControlScheme(Player.ControlScheme.P1);
            p2Squad[5].SetControlScheme(Player.ControlScheme.P2);
        }
    }

    public void ResetControlSchemes() {
        foreach (Player[] squad in new Player[][] { squadHome, squadAway })
        {
            foreach (Player player in squad)
            {
                player.SetControlScheme(Player.ControlScheme.CPU);
            }
        }
    }
    
    // IEnumerator LoadGame()
    // {
    //     var async = SceneManager.LoadSceneAsync("Game");
    //     async.allowSceneActivation = false;
    //
    //     // progress 到 0.9
    //     while (async.progress < 0.9f)
    //         yield return null;
    //
    //     // 激活场景
    //     async.allowSceneActivation = true;
    //
    //     yield return null; // 等一帧，确保场景激活完
    //
    //     FindObjectOfType<GameManager>().Init();
    // }

    public Player[] SpawnPlayers(string country, Goal ownGoal) {
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
            player.transform.SetParent(this.transform);
        }

        return players.ToArray();
    }
    
    public Player SpawnPlayer(
        Vector2 playerPosition,
        Vector2 kickoffPosition,
        Goal ownGoal,
        Goal targetGoal,
        PlayerResource playerData,
        string country)
    {
        Player player = Instantiate(playerPrefab);
        player.Initialize(playerPosition, kickoffPosition, ball, ownGoal, targetGoal, playerData, country);

        GameInput.Instance.OnSwapAction+= InstanceOnOnSwapAction;

        return player;
    }

    private void InstanceOnOnSwapAction(object sender, EventArgs e) {
        throw new NotImplementedException();
    }
}
