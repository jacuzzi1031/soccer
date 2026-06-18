using System;
using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
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
    public BallSim ballSim;
    private bool _matchStarted = false;

    private void Awake() {
        RoomMatchType currentMathType = GameInterface.Interface.GameManager.currentMatchType;
        if (currentMathType == RoomMatchType.Training||currentMathType == RoomMatchType.TrainingWithEnemy) {
            spawnPosition=trainingSpawnPosition.position;
        }
        transform.position = spawnPosition;
    }

    private void Start() {
    }

    private void OnEnable() {
        GameInterface.Interface.EventSystem.Subscribe<MatchStartEvent>(OnMatchStart);
        GameInterface.Interface.EventSystem.Subscribe<BallBacktoSpawnPositionEvent>(OnBallBacktoSpawnPosition);
    }

    private void OnDestroy() {
        GameInterface.Interface.EventSystem.Unsubscribe<MatchStartEvent>(OnMatchStart);
        GameInterface.Interface.EventSystem.Unsubscribe<BallBacktoSpawnPositionEvent>(OnBallBacktoSpawnPosition);
    }

    private void OnBallBacktoSpawnPosition(BallBacktoSpawnPositionEvent obj) {
        Vector2 pos = ballSim.Position.ToVector2();

        prevPos = pos;
        targetPos = pos;

        prevHeight = ballSim.Height.ToFloat();
        targetHeight = ballSim.Height.ToFloat();

        transform.position = pos;
    }

    private void OnMatchStart(MatchStartEvent obj) {
        _matchStarted = true; 
    }
    public int GetProximityTeammatesCount(string playerCountry) {
        return playerListInProximityArea.FindAll(p => p.country == playerCountry).Count;
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
                SetBallAnimationAndCameraFromVelocity();
                break;
            case BallState.FREEFORM:
                SetBallAnimationFromVelocity();
                break;
            case BallState.SHOT:
                SetBallAnimationFromVelocity();
                break;
        }
    }
    
    // private float cameraCheckTimer = 0f;
    // public float cameraCheckInterval = 0.2f;
    private void SetBallAnimationAndCameraFromVelocity() {
        var velocity = ballSim.carrier != null 
            ? ballSim.carrier.Velocity 
            : ballSim.Velocity;
        // if (velocity.sqrMagnitude > idleThreshold)
        // {
        //     cameraCheckTimer += Time.deltaTime;
        //
        //     if (cameraCheckTimer > cameraCheckInterval)
        //     {
        //         cameraCheckTimer = 0f;
        //         CheckCameraY(velocity.y);
        //     }
        // }
        BallAnimState newState;

        if (velocity.sqrMagnitude <= (FixedFloat)idleThreshold)
        {
            newState = BallAnimState.Idle;
        }
        else if (velocity.x > 0)
        {
            newState = BallAnimState.Roll;
        }
        else
        {
            newState = BallAnimState.Rollback;
        }
        if (newState != _currentState)
        {
            _currentState = newState;

            switch (_currentState)
            {
                case BallAnimState.Idle:
                    animator.Play("idle");
                    break;

                case BallAnimState.Roll:
                    animator.Play("roll");
                    break;

                case BallAnimState.Rollback:
                    animator.Play("rollback");
                    break;
            }

            animator.speed = 1;
        }
    }
    void CheckCameraY(float velocityY)
    {   
        // if (CameraManager.Instance.IsLerpingScreenY) return;
        if (Mathf.Abs(velocityY) > idleThreshold)
            CameraManager.Instance.LerpScreenY(velocityY);
        else
            CameraManager.Instance.LerpScreenY(0);
    }
    enum BallAnimState
    {
        Idle,
        Roll,
        Rollback
    }
    private BallAnimState _currentState;
    const float idleThreshold = 0.0001f;
    public void SetBallAnimationFromVelocity()
    {
        BallAnimState newState;
        if (ballSim.Velocity.sqrMagnitude <= (FixedFloat)idleThreshold)
            newState = BallAnimState.Idle;
        else if (ballSim.Velocity.x > 0)
            newState = BallAnimState.Roll;
        else
            newState = BallAnimState.Rollback;

        if (newState != _currentState)
        {
            _currentState = newState;

            animator.Play(_currentState.ToString().ToLower());
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
    public const float FRAME_DT = SimulationClock.FRAME_DT;
    private void UpdateInterpolatedTransform() {
        //逻辑帧推进
        if (ballSim.Frame != lastConsumedFrame)
        {
            prevPos = targetPos;
            targetPos = ballSim.Position.ToVector2();

            prevHeight = targetHeight;
            targetHeight = ballSim.Height.ToFloat();

            interpTimer = 0f;
            lastConsumedFrame = ballSim.Frame;
        }
        interpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(interpTimer / FRAME_DT);

        Vector2 pos = Vector2.Lerp(prevPos, targetPos, t);
        float height = Mathf.Lerp(prevHeight, targetHeight, t);

        transform.position = pos;
        ballSprite.localPosition =  height * 20f*Vector3.up;
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
                GameInterface.Interface.EventSystem.Publish(new OnBallReleasedEvent());
                CameraFollowObject.Instance.FollowBall(ballSim);
                break;
            case BallState.FREEFORM:
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
                GameInterface.Interface.EventSystem.Publish(new OnBallPossessedEvent(ballSim.carrier.fullName));
                CameraFollowObject.Instance.FollowCarrier(ballSim.carrier);
                
                if(GameInterface.Interface.RoomManager.IsHome&&ballSim.carrier.isHome||
                   !GameInterface.Interface.RoomManager.IsHome&&!ballSim.carrier.isHome)
                    GameInterface.Interface.EventSystem.Publish(new BallFreeformToLerpCameraOffsetEvent(false));
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
        CameraFollowObject.Instance.FollowBall(ballSim);
    }
}
