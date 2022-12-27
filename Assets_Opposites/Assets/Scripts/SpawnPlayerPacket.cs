using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayerPacket : Packet
{
    public SpawnPlayerPacket()
    {
        packetID = PacketID.SpawnPlayer;
    }


}
