using System;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class Packet
{
    public enum PacketID
    {
        none = 0,
        Connect = 1,
        Acknowledge = 2,
        clientRespond = 3,
        position = 4,
        Idle = 5,
        Random,
        SpawnPlayer,
        NewPlayerSpawnPacket,
        SpawnTower,
        SpawnMinion,
        JoinGameDataPacket,
        Disconnect,
        DisconnectBroadcast,
        TowerUpdate,
        MinionUpdate,
        WorldUpdate

    }
    public PacketID packetID;

    public byte[] packetData;

    public Server serverRef;

    public Packet()
    {
        // default no packet id
        packetID = PacketID.none;
        serverRef = null;
    }

    public virtual void Unpack()
    {
        Debug.Log("packet " + (int)packetID + "unpacking");
        return;
    }

    public virtual void Pack()
    {
        Debug.Log("packet " + (int)packetID + "unpacking");
        return;
    }
    // i didnt need to havte the byte[] packetdata since it is stored in the async event object but since i added the event later and took a while to realise i had the packet stored in it i never changed it
    public virtual void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
       // Debug.Log("packet " + (int)packetID + " handling");
        return;
    }

    public void SendSerialisedPacket(byte[] packetData)
    {
        Debug.Log("packet " + (int)packetID + " sending");
        return;
    }

    protected void AddPacketHeadersAndSend(List<byte> packet, int[] clients)
    {      

        short packetLength = (short)(packet.Count);

        packet.InsertRange(0, BitConverter.GetBytes(packetLength));

        packet.InsertRange(0,BitConverter.GetBytes((short)packetID));

        serverRef.AddPacketToQueue(clients, packet.ToArray());
    }

}
