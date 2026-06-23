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

    private readonly ObjectPool<ReqFrameSyncData> _mReqFrameSyncDataPool;
    public int UdpListenPort { get; private set; }
    public bool Disposed { get; private set; }
    public IPEndPoint RemoteEp { get; private set; }
    public event Action<ResFrameSyncData> OnReceiveFrameSync;
    public event Action<ResFrameSyncData> OnReceiveFrameDismatch;

    public UdpListener()
    {
        _mReqFrameSyncDataPool = new ObjectPool<ReqFrameSyncData>(() => new ReqFrameSyncData());
        _mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _mSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
        IPEndPoint clientLocalEndPoint = _mSocket.LocalEndPoint as IPEndPoint;
        UdpListenPort = clientLocalEndPoint.Port;
        Debug.Log("Current UDP available port:" + UdpListenPort);
        RemoteEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10040);
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

        try
        {
            if (e.SocketError == SocketError.Success)
            {
                int length = e.BytesTransferred;

                if (length > 0)
                {
                    var msg =
                        ResFrameSyncData.Parser.ParseFrom(
                            e.Buffer,
                            0,
                            length);
                    switch (msg.MessageType)
                    {
                        case MessageType.FrameSync:
                            OnReceiveFrameSync?.Invoke(msg);
                            SendAck();
                            break;

                        case MessageType.Dismatch:
                            OnReceiveFrameDismatch?.Invoke(msg);
                            break;

                        case MessageType.Checksum:
                            Debug.Log("Receive Checksum");
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        finally
        {
            StartReceive(e);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void Send(in ReqFrameSyncData reqFrameSyncData)
    {
        if (Disposed) return;
        reqFrameSyncData.MessageType = MessageType.FrameSync;

        try
        {
            byte[] data = Serialize(reqFrameSyncData);

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
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void SendChecksum(ReqFrameSyncData req) {
        if (Disposed) return;
        req.MessageType=MessageType.Checksum;
        try
        {
            byte[] data = Serialize(req);
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
    }

    private void SendAck()
    {
        if (Disposed) return;


        ReqFrameSyncData resFrameSyncData = _mReqFrameSyncDataPool.Allocate();
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
            _mReqFrameSyncDataPool.Release(resFrameSyncData);
        }
    }

    private void Close()
    {

        _mSocket.Dispose();
    }

    private byte[] Serialize(ReqFrameSyncData resFrameSyncData)
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