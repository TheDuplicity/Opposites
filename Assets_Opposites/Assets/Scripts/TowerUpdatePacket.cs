using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerUpdatePacket : Packet
{
    public TowerUpdatePacket()
    {
        packetID = PacketID.TowerUpdate;
    }

    public void SendPacket(GameManager.towerDefaultMessage message)
    {

        PacketSerialiser packet = new PacketSerialiser();

        packet.WriteToPacket(message.time);
        packet.WriteToPacket(message.zRotation);
        AddPacketHeadersAndSend(packet.m_packet);
        
    }
}
