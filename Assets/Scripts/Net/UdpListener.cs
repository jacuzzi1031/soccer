using System;
using System.Net;
using System.Net.Sockets;
using GameFrameSync;
using Google.Protobuf;
using UnityEngine;
public class UdpListener : IDisposable
{
    // private UdpClient _mUdpClient;
    private Socket _mSocket;
    private SocketAsyncEventArgs _recvArgs;
    private byte[] cacheBytes = new byte[1024];
    private int _mCurrentDataSequence = 0;

    private readonly ObjectPool<ResFrameSyncData> _mResFrameSyncDataPool;
    private readonly ObjectPool<MessageHead> _mMessageHeadPool;
    public int UdpListenPort { get; private set; }
    public bool Disposed { get; private set; }
    public IPEndPoint RemoteEp { get; private set; }
    public event Action<ResFrameSyncData> OnReceiveFrameSync;

    public UdpListener()
    {
        _mResFrameSyncDataPool = new ObjectPool<ResFrameSyncData>(() => new ResFrameSyncData());
        _mMessageHeadPool = new ObjectPool<MessageHead>(() => new MessageHead());
        _mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _mSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
        IPEndPoint clientLocalEndPoint = _mSocket.LocalEndPoint as IPEndPoint;
        UdpListenPort = clientLocalEndPoint.Port;
        Debug.Log("Current UDP available port:" + UdpListenPort);
        
    }

    public void StartListen()
    {   
        _recvArgs=new SocketAsyncEventArgs();
        _recvArgs.SetBuffer(cacheBytes, 0, cacheBytes.Length);
        _recvArgs.Completed+= ReceiveCallback;
        
        StartReceive(_recvArgs);
    }

    private void StartReceive(SocketAsyncEventArgs e)
    {
        if (Disposed) return;
        e.SetBuffer(0, cacheBytes.Length);
        e.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        bool pending = _mSocket.ReceiveFromAsync(e);
        if (!pending)
        {
            ReceiveCallback(_mSocket, e);
        }
    }

    private void ReceiveCallback(object sender, SocketAsyncEventArgs e)
    {
        if (Disposed) return;
        if (e.SocketError == SocketError.Success) {
            int length = e.BytesTransferred;
            if (length <= 0)
            {
                StartReceive(e);
                return;
            }
            RemoteEp = (IPEndPoint)e.RemoteEndPoint;
            ResFrameSyncData resFrameSyncData = ResFrameSyncData.Parser.ParseFrom(e.Buffer, 0, length);

            if (resFrameSyncData.MessageType is MessageType.FrameSync)
            {
                OnReceiveFrameSync?.Invoke(resFrameSyncData);

                SendAck(resFrameSyncData.MessageHead.Index);
            }
            
            StartReceive(e);
        }else
        {
            Debug.LogError("接收消息出错" + e.SocketError);
        }
    }

    public void Send(in ResFrameSyncData resFrameSyncData)
    {
        if (Disposed) return;
        MessageHead messageHead = _mMessageHeadPool.Allocate();
        messageHead.Index = _mCurrentDataSequence;

        resFrameSyncData.MessageHead = messageHead;
        resFrameSyncData.MessageType = MessageType.FrameSync;

        try
        {
            byte[] data = Serialize(resFrameSyncData);

            _mCurrentDataSequence++;

            _mSocket.SendTo(data, RemoteEp);
        }
        catch (SocketException e)
        {
            Debug.LogError("UDP SocketError:" + e);
        }
        catch (Exception e)
        {
            Debug.LogError("Exception:" + e);
        }
        finally
        {
            _mMessageHeadPool.Release(messageHead);
        }
    }

    private void SendAck(int index)
    {
        if (Disposed) return;
        MessageHead messageHead = _mMessageHeadPool.Allocate();
        messageHead.Index = index;
        messageHead.Ack = true;
        // messageHead.ClientIp = GameInterface.Interface.TcpClient.ClientIp;

        ResFrameSyncData resFrameSyncData = _mResFrameSyncDataPool.Allocate();
        resFrameSyncData.MessageHead = messageHead;
        resFrameSyncData.MessageType = MessageType.Ack;

        try
        {
            byte[] data = Serialize(resFrameSyncData);
            _mSocket.SendTo(data, RemoteEp);
        }
        catch (SocketException e)
        {
            Debug.LogError("UDP SocketError:" + e);
        }
        catch (Exception e)
        {
            Debug.LogError("Exception:" + e);
        }
        finally
        {
            _mMessageHeadPool.Release(messageHead);
            _mResFrameSyncDataPool.Release(resFrameSyncData);
        }
    }

    private void Close()
    {

        _mSocket.Dispose();
    }

    private byte[] Serialize(ResFrameSyncData resFrameSyncData)
    {
        byte[] data = resFrameSyncData.ToByteArray();
        return data;
    }

    private ResFrameSyncData Deserialize(byte[] data)
    {
        ResFrameSyncData resFrameSyncData = ResFrameSyncData.Parser.ParseFrom(data);
        return resFrameSyncData;
    }

    public void Dispose()
    {
        Close();
        _mSocket.Dispose();
        Disposed = true;
    }
}