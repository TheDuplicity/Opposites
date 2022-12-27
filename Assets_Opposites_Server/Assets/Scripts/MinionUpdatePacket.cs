using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class MinionUpdatePacket : Packet
{
    public MinionUpdatePacket()
    {
        packetID = PacketID.MinionUpdate;
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        GameManager.minionDefaultMessage message;
        PacketSerialiser packet = new PacketSerialiser(packetData);
        message.time = packet.ReadFloatFromPacket();
        message.position = new Vector2(packet.ReadFloatFromPacket(), packet.ReadFloatFromPacket());
        int arrpos;
        message.clientId = serverRef.FindClient((System.Net.IPEndPoint)asyncEvent.RemoteEndPoint, out arrpos).m_ID;
        GameManager.Instance.UpdateMinion(message.clientId, message);
    }
}
