using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSim {
    public int playerId;
    public PlayerSimStateFactory stateFactory;
    public ControlScheme controlScheme;
    public Facing facing;
    public Vector2 playerPosition;
    public Vector2 kickoffPosition;
    public Vector2 Velocity;
    public float Height;
    public bool HeadingRight;
    public PlayerSimState CurrentSimState;
    public PlayerStateId CurrentStateId;
    public bool isHome;
    public string fullName;
    public string country;
    public float power;
    public float speed;
    public Role role = Role.MIDFIELD;
    public float weightOnDutySteering;

    public PlayerSim(int nextPlayerId,PlayerResource contextPlayerData, Vector2 contextplayerPosition, Vector2 contextkickoffPosition, string contextcountry, bool contextisHome) {
        playerId = nextPlayerId;
        playerPosition= contextplayerPosition;
        kickoffPosition= contextkickoffPosition;
        country= contextcountry;
        isHome = contextisHome;
        speed = contextPlayerData.speed;
        power = contextPlayerData.power;
        role = contextPlayerData.role;
        fullName = contextPlayerData.fullName;
        controlScheme = ControlScheme.CPU;
    }

    public void Tick()
    {
        CurrentSimState?._Update();
    }

    public void OnTakeTackleHit(Vector2 dir)
    {
        // if (!HasBall()) return; 只有carrierSnapshot
        SwitchState(PlayerStateId.HURT, PlayerStateData.Build().SetMoveDir(dir));
    }
    public void SwitchState(PlayerStateId id, PlayerStateData data = null)
    {
        CurrentSimState?.OnExit();

        CurrentStateId = id;
        CurrentSimState = stateFactory.GetFreshState(id);
        CurrentSimState.Setup(this, data ?? PlayerStateData.Build());

        CurrentSimState.OnEnter();
    }
    public bool IsReadyForKickoff() {
        return CurrentSimState != null && CurrentSimState.IsReadyForKickoff();
    }

    public void SetControlScheme(ControlScheme ContextControlScheme) {
        controlScheme = ContextControlScheme;
    }
}
