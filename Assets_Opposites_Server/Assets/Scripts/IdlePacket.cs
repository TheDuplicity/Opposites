using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class IdlePacket : Packet
{
    public IdlePacket()
    {
        packetID = PacketID.Idle;
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        //serverRef.FindClient((System.Net.IPEndPoint)asyncEvent.RemoteEndPoint);
    }

    public void SendPacket(int[] clients)
    {
        AddPacketHeadersAndSend(new List<byte>(), clients);
    }

}