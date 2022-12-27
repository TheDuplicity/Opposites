using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class SpawnTowerPacket : Packet
{
    public SpawnTowerPacket()
    {
        packetID = PacketID.SpawnTower;
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        PacketSerialiser pack = new PacketSerialiser(packetData);
        Vector3 towerMousePos = new Vector3(pack.ReadFloatFromPacket(), pack.ReadFloatFromPacket(), pack.ReadFloatFromPacket());
        int clientArrPos;
        int clientID = -1;
        clientID = serverRef.FindClient((System.Net.IPEndPoint)asyncEvent.RemoteEndPoint, out clientArrPos).m_ID;
        //if you were spawned and the game has started: send welcome package
        if (GameManager.Instance.trySpawnClientAsTower(clientID, towerMousePos))
        {
            bool inGame = pack.ReadBoolFromPacket();
            if (!inGame)
            {
                if (GameManager.Instance.gameStarted)
                {
                    GameManager.Instance.sendWelcomePackage(clientID);
                }

            }
            else
            {
                GameManager.Instance.tellOtherPlayersIExist(clientID);
            }
        }
    }
}
