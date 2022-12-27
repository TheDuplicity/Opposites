using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class PlayerDiedPacket : Packet
{
    public PlayerDiedPacket()
    {
        packetID = PacketID.PlayerDied;
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        PacketSerialiser packet = new PacketSerialiser(packetData);
        int deadPlayerId = packet.ReadIntFromPacket();
        GameManager.Instance.KillObject(deadPlayerId);
    }
}
