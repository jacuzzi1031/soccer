using System;
using System.Collections.Generic;
using SocketProtocol;
using UnityEngine;

public class RequestManager : BaseManager
{   
    private readonly Dictionary<ActionCode, BaseRequest> _mRequestDict = new();

    // private List<BaseRequest> _mRequestList = new();
    private readonly Dictionary<Type, BaseRequest> _mRequestTypeDict = new();


    public override void OnInit()
    {
        base.OnInit();
        InitRequests();

        GameInterface.Interface.TcpClient.OnServerResponse += HandleServerResponse;
    }

    private void InitRequests()
    {   
        RegisterRequest(new SignInRequest());
        RegisterRequest(new SignUpRequest());
        RegisterRequest(new SearchRoomRequest());
        RegisterRequest(new RoomPlayerConfirmCountryRequest());
        RegisterRequest(new JoinRoomRequest());
        RegisterRequest(new CreateRoomRequest());
        RegisterRequest(new QuitRoomRequest());
        RegisterRequest(new ReadyStartGameResponse());
        RegisterRequest(new LoadGameSceneCompleteRequest());
        RegisterRequest(new GameStateChangeResponse());
        RegisterRequest(new RoomPlayerSelectCountryRequest());
    }
    private void RegisterRequest(BaseRequest request)
    {
        _mRequestDict.Add(request.Action, request);
        _mRequestTypeDict.Add(request.GetType(), request);
    }
    private void HandleServerResponse(MainPack pack)
    {
        
        if (_mRequestDict.TryGetValue(pack.ActionCode, out var request))
        {
            request.HandleServerResponse(pack);
        }
        else
        {
            Debug.LogWarning($"Unhandled ActionCode: {pack.ActionCode}");
        }
    }
    // ReSharper disable Unity.PerformanceAnalysis
    public T GetRequest<T>() where T : BaseRequest
    {
        if (_mRequestTypeDict.TryGetValue(typeof(T), out var req))
            return (T)req;

        Debug.LogError($"未找到类型 {typeof(T).Name} 对应的请求...");
        return null;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();


        _mRequestDict.Clear();
        _mRequestTypeDict.Clear();
        GameInterface.Interface.TcpClient.OnServerResponse -= HandleServerResponse;
    }
}