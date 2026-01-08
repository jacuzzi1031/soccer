using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using SocketProtocol;
using UnityEngine;
using Timer = System.Timers.Timer;
public class TcpClient : IDisposable
{
    private Socket _mSocket;
    SocketAsyncEventArgs connectArgs;
    SocketAsyncEventArgs receiveArgs;
    private bool isClosing;
    IPEndPoint remoteEndPoint;
    
    private Message _mMessage;
    private string _mIpAddress;
    private int _mPort;
    private DateTime _mLastPongTime;

    private Timer _mSendHeartbeatTimer;
    private Timer _mCheckHeartbeatTimer;
    private Timer _mReconnectTimer;
    private int _mReconnectCount = 0;

    public int ClientId { get; private set; }
    public string ClientIp { get; private set; }
    public bool IsOnline { get; set; }

    public event Action<MainPack> OnServerResponse;
    public event Action OnClientCloseConnection;

    public TcpClient(string ip, int port)
    {
        _mIpAddress = ip;
        _mPort = port;
        _mMessage = new Message();

        InitSocket(ip, port);
    }

    private void InitSocket(string ip, int port)
    {
        try
        {   
            Connect(ip, port);


            _mLastPongTime = DateTime.Now;
            _mSendHeartbeatTimer = new Timer(5000)
            {
                AutoReset = true,
                Enabled = true,
            };
            _mSendHeartbeatTimer.Elapsed += SendHeartbeat;
            _mSendHeartbeatTimer.Start();

            _mCheckHeartbeatTimer = new Timer(10000)
            {
                AutoReset = true,
                Enabled = true,
            };
            _mCheckHeartbeatTimer.Elapsed += CheckHeartbeatTimeout;
            _mCheckHeartbeatTimer.Start();

            _mReconnectTimer = new Timer(GameAssets.RECONNECT_INTERVAL_MS)
            {
                AutoReset = true,
                Enabled = false
            };
            _mReconnectTimer.Elapsed += HandleReconnect;
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception: {e.StackTrace}");
        }
    }
    
    public void Connect(string ip, int port)
    {
        if (_mSocket != null && _mSocket.Connected) {
            return;
        }
        isClosing = false;

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        _mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        connectArgs = new SocketAsyncEventArgs();
        connectArgs.RemoteEndPoint = remoteEndPoint;
        connectArgs.Completed += OnConnectCompleted;

        bool pending = _mSocket.ConnectAsync(connectArgs);
        if (!pending)
        {
            OnConnectCompleted(_mSocket, connectArgs);
        }
    }
    void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)
        {
            Debug.LogError("Connect failed: " + e.SocketError);
            Reconnect();
            return;
        }
        IPEndPoint localIpEndPoint = _mSocket.LocalEndPoint as IPEndPoint;
        ClientIp = localIpEndPoint.Address.ToString();

        Debug.Log( "Connected. Local IP:" +ClientIp+"Socket connected to" + _mIpAddress + ":" + _mPort);

        receiveArgs = new SocketAsyncEventArgs();
        receiveArgs.SetBuffer(_mMessage.Buffer, _mMessage.MessageLen, _mMessage.RemainSize);
        receiveArgs.Completed += ReceiveCallback;

        StartReceive();
    }
    
    private void StartReceive()
    {
        if (_mSocket == null || !_mSocket.Connected)
            return;

        bool pending = _mSocket.ReceiveAsync(receiveArgs);
        if (!pending)
        {
            ReceiveCallback(_mSocket, receiveArgs);
        }
    }

    private void ReceiveCallback(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success )
        {
            Debug.LogWarning("Disconnected");
            Reconnect();
            return;
        }

        if (e.BytesTransferred == 0) {
            Debug.LogWarning("BytesTransferred == 0");
            return;
        }

        _mMessage.ReadBuffer(e.BytesTransferred, HandleServerResponse);

        e.SetBuffer(_mMessage.MessageLen, _mMessage.RemainSize);

        StartReceive();
    }

    private void HandleServerResponse(MainPack pack)
    {
        if (pack.ResponseCode is ResponseCode.HeartBeatResponse && pack.Heartbeat is { Triggered: true, Type: "PONG" })
        {
            Debug.Log("接收到服务端的心跳...");
            _mLastPongTime = DateTime.Now;
            return;
        }

        if (pack.ActionCode == ActionCode.AssignClient)
        {
            ClientId = pack.ClientPack.ClientId;
            Debug.Log($"当前客户端Id: {ClientId}");
            // GameInterface.Interface.UdpListener.UdpListenPort = pack.ClientPack.UdpListenPort;
            return;
        }

        OnServerResponse?.Invoke(pack);
    }
    private void CheckHeartbeatTimeout(object state, ElapsedEventArgs e)
    {
        Debug.Log("Checking heartbeat timeout:" + (DateTime.Now - _mLastPongTime).TotalSeconds);
        if (_mSocket.Connected && (DateTime.Now - _mLastPongTime).TotalSeconds > 20)
        {
            Debug.Log("服务器心跳未返回，尝试重新连接");
            CloseSocket();

            ReConnectSocket();
        }
    }

    private void ReConnectSocket()
    {
        _mReconnectTimer.Start();
    }
    void Reconnect()
    {
        if (isClosing)
            return;

        CloseSocket();

        Connect(remoteEndPoint.Address.ToString(), remoteEndPoint.Port);

        _mReconnectTimer.Stop();
        _mSendHeartbeatTimer.Start();
        _mCheckHeartbeatTimer.Start();
        _mLastPongTime = DateTime.Now;

        // 重连后，完成登录操作
        Invoker.Instance.DelegateList.Add(ReSignInByAuthorization);
        Debug.Log("重连成功!");
    }
    private void HandleReconnect(object sender, ElapsedEventArgs e)
    {
        try {
            Reconnect();
        }
        catch (SocketException ex)
        {
            _mReconnectCount++;
            Debug.LogError($"重连失败，次数：{_mReconnectCount}  "+ex);
            if (_mReconnectCount > GameAssets.RECONNECT_COUNT_MAX)
            {
                _mReconnectTimer.Stop();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"重连失败, {ex}");
        }
    }

    private void ReSignInByAuthorization()
    {
        string authJson = PlayerPrefs.GetString(GameAssets.AUTHORIZATION_KEY, "");
        if (string.IsNullOrEmpty(authJson))
        {
            Debug.LogError("本地没有存储登录凭证!");
            CloseSocket();
        }

        Authorization auth = JsonUtility.FromJson<Authorization>(authJson);
        SignInPack signInPack = new SignInPack
        {
            Username = auth.username,
            Password = auth.password,
        };

        MainPack mainPack = new MainPack()
        {
            RequestCode = RequestCode.User,
            ActionCode = ActionCode.SignIn,
            SignInPack = signInPack
        };

        Send(mainPack);
    }

    private void SendHeartbeat(object state, ElapsedEventArgs e)
    {
        Debug.Log("发送心跳包...");
        HeartbeatPack heartbeatPack = new HeartbeatPack
        {
            Triggered = true,
            Type = "PING",
            Timestamp = DateTime.Now.Ticks
        };
        MainPack pack = new MainPack
        {
            RequestCode = RequestCode.HeartBeat,
            Heartbeat = heartbeatPack
        };
        Send(pack);
    }




    public void CloseSocket()
    {
        Debug.Log("Close Socket!!!");
        if (_mSocket != null)
        {   
            OnClientCloseConnection?.Invoke();
            _mSocket.Shutdown(SocketShutdown.Both);
            _mSocket.Close();

            _mSendHeartbeatTimer?.Stop();
            _mCheckHeartbeatTimer?.Stop();
            _mReconnectTimer?.Stop();
        }
    }

    public void Send(MainPack pack)
    {
        try
        {
            if (_mSocket.Connected)
            {
                pack.ClientPack = new ClientPack
                {
                    ClientId = ClientId,
                    UdpListenPort = GameInterface.Interface.UdpListener.UdpListenPort,
                };
                _mSocket.Send(Message.GetPackData(pack));
            }
            else
            {
                Debug.LogError("连接已关闭，尝试重新连接...");
                ReConnectSocket();
            }
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketError:" + e);
        }
    }

    public async Task<int> SendAsync(MainPack pack)
    {

        if (_mSocket.Connected)
        {
            _mSocket.Send(Message.GetPackData(pack));
            ReadOnlyMemory<byte> rm = new ReadOnlyMemory<byte>(_mMessage.Buffer);
            return await _mSocket.SendAsync(rm, SocketFlags.None);
        }

        Debug.LogError("连接已关闭，无法发送消息...");
        return await Task.FromResult(0);
    }

    public void Dispose() {
        isClosing = true;
        _mSendHeartbeatTimer.Elapsed -= SendHeartbeat;
        _mCheckHeartbeatTimer.Elapsed -= CheckHeartbeatTimeout;
        _mReconnectTimer.Elapsed -= HandleReconnect;
        CloseSocket();
        _mSocket?.Dispose();
        _mSendHeartbeatTimer?.Dispose();
        _mCheckHeartbeatTimer?.Dispose();
        _mReconnectTimer.Stop();
        _mReconnectTimer?.Dispose();
    }
}