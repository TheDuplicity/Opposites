using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerSpawnPacket : Packet
{

    public NewPlayerSpawnPacket()
    {
        packetID = PacketID.NewPlayerSpawnPacket;
    }

   
    public void SendPacket(Vector2 position, int id, int type, float zRotations, int[] sendToClients)
    {

        PacketSerialiser pack = new PacketSerialiser();
        pack.WriteToPacket(id);
        pack.WriteToPacket(type);
        pack.WriteToPacket(zRotations);
        pack.WriteToPacket(position.x);
        pack.WriteToPacket(position.y);

        AddPacketHeadersAndSend(pack.m_packet, sendToClients);

    }
}
