using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class DisconnectPacket : Packet
{
    public DisconnectPacket()
    {
        packetID = PacketID.Disconnect;
    }
    public void SendPacket(int clientToDisconnect)
    {
        AddPacketHeadersAndSend(new List<byte>(), new int[] { clientToDisconnect});
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        serverRef.RemoveClient((System.Net.IPEndPoint)asyncEvent.RemoteEndPoint);
    }

}
