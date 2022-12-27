using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class TowerUpdatePacket : Packet
{
    public TowerUpdatePacket()
    {
        packetID = PacketID.TowerUpdate;
    }
    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        PacketSerialiser packet = new PacketSerialiser(packetData);
        GameManager.towerDefaultMessage message;
        message.time = packet.ReadFloatFromPacket();
        message.zRotation = packet.ReadFloatFromPacket();
        int arrPos;
        int clientID = serverRef.FindClient((System.Net.IPEndPoint)asyncEvent.RemoteEndPoint, out arrPos).m_ID;
        message.clientId = clientID;
        GameManager.Instance.updateTower(clientID, message);
    }

}
