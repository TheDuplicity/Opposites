using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMinionPacket : Packet
{
    public SpawnMinionPacket()
    {
        packetID = PacketID.SpawnMinion;
    }

    public void SendPacket(bool inGame)
    {

        PacketSerialiser packet = new PacketSerialiser();
        packet.WriteToPacket(inGame);
        AddPacketHeadersAndSend(packet.m_packet);
        
    }
}
