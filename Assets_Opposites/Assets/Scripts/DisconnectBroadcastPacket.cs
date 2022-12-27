using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class DisconnectBroadcastPacket : Packet
{
    public DisconnectBroadcastPacket()
    {
        packetID = PacketID.DisconnectBroadcast;
    }
    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        PacketSerialiser packet = new PacketSerialiser(packetData);
        int disconnectedClientID = packet.ReadIntFromPacket();
        GameManager.Instance.RemoveDisconnectedPlayer(disconnectedClientID);
        //disconnect client from game

    }
}
