using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour {


    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button optionsButton;


    private void Awake() {
        resumeButton.onClick.AddListener(() => {
            PauseManager.Instance.TogglePauseGame();
        });
        mainMenuButton.onClick.AddListener(() => {
            GameSceneBootstrap.Instance.PauseThenQuitMatch();
        });
        optionsButton.onClick.AddListener(() => {
            Hide();
            OptionsUI.Instance.Show(Show);
        });
    }

    private void Start() {
        PauseManager.Instance.OnGamePaused += OnGamePaused;
        PauseManager.Instance.OnGameUnpaused += OnGameUnpaused;
        Hide();
    }

    private void OnDestroy() {
        PauseManager.Instance.OnGamePaused -= OnGamePaused;
        PauseManager.Instance.OnGameUnpaused -= OnGameUnpaused;
    }

    private void OnGameUnpaused() {
        Debug.Log("OnGameUnpaused");
        Hide();
    }

    private void OnGamePaused() {
        Debug.Log("OnGamepaused");
        Show();
    }

    private void Show() {
        gameObject.SetActive(true);
        // resumeButton.Select();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}