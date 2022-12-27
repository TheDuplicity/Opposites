using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUpdatePacket : Packet
{
    public WorldUpdatePacket()
    {
        packetID = PacketID.WorldUpdate;
    }
    public void SendPacket(int[] toClients, float gameTime, int minionScore, int towerScore, GameManager.minionDefaultMessage[] minionMessages, GameManager.towerDefaultMessage[] towerMessages)
    {
        PacketSerialiser packet = new PacketSerialiser();

        packet.WriteToPacket(gameTime);
        packet.WriteToPacket(minionScore);
        packet.WriteToPacket(towerScore);
        packet.WriteToPacket(minionMessages.Length);
        for (int i = 0; i < minionMessages.Length; i++)
        {
            packet.WriteToPacket(minionMessages[i].clientId);
            packet.WriteToPacket(minionMessages[i].position.x);
            packet.WriteToPacket(minionMessages[i].position.y);
        }
        packet.WriteToPacket(towerMessages.Length);
        for (int i = 0; i < towerMessages.Length; i++)
        {
            packet.WriteToPacket(towerMessages[i].clientId);
            packet.WriteToPacket(towerMessages[i].zRotation);
        }
        AddPacketHeadersAndSend(packet.m_packet, toClients);

    }
}
