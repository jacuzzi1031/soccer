using System;
using SocketProtocol;

public class RoomPlayerConfirmCountryRequest : BaseRequest
{

    // public event Action<int> OnPlayerReadyChanged;

    public RoomPlayerConfirmCountryRequest()
    {
        Request = RequestCode.Room;
        Action = ActionCode.PlayerConfirmCountry;
    }

    protected override void HandleServerSuccessResponse(MainPack pack)
    {
        // int clientId = pack.ClientPack.ClientId;
        string countryName = pack.RoomPlayerConfirmCountryPack.SelectCountry;
        int playerId = pack.PlayerInfoPack.Id;

        GameInterface.Interface.RoomManager.RoomPlayerConfirmCountry(playerId, countryName);


        base.HandleServerSuccessResponse(pack);
    }

    protected override void HandleServerFailResponse(MainPack pack)
    {
        base.HandleServerFailResponse(pack);
    }

    public void SendRoomPlayerConfirmCountryRequest(string countryName, Action onSuccess = null, Action onFail = null)
    {
        RoomPlayerConfirmCountryPack roomPlayerConfirmCountryPack = new RoomPlayerConfirmCountryPack { SelectCountry = countryName };

        string roomCode = GameInterface.Interface.RoomManager.CurrentRoomInfo.roomCode;
        roomCode = CharsetUtil.DefaultToUTF8(roomCode);

        RoomInfoPack roomInfoPack = new RoomInfoPack
        {
            RoomCode = roomCode,
        };

        MainPack mainPack = new MainPack
        {
            RequestCode = Request,
            ActionCode = Action,
            RoomPlayerConfirmCountryPack = roomPlayerConfirmCountryPack,
            RoomInfoPack = roomInfoPack
        };

        SendRequest(mainPack);
    }
}