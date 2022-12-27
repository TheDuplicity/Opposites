using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTowerPacket : Packet
{
    public SpawnTowerPacket()
    {
        packetID = PacketID.SpawnTower;
    }

    public void SendPacket(Vector3 towerMousePos, bool inGame)
    {

        PacketSerialiser packet = new PacketSerialiser();

        packet.WriteToPacket(towerMousePos.x);
        packet.WriteToPacket(towerMousePos.y);
        packet.WriteToPacket(towerMousePos.z);
        packet.WriteToPacket(inGame);

        AddPacketHeadersAndSend(packet.m_packet);
        
    }
}
