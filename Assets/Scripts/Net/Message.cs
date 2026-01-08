using System;
using System.Linq;
using System.Reflection;
using Google.Protobuf;
using SocketProtocol;
using UnityEngine;

public class Message
{
    /// <summary>
    /// 消息头长度
    /// </summary>
    public const int MESSAGE_HEADER_LEN = 4;

    private byte[] _mBuffer; //字段保证直是同一块内存；不会频繁分配、释放；
    private int _mMessageLen;

    public Message()
    {
        _mBuffer = new byte[1024];
        _mMessageLen = 0;
    }

    public byte[] Buffer => _mBuffer;

    public int MessageLen => _mMessageLen;

    public int RemainSize => _mBuffer.Length - _mMessageLen;

    /// <summary>
    /// 可以用这样的方式代替 MainPack.Parser.ParseFrom
    /// </summary>
    /// <param name="bytes"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetProto<T>(byte[] bytes) where T : class {
        //TestMsg.Parser.ParseFrom(MemoryStream ms);
        
        Type type = typeof(T); //反射
        PropertyInfo propertyInfo = type.GetProperty("Parser");
        object parserObj = propertyInfo.GetValue(null, null);//得到静态成员属性对象
        //已经得到对象 进而得到对象的对应方法
        Type parserType = parserObj.GetType();
        // object msg = parserType.GetMethod("ParseFrom",new Type[] { typeof(byte[]) }).Invoke(parserObj, new object[] { bytes });
        MethodInfo methodInfo = parserType.GetMethod("ParseFrom",new Type[] { typeof(byte[]) });
        object msg = methodInfo.Invoke(parserObj, new object[] { bytes });

        return msg as T;
    }
    
    
    
    public void ReadBuffer(int len, Action<MainPack> onMainPackDeserialize)
    {
        _mMessageLen += len;

        while (true)
        {
            if (_mMessageLen < MESSAGE_HEADER_LEN)
                break;

            int bodyLen = BitConverter.ToInt32(_mBuffer, 0);
            if (_mMessageLen < bodyLen + MESSAGE_HEADER_LEN)
                break;

            try
            {
                MainPack pack = MainPack.Parser.ParseFrom(
                    _mBuffer,
                    MESSAGE_HEADER_LEN,
                    bodyLen
                );

                onMainPackDeserialize?.Invoke(pack);

                int remain = _mMessageLen - (bodyLen + MESSAGE_HEADER_LEN);
                System.Buffer.BlockCopy(
                    _mBuffer,
                    bodyLen + MESSAGE_HEADER_LEN,
                    _mBuffer,
                    0,
                    remain
                );

                _mMessageLen = remain;
            }
            catch (Exception e)
            {
                Debug.LogError($"Tcp message serialize error: {e}");
                break;
            }
        }
    }

    public static byte[] GetPackData(MainPack pack)
    {
        byte[] body = pack.ToByteArray();
        byte[] head = BitConverter.GetBytes(body.Length);
        return head.Concat(body).ToArray();
    }
}