using System;
using System.Collections.Generic;
using SocketProtocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class RoomListUI : BaseUIPanel
{
    [SerializeField] private TMP_InputField searchRoomNameInput;
    [SerializeField] private TMP_Dropdown GameTypeDropdown;
    [SerializeField] private Button searchRoomButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private GameObject roomTabPrefab;
    [SerializeField] private RectTransform roomTabsContainer;
    [SerializeField] private Button closeButton;

    private SearchRoomRequest _mSearchRoomRequest;
    private List<RoomTabUI> _activeRoomTabs = new List<RoomTabUI>();
    private int _searchVersion = 0;
    public override void OnInit()
    {
        _mSearchRoomRequest = GameInterface.Interface.RequestManager.GetRequest<SearchRoomRequest>();
        base.OnInit();
    }

    private void Start()
    {
        searchRoomButton.onClick.AddListener(SearchRoom);
        createRoomButton.onClick.AddListener(() =>
        {
            GameInterface.Interface.UIManager.PushUIPanelAppend(UIPanelType.CreateRoomUI,
                ShowUIPanelType.MoveFadeIn);
        });
        closeButton.onClick.AddListener(() =>
        {
            GameInterface.Interface.UIManager.PopUIPanel();
        });
    }

    public override void OnShow()
    {
        RequestRoomList(roomInfo =>
        {
            roomInfo.RoomMatchType = RoomMatchType.None;
            roomInfo.roomName = string.Empty;
        });

        base.OnShow();
    }
    private void RequestRoomList(Action<RoomInfo> filter)
    {
        _searchVersion++;
        int currentVersion = _searchVersion;

        _mSearchRoomRequest.SendSearchRoomRequest(filter, roomList =>
        {
            if (currentVersion != _searchVersion)
                return;

            UpdateRoomList(roomList);
        });
    }
    private void SearchRoom()
    {
        string roomName = searchRoomNameInput.text;
        RoomMatchType roomMatchType = (RoomMatchType)GameTypeDropdown.value;

        RequestRoomList(roomInfo =>
        {
            roomInfo.roomName = roomName;
            roomInfo.RoomMatchType = roomMatchType;
        });
    }
    private void ClearRoomList()
    {
        for (int i = roomTabsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(roomTabsContainer.GetChild(i).gameObject);
        }

        _activeRoomTabs.Clear();
    }
    private void UpdateRoomList(List<RoomInfo> roomInfoList)
    {
        ClearRoomList();

        foreach (var roomInfo in roomInfoList)
        {
            GameObject go = Instantiate(roomTabPrefab, roomTabsContainer);
            RoomTabUI roomTabUI = go.GetComponent<RoomTabUI>();
            roomTabUI.SetRoomTab(roomInfo);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(roomTabsContainer);
    }
    public override void OnHide()
    {
        _searchVersion++;
        ClearRoomList();
        base.OnHide();
    }
}