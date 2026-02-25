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
    
    public TcpClient TcpClient { get; private set; }
    public UdpListener UdpListener { get; private set; }
    public RequestManager RequestManager { get; private set; }
    public GameFrameSyncManager GameFrameSyncManager { get; private set; }
    
    [Header("UI")] [SerializeField] private UIPanelSoListSo uiPanelSoListSo;
    [Header("网络")] [SerializeField] private string serverIP;
    [SerializeField] private int serverPort;
    private void Awake() {
        if (Interface != null)
        {
            Destroy(gameObject);
            return;
        }
        SceneLoader = new SceneLoader();

        #if UNITY_EDITOR
                serverIP = "127.0.0.1";
        #endif
        TcpClient = new TcpClient(serverIP, serverPort);
        UdpListener = new UdpListener();
        Interface = this;
        DontDestroyOnLoad(gameObject);
        EventSystem = new EventSystem();
        UIManager = new UIManager(uiPanelSoListSo);
        GameManager=new GameManager();
        RoomManager=new RoomManager();
        RequestManager = new RequestManager();
        GameFrameSyncManager = new GameFrameSyncManager();
        RoomManager.OnInit();
        RequestManager.OnInit();
        GameFrameSyncManager.OnInit();
        GameManager.OnInit();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenuScene")
            return;
        foreach (var roomPlayer in RoomManager.RoomPlayerList) {
            roomPlayer.ready=false;
        }
        GameManager.OnInit();
        UIManager.OnInit();
        UIManager.PushUIPanel(
            UIPanelType.MainMenuUI,
            ShowUIPanelType.FadeIn
        );
    }

    private void OnDestroy() {
        TcpClient?.Dispose();
        TcpClient = null;

        UdpListener?.Dispose();
        UdpListener = null;
    }
}
