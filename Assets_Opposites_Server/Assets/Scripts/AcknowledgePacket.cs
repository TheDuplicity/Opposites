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
        Debug.Log("received client acknowledgment of our acknowledgement");
    }

    public void SendPacket(int clientID, int[] clients)
    {

        //setup packet to send
        List<byte> sendPacket = new List<byte>();
        sendPacket.AddRange(BitConverter.GetBytes(clientID));
        //send packet
        AddPacketHeadersAndSend(sendPacket, clients);

    }
}
