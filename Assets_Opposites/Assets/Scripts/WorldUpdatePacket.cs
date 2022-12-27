using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class WorldUpdatePacket : Packet
{

    public WorldUpdatePacket()
    {
        packetID = PacketID.WorldUpdate;
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        PacketSerialiser packet = new PacketSerialiser(packetData);
        float gameTime = packet.ReadFloatFromPacket();
        if (GameManager.Instance.updateTimerWithOffsetTime)
        {
            GameManager.Instance.updateTimerWithOffsetTime = false;
            GameManager.Instance.updateTimerInGameLoop = true;
            GameManager.Instance.setGameTime(gameTime + GameManager.Instance.offsetTime);
        }
        GameManager.Instance.latestServerTime = gameTime;
        int minionScore = packet.ReadIntFromPacket();
        int towerScore = packet.ReadIntFromPacket();
        int numMinions = packet.ReadIntFromPacket();
        GameManager.minionDefaultMessage[] minionMessages = new GameManager.minionDefaultMessage[numMinions];
        for (int i = 0; i < numMinions; i++)
        {
            minionMessages[i].clientId = packet.ReadIntFromPacket();
            minionMessages[i].position = new Vector2(packet.ReadFloatFromPacket(), packet.ReadFloatFromPacket());
            minionMessages[i].time = gameTime;
        }
        int numTowers = packet.ReadIntFromPacket();
        GameManager.towerDefaultMessage[] towerMessages = new GameManager.towerDefaultMessage[numTowers];
        for (int i = 0; i < numTowers; i++)
        {
            towerMessages[i].clientId = packet.ReadIntFromPacket();
            towerMessages[i].zRotation = packet.ReadFloatFromPacket();
            towerMessages[i].time = gameTime;
        }
        GameManager.Instance.sendWorldUpdateToObjects(minionScore, towerScore, minionMessages, towerMessages);
    }

}
