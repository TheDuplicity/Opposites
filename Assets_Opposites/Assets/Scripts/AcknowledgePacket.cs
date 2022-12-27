using System;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcknowledgePacket : Packet
{
    public AcknowledgePacket()
    {
        packetID = PacketID.Acknowledge;
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
       base.HandlePacket(packetData, asyncEvent);
       // tell the server we exist so they know we know theyve accepted our connection
       SendPacket();
        PacketSerialiser packet = new PacketSerialiser(packetData);
        int ourClientID = packet.ReadIntFromPacket();
        //add a timer or something to resend after no response
        //once we read their response saying we know they're connected, both parties will have knowledge of the others connection and we will be secures
        // i.e. client connects, server acks, player acks and server acks 
        clientRef.setOurID(ourClientID);
       UIManager.instance.afterResp();

    }

    public void SendPacket()
    {
        //setup packet to send
        List<byte> sendPacket = new List<byte>();
        //send packet
        AddPacketHeadersAndSend(sendPacket);

    }
}
