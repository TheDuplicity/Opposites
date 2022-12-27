using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class TowerShotPacket : Packet
{
    public TowerShotPacket()
    {
        packetID = PacketID.TowerShot;
    }
    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        int arrPos;
        int shotClientID = serverRef.FindClient((System.Net.IPEndPoint)asyncEvent.RemoteEndPoint, out arrPos).m_ID;
        GameManager.Instance.shootBulletFromTower(shotClientID);
        SendPacket(GameManager.Instance.GetInGamePlayerAndSpectatorIDs(), shotClientID);
    }
    public void SendPacket(int[] toClients, int towerShotId)
    {

        PacketSerialiser packet = new PacketSerialiser();

        packet.WriteToPacket(towerShotId);

        AddPacketHeadersAndSend(packet.m_packet, toClients);
      
    }
}
