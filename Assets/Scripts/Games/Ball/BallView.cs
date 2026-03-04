using System;
using System.Collections;
using System.Collections.Generic;
using SocketProtocol;
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
    public LayerMask LayerMask;
    public CircleCollider2D  colliderForWall;
    [HideInInspector]public List<PlayerView> playerListInProximityArea = new List<PlayerView>();

    public float frictionAir = 35f;
    public float frictionGround = 250f;
    private BallSim ballSim;
    private bool _matchStarted = false;

    private void Awake() {
        RoomMatchType currentMathType = GameInterface.Interface.GameManager.currentMatchType;
        if (currentMathType == RoomMatchType.Training||currentMathType == RoomMatchType.TrainingWithEnemy) {
            spawnPosition=trainingSpawnPosition.position;
        }
        transform.position = spawnPosition;
    }

    private void Start() {
        playerDetectArea.OnStay += PlayerDetectAreaOnOnEnter;
        playerProximityArea.OnTriggered+= PlayerProximityAreaOnOnTriggered;
        playerProximityArea.OnTriggerExit+= PlayerProximityAreaOnOnTriggerExit;

    }

    private void OnEnable() {
        GameInterface.Interface.EventSystem.Subscribe<MatchStartEvent>(OnMatchStart);
    }

    private void OnMatchStart(MatchStartEvent obj) {
        _matchStarted = true; 
    }

    private void OnDestroy() {
        playerDetectArea.OnStay -= PlayerDetectAreaOnOnEnter;
        playerProximityArea.OnTriggered-= PlayerProximityAreaOnOnTriggered;
        playerProximityArea.OnTriggerExit-= PlayerProximityAreaOnOnTriggerExit;
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
        // if (!CurrentViewState.CanCarriedBall())
        //     return;
        // PlayerView body = obj.GetComponentInParent<PlayerView>();
        // if (!body) return;
        //
        // if (body.CanCarryBall() && height < MAX_CAPTURE_HEIGHT)
        // {   
        //     carrier = body;
        //     body.ControlBall();
        //     SwitchViewState(BallState.CARRIED);
        // }
    }




    private void Update() {
        if (!_matchStarted)
            return;
        ConsumeStateChange();
        UpdateInterpolatedTransform();
        UpdateStateView();
    }

    private void UpdateStateView() {
        switch (ballSim.ballState)
        {
            case BallState.CARRIED:
                break;
            case BallState.FREEFORM:
                SetBallAnimationFromVelocity();
                break;
            case BallState.SHOT:
                SetBallAnimationFromVelocity();
                break;
        }
    }
    const float idleThreshold = 0.01f;
    public void SetBallAnimationFromVelocity()
    {
        if (ballSim.Velocity.sqrMagnitude <= idleThreshold)
        {
            animator.Play("idle");
            animator.speed = 1;
            return;
        }

        if (ballSim.Velocity.x > 0)
        {
            animator.Play("roll");
            animator.speed = 1;
        }
        else
        {
            animator.Play("rollback");
            animator.speed = 1;
        }
    }
    [HideInInspector]public int lastConsumedFrame;
    Vector2 prevPos;
    Vector2 targetPos;
    float prevHeight;
    float targetHeight;
    float interpTimer;
    BallState lastState;
    public const float FRAME_DT = 1f / 60f;
    private void UpdateInterpolatedTransform() {
        //逻辑帧推进
        if (ballSim.Frame != lastConsumedFrame)
        {
            prevPos = targetPos;
            targetPos = ballSim.Position;

            prevHeight = targetHeight;
            targetHeight = ballSim.height;

            interpTimer = 0f;
            lastConsumedFrame = ballSim.Frame;
        }
        interpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(interpTimer / FRAME_DT);

        Vector2 pos = Vector2.Lerp(prevPos, targetPos, t);
        float height = Mathf.Lerp(prevHeight, targetHeight, t);

        transform.position = pos;
        ballSprite.localPosition =  height * 3f*Vector3.up;
    }

    private void ConsumeStateChange() {
        if (ballSim.ballState != lastState)
        {
            OnStateExit(lastState);
            lastState = ballSim.ballState;
            OnStateEnter(ballSim.ballState);
        }
    }

    private void OnStateExit(BallState ballSimBallState) {
        switch (ballSimBallState) {
            case BallState.CARRIED:
                break;
            case BallState.FREEFORM:
                GameInterface.Interface.EventSystem.Publish(new BallFreeformToLerpCameraOffsetEvent(false));
                break;
            case BallState.SHOT:
                ballSprite.localScale = new Vector3(1, 1, 1);
                shotParticles.Stop();
                break;
        }
    }
    private const float SHOT_SPRITE_SCALE = 0.8f;
    private void OnStateEnter(BallState ballSimBallState) {
        switch (ballSimBallState) {
            case BallState.CARRIED:
                break;
            case BallState.FREEFORM:
                GameInterface.Interface.EventSystem.Publish(new BallFreeformToLerpCameraOffsetEvent(true));
                break;
            case BallState.SHOT:
                ballSprite.localScale = new Vector3(1, SHOT_SPRITE_SCALE, 1);
                shotParticles.Play();
                break;
        }
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
        // SwitchViewState(BallState.FREEFORM, BallStateData.Build().SetLockDuration(DURATION_TUMBLE_LOCK));
    }
    public void Stop() {
        velocity = Vector2.zero;
    }




    


    public void InjectSim(BallSim ballSim) {
        this.ballSim=ballSim;
    }
}
