using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectPacket : Packet
{
   public ConnectPacket()
    {
        packetID = PacketID.Connect;
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        if (serverRef.CreateClient((System.Net.IPEndPoint)asyncEvent.RemoteEndPoint))
        {
            ((AcknowledgePacket)serverRef.FindPacket((int)PacketID.Acknowledge)).SendPacket(92);

        }
        //
        // maybe have packet queue for each client and send packets to them that way
        // we would then maybe send an array of clientids on the sendpacket call alongside the packet data

    }
}
