using System;
using SocketProtocol;



public class RoomInfo
{
    public string roomCode;
    public string roomName;
    public int maxPlayer;
    public int currentPlayers;
    public RoomMatchType RoomMatchType;

    public RoomInfo()
    {

    }

    public RoomInfo(RoomInfoPack roomInfoPack)
    {
        roomCode = roomInfoPack.RoomCode;
        roomName = roomInfoPack.RoomName;
        RoomMatchType = roomInfoPack.RoomMatchType;
        maxPlayer = roomInfoPack.MaxPlayer;
        currentPlayers = roomInfoPack.CurrentPlayers;
    }

    public override string ToString()
    {
        return $"roomCode: {roomCode}, roomName: {roomName}, roomVisibility: {RoomMatchType}" +
               $", currentPlayers: {currentPlayers}, maxPlayer: {maxPlayer}";
    }
}