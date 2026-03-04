
    using System;
    using SocketProtocol;

    public class TimeSyncRequest:BaseRequest {
        public TimeSyncRequest()
        {
            Request = RequestCode.Room;
            Action = ActionCode.OnTimeSync;
        }
        protected override void HandleServerSuccessResponse(MainPack pack) {
            long clientSendTime = pack.SyncTimeForOffsetPack.ClientSendTime;
            long serverTime = pack.SyncTimeForOffsetPack.ServerTime;
            TimeSyncManager.Instance.OnTimeSyncResponse(clientSendTime,serverTime);
            base.HandleServerSuccessResponse(pack);
        }

        public void SendTimeSyncRequest(Action onSuccess = null, Action onFail = null)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            SyncTimeForOffsetPack syncTimeForOffsetPack = new SyncTimeForOffsetPack { ClientSendTime = now };

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
                RoomInfoPack = roomInfoPack,
                SyncTimeForOffsetPack=syncTimeForOffsetPack
            };

            SendRequest(mainPack);
        }
    }
