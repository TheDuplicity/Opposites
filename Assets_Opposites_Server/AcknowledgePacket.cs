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
        //setup packet to send
        List<byte> sendPacket = new List<byte>();
        //send packet
        serverRef.AsyncSendPacket(sendPacket.ToArray());
    }

    public void SendPacket(int ackMessage)
    {
        
        //setup packet to send
        List<byte> sendPacket = new List<byte>();
        sendPacket.AddRange(BitConverter.GetBytes(ackMessage));
        //send packet
        AddPacketHeadersAndSend(sendPacket);
    }
}
