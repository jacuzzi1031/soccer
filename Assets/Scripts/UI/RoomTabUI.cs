using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class RoomTabUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private TextMeshProUGUI RoomMatchType;
    [SerializeField] private TextMeshProUGUI roomPlayers;

    private JoinRoomRequest _mJoinRoomRequest;
    private string _mRoomCode;

    private void Start()
    {
        _mJoinRoomRequest = GameInterface.Interface.RequestManager.GetRequest<JoinRoomRequest>();
    }

    public void SetRoomTab(RoomInfo roomInfo)
    {
        _mRoomCode = roomInfo.roomCode;
        roomName.text = roomInfo.roomName;
        RoomMatchType.text = roomInfo.RoomMatchType.ToString();
        roomPlayers.text = $"{roomInfo.currentPlayers}/{roomInfo.maxPlayer}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int clickCount = eventData.clickCount;
        if (clickCount == 2)
        {
            Debug.Log($"进入房间=> {_mRoomCode}");
            _mJoinRoomRequest.SendJoinRoomRequest(_mRoomCode, onJoinRoomSuccess: OnJoinRoomSuccess);
        }
    }

    private void OnJoinRoomSuccess()
    {
        GameInterface.Interface.UIManager.HideUIPanel(UIPanelType.CreateRoomUI);
        GameInterface.Interface.UIManager.HideUIPanel(UIPanelType.RoomListUI);
        GameInterface.Interface.UIManager.HideUIPanel(UIPanelType.MainMenuUI);
        //UIManager删除全部uiPanel
        GameInterface.Interface.EventSystem.Publish<PlayerEnterRoomEvent>();
        
        GameInterface.Interface.SceneLoader.LoadScene(Scene.RoomScene);
    }
}