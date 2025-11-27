using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    

    [Header("References")]

    public ControlScriptObject controlSchemeSO;
    [Space]
    [Header("Settings")]

    [HideInInspector] public float power;
    [HideInInspector] public float speed;
    [HideInInspector] public Role role = Role.MIDFIELD;
    [HideInInspector] public string country;
    [HideInInspector] public enum ControlScheme{ CPU,P1,P2};
    public ControlScheme controlScheme = ControlScheme.CPU;
    [HideInInspector] public Vector2 KickoffPosition;
    [HideInInspector] public Goal ownGoal;
public Goal targetGoal;
public Ball ball;
    [HideInInspector] public SkinColor skinColor;
    [HideInInspector] public string fullName;
    [Space]
    [Header("Components")]
    [SerializeField] private SpriteRenderer controlSprite;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D TeammateDetectionArea;
    [SerializeField] private ParticleSystem runParticles;
    [SerializeField] private Transform FlipComponent;
    public Rigidbody2D rb;

    public enum State {
        MOVING,
        TACKLING,
        RECOVERING,
        PREPPING_SHOT,
        SHOOTING,
        PASSING,
        HEADER,
        VOLLEY_KICK,
        BICYCLE_KICK,
        CHEST_CONTROL,
        HURT,
        DIVING,
        CELEBRATING,
        MOURNING,
        RESETING
    }

    public enum Role {
        GOALIE,
        DEFENSE,
        MIDFIELD,
        OFFENSE
    }

    public enum SkinColor {
        LIGHT,
        MEDIUM,
        DARK
    }



    private Vector3 lastInteractDir;
    [HideInInspector]public bool headingRight = true;
    private PlayerStateFactory playerStateFactory = new PlayerStateFactory();
    [HideInInspector]public PlayerState currentState;
    private const float BALL_CONTROL_HEIGHT_MAX = 0.55f;

    

    private void Start() {
         controlSprite.sprite = controlSchemeSO.GetSprite(controlScheme);
         SwitchState(State.MOVING, PlayerStateData.Build());
         
         //充当player reseting state
         FaceTowardsTargetGoal();
         Flip(headingRight);
         

    }
    private void Update() {
        currentState?._Update();
        FlipSprite();


        // set_sprite_visibility();
        // process_gravity(delta);
    }


    private void FixedUpdate() {
        currentState?._FixedUpdate();
    }
    public void SwitchState(State type, PlayerStateData data = null)
    {
        if (currentState != null) {
            currentState.StateTransitionRequested -= SwitchState;
            currentState.OnExit();
        }


        currentState = playerStateFactory.GetFreshState(type);
        currentState.Setup(this, data ?? new PlayerStateData(),rb,animator,ball,runParticles);
        currentState.StateTransitionRequested += SwitchState;
        currentState.OnEnter();
    }

    public void FlipSprite() {
        
        if (rb.velocity.x > 0 && !headingRight)
        {
            Flip(true);
        }
        else if (rb.velocity.x < 0 && headingRight)
        {
            Flip(false);
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

    public bool CanCarryBall() {
        return currentState != null && currentState.CanCarryBall();
    }
    public void ControlBall() {
        if(ball.height > BALL_CONTROL_HEIGHT_MAX) {
            SwitchState(Player.State.CHEST_CONTROL);
        }
    }

    public bool HasBall() {
        return ball.carrier == this;
    }

    public bool IsFacingTargetGoal() {

        float directionToTarget = targetGoal.transform.position.x - transform.position.x;
        return (headingRight && directionToTarget > 0) || (!headingRight && directionToTarget < 0);
    }

    public void OnAnimationComplete() {
        currentState?.OnAnimationComplete();
    }

    public void Initialize(
        Vector2 contextPosition,
        Vector2 contextKickoffPosition,
        Ball contextBall,
        Goal contextOwnGoal,
        Goal contextTargetGoal,
        PlayerResource contextPlayerData,
        string contextCountry)
    {
        transform.position = contextPosition;
        KickoffPosition = contextKickoffPosition;
        ball = contextBall;
        ownGoal = contextOwnGoal;
        targetGoal = contextTargetGoal;

        speed = contextPlayerData.speed;
        power = contextPlayerData.power;
        role = contextPlayerData.role;
        skinColor = contextPlayerData.skin;
        fullName = contextPlayerData.fullName;

        // Godot: Vector2.LEFT if target_goal.position.x < position.x else Vector2.RIGHT
        if (targetGoal.transform.position.x < transform.position.x)
            headingRight = false;
        else
            headingRight = true;

        country = contextCountry;
        
    }
    
    void FaceTowardsTargetGoal()
    {
        if (!IsFacingTargetGoal())
        {
            headingRight =!headingRight;
        }
    }

    public void SetControlScheme(ControlScheme newScheme) {
        if (controlScheme == newScheme) return;
        controlScheme = newScheme;
        SetControlTexture();
    }
    private void InstanceOnOnShootAction(object sender, EventArgs e) {
        currentState?.OnShoot();
    }
    public void SetControlTexture() {
        controlSprite.sprite=controlSchemeSO.GetSprite(controlScheme);
    }
}