using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameInterface : MonoBehaviour
{
    public static GameInterface Interface { get; private set; }
    public SceneLoader SceneLoader { get; private set; }
    public EventSystem EventSystem { get; private set; }
    public GameManager GameManager { get; private set; }
    public UIManager UIManager { get; private set; }
    public RoomManager RoomManager { get; private set; }
    
    public PlayerInfo LocalPlayerInfo { get; set; }
    
    [Header("UI")] [SerializeField] private UIPanelSoListSo uiPanelSoListSo;
    private void Awake() {
        if (Interface != null)
        {
            Destroy(gameObject);
            return;
        }
        SceneLoader = new SceneLoader();

        Interface = this;
        DontDestroyOnLoad(gameObject);
        EventSystem = new EventSystem();
        UIManager = new UIManager(uiPanelSoListSo);
        GameManager=new GameManager();
        RoomManager=new RoomManager();
        RoomManager.OnInit();


        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenuScene")
            return;
        foreach (var roomPlayer in RoomManager.RoomPlayerList) {
            roomPlayer.comfirmed=false;
        }
        GameManager.OnInit();
        UIManager.OnInit();
        UIManager.PushUIPanel(
            UIPanelType.MainMenuUI,
            ShowUIPanelType.FadeIn
        );
    }

    private void OnDestroy() {
    }
}
