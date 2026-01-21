using System;
using System.Collections.Generic;
using SocketProtocol;
using UnityEngine;

public class CreateRoomRequest : BaseRequest
{
    public CreateRoomRequest()
    {
        Request = RequestCode.Room;
        Action = ActionCode.CreateRoom;
    }

    protected override void HandleServerSuccessResponse(MainPack pack)
    {
        Debug.Log("房间创建成功！");

        RoomInfoPack roomInfoPack = pack.RoomInfoPack;
        RoomInfo currentRoomInfo = new RoomInfo
        {
            roomCode = roomInfoPack.RoomCode,
            roomName = roomInfoPack.RoomName,
            RoomMatchType = roomInfoPack.RoomMatchType,
            currentPlayers = roomInfoPack.CurrentPlayers,
            maxPlayer = roomInfoPack.MaxPlayer,
        };
        RoomPlayerInfoPack roomPlayerInfoPack = pack.RoomPlayerInfoPack;
        RoomPlayerInfo roomPlayerInfo = new RoomPlayerInfo {
            id = roomPlayerInfoPack.Id,
            seatIndex = roomPlayerInfoPack.SeatIndex,
            isHome = roomPlayerInfoPack.SeatIndex==0,
            username = roomPlayerInfoPack.Username,
            nickname = roomPlayerInfoPack.Nickname,
            ready = roomPlayerInfoPack.Ready
        };
        GameInterface.Interface.RoomManager.JoinRoom(currentRoomInfo, new List<RoomPlayerInfo> { roomPlayerInfo });
        
        base.HandleServerSuccessResponse(pack);
    }

    protected override void HandleServerFailResponse(MainPack pack)
    {
        Debug.Log("房间创建失败");
        base.HandleServerFailResponse(pack);
    }


    public void SendCreateRoomRequest(Action<RoomInfo> condition, Action onComplete = null)
    {
        RoomInfo roomInfo = new RoomInfo();
        condition.Invoke(roomInfo);
        Debug.Log("创建房间，参数" + roomInfo);

        CreateRoomPack createRoomPack = new CreateRoomPack
        {
            RoomName = CharsetUtil.DefaultToUTF8(roomInfo.roomName),
            RoomMatchType =(SocketProtocol.RoomMatchType)roomInfo.RoomMatchType,
            MaxPlayer = roomInfo.maxPlayer,
        };

        MainPack mainPack = new MainPack
        {
            RequestCode = Request,
            ActionCode = Action,
            CreateRoomPack = createRoomPack
        };

        OnServerSuccessResponse += onComplete;

        SendRequest(mainPack);
    }
}