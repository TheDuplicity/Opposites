using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class SpawnMinionPacket : Packet
{
    public SpawnMinionPacket()
    {
        packetID = PacketID.SpawnMinion;
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        int empty;
        int fromClient = serverRef.FindClient((System.Net.IPEndPoint)asyncEvent.RemoteEndPoint, out empty).m_ID;
        PacketSerialiser packet = new PacketSerialiser(packetData);
        bool inGame = packet.ReadBoolFromPacket();

        if (GameManager.Instance.addMinion(fromClient))
        {
            if (!inGame)
            {
                if (GameManager.Instance.gameStarted)
                {
                    GameManager.Instance.sendWelcomePackage(fromClient);
                }
            }
            else
            {
                GameManager.Instance.tellOtherPlayersIExist(fromClient);
            }
        }

    }
}
