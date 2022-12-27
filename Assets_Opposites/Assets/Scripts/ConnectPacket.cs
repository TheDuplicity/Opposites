using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectPacket : Packet
{
    public ConnectPacket()
    {
        packetID = PacketID.Connect;
    }

    public void SendPacket()
    {
        AddPacketHeadersAndSend(new List<byte>());
    }
}
