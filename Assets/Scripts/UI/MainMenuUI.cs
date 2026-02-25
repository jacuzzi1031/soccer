using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : BaseUIPanel {


    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button trainButton;
    [SerializeField] private Button trainWithEnemyButton;
    [SerializeField] private Button championShipButton;
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
        
        //for test single player
        trainButton.onClick.AddListener(() => OnModeButtonClicked(MatchType.Training));
        trainWithEnemyButton.onClick.AddListener(() => OnModeButtonClicked(MatchType.TrainingWithEnemy));
        championShipButton.onClick.AddListener(() => OnModeButtonClicked(MatchType.UltimateTeam));
    }

    private void Start() {
        startAnimation.Play("StartMainMenuAnimation");
    }

    private void OnModeButtonClicked(MatchType mode)
    {
        GameInterface.Interface.GameManager.SetCurrentMatchType(mode);
        
        var ui = GameInterface.Interface.UIManager;
        ui.HideUIPanel(UIPanelType.CreateRoomUI);
        ui.HideUIPanel(UIPanelType.RoomListUI);
        ui.HideUIPanel(UIPanelType.MainMenuUI);
        
        GameInterface.Interface.SceneLoader.LoadScene(Scene.RoomScene);
    }

}