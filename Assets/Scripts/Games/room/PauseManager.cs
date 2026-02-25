using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour {
    [SerializeField] private GameObject entityPrefab;
    private List<RoomPlayerInfo> roomPlayerInfoList;

    public static PauseManager Instance{get; private set;}

    private void Awake() {
        Instance=this;
    }
    #region for PauseGame
    private bool isGamePaused = false;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
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
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else {
            // Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion
}
