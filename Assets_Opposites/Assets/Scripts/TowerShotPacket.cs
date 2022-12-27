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
    public void SendPacket()
    {
        List<byte> empty = new List<byte>();
        AddPacketHeadersAndSend(empty);
    }
    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        PacketSerialiser packet = new PacketSerialiser(packetData);
        int shootingTowerId = packet.ReadIntFromPacket();
        // would be nicer to have the orientation of the firing bullet be sent from the server and have the server extrapolate backwards in time to get the orientation at time of shooting
        GameManager.Instance.OtherTowerShot(shootingTowerId);
    }

}
