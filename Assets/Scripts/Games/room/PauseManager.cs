using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour {
    public static PauseManager Instance{get; private set;}

    private void Awake() {
        Instance=this;
    }
    #region for PauseGame
    private bool isGamePaused = false;
    public event Action OnGamePaused;
    public event Action OnGameUnpaused;
    public void OnEnable() {
        GameInput.Instance.OnPauseAction+= OnPauseAction;
    }
    private void OnPauseAction(object sender, EventArgs e) {
        TogglePauseGame();
    }
    public void TogglePauseGame() {
        isGamePaused=!isGamePaused;
        if (isGamePaused) {
            // Time.timeScale = 0f;
            OnGamePaused?.Invoke();
        }
        else {
            // Time.timeScale = 1f;
            OnGameUnpaused?.Invoke();
        }
    }
    #endregion
}
