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
    public Rigidbody2D rb;
    [SerializeField] private TriggerDetection ballDetectionArea;
    [HideInInspector]public float height=0f;
    [HideInInspector]public float heightVelocity=0f;
    [HideInInspector]public float GRAVITY = 6f;
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
    [HideInInspector]public bool headingRight = true;


    private const float BALL_CONTROL_HEIGHT_MAX = 10f;
    private float originalPlayerSpriteY;
    private float originalControlSpriteY;
    [HideInInspector]public Vector2 spawnPosition;

    [HideInInspector]public int playerId;
    private Coroutine traitRoutine;
    [HideInInspector] public float weightOnDutySteering;
    [HideInInspector]public List<PlayerView> opponentListNearby = new List<PlayerView>();
    [HideInInspector] public bool isHome;

    private PlayerSim playerSim;
    public void OnEnable() {
        GameInterface.Interface.EventSystem.Subscribe<OnGameOverEvent>(OnGameOver);

    }
    public void OnDestroy() {
        GameInterface.Interface.EventSystem.Unsubscribe<OnGameOverEvent>(OnGameOver);

        ballDetectionArea.OnStay-= BallDetectionAreaOnOnStay;
        opponentDetectionArea.OnTriggered-= OpponentDetectionAreaOnOnTriggered;
        opponentDetectionArea.OnTriggerExit-= OpponentDetectionAreaOnOnTriggerExit;
        tackleAcceptArea.OnTriggered-= TackleAcceptAreaOnOnTriggered;
    }

    public void InjectSim(PlayerSim playerSim) {
        this.playerSim=playerSim;
    }

    
    private void OnGameOver(OnGameOverEvent obj) {
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
         SetupAIBehavior();
         setShaderProperties();
         if (role == Role.GOALIE) {
             goalieHandsArea.enabled = true;
             rb.constraints =
                 RigidbodyConstraints2D.FreezePositionX |
                 RigidbodyConstraints2D.FreezeRotation;
         }
         else {
             goalieHandsArea.enabled = false;
         }
         tackleEmitterArea.enabled = false;
         spawnPosition=transform.position;
         Vector2 initialPosition=country==GameInterface.Interface.GameManager.MatchController.currentMatch.countryHome?KickoffPosition:spawnPosition;


         
         //充当player reseting state
         FaceTowardsTargetGoal();
         Flip(headingRight);
         
         ballDetectionArea.OnStay+= BallDetectionAreaOnOnStay;
         opponentDetectionArea.OnTriggered+= OpponentDetectionAreaOnOnTriggered;
         opponentDetectionArea.OnTriggerExit+= OpponentDetectionAreaOnOnTriggerExit;
         tackleAcceptArea.OnTriggered+= TackleAcceptAreaOnOnTriggered;
         
         originalPlayerSpriteY = playerSprite.transform.localPosition.y;
         originalControlSpriteY = controlSprite.transform.localPosition.y;

    }



    private void TackleAcceptAreaOnOnTriggered(Collider2D obj) {
        PlayerView p = obj.GetComponentInParent<PlayerView>();
        if (p != null)
        {
            TakeTackleHit(p.rb.velocity);
        }
    }

    private void TakeTackleHit(Vector2 emitterVelocity) {
        if (!HasBall()) return;
    }

    private void OpponentDetectionAreaOnOnTriggerExit(Collider2D obj) {
        PlayerView p = obj.GetComponentInParent<PlayerView>();
        if (p != null)
            opponentListNearby.Remove(p);
    }

    private void OpponentDetectionAreaOnOnTriggered(Collider2D obj) {
        PlayerView p = obj.GetComponentInParent<PlayerView>();
        if (p != null)
            opponentListNearby.Add(p);
    }
    public bool HasOpponentsNearby() {
        return opponentListNearby.Find(p => p.country != country);
    }

    private void BallDetectionAreaOnOnStay(Collider2D obj) {

        if (obj.CompareTag("PlayerDetectArea")) {
        }

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

    private void SetupAIBehavior() {
        currentAIBehavior = aiBehaviorFactory.GetFreshAIBehavior(role);
        currentAIBehavior.Setup(this, ballView, opponentDetectionArea);
    }
    [HideInInspector]public int lastConsumedFrame;
    Vector2 prevPos;
    Vector2 targetPos;
    float interpTimer;
    PlayerState lastState;
    public const float FRAME_DT = 1f / 30f;
    private void Update() {
        
        ConsumeStateChange();
        UpdateInterpolatedTransform();
        UpdateStateView();
    }

    private void UpdateStateView() {
        switch (playerSim.CurrentState)
        {
            case PlayerState.RESETING:
                SetMovementAnimation();
                break;
            case PlayerState.MOVING:
                SetMovementAnimation();
                break;
        }
    }

    private void UpdateInterpolatedTransform() {
        //逻辑帧推进
        if (playerSim.Frame != lastConsumedFrame)
        {
            prevPos = targetPos;
            targetPos = playerSim.Position;
            interpTimer = 0f;
            lastConsumedFrame = playerSim.Frame;
        }
        interpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(interpTimer / FRAME_DT);
        transform.position = Vector2.Lerp(prevPos, targetPos, t);
        FlipSprite();
    }
    /// <summary>
    /// 仿真阶段（Deterministic Simulation）”内部修改 Unity 对象，都必须通过 Command / Invoker 隔离。否则破坏确定性
    /// 轮询 View Pull Model，纯表现层逻辑，不影响 Simulation，是可以直接调用 Unity View 的
    /// 但是View 的行为会反向影响 Simulation。Unity 碰撞触发回写 Simulation 需要CommandBuffer
    /// </summary>
    private void ConsumeStateChange() {
        if (playerSim.CurrentState != lastState)
        {
            OnStateExit(playerSim.CurrentState);
            OnStateEnter(playerSim.CurrentState);
            lastState = playerSim.CurrentState;
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
                break;
            case PlayerState.CELEBRATING:
                break;
            case PlayerState.CHEST_CONTROL:
                break;
            case PlayerState.DIVING:
                break;
            case PlayerState.HURT:
                break;
            case PlayerState.MOURNING:
                break;
            case PlayerState.RESETING:
            case PlayerState.MOVING:
                animator.Play("movement");
                break;
            case PlayerState.PASSING:
                break;
            case PlayerState.PREPPING_SHOT:
                animator.Play("pre_kick");
                CameraManager.Instance.PowerShotZoom(true);
                break;
            case PlayerState.RECOVERING:
                animator.Play("recover");
                break;
            case PlayerState.SHOOTING:
                if (playerSim.CurrentSimState.stateData.IsInstant) {
                    animator.Play("instant_kick");
                }
                else {
                    animator.Play("kick");
                }
                break;
            case PlayerState.TACKLING:
                break;
            case PlayerState.VOLLEY_KICK:
                break;
            case PlayerState.HEADER:
                break;
        }

    }
    public void FlipSprite() {
        float flipX = playerSim.Velocity.x;
        if (Mathf.Abs(flipX) < 0.001f) 
            return;
        if (flipX > 0 && !headingRight)
        {
            Flip(true);
        }
        else if (flipX < 0 && headingRight)
        {
            Flip(false);
        }
    }
    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");
    private float runThreshold = 45f;
    public void SetMovementAnimation() {
        float speed = playerSim.Velocity.magnitude;
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

    private void ApplyHeight()
    {
        Vector3 playerPos = playerSprite.transform.localPosition;
        Vector3 controlPos = controlSprite.transform.localPosition;
        float dt = Time.deltaTime;
        if (height > 0f)
        {
            heightVelocity -= GRAVITY*dt;
            height += heightVelocity;

            if (height <= 0f)
            {
                height = 0f;
                playerPos.y = originalPlayerSpriteY;
                controlPos.y = originalControlSpriteY;
            }
            else
            {
                playerPos.y = originalPlayerSpriteY + height;
                controlPos.y = originalControlSpriteY + height;
            }

            playerSprite.transform.localPosition = playerPos;
            controlSprite.transform.localPosition = controlPos;
        }
    }





    public void Flip(bool faceRight) {
        headingRight = faceRight;
        float yRotation = faceRight ? 0f : 180f;
        Vector3 rotator=new Vector3(transform.rotation.eulerAngles.x, yRotation, transform.rotation.eulerAngles.x);
        FlipComponent.transform.rotation=Quaternion.Euler(rotator);
        if (HasBall()) {
            CameraFollowObject.Instance.CallTurn();
        }

    }



    public bool HasBall() {
        return ballView.carrier == this;
    }

    public bool IsFacingTargetGoal() {

        float directionToTarget = targetGoal.transform.position.x - transform.position.x;
        return (headingRight && directionToTarget > 0) || (!headingRight && directionToTarget < 0);
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

        // Godot: Vector2.LEFT if target_goal.position.x < position.x else Vector2.RIGHT
        if (targetGoal.transform.position.x < transform.position.x)
            headingRight = false;
        else
            headingRight = true;

        country = contextCountry;
        isHome = contextIsHome;
        
    }
    
    public void FaceTowardsTargetGoal()
    {
        if (!IsFacingTargetGoal())
        {
            headingRight =!headingRight;
            Flip(headingRight);
        }
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

    public void OnAnimationComplete() {
    }
}