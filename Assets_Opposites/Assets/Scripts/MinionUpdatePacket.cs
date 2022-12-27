using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionUpdatePacket : Packet
{
    public MinionUpdatePacket()
    {
        packetID = PacketID.MinionUpdate;
    }

    public void SendPacket(GameManager.minionDefaultMessage message)
    {

        PacketSerialiser packet = new PacketSerialiser();
            packet.WriteToPacket(message.time);
            packet.WriteToPacket(message.position.x);
            packet.WriteToPacket(message.position.y);
        AddPacketHeadersAndSend(packet.m_packet);
        
    }
}
