using System;
using SocketProtocol;

public class RoomPlayerSelectCountryRequest : BaseRequest
{

    public RoomPlayerSelectCountryRequest()
    {
        Request = RequestCode.Room;
        Action = ActionCode.PlayerSelectCountry;
    }

    protected override void HandleServerSuccessResponse(MainPack pack)
    {
        int playerId = pack.PlayerInfoPack.Id;

        GameInterface.Interface.RoomManager.RoomPlayerSelectCountry(playerId, pack.RoomPlayerSelectCountryPack.CountryIndex, pack.RoomPlayerSelectCountryPack.CountryName);
        base.HandleServerSuccessResponse(pack);
    }

    protected override void HandleServerFailResponse(MainPack pack)
    {
        base.HandleServerFailResponse(pack);
    }

    public void SendSelectCountryRequest(int countryIndex,string countryName, Action onSuccess = null, Action onFail = null)
    {
        RoomPlayerSelectCountryPack roomPlayerSelectCountryPack = new RoomPlayerSelectCountryPack { CountryIndex=countryIndex,CountryName = countryName};

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
            RoomPlayerSelectCountryPack = roomPlayerSelectCountryPack,
            RoomInfoPack = roomInfoPack
        };
        
        SendRequest(mainPack);
    }
}