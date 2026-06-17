using System;
using SocketProtocol;
using UnityEngine;

public class QuitRoomRequest : BaseRequest
{
    public QuitRoomRequest()
    {
        Request = RequestCode.Room;
        Action = ActionCode.QuitRoom;
    }

    protected override void HandleServerSuccessResponse(MainPack pack)
    {
        int localPlayerId = GameInterface.Interface.LocalPlayerInfo.id;

        int playerId = pack.PlayerInfoPack.Id;
        
        GameInterface.Interface.GameFrameSyncManager.ClearInputBuffer();
        SimulationClock.Instance.OnGameOver();
        
        GameInterface.Interface.RoomManager.QuitRoom(playerId,localPlayerId);

        if (localPlayerId == playerId)
        {
            Invoker.Instance.DelegateList.Add(() => {
                GameInterface.Interface.SceneLoader.LoadSceneAsync(Scene.MainMenuScene,
                    () =>
                    {

                        
                        GameInterface.Interface.UIManager.PushUIPanelAppend(UIPanelType.RoomListUI,
                            ShowUIPanelType.FadeIn);
                    });
            });
        }
        else {
            SendQuitRoomRequest();
        }
        base.HandleServerSuccessResponse(pack);
    }


    public void SendQuitRoomRequest()
    {
        var roomCode = GameInterface.Interface?
            .RoomManager?
            .CurrentRoomInfo?
            .roomCode;

        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("CurrentRoomInfo is null, skip quit room request.");
            return;
        }

        roomCode = CharsetUtil.DefaultToUTF8(roomCode);

        RoomInfoPack roomInfoPack = new RoomInfoPack { RoomCode = roomCode };

        MainPack mainPack = new MainPack
        {
            RequestCode = Request,
            ActionCode = Action,
            RoomInfoPack = roomInfoPack,
        };

        SendRequest(mainPack);
    }
}