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
        serverRef.FindClient((System.Net.IPEndPoint)asyncEvent.RemoteEndPoint).ResetTimoutTimer();
    }

    public void SendPacket()
    {
        AddPacketHeadersAndSend(new List<byte>());
    }

}