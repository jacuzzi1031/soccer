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
    private int _searchVersion = 0;
    
    private ObjectPool<RoomTabUI> _roomTabPool;
    private List<RoomTabUI> _activeTabs = new List<RoomTabUI>();
    private const float ITEM_WIDTH = 480f;
    private const float ITEM_HEIGHT = 48f;
    private const float ITEM_SPACING = 2f;
    public override void OnInit()
    {
        _mSearchRoomRequest = GameInterface.Interface.RequestManager.GetRequest<SearchRoomRequest>();
        base.OnInit();
        _roomTabPool = new ObjectPool<RoomTabUI>(() =>
        {
            GameObject go = Instantiate(roomTabPrefab, roomTabsContainer);
            return go.GetComponent<RoomTabUI>();
        });
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
    
    private void UpdateRoomList(List<RoomInfo> roomInfoList)
    {
        // 不够就申请
        while (_activeTabs.Count < roomInfoList.Count)
        {
            RoomTabUI tab = _roomTabPool.Allocate();

            tab.gameObject.SetActive(true);
            tab.RectTransform.SetParent(roomTabsContainer, false);

            _activeTabs.Add(tab);
        }

        // 多出来就回收
        while (_activeTabs.Count > roomInfoList.Count)
        {
            RoomTabUI tab = _activeTabs[^1];

            tab.gameObject.SetActive(false);

            _roomTabPool.Release(tab);

            _activeTabs.RemoveAt(_activeTabs.Count - 1);
        }

        // 更新数据和位置
        for (int i = 0; i < roomInfoList.Count; i++)
        {
            RoomTabUI tab = _activeTabs[i];

            tab.RectTransform.sizeDelta =
                new Vector2(ITEM_WIDTH, ITEM_HEIGHT);

            tab.RectTransform.anchoredPosition =
                new Vector2(0, -i * (ITEM_HEIGHT + ITEM_SPACING));

            tab.SetRoomTab(roomInfoList[i]);
        }

        // 更新Content高度
        float totalHeight = roomInfoList.Count * (ITEM_HEIGHT + ITEM_SPACING);

        if (roomInfoList.Count > 0)
            totalHeight -= ITEM_SPACING;

        roomTabsContainer.sizeDelta =
            new Vector2(roomTabsContainer.sizeDelta.x, totalHeight);
    }
    
    // private void ClearRoomList()
    // {
    //     for (int i = roomTabsContainer.childCount - 1; i >= 0; i--)
    //     {
    //         Destroy(roomTabsContainer.GetChild(i).gameObject);
    //     }
    //
    //     _activeRoomTabs.Clear();
    // }
    // private void UpdateRoomList(List<RoomInfo> roomInfoList)
    // {
    //     ClearRoomList();
    //
    //     foreach (var roomInfo in roomInfoList)
    //     {
    //         GameObject go = Instantiate(roomTabPrefab, roomTabsContainer);
    //         RoomTabUI roomTabUI = go.GetComponent<RoomTabUI>();
    //         roomTabUI.SetRoomTab(roomInfo);
    //     }
    //
    //     LayoutRebuilder.ForceRebuildLayoutImmediate(roomTabsContainer);
    // }
    public override void OnHide()
    {
        _searchVersion++;
        base.OnHide();
    }

    public void OnDestroy() {
        foreach (var tab in _activeTabs)
        {
            tab.gameObject.SetActive(false);
            _roomTabPool.Release(tab);
        }

        _activeTabs.Clear();
    }
}