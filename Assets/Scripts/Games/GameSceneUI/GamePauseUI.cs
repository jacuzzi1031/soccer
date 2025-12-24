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
            EntityManager.Instance.TogglePauseGame();
        });
        mainMenuButton.onClick.AddListener(() => {
            GameInterface.Interface.SceneLoader.LoadScene(Scene.MainMenuScene);
        });
        optionsButton.onClick.AddListener(() => {
            Hide();
            OptionsUI.Instance.Show(Show);
        });
    }

    private void Start() {
        EntityManager.Instance.OnGamePaused += OnGamePaused;
        EntityManager.Instance.OnGameUnpaused += OnGameUnpaused;

        Hide();
    }

    private void OnDestroy() {
        EntityManager.Instance.OnGamePaused -= OnGamePaused;
        EntityManager.Instance.OnGameUnpaused -= OnGameUnpaused;
    }

    private void OnGameUnpaused(object sender, System.EventArgs e) {
        Hide();
    }

    private void OnGamePaused(object sender, System.EventArgs e) {
        Show();
    }

    private void Show() {
        gameObject.SetActive(true);

        resumeButton.Select();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}