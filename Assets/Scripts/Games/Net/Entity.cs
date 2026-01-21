using System;
using System.Collections;
using System.Collections.Generic;
using GameFrameSync;
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
    public int playerId;
    /// <summary>
    /// 主客队
    /// </summary>
    public bool isHome;
    public GameFrameSync.InputType playerInputType;
    /// <summary>
    /// 当前位置（远程）
    /// </summary>
    public Vector2 playerPosition;
    /// <summary>
    /// 当前位置（本地）
    /// </summary>
    public Vector2 localPlayerPosition;
    /// <summary>
    /// 移动向量(远程)
    /// </summary>
    public Vector2 moveVector;
    /// <summary>
    /// 移动向量(本地)
    /// </summary>
    public Vector2 localMoveVector;
    /// <summary>
    /// 控制权(本地)
    /// </summary>
    public int activeUnitIndex;
    /// <summary>
    /// 行为目标(远程)
    /// </summary>
    public int commandIndex;
    
    
    public Player.ControlScheme controlScheme;
    private void Start() {
        ball = PlayerManager.Instance.ball;
    }
    private void OnEnable()
    {
        GameInterface.Interface.GameFrameSyncManager.OnFrameSync += OnFrameSync;
        GameInterface.Interface.EventSystem.Subscribe<OnSquadsReadyEvent>(OnTeamsReady);
        GameInterface.Interface.EventSystem.Subscribe<PlayerBecomesCarrierEvent>(OnPlayerBecomesCarrier);
    }
    private void OnFrameSync(List<ResFrameInputData> frameDataList)
    {
        ResFrameInputData frameInputData = frameDataList?.Find(item => item.PlayerId == playerId);
        if (frameInputData != null)
        {
            InputType inputType = frameInputData.InputType;
            activeUnitIndex = frameInputData.ActiveUnitIndex;
            commandIndex = frameInputData.CommandIndex;
            if (inputType != playerInputType) 
            {
                Invoker.Instance.DelegateList.Add(() =>
                {
                    // OnPlayerInputChanged?.Invoke(this, new OnPlayerInputChangedEventArgs
                    //     { playerId = playerId, inputType = inputType });
                });
            }
            playerInputType = inputType;
            
            if (frameInputData.Position != null&&!IsLocal)
            {
                playerPosition = new Vector3(frameInputData.Position.X, 0,
                    frameInputData.Position.Y);
            }

            if (frameInputData.MoveVector != null&&!IsLocal)
            {
                moveVector = new Vector2(frameInputData.MoveVector.X, frameInputData.MoveVector.Y);
            }
        }
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
        GameInterface.Interface.EventSystem.Publish(new EntityForGameKickoffEvent(playerId));
    }
    private void OnShootCancel(object sender, EventArgs e) {
        currentControlPlayer?.currentState?.OnShootCancel();
    }


}
