using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinGameDataPacket : Packet
{

    public JoinGameDataPacket()
    {
        packetID = PacketID.JoinGameDataPacket;
    }

    public void SendPacket(int toClient, float gameTime, int numPlayers, Vector2[] positions, int[] ids, int[] type, float[] zRotations)
    {

        PacketSerialiser packet = new PacketSerialiser();
        //0 for tower 1 for minion
        packet.WriteToPacket(gameTime);
        packet.WriteToPacket(numPlayers);
        for (int i = 0; i < numPlayers; i++)
        {
            packet.WriteToPacket(type[i]);
            packet.WriteToPacket(ids[i]);
            packet.WriteToPacket(positions[i].x);
            packet.WriteToPacket(positions[i].y);
            packet.WriteToPacket(zRotations[i]);
            if (ids[i] == toClient)
            {
                string towermin = "";
                if (type[i] == 0)
                {
                    towermin = "tower";

                }
                else if (type[i] == 1)
                {
                    towermin = "minion";
                }
                Debug.Log($"send start game to client {toClient}, they will be type {towermin} and spawn at x,y: {positions[i].x}, {positions[i].y}");
            }
        }

        AddPacketHeadersAndSend(packet.m_packet, new int[] {toClient});

    }
}
