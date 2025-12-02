using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{   
    private const float DISTANCE_HIGH_PASS = 5.0f;
    private const float DURATION_TUMBLE_LOCK = 0.2f;
    private const float DURATION_PASS_LOCK = 0.5f;
    private const float KICKOFF_PASS_DISTANCE = 30f;
    private const float TUMBLE_HEIGHT_VELOCITY = 3f;
    public Vector2 velocity=Vector2.zero;
    private float castDistance = 60f;
    public float height=0.0f;
    public Player carrier;
    public float heightVelocity=0.0f;
    [SerializeField] public LayerMask collideForScoreAreaLayer;
    
    
    [SerializeField] private TriggerDetection playerDetectArea;
    [SerializeField] private TriggerDetection playerProximityArea;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem shotParticles;
    [SerializeField] private Transform ballSprite;
    [SerializeField]  public Rigidbody2D rb;
    public enum State {CARRIED, FREEFORM, SHOT}
    public BallStateFactory ballStateFactory=new BallStateFactory();
    [HideInInspector]public BallState currentState;
    public LayerMask LayerMask;
    public CircleCollider2D  colliderForWall;
    private const float MAX_CAPTURE_HEIGHT = 25f;

    public float frictionAir = 35f;
    public float frictionGround = 250f;
    private void Awake() {

        SwitchState(State.FREEFORM);
    }

    private void Start() {
        playerDetectArea.OnTriggered += PlayerDetectAreaOnOnEnter;
    }
    private void PlayerDetectAreaOnOnEnter(Collider2D obj) {
        Player body = obj.GetComponentInParent<Player>();
        if (!body) return;

        if (body.CanCarryBall() && height < MAX_CAPTURE_HEIGHT)
        {   
            carrier = body;
            body.ControlBall();
            SwitchState(State.CARRIED);
        }
    }

    private void OnDestroy() {
        playerDetectArea.OnTriggered -= PlayerDetectAreaOnOnEnter;
    }

    private void Update()
    {
        ballSprite.localPosition = Vector3.up * height;
        currentState?._Update();
    }
    private void FixedUpdate()
    {
        currentState?._FixedUpdate();
    }
    
    public void SwitchState(State type, BallStateData data = null)
    {
        if (currentState != null)
        {
            currentState.StateTransitionRequested -= SwitchState;
            currentState.OnExit();
        }
        currentState = ballStateFactory.GetFreshState(type);
        currentState.Setup(this, data ?? new BallStateData(),animator,shotParticles,playerDetectArea,carrier,ballSprite,rb,LayerMask,colliderForWall);
        currentState.StateTransitionRequested += SwitchState;
        currentState.OnEnter();
    }
    bool IsHeadedForScoringArea(Collider2D scoringArea)
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
        SwitchState(State.FREEFORM, BallStateData.Build().SetLockDuration(DURATION_TUMBLE_LOCK));
    }
    public void Stop() {
        velocity = Vector2.zero;
    }

    public bool CanAirInteract() {
        return currentState != null && currentState.CanAirInteract();
    }

    public void shoot(Vector2 ShotVelocity) {
        velocity = ShotVelocity;
        carrier = null;
        SwitchState(State.SHOT);
    }

    public void passTo(Vector2 destination,bool overground,Player passTarget, float lockDuration = DURATION_PASS_LOCK) {
        Vector2 direction = (destination - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, destination);
        float intensity = Mathf.Sqrt(2f * distance * frictionGround);
        velocity = intensity * direction;
        if (passTarget != null) {
            StartCoroutine(DelayToSwap(distance/velocity.magnitude/1.2f,passTarget));
        }
        // 如果是高空的，视为水平是匀速，速度为原本速度intensity  x=Vx*t  t=x/Vx
        // 垂直方向终点y=0,Vy=gt/2  代入t Vy=gx/2Vx 高度增加 /2->/1.85 也会有更快速度，飞到球员脸上而不是脚下
        if (!overground)
        {
            heightVelocity = BallState.GRAVITY * distance / (1.85f * intensity);
        }
        else
        {
            heightVelocity = 0f;
        }
        carrier = null;
        SwitchState(State.FREEFORM, BallStateData.Build().SetLockDuration(lockDuration));
    }
    private IEnumerator DelayToSwap(float delay,Player passTarget)
    {
        yield return new WaitForSeconds(delay);
        GameInterface.Interface.EventSystem.Publish(new PassBallToSwapPlayer(passTarget));
        PlayerManager.Instance.SwapTo(passTarget);
    }
}
