using System;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class SendIDPacket : Packet
{
    public SendIDPacket()
    {
        packetID = PacketID.Random;
    }
    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        PacketSerialiser pack = new PacketSerialiser(packetData);
        int i = pack.ReadIntFromPacket();
        float f = pack.ReadFloatFromPacket();
        string s = pack.ReadStringFromPacket();
        bool b = pack.ReadBoolFromPacket();
         char c = pack.ReadCharFromPacket();
        short sh = pack.ReadShortFromPacket();

        Debug.Log("int: " + i+", float: " + f+", string: " + s+", bool: " + b+", char: " + c+", short: " + sh);
    }
    public void SendPacket(int sendval)
    {
        int[] clientIds = serverRef.GetAllClientIDs();
        if (clientIds.Length > 0)
        {
            List<byte> locations = new List<byte>();
            locations.AddRange(BitConverter.GetBytes(sendval));
            AddPacketHeadersAndSend(locations, clientIds);
        }

    }
}
