using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDiedPacket : Packet
{
    public PlayerDiedPacket()
    {
        packetID = PacketID.PlayerDied;
    }

    public void SendPacket(int[] toClients, int deadPlayerId)
    {

        PacketSerialiser packet = new PacketSerialiser();
            //send data
        packet.WriteToPacket(deadPlayerId);
        AddPacketHeadersAndSend(packet.m_packet, toClients);

        
    }
}
