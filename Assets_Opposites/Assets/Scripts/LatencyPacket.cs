using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class LatencyPacket : Packet
{

    public LatencyPacket()
    {
        packetID = PacketID.LatencyPacket;
    }

    public void SendPacket(int latencyID)
    {

        PacketSerialiser packet = new PacketSerialiser();
        packet.WriteToPacket(latencyID);
        AddPacketHeadersAndSend(packet.m_packet);
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        PacketSerialiser packet = new PacketSerialiser(packetData);
        int latencyID = packet.ReadIntFromPacket();
        // add latency to player
        clientRef.EndLatencyCheck(latencyID);
    }
}
