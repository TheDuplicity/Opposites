using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlePacket : Packet
{
    public IdlePacket()
    {
        packetID = PacketID.Idle;
    }

    public void SendPacket()
    {
        AddPacketHeadersAndSend(new List<byte>());
    }

}
