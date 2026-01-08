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
    private ObjectPool<RoomTabUI> _roomTabPool;
    private List<RoomTabUI> _activeRoomTabs = new List<RoomTabUI>();
    public override void OnInit()
    {
        _mSearchRoomRequest = GameInterface.Interface.RequestManager.GetRequest<SearchRoomRequest>();
        base.OnInit();
    }

    private void Start()
    {
        searchRoomButton.onClick.AddListener(SearchRoom);
        SearchRoom();
        createRoomButton.onClick.AddListener(() =>
        {
            GameInterface.Interface.UIManager.PushUIPanelAppend(UIPanelType.CreateRoomUI,
                ShowUIPanelType.MoveFadeIn);
        });
        closeButton.onClick.AddListener(() =>
        {
            GameInterface.Interface.UIManager.PopUIPanel();
        });
        
        _roomTabPool = new ObjectPool<RoomTabUI>(() =>
        {
            GameObject go = Instantiate(roomTabPrefab, roomTabsContainer);
            go.SetActive(false);
            return go.GetComponent<RoomTabUI>();
        });
    }

    public override void OnShow()
    {
        Debug.Log("搜索房间中...");
        // 搜索所有房间
        _mSearchRoomRequest.SendSearchRoomRequest(roomInfo =>
        {
            roomInfo.RoomMatchType = RoomMatchType.None;
            roomInfo.roomName = string.Empty;
        }, UpdateRoomList);

        base.OnShow();
    }

    private void SearchRoom()
    {
        string roomName = searchRoomNameInput.text;
        RoomMatchType roomMatchType =(RoomMatchType)GameTypeDropdown.value;
        _mSearchRoomRequest.SendSearchRoomRequest(roomInfo =>
        {
            roomInfo.roomName = roomName;
            roomInfo.RoomMatchType = roomMatchType;
        }, UpdateRoomList);
    }
    private void UpdateRoomList(List<RoomInfo> roomInfoList)
    {
        Debug.Log($"RoomListUI搜索结果:{roomInfoList.Count}");
        
        for (int i = 0; i < _activeRoomTabs.Count; i++)
        {
            RoomTabUI tab = _activeRoomTabs[i];
            tab.gameObject.SetActive(false);
            _roomTabPool.Release(tab);
        }
        _activeRoomTabs.Clear();
        
        var layout = roomTabsContainer.GetComponent<VerticalLayoutGroup>();
        float spacing = layout.spacing;
        float perHeight = roomTabPrefab.GetComponent<RectTransform>().rect.height;

        float height = roomInfoList.Count > 0
            ? perHeight * roomInfoList.Count + (roomInfoList.Count - 1) * spacing
            : 0f;

        roomTabsContainer.sizeDelta =
            new Vector2(roomTabsContainer.sizeDelta.x, height);

        foreach (var roomInfo in roomInfoList)
        {
            RoomTabUI roomTabUI = _roomTabPool.Allocate();
            roomTabUI.transform.SetParent(roomTabsContainer, false);
            roomTabUI.gameObject.SetActive(true);
            roomTabUI.SetRoomTab(roomInfo);

            _activeRoomTabs.Add(roomTabUI);
        }
    }
    private void UpdateRoomList2(List<RoomInfo> roomInfoList)
    {
        Debug.Log($"RoomListUI搜索结果:{roomInfoList.Count}");
        foreach (Transform roomTabTransform in roomTabsContainer)
        {
            Destroy(roomTabTransform.gameObject);
        }

        float spacing = roomTabsContainer.GetComponent<VerticalLayoutGroup>().spacing;
        float perHeight = roomTabPrefab.GetComponent<RectTransform>().rect.height;
        Debug.Log("RoomTab Per Height:" + perHeight);
        float height = perHeight * roomInfoList.Count + (roomInfoList.Count - 1) * spacing;
        Debug.Log("RoomTab Container Height:" + height);
        roomTabsContainer.sizeDelta = new Vector2(roomTabsContainer.sizeDelta.x, height);

        foreach (var roomInfo in roomInfoList)
        {
            GameObject roomTabUIGameObject = Instantiate(roomTabPrefab, roomTabsContainer);
            RoomTabUI roomTabUI = roomTabUIGameObject.GetComponent<RoomTabUI>();
            roomTabUI.SetRoomTab(roomInfo);
        }
    }
}