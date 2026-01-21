using System;
using System.Security.Cryptography;
using GameFrameSync;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour {


    private const string PLAYER_PREFS_BINDINGS = "InputBindings";


    public static GameInput Instance { get; private set; }
    

    public event EventHandler OnSwapAction;
    public event EventHandler OnIncesivePassAction;
    public event EventHandler OnShortPassAction;
    public event EventHandler OnLongPassAction;
    public event EventHandler OnShootAction;
    public event EventHandler OnShootCancelAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnBindingRebind;


    public Vector2 MoveVector { get; private set; }

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
    // public GameFrameSyncManager.PlayerInputType LocalPlayerInputType { get; private set; }
    public InputType LocalPlayerInputType { get; private set; }


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
        playerInputActions.Player.Swap.canceled += Swap_canceled;
        playerInputActions.Player.IncisivePass.performed += IncisivePass_performed;
        playerInputActions.Player.IncisivePass.canceled += IncisivePass_canceled;
        playerInputActions.Player.ShortPass.performed += ShortPass_performed;
        playerInputActions.Player.ShortPass.canceled += ShortPass_canceled;
        playerInputActions.Player.LongPass.performed += LongPass_performed;
        playerInputActions.Player.LongPass.canceled += LongPass_canceled;
        playerInputActions.Player.Shoot.performed += Shoot_performed;
        playerInputActions.Player.Shoot.canceled += Shoot_canceled;
        playerInputActions.Player.Pause.performed += Pause_performed;
    }
    
    private void Move_canceled(InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.None;
        MoveVector = Vector2.zero;
    }

    private void Move_performed(InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.Move;
        MoveVector = obj.ReadValue<Vector2>();
    }

    private void OnDestroy() {
        if (playerInputActions == null) return;
        playerInputActions.Player.Move.performed -= Move_performed;
        playerInputActions.Player.Move.canceled -= Move_canceled;
        playerInputActions.Player.Swap.performed -= Swap_performed;
        playerInputActions.Player.Swap.canceled -= Swap_canceled;
        playerInputActions.Player.IncisivePass.performed -= IncisivePass_performed;
        playerInputActions.Player.IncisivePass.canceled -= IncisivePass_canceled;
        playerInputActions.Player.ShortPass.performed -= ShortPass_performed;
        playerInputActions.Player.ShortPass.canceled -= ShortPass_canceled;
        playerInputActions.Player.LongPass.performed -= LongPass_performed;
        playerInputActions.Player.LongPass.canceled -= LongPass_canceled;
        playerInputActions.Player.Pause.performed -= Pause_performed;

        playerInputActions.Dispose();
    }

    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Swap_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.Swap;
        OnSwapAction?.Invoke(this, EventArgs.Empty);
    }
    private void Swap_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.None;
    }
    private void IncisivePass_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.IncisivePass;
        OnIncesivePassAction?.Invoke(this, EventArgs.Empty);
    }
    private void IncisivePass_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.None;
    }
    private void ShortPass_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.ShortPass;
        OnShortPassAction?.Invoke(this, EventArgs.Empty);
    }
    private void ShortPass_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.None;
        
    }
    private void LongPass_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.LongPass;
        OnLongPassAction?.Invoke(this, EventArgs.Empty);
    }
    private void LongPass_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.None;
    }
    private void Shoot_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.ShootPress;
        OnShootAction?.Invoke(this, EventArgs.Empty);
    }
    private void Shoot_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        LocalPlayerInputType = InputType.ShootRelease;
        OnShootCancelAction?.Invoke(this, EventArgs.Empty);
    }


    public Vector2 GetMovementVectorNormalized() {
        return MoveVector.normalized;
    }
    public Vector2 GetMovementVector() {
        return MoveVector;
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

}