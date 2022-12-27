using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NewPlayerSpawnPacket : Packet
{

    public NewPlayerSpawnPacket()
    {
        packetID = PacketID.NewPlayerSpawnPacket;
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        PacketSerialiser pack = new PacketSerialiser(packetData);
        int id = pack.ReadIntFromPacket();
        int type = pack.ReadIntFromPacket();
        float zRot = pack.ReadFloatFromPacket();
        Vector2 position = new Vector2(pack.ReadFloatFromPacket(), pack.ReadFloatFromPacket());

        GameManager.Instance.CreateNewObject(id, type, position, zRot);
    }

}
