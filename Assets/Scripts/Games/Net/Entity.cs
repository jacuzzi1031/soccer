using System;
using System.Collections;
using System.Collections.Generic;
using GameFrameSync;
using UnityEngine;
using UnityEngine.Serialization;

public class Entity : MonoBehaviour
{
    [HideInInspector]public IReadOnlyList<PlayerView> currentTeam;
    [HideInInspector]public IReadOnlyList<PlayerView> opponentTeam;
    [FormerlySerializedAs("currentControlPlayer")] [HideInInspector]public PlayerView currentControlPlayerView;
    [FormerlySerializedAs("ball")] [HideInInspector]public BallView ballView = null;

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
    
    
    public ControlScheme controlScheme;
    private void Start() {
        ballView = PlayerManager.Instance.ballView;
    }
    private void OnEnable()
    {
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
                    // OnPlayerInputChanged?.Invoke(new Command{
					// frame = SimulationDriver.Instance.CurrentFrame,
					// playerId = playerId, 
					// inputType = inputType,
					// });
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
        PlayerView newPlayerView = PlayerManager.Instance.GetPlayerById(obj.playerId);
        if (newPlayerView == null)
            return;
        if (currentControlPlayerView != null && currentControlPlayerView.isHome == newPlayerView.isHome) {
            //如果不是一个队的，由那个队的enttiy去publish
            SwitchControlTo(newPlayerView);
        }
    }
    private void SwitchControlTo(PlayerView newPlayerView)
    {
        if (currentControlPlayerView == newPlayerView)
            return;
        int oldId = currentControlPlayerView != null ? currentControlPlayerView.playerId : -1;
            GameInterface.Interface.EventSystem.Publish(
                new OnControlSwitchEvent(oldId, newPlayerView.playerId, controlScheme)
            );
            currentControlPlayerView = newPlayerView;
    }
    


    public void OnTeamsReady(OnSquadsReadyEvent e) {
        if(currentTeam==null) currentTeam = PlayerManager.Instance.GetSquad(isHome);
        if(opponentTeam==null) opponentTeam = PlayerManager.Instance.GetSquad(!isHome);
        if (IsLocal) {
            initializeControlScheme();
        }
    }

    private void initializeControlScheme() {
        switch (GameInterface.Interface.GameManager.currentGameMode) {
            default:
            case GameManager.GameMode.Single:
            case GameManager.GameMode.Versus:
                currentControlPlayerView = currentTeam[^1];
                break;

            case GameManager.GameMode.Coop:
                currentControlPlayerView = controlScheme == ControlScheme.P1 
                    ? currentTeam[^1] 
                    : currentTeam[^2];
                break;
        }
        GameInterface.Interface.EventSystem.Publish(
            new OnControlSwitchEvent(-1, currentControlPlayerView.playerId, controlScheme)
        );
    }
    
    public void OnDestroy() {
        GameInterface.Interface.EventSystem.Unsubscribe<PlayerBecomesCarrierEvent>(OnPlayerBecomesCarrier);
        GameInterface.Interface.EventSystem.Unsubscribe<OnSquadsReadyEvent>(OnTeamsReady);
    }
    private void PlayerOnOnSwap(object sender, EventArgs e)
    {
        if (currentControlPlayerView.HasBall()) return;
        PlayerView closestCpuToBall = null;
        float closestDist = float.MaxValue;

        foreach (var p in currentTeam)
        {
            if (p.controlScheme != ControlScheme.CPU || 
                p.role == Role.GOALIE ||
                p == currentControlPlayerView)
                continue;
            float dist = (p.transform.position - ballView.transform.position).sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closestCpuToBall = p;
            }
        }

        if (closestCpuToBall == null) return;

        float senderDist = (currentControlPlayerView.transform.position - ballView.transform.position).sqrMagnitude;

        if (closestDist < senderDist) {
            SwitchControlTo(closestCpuToBall);
        }
    }
    private void OnShootInput(object sender, EventArgs e)
    {   
        currentControlPlayerView?.CurrentViewState?.OnShoot();
    }
    private void OnPassInput(object sender, EventArgs e) {
        var passType = GameInput.Instance.LocalPlayerInputType;
        currentControlPlayerView?.CurrentViewState?.OnPass(passType);
        
        // kickoff signal for GameStateKickoff
        GameInterface.Interface.EventSystem.Publish(new EntityForGameKickoffEvent(playerId));
    }
    private void OnShootCancel(object sender, EventArgs e) {
        currentControlPlayerView?.CurrentViewState?.OnShootCancel();
    }


}
