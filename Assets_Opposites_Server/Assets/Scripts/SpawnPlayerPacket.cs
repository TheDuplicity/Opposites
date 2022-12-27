using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class SpawnPlayerPacket : Packet
{
    public SpawnPlayerPacket()
    {
        packetID = PacketID.SpawnPlayer;
    }


}
