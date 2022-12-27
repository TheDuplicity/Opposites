using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectBroadcastPacket : Packet
{
    public DisconnectBroadcastPacket()
    {
        packetID = PacketID.DisconnectBroadcast;
    }

    public void SendPacket(int disconnectedClientID, int[] sendToClients)
    {
        PacketSerialiser packet = new PacketSerialiser();
        packet.WriteToPacket(disconnectedClientID);
        AddPacketHeadersAndSend(packet.m_packet, sendToClients);
    }

}
