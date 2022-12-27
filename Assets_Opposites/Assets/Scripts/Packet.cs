﻿using System;
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

    public Client clientRef;

    public Packet()
    {
        // default no packet id
        packetID = PacketID.none;
        clientRef = null;
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

    public virtual void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        Debug.Log("packet " + (int)packetID + " handling");
        return;
    }

    public void SendSerialisedPacket(byte[] packetData)
    {
        Debug.Log("packet " + (int)packetID + " sending");
        return;
    }

    protected void AddPacketHeadersAndSend(List<byte> packet)
    {
 
        short packetLength = (short)(packet.Count);
        
        packet.InsertRange(0, BitConverter.GetBytes(packetLength));

        packet.InsertRange(0, BitConverter.GetBytes((short)packetID));


        clientRef.AddPacketToQueue(packet.ToArray());
    }

}
