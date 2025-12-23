using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [HideInInspector]public IReadOnlyList<Player> currentTeam;
    [HideInInspector]public IReadOnlyList<Player> opponentTeam;
    [HideInInspector]public Player currentControlPlayer;
    [HideInInspector]public Ball ball = null;
    
    public enum PlayerType
    {
        Local,
        Remote
    }
    /// <summary>
    /// 玩家类型（本地、远程）
    /// </summary>
    public PlayerType playerType;
    public bool IsLocal => playerType is PlayerType.Local;
    /// <summary>
    /// 玩家id
    /// </summary>
    public int entityId;
    /// <summary>
    /// 主客队
    /// </summary>
    public bool isHome;
    public Player.ControlScheme controlScheme;
    private void Start() {
        ball = PlayerManager.Instance.ball;
    }
    private void OnEnable()
    {
        GameInterface.Interface.EventSystem.Subscribe<OnSquadsReadyEvent>(OnTeamsReady);
        GameInterface.Interface.EventSystem.Subscribe<PlayerBecomesCarrierEvent>(OnPlayerBecomesCarrier);
    }

    private void OnPlayerBecomesCarrier(PlayerBecomesCarrierEvent obj) {
        Player newPlayer = PlayerManager.Instance.GetPlayerById(obj.playerId);
        if (newPlayer == null)
            return;
        if (currentControlPlayer != null && currentControlPlayer.isHome == newPlayer.isHome) {
            //如果不是一个队的，由那个队的enttiy去publish
            SwitchControlTo(newPlayer);
        }
    }
    private void SwitchControlTo(Player newPlayer)
    {
        if (currentControlPlayer == newPlayer)
            return;
        int oldId = currentControlPlayer != null ? currentControlPlayer.playerId : -1;
            GameInterface.Interface.EventSystem.Publish(
                new OnControlSwitchEvent(oldId, newPlayer.playerId, controlScheme)
            );
            currentControlPlayer = newPlayer;
    }
    


    public void OnTeamsReady(OnSquadsReadyEvent e) {
        if(currentTeam==null) currentTeam = PlayerManager.Instance.GetSquad(isHome);
        if(opponentTeam==null) opponentTeam = PlayerManager.Instance.GetSquad(!isHome);
        if (IsLocal) {
            initializeControlScheme();
            SubscribeGlobalInputs();
        }
    }

    private void initializeControlScheme() {
        switch (GameInterface.Interface.GameManager.currentGameMode) {
            default:
            case GameManager.GameMode.Single:
            case GameManager.GameMode.Versus:
                currentControlPlayer = currentTeam[^1];
                break;

            case GameManager.GameMode.Coop:
                currentControlPlayer = controlScheme == Player.ControlScheme.P1 
                    ? currentTeam[^1] 
                    : currentTeam[^2];
                break;
        }
        GameInterface.Interface.EventSystem.Publish(
            new OnControlSwitchEvent(-1, currentControlPlayer.playerId, controlScheme)
        );
        
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
    public void OnDestroy() {
        GameInterface.Interface.EventSystem.Unsubscribe<PlayerBecomesCarrierEvent>(OnPlayerBecomesCarrier);
        GameInterface.Interface.EventSystem.Unsubscribe<OnSquadsReadyEvent>(OnTeamsReady);
        GameInput.Instance.OnSwapAction -= PlayerOnOnSwap;
        GameInput.Instance.OnShootAction -= OnShootInput;
        GameInput.Instance.OnShortPassAction -= OnPassInput;
        GameInput.Instance.OnLongPassAction -= OnPassInput;
        GameInput.Instance.OnIncesivePassAction -= OnPassInput;
        GameInput.Instance.OnShootCancelAction+= OnShootCancel;
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
            SwitchControlTo(closestCpuToBall);
        }
    }
    private void OnShootInput(object sender, EventArgs e)
    {   
        currentControlPlayer?.currentState?.OnShoot();
    }
    private void OnPassInput(object sender, EventArgs e) {
        var passType = GameInput.Instance.LocalPlayerInputType;
        currentControlPlayer?.currentState?.OnPass(passType);
        
        // kickoff signal for GameStateKickoff
        GameInterface.Interface.EventSystem.Publish(new EntityForGameKickoffEvent(entityId));
    }
    private void OnShootCancel(object sender, EventArgs e) {
        currentControlPlayer?.currentState?.OnShootCancel();
    }

}
