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
        GameInterface.Interface.RoomManager.OnRoomPlayerQuit += OnRoomPlayerQuit;
        GameInterface.Interface.RoomManager.OnRoomPlayerJoin += OnRoomPlayerJoin;
        GameInterface.Interface.RoomManager.OnRoomPlayerSelectCountryChanged+= OnSelectCountryChanged;
        GameInterface.Interface.RoomManager.OnRoomPlayerCountryConfirmed += OnCountryConfirmed;
        SpawnRoomPlayers();

    }
    private void OnDestroy() {
        GameInterface.Interface.RoomManager.OnRoomPlayerQuit -= OnRoomPlayerQuit;
        GameInterface.Interface.RoomManager.OnRoomPlayerJoin -= OnRoomPlayerJoin;
        GameInterface.Interface.RoomManager.OnRoomPlayerSelectCountryChanged-= OnSelectCountryChanged;
        GameInterface.Interface.RoomManager.OnRoomPlayerCountryConfirmed -= OnCountryConfirmed;
    }
    private void OnSelectCountryChanged(RoomPlayerInfo roomPlayerInfo, int countryIndex, string countryName) {
        Invoker.Instance.DelegateList.Add(() =>
        {
            RoomPlayer roomPlayer = _mRoomPlayerInfoToRoomPlayerDict[roomPlayerInfo];
            if (roomPlayer.HasComfirmed) return;
        
            roomPlayer.ApplyAppearance(countryName);
            RoomUI.Instance.OnCountrySelect(roomPlayer.RoomIndex, countryIndex);
        });
    }
    private void OnCountryConfirmed(RoomPlayerInfo info, string countryName)
    {
        Invoker.Instance.DelegateList.Add(() =>
        {
            RoomPlayer rp = _mRoomPlayerInfoToRoomPlayerDict[info];
            if (rp.HasComfirmed)
                return;
            rp.SetComfirmed(true);
            rp.ApplyAppearance(countryName);
            info.isHome = rp.RoomIndex == 0;
            SoundManager.Instance.Play(SoundManager.Instance.audioRefs.UI_SELECT);
            GameInterface.Interface.GameManager
                .SetMatchCountry(rp.RoomIndex, countryName);

            SpawnCPUIfNeeded(countryName,info.seatIndex);
        });
    }
    private void SpawnCPUIfNeeded(string excludeCountry,int excludeSeatIndex)
    {
        //只在“只有 1 个真人玩家”时补 CPU
        if (GameInterface.Interface.RoomManager.RoomPlayerList.Count != 1)
            return;
        SpawnCPU(excludeCountry,excludeSeatIndex,(index, country) =>
        {
            GameInterface.Interface.GameManager
                .SetMatchCountry(index, country);

            SoundManager.Instance.Play(
                SoundManager.Instance.audioRefs.UI_SELECT
            );
        });
    }
    public void SpawnCPU(string excludeCountry,int excludeSeatIndex,  Action<int, string> onSelect)
    {

        int index = excludeSeatIndex == 0 ? 1 : 0;

        RoomUI.Instance.CreateSelector(index);
        RoomPlayer rp = SpawnRoomPlayer(index, "Bot");
        rp.SetComfirmed(true);
        RoomUI.Instance.selectCpuCountry(excludeCountry, country =>
        {
            rp.ApplyAppearance(country);
            onSelect?.Invoke(index, country);
        });
    }




    private void OnRoomPlayerQuit(RoomPlayerInfo roomPlayerInfoForRemove,RoomPlayerInfo roomPlayerInfoForReset)
    {
        Invoker.Instance.DelegateList.Add(() =>
        {
            RoomPlayer roomPlayer = _mRoomPlayerInfoToRoomPlayerDict[roomPlayerInfoForRemove];
            _mRoomPlayerInfoToRoomPlayerDict.Remove(roomPlayerInfoForRemove);
            int playerIndex = roomPlayer.RoomIndex;
            _mRoomPlayerPositionAvailable[playerIndex] = true;
            Destroy(roomPlayer.gameObject);
            RoomUI.Instance.DeleteSelectCursor(playerIndex);

            if (roomPlayerInfoForReset != null) {
                RoomPlayer roomPlayerForReset = _mRoomPlayerInfoToRoomPlayerDict[roomPlayerInfoForReset];
                //本地设置为false，Quit服务端也要设置false
                roomPlayerForReset.SetComfirmed(false);
            }
        });
    }
    private void SpawnRoomPlayers()
    {
        List<RoomPlayerInfo> list = GameInterface.Interface.RoomManager.RoomPlayerList;
        foreach (var roomPlayerInfo in list) {
            int seatIndex = roomPlayerInfo.seatIndex;
            RoomUI.Instance.CreateSelector(seatIndex);
            RoomPlayer rp = SpawnRoomPlayer(seatIndex, roomPlayerInfo.nickname);
            _mRoomPlayerInfoToRoomPlayerDict.Add(roomPlayerInfo, rp);
        }
    }
    private void OnRoomPlayerJoin(RoomPlayerInfo info)
    {
        Invoker.Instance.DelegateList.Add(() =>
        {
            int index = info.seatIndex;
            if (index < 0) return;

            RoomUI.Instance.CreateSelector(index);
            RoomPlayer rp = SpawnRoomPlayer(index, info.nickname);
            _mRoomPlayerInfoToRoomPlayerDict[info] = rp;
        });
    }
    private RoomPlayer SpawnRoomPlayer(int seatIndex, string nickname)
    {
        Transform pos = roomPlayerPositionArray[seatIndex];
        RoomPlayer roomPlayer = Instantiate(
            roomPlayerPrefab,
            pos.position,
            pos.rotation
        );
        roomPlayer.SetRoomPlayer(seatIndex, nickname);
        return roomPlayer;
    }

}
