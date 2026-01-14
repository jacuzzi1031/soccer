using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPlayerInfo : PlayerInfo
{
    public bool ready=false;
    public int seatIndex;
    public bool isHome;
    public override string ToString() {
        return ready.ToString()+seatIndex.ToString()+username;
    }
}