using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomVisual : MonoBehaviour {
    public static RoomVisual Instance{get; private set;}
    [SerializeField] private Transform[] roomPlayerPositionArray;
    [HideInInspector]public Dictionary<RoomPlayerInfo, RoomPlayer> _mRoomPlayerInfoToRoomPlayerDict = new();
    [SerializeField] private RoomPlayer roomPlayerPrefab;
    private bool[] _mRoomPlayerPositionAvailable;

    public void Awake() {
        Instance=this;
    }

    public void Start() {
        _mRoomPlayerPositionAvailable = new bool[roomPlayerPositionArray.Length];
        Array.Fill(_mRoomPlayerPositionAvailable, true);
        SpawnRoomPlayers();
        
    }

    public void OnEnable() {
        GameInterface.Interface.EventSystem.Subscribe<CountrySelectEvent>(OnCountrySelect);
        GameInterface.Interface.RoomManager.OnEnter();
    }
    void OnDisable()
    {
        GameInterface.Interface.EventSystem.Unsubscribe<CountrySelectEvent>(OnCountrySelect);
        GameInterface.Interface.RoomManager.OnExit();
    }
    private void OnCountrySelect(CountrySelectEvent obj) {
        //实际是转到RoomManager的RoomPlayerReady的获取的localId;
        int localId = GameInterface.Interface.LocalPlayerInfo.id;
        RoomPlayerInfo roomPlayerInfo = GameInterface.Interface.RoomManager.RoomPlayerList.Find(roomPlayer=>roomPlayer.id == localId);
        RoomPlayer roomPlayer = _mRoomPlayerInfoToRoomPlayerDict[roomPlayerInfo];
        roomPlayer.ApplyAppearance(obj.countryName);
        
        //RoomUI的SelectCurSor移动  ，其中RoomPlayer保存了cursor的key i ，也就是_mRoomPlayerPositionAvailable的i
        RoomUI.Instance.OnCountrySelect(roomPlayer.RoomIndex, obj.index);
    }
    private void SpawnRoomPlayers()
    {
        List<RoomPlayerInfo> roomPlayerInfoList = GameInterface.Interface.RoomManager.RoomPlayerList;
        for (int i = 0; i < roomPlayerInfoList.Count; i++)
        {
            RoomPlayer roomPlayer = SpawnRoomPlayer(i, roomPlayerInfoList[i]);
            _mRoomPlayerInfoToRoomPlayerDict.Add(roomPlayerInfoList[i], roomPlayer);
            _mRoomPlayerPositionAvailable[i] = false;
            RoomUI.Instance.OnPlayerPositionAvailable(i,false);
        }
    }
    private RoomPlayer SpawnRoomPlayer(int index, RoomPlayerInfo roomPlayerInfo)
    {
        Transform roomPlayerPosition = roomPlayerPositionArray[index];
        RoomPlayer roomPlayer = Instantiate(roomPlayerPrefab, roomPlayerPosition.position, roomPlayerPosition.rotation);
        roomPlayer.SetRoomPlayer(index, roomPlayerInfo.nickname);
        return roomPlayer;
    }


    public void SpawnCPU(string excludeCountry,Action<int,string>Onselect) {
        int availableIndex = -1;
        for (int i = 0; i < _mRoomPlayerPositionAvailable.Length; i++)
        {
            if (_mRoomPlayerPositionAvailable[i])
            {
                availableIndex = i;
                break;
            }
        }

        _mRoomPlayerPositionAvailable[availableIndex] = false;
        
        Transform roomPlayerPosition = roomPlayerPositionArray[availableIndex];
        RoomPlayer roomPlayer = Instantiate(roomPlayerPrefab, roomPlayerPosition.position, roomPlayerPosition.rotation);
        roomPlayer.SetRoomPlayer(availableIndex, "Bot");
        string cpuCountryName=null;
        RoomUI.Instance.selectCpuCountry(excludeCountry,
            countryName => {
                roomPlayer.ApplyAppearance(countryName);
                cpuCountryName = countryName;
            });
        Onselect?.Invoke(availableIndex,cpuCountryName);
    }
}
