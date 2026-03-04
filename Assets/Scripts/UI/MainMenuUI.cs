using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : BaseUIPanel {


    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Animator startAnimation;


    private void Awake() {
        playButton.onClick.AddListener(() => {
            GameInterface.Interface.UIManager.PushUIPanelAppend(
                GameInterface.Interface.TcpClient.IsOnline ? UIPanelType.RoomListUI : UIPanelType.SignInUI,
                ShowUIPanelType.FadeIn);
        });
        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });
        Time.timeScale = 1f;
    }

    private void Start() {
        startAnimation.Play("StartMainMenuAnimation");
    }

}