using SocketProtocol;
using UnityEngine;

public class ReadyStartGameResponse : BaseRequest
{

    public ReadyStartGameResponse()
    {
        Request = RequestCode.Room;
        Action = ActionCode.ReadyStartGame;
    }

    protected override void HandleServerSuccessResponse(MainPack pack)
    {
        Invoker.Instance.DelegateList.Add(() =>
        {
            GameInterface.Interface.UIManager.ShowMessage("开始游戏!");
            GameInterface.Interface.GameFrameSyncManager.PrepareControlContext();
            
            TimeSyncManager.Instance.EnterGameWhenSynced();
            // GameInterface.Interface.SceneLoader.LoadScene(Scene.LoadingScene);
        });

        GameInterface.Interface.UdpListener.StartListen();

        base.HandleServerSuccessResponse(pack);
    }

    protected override void HandleServerFailResponse(MainPack pack)
    {
        base.HandleServerFailResponse(pack);
    }

}