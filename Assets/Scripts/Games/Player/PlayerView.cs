using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour {
    

    [Header("References")]
    public ControlScriptObject controlSchemeSO;
    public Texture2D teamPaletteTex;
    public Texture2D skinPaletteTex;
    [SerializeField] private PlayerStyleSpriteFactory playerStyleSpriteFactory=new PlayerStyleSpriteFactory();
    [Space]
    [Header("Settings")]
    public string fullName;
    public string country;
    public float power;
    public float speed;
    public Role role = Role.MIDFIELD;
    [HideInInspector] public ControlScheme controlScheme = ControlScheme.CPU;
    [HideInInspector] public Vector2 KickoffPosition;
    [HideInInspector] public Goal ownGoal;
    [HideInInspector] private AIBehaviorFactory aiBehaviorFactory=new AIBehaviorFactory();
    private AIBehavior currentAIBehavior;
    [HideInInspector]public Goal targetGoal;
    [HideInInspector]public BallView ballView;
    [HideInInspector] public SkinColor skinColor;

    [Space]
    [Header("Components")]
    [SerializeField] private SpriteRenderer playStyleRenderer;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private SpriteRenderer controlSprite;
    [SerializeField] private Animator animator;
    [SerializeField] private TriggerDetection opponentDetectionArea;
    [SerializeField] public TriggerDetection tackleAcceptArea;
    [SerializeField] private Collider2D tackleEmitterArea;
    [SerializeField] private Collider2D goalieHandsArea;
    [SerializeField] private ParticleSystem runParticles;
    [SerializeField] private Transform FlipComponent;
    [SerializeField] private TriggerDetection ballDetectionArea;
    [HideInInspector]public float height=0f;
    [HideInInspector]public float heightVelocity=0f;
    public enum State {
        MOVING,
        TACKLING,
        RECOVERING,
        PREPPING_SHOT,
        SHOOTING,
        PASSING,
        VOLLEY_KICK_OR_HEADER,
        BICYCLE_KICK,
        CHEST_CONTROL,
        HURT,
        DIVING,
        CELEBRATING,
        MOURNING,
        RESETING
    }





    public enum SkinColor {
        LIGHT,
        MEDIUM,
        DARK
    }
    private Vector3 lastInteractDir;
    private const float BALL_CONTROL_HEIGHT_MAX = 10f;
    private float originalPlayerSpriteY;
    private float originalControlSpriteY;
    [HideInInspector]public Vector2 spawnPosition;

    [HideInInspector]public int playerId;
    private Coroutine traitRoutine;
    [HideInInspector] public float weightOnDutySteering;
    [HideInInspector]public List<PlayerView> opponentListNearby = new List<PlayerView>();
    [HideInInspector] public bool isHome;

    public PlayerSim playerSim;


    public void InjectSim(PlayerSim playerSim) {
        this.playerSim=playerSim;
    }
    private void Awake() {
        // 先检查自己是否还活着
        if (this == null || gameObject == null) return;
    
        animator = GetComponentInChildren<Animator>(true); // true = 包含 inactive 的子对象
    
        if (animator == null) {
            Debug.LogError($"[{gameObject.name}] Animator not found!", this);
        }
    }

    private void Start() {
         SetControlSprite();
         setShaderProperties();
         tackleEmitterArea.enabled = false;
         spawnPosition=transform.position;
         Vector2 initialPosition=country==GameSceneBootstrap.Instance.MatchController.countryHome?KickoffPosition:spawnPosition;

         transform.position=initialPosition;
         
         //充当player reseting state
         Flip(playerSim.HeadingRight);
         
         originalPlayerSpriteY = playerSprite.transform.localPosition.y;
         originalControlSpriteY = controlSprite.transform.localPosition.y;

    }
    public bool HasOpponentsNearby() {
        return opponentListNearby.Find(p => p.country != country);
    }

    private void setShaderProperties()
    {
        if (playerSprite == null || teamPaletteTex == null || skinPaletteTex == null)
            return;
        Material mat = playerSprite.material;
        mat.SetTexture("_TeamPalette", teamPaletteTex);
        mat.SetVector("_TeamPalette_TexelSize", new Vector4(
            1f / teamPaletteTex.width,  // x
            1f / teamPaletteTex.height, // y
            teamPaletteTex.width,       // z = 列数（Shader用）
            teamPaletteTex.height       // w = 行数（Shader用）
        ));
        
        mat.SetTexture("_SkinPalette", skinPaletteTex);
        mat.SetVector("_SkinPalette_TexelSize", new Vector4(
            1f / skinPaletteTex.width,
            1f / skinPaletteTex.height,
            skinPaletteTex.width,
            skinPaletteTex.height
        ));
        
        mat.SetInt("_SkinColor", Mathf.Clamp((int)skinColor, 0, skinPaletteTex.height - 1));

        var countries = DataLoader.Instance.GetCountries();
        int teamIndex = countries.IndexOf(country);
        teamIndex = Mathf.Clamp(teamIndex, 0, teamPaletteTex.height - 1);
        mat.SetInt("_TeamColor", teamIndex);
    }



    private void SetControlSprite() {
        controlSprite.sprite = controlSchemeSO.GetSprite(controlScheme);
    }
    [HideInInspector]public int lastConsumedFrame;
    Vector2 prevPos;
    Vector2 targetPos;
    float interpTimer;
    PlayerState lastState;
    public const float FRAME_DT = SimulationClock.FRAME_DT;
    private void Update() {
        
        ConsumeStateChange();
        UpdateInterpolatedTransform();
        UpdateStateView();
    }

    private void UpdateStateView() {
        switch (playerSim.playerState)
        {
            case PlayerState.RESETING:
                SetMovementAnimation();
                break;
            case PlayerState.MOVING:
                SetMovementAnimation();
                break;
        }
    }
    float prevHeight;
    float targetHeight;
    private bool prevHeading;
    private void UpdateInterpolatedTransform() {
        //逻辑帧推进
        if (playerSim.Frame != lastConsumedFrame)
        {
            prevPos = targetPos;
            targetPos = playerSim.Position.ToVector2();
            prevHeight = targetHeight;
            targetHeight = playerSim.Height.ToFloat();
            interpTimer = 0f;
            lastConsumedFrame = playerSim.Frame;
        }
        interpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(interpTimer / FRAME_DT);
        transform.position = Vector2.Lerp(prevPos, targetPos, t);

        float height = Mathf.Lerp(prevHeight, targetHeight, t);
        playerSprite.transform.localPosition =new Vector3(0f,height+originalPlayerSpriteY,0f);
        controlSprite.transform.localPosition =new Vector3(0f,height+originalControlSpriteY,0f);
        
        FlipSprite();
    }
    /// <summary>
    /// 仿真阶段（Deterministic Simulation）”内部修改 Unity 对象，都必须通过 Command / Invoker 隔离。否则破坏确定性
    /// 轮询 View Pull Model，纯表现层逻辑，不影响 Simulation，是可以直接调用 Unity View 的
    /// 但是View 的行为会反向影响 Simulation。Unity 碰撞触发回写 Simulation 需要CommandBuffer
    /// </summary>
    private void ConsumeStateChange() {
        if (playerSim.playerState != lastState)
        {
            OnStateExit(lastState);
            OnStateEnter(playerSim.playerState);
            lastState = playerSim.playerState;
        }
    }

    private void OnStateExit(PlayerState state) {
        switch (state) {
            case PlayerState.PREPPING_SHOT:
                CameraManager.Instance.PowerShotZoom(false);
                break;
            case PlayerState.CELEBRATING:
                break;
        }
    }

    public void OnStateEnter(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.BICYCLE_KICK:
                animator.Play("bicycle_kick");
                break;
            case PlayerState.CELEBRATING:
                animator.Play("celebrate");
                break;
            case PlayerState.CHEST_CONTROL:
                animator.Play("chest_control");
                break;
            case PlayerState.DIVING:
                if (playerSim.currentState.diveDirY > 0) {
                    animator.Play("dive_up");
                }
                else {
                    animator.Play("dive_down");
                }
                break;
            case PlayerState.HURT:
                animator.Play("hurt");
                break;
            case PlayerState.MOURNING:
                animator.Play("mourn");
                break;
            case PlayerState.RESETING:
                animator.Play("movement");
                break;
            case PlayerState.MOVING:
                animator.Play("movement");
                break;
            case PlayerState.PASSING:
                animator.Play("kick");
                GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(playerId,PlayerState.PASSING));
                break;
            case PlayerState.PREPPING_SHOT:
                animator.Play("pre_kick");
                CameraManager.Instance.PowerShotZoom(true);
                break;
            case PlayerState.RECOVERING:
                animator.Play("recover");
                break;
            case PlayerState.SHOOTING:
                if (playerSim.currentState.stateData.IsInstant) {
                    animator.Play("instant_kick");
                }
                else {
                    animator.Play("kick");
                }
                break;
            case PlayerState.TACKLING:
                animator.Play("tackle");
                break;
            case PlayerState.VOLLEY_KICK:
                animator.Play("volley_kick");
                break;
            case PlayerState.HEADER:
                animator.Play("header");
                break;
        }

    }
    public void FlipSprite() {
        bool currentHeading = playerSim.HeadingRight;
        
        if (currentHeading == prevHeading)
            return;

        Flip(currentHeading);
        prevHeading = currentHeading;
    }
    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");
    private float runThreshold = 45f;
    public void SetMovementAnimation() {
        float speed = playerSim.Velocity.magnitude.ToFloat();
        animator.SetFloat(SpeedHash, speed);
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        if (!info.IsName("movement"))
        {
            if (runParticles.isPlaying)
                runParticles.Stop();
            return;
        }
        if (speed >= runThreshold)
        {
            if (!runParticles.isPlaying)
                runParticles.Play();
        }
        else
        {
            if (runParticles.isPlaying)
                runParticles.Stop();
        }
    }
    public void Flip(bool faceRight) {
        float yRotation = faceRight ? 0f : 180f;
        Vector3 rotator=new Vector3(transform.rotation.eulerAngles.x, yRotation, transform.rotation.eulerAngles.z);
        FlipComponent.transform.rotation=Quaternion.Euler(rotator);
        if (playerSim._ballSim.BallCarrierId==playerId) {
            CameraFollowObject.Instance.CallTurn(faceRight);
        }
    }



    public bool HasBall() {
        return ballView.carrier == this;
    }
    public void Initialize(
        int playerid,
        Vector2 contextPosition,
        Vector2 contextKickoffPosition,
        BallView contextBallView,
        Goal contextOwnGoal,
        Goal contextTargetGoal,
        PlayerResource contextPlayerData,
        string contextCountry,bool contextIsHome)
    {   
        playerId=playerid;
        playStyleRenderer.enabled=false;
        transform.position = contextPosition;
        KickoffPosition = contextKickoffPosition;
        ballView = contextBallView;
        ownGoal = contextOwnGoal;
        targetGoal = contextTargetGoal;

        speed = contextPlayerData.speed;
        power = contextPlayerData.power;
        role = contextPlayerData.role;
        skinColor = contextPlayerData.skin;
        fullName = contextPlayerData.fullName;
        SetControlScheme(ControlScheme.CPU);
        
        country = contextCountry;
        isHome = contextIsHome;
        
    }
    

    public void SetControlScheme(ControlScheme newScheme) {
        if (controlScheme == newScheme) return;
        controlScheme = newScheme;
        SetControlTexture();
    }

    public void SetControlTexture() {
        controlSprite.sprite=controlSchemeSO.GetSprite(controlScheme);
    }

    public void ShowPlayStyle(PlayerState playerState) {
        
        playStyleRenderer.sprite = playerStyleSpriteFactory.GetPlayerSytleSprite(playerState);
        if (traitRoutine != null)
            StopCoroutine(traitRoutine);

        traitRoutine = StartCoroutine(ShowTraitRoutine());
    }
    private IEnumerator ShowTraitRoutine()
    {   
        playStyleRenderer.enabled = true;
        yield return new WaitForSeconds(3f);
        playStyleRenderer.enabled = false;
    }
    
}