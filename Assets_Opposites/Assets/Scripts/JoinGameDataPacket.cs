using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class JoinGameDataPacket : Packet
{
    public JoinGameDataPacket()
    {

        packetID = PacketID.JoinGameDataPacket;

    }
    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        PacketSerialiser packet = new PacketSerialiser(packetData);
        DataFromMenuToLevel dataStorage = GameObject.FindObjectOfType<DataFromMenuToLevel>();
        dataStorage.serverGameTime = packet.ReadFloatFromPacket();
        int numPlayers = packet.ReadIntFromPacket();
        dataStorage.instantiateArrays(numPlayers);
        for (int i = 0; i < numPlayers; i++)
        {
            dataStorage.types[i] = packet.ReadIntFromPacket();
            dataStorage.ids[i] = packet.ReadIntFromPacket();
            dataStorage.positions[i] = new Vector2(packet.ReadFloatFromPacket(), packet.ReadFloatFromPacket());
            dataStorage.zRotations[i] = packet.ReadFloatFromPacket();
        }

        Debug.Log("got data from server, loading level");
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
}
