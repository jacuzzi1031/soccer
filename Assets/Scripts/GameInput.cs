using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using GameFrameSync;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour {


    private const string PLAYER_PREFS_BINDINGS = "InputBindings";


    public static GameInput Instance { get; private set; }
    

    // public event EventHandler OnSwapAction;
    // public event EventHandler OnIncesivePassAction;
    // public event EventHandler OnShortPassAction;
    // public event EventHandler OnLongPassAction;
    // public event EventHandler OnShootCancelAction;
    public event Action OnShootAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnBindingRebind;


    public int MoveX { get; private set; }
    public int MoveY { get; private set; }

    public enum Binding {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Swap,
        IncisivePass,
        ShortPass,
        LongPass,
        Shoot,
        Pause,

    }
    // public InputType LocalPlayerInputType { get; private set; }
    [Flags]
    public enum InputEventFlags
    {
        None = 0,
        ShootPress = 1 << 0,
        ShootRelease = 1 << 1,
        Swap=1<<2,
        IncisivePass=1<<3,
        ShortPass=1<<4,
        LongPass=1<<5,
    }
    public InputEventFlags _eventFlags { get; private set; }



    private PlayerInputActions playerInputActions;


    private void Awake() {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);


        playerInputActions = new PlayerInputActions();
        
        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS)) {
            playerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }

        playerInputActions.Player.Enable();

        playerInputActions.Player.Move.performed += Move_performed;
        playerInputActions.Player.Move.canceled += Move_canceled;
        playerInputActions.Player.Swap.performed += Swap_performed;

        playerInputActions.Player.IncisivePass.performed += IncisivePass_performed;

        playerInputActions.Player.ShortPass.performed += ShortPass_performed;

        playerInputActions.Player.LongPass.performed += LongPass_performed;

        playerInputActions.Player.Shoot.performed += Shoot_performed;
        playerInputActions.Player.Shoot.canceled += Shoot_canceled;
        playerInputActions.Player.Pause.performed += Pause_performed;
    }
    
    private void Move_canceled(InputAction.CallbackContext obj)
    {
        MoveX = 0;
        MoveY = 0;
    }

    private void Move_performed(InputAction.CallbackContext obj)
    {
        Vector2 v = obj.ReadValue<Vector2>();
        MoveX = QuantizeAxis(v.x);
        MoveY = QuantizeAxis(v.y);
    }
    private sbyte QuantizeAxis(float v)
    {
        const float deadZone = 0.15f;

        if (Mathf.Abs(v) < deadZone)
            return 0;

        v = Mathf.Clamp(v, -1f, 1f);
        return (sbyte)(v * 127);
    }
    private void OnDestroy() {
        if (playerInputActions == null) return;
        playerInputActions.Player.Move.performed -= Move_performed;
        playerInputActions.Player.Move.canceled -= Move_canceled;
        playerInputActions.Player.Swap.performed -= Swap_performed;
        playerInputActions.Player.IncisivePass.performed -= IncisivePass_performed;

        playerInputActions.Player.ShortPass.performed -= ShortPass_performed;

        playerInputActions.Player.LongPass.performed -= LongPass_performed;

        playerInputActions.Player.Shoot.performed -= Shoot_performed;
        playerInputActions.Player.Shoot.canceled -= Shoot_canceled;
        playerInputActions.Player.Pause.performed -= Pause_performed;

        playerInputActions.Dispose();
    }

    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Swap_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _eventFlags |= InputEventFlags.Swap;
        // OnSwapAction?.Invoke(this, EventArgs.Empty);
    }
    private void IncisivePass_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _eventFlags |= InputEventFlags.IncisivePass;
        // OnIncesivePassAction?.Invoke(this, EventArgs.Empty);
    }
    private void ShortPass_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _eventFlags |= InputEventFlags.ShortPass;
        // OnShortPassAction?.Invoke(this, EventArgs.Empty);
    }
    private void LongPass_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _eventFlags |= InputEventFlags.LongPass;
        // OnLongPassAction?.Invoke(this, EventArgs.Empty);
    }
    private void Shoot_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _eventFlags |= InputEventFlags.ShootPress;
        OnShootAction?.Invoke();
    }
    private void Shoot_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _eventFlags |= InputEventFlags.ShootRelease;
    }


    public Vector2 GetMovementVectorNormalized() {
        return  new Vector2(MoveX / 127f,MoveY / 127f);
    }
    public Vector2D GetMovementVector() {
        return new Vector2D
        {
            X = MoveX,
            Y = MoveY
        };
    }

    public string GetBindingText(Binding binding) {
        switch (binding) {
            default:
            case Binding.Move_Up:
                return playerInputActions.Player.Move.bindings[1].ToDisplayString();
            case Binding.Move_Down:
                return playerInputActions.Player.Move.bindings[2].ToDisplayString();
            case Binding.Move_Left:
                return playerInputActions.Player.Move.bindings[3].ToDisplayString();
            case Binding.Move_Right:
                return playerInputActions.Player.Move.bindings[4].ToDisplayString();
            case Binding.Swap:
                return playerInputActions.Player.Swap.bindings[0].ToDisplayString();
            case Binding.IncisivePass:
                return playerInputActions.Player.IncisivePass.bindings[0].ToDisplayString();
            case Binding.ShortPass:
                return playerInputActions.Player.ShortPass.bindings[0].ToDisplayString();
            case Binding.LongPass:
                return playerInputActions.Player.LongPass.bindings[0].ToDisplayString();
            case Binding.Shoot:
                return playerInputActions.Player.Shoot.bindings[0].ToDisplayString(); 
            case Binding.Pause:
                return playerInputActions.Player.Pause.bindings[0].ToDisplayString();
        }
    }

    public void RebindBinding(Binding binding, Action onActionRebound) {
        playerInputActions.Player.Disable();

        InputAction inputAction;
        int bindingIndex;

        switch (binding) {
            default:
            case Binding.Move_Up:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 1;
                break;
            case Binding.Move_Down:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 2;
                break;
            case Binding.Move_Left:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 3;
                break;
            case Binding.Move_Right:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 4;
                break;
            case Binding.Swap:
                inputAction = playerInputActions.Player.Swap;
                bindingIndex = 0;
                break;
            case Binding.IncisivePass:
                inputAction = playerInputActions.Player.IncisivePass;
                bindingIndex = 0;
                break;
            case Binding.ShortPass:
                inputAction = playerInputActions.Player.ShortPass;
                bindingIndex = 0;
                break;
            case Binding.LongPass:
                inputAction = playerInputActions.Player.LongPass;
                bindingIndex = 0;
                break;
            case Binding.Shoot:
                inputAction = playerInputActions.Player.Shoot;
                bindingIndex = 0;
                break;
            case Binding.Pause:
                inputAction = playerInputActions.Player.Pause;
                bindingIndex = 0;
                break;
        }

        inputAction.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(callback => {
                callback.Dispose();
                playerInputActions.Player.Enable();
                onActionRebound();

                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, playerInputActions.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();

                OnBindingRebind?.Invoke(this, EventArgs.Empty);
            })
            .Start();
    }

    public int ConsumeEventFlags() {
        var flags = _eventFlags;
        _eventFlags = InputEventFlags.None;
        return (int)flags;
    }
}