using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SocketProtocol;


public class RoomTabUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private TextMeshProUGUI roomMatchTypeText;
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
        roomMatchTypeText.text =  roomInfo.RoomMatchType switch
        {
            RoomMatchType.Training           => "训练模式",
            RoomMatchType.TrainingWithEnemy  => "对抗训练",
            RoomMatchType.UltimateTeam       => "锦标赛",
            _                            => "未知模式"
        };
        roomPlayers.text = $"{roomInfo.currentPlayers}/{roomInfo.maxPlayer}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int clickCount = eventData.clickCount;
        if (clickCount == 2)
        {
            Debug.Log($"进入房间=> {roomName}");
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