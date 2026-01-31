using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallView : MonoBehaviour
{   
    private const float DISTANCE_HIGH_PASS = 5.0f;
    private const float DURATION_TUMBLE_LOCK = 0.2f;
    private const float DURATION_PASS_LOCK = 0.2f;
    private const float KICKOFF_PASS_DISTANCE = 30f;
    private const float TUMBLE_HEIGHT_VELOCITY = 3f;
    private const float MAX_CAPTURE_HEIGHT = 25f;
    public Vector2 velocity=Vector2.zero;
    private float castDistance = 60f;
    public float height=0.0f;
    public PlayerView carrier;
    public float heightVelocity=0.0f;

    [SerializeField] public Transform trainingSpawnPosition;
    [HideInInspector] public Vector2 spawnPosition;
    
    [SerializeField] public LayerMask collideForScoreAreaLayer;
    
    
    [SerializeField] private TriggerDetection playerDetectArea;
    [SerializeField] private TriggerDetection playerProximityArea;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem shotParticles;
    [SerializeField] private Transform ballSprite;
    [SerializeField]  public Rigidbody2D rb;
    public BallStateFactory ballStateFactory=new BallStateFactory();
    [HideInInspector]public BallViewState CurrentViewState;
    public LayerMask LayerMask;
    public CircleCollider2D  colliderForWall;
    [HideInInspector]public List<PlayerView> playerListInProximityArea = new List<PlayerView>();

    public float frictionAir = 35f;
    public float frictionGround = 250f;
    private BallSim ballSim;

    private void Awake() {

        SwitchViewState(BallStateId.FREEFORM);
    }

    private void Start() {
        //OnTriggerStay2D是因为freedomState有LockDuration
        playerDetectArea.OnStay += PlayerDetectAreaOnOnEnter;
        playerProximityArea.OnTriggered+= PlayerProximityAreaOnOnTriggered;
        playerProximityArea.OnTriggerExit+= PlayerProximityAreaOnOnTriggerExit;
        GameManager.MatchType currentMathType = GameInterface.Interface.GameManager.currentMatchType;
        if (currentMathType == GameManager.MatchType.Training||currentMathType == GameManager.MatchType.TrainingWithEnemy) {
            transform.position=trainingSpawnPosition.position;
        }
        spawnPosition = transform.position;
    }
    private void OnEnable() {
        GameInterface.Interface.EventSystem.Subscribe<BallResetEvent>(OnBallReset);
        GameInterface.Interface.EventSystem.Subscribe<OnKickoffStartedEvent>(OnKickoffStarted);
    }
    private void OnDestroy() {
        SwitchViewState(BallStateId.FREEFORM);
        playerDetectArea.OnStay -= PlayerDetectAreaOnOnEnter;
        playerProximityArea.OnTriggered-= PlayerProximityAreaOnOnTriggered;
        playerProximityArea.OnTriggerExit-= PlayerProximityAreaOnOnTriggerExit;
        
        GameInterface.Interface.EventSystem.Unsubscribe<BallResetEvent>(OnBallReset);
        GameInterface.Interface.EventSystem.Unsubscribe<OnKickoffStartedEvent>(OnKickoffStarted);
    }
    private void OnKickoffStarted(OnKickoffStartedEvent obj) {
        passTo(spawnPosition + Vector2.down * KICKOFF_PASS_DISTANCE, true, null);
    }

    private void OnBallReset(BallResetEvent obj) {
        transform.position = spawnPosition;
        rb.velocity = Vector2.zero;
        height=0.0f;
        SwitchViewState(BallStateId.FREEFORM);
    }

    private void PlayerProximityAreaOnOnTriggerExit(Collider2D obj) {
        PlayerView p = obj.GetComponentInParent<PlayerView>();
        if (p != null)
            playerListInProximityArea.Remove(p);
    }

    private void PlayerProximityAreaOnOnTriggered(Collider2D obj) {
        PlayerView p = obj.GetComponentInParent<PlayerView>();
        if (p != null && !playerListInProximityArea.Contains(p))
            playerListInProximityArea.Add(p);
    }
    public int GetProximityTeammatesCount(string playerCountry) {
        return playerListInProximityArea.FindAll(p => p.country == playerCountry).Count;
    }

    private void PlayerDetectAreaOnOnEnter(Collider2D obj) {
        if (!CurrentViewState.CanCarriedBall())
            return;
        PlayerView body = obj.GetComponentInParent<PlayerView>();
        if (!body) return;

        if (body.CanCarryBall() && height < MAX_CAPTURE_HEIGHT)
        {   
            carrier = body;
            body.ControlBall();
            SwitchViewState(BallStateId.CARRIED);
        }
    }



    private void Update()
    {
        ballSprite.localPosition = height*3f*Vector3.up;
        CurrentViewState?._Update();
    }
    private void FixedUpdate()
    {
        CurrentViewState?._FixedUpdate();
    }
    
    public void SwitchViewState(BallStateId type, BallStateData data = null)
    {
        if (CurrentViewState != null)
        {
            CurrentViewState.StateTransitionRequested -= SwitchViewState;
            CurrentViewState.OnExit();
        }
        CurrentViewState = ballStateFactory.GetFreshState(type);
        CurrentViewState.Setup(this, data ?? new BallStateData(),animator,shotParticles,playerDetectArea,carrier,ballSprite,rb,LayerMask,colliderForWall);
        CurrentViewState.StateTransitionRequested += SwitchViewState;
        CurrentViewState.OnEnter();
    }
    public bool IsHeadedForScoringArea(Collider2D scoringArea)
    {
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, velocity.normalized, castDistance, collideForScoreAreaLayer);
        if (hit.collider == null)
            return false;
        return hit.collider == scoringArea;
    }
    public void Tumble(Vector2 tumbleVelocity)
    {
        velocity = tumbleVelocity;
        carrier = null;
        heightVelocity = TUMBLE_HEIGHT_VELOCITY;

        // 切换状态
        SwitchViewState(BallStateId.FREEFORM, BallStateData.Build().SetLockDuration(DURATION_TUMBLE_LOCK));
    }
    public void Stop() {
        velocity = Vector2.zero;
    }

    public bool CanAirInteract() {
        return CurrentViewState != null && CurrentViewState.CanAirInteract();
    }

    public void shoot(Vector2 ShotVelocity) {
        velocity = ShotVelocity;
        carrier = null;
        SwitchViewState(BallStateId.SHOT);
    }

    public void passTo(Vector2 destination,bool overground,PlayerView passTarget, float lockDuration = DURATION_PASS_LOCK) {
        Vector2 direction = (destination - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, destination);
        float intensity = Mathf.Sqrt(2f * distance * frictionGround);
        velocity = intensity * direction;

        // 如果是高空的，视为水平是匀速，速度为原本速度intensity  x=Vx*t  t=x/Vx
        // 垂直方向终点y=0,Vy=gt/2  代入t Vy=gx/2Vx 高度增加 /2->/1.85 也会有更快速度，飞到球员脸上而不是脚下
        if (!overground)
        {
            heightVelocity = BallViewState.GRAVITY * distance / (1.85f * intensity);
        }
        else
        {
            heightVelocity = 0f;
        }
        carrier = null;
        SwitchViewState(BallStateId.FREEFORM, BallStateData.Build().SetLockDuration(lockDuration));
    }


    public void InjectSim(BallSim ballSim) {
        this.ballSim=ballSim;
    }
}
