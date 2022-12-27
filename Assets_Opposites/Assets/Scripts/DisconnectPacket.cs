using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine;

public class DisconnectPacket : Packet
{
    public DisconnectPacket()
    {
        packetID = PacketID.Disconnect;
    }

    public void SendPacket()
    {
        AddPacketHeadersAndSend(new List<byte>());
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        //load the menu when server disconnects us so we can conect back if needed
        SceneManager.LoadScene(0, LoadSceneMode.Single);
        //maybe have a connected state/ bool and only send packets if youre connected unless youre retrying a connection
    }

}
