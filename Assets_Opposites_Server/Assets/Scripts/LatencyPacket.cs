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

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        PacketSerialiser packet = new PacketSerialiser(packetData);
        int arrpos;
        int clientID = serverRef.FindClient((System.Net.IPEndPoint)asyncEvent.RemoteEndPoint, out arrpos).m_ID;
        SendPacket(clientID, packet.ReadIntFromPacket());
    }

    public void SendPacket(int clientID, int latencyID)
    {

        PacketSerialiser packet = new PacketSerialiser();

        packet.WriteToPacket(latencyID);

        AddPacketHeadersAndSend(packet.m_packet,new int[] { clientID});
    }


}
