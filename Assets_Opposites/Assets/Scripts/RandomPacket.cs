using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class RandomPacket : Packet
{
    public RandomPacket()
    {
        packetID = PacketID.Random;
    }
    public void SendPacket(int i, float f, string s, bool b, char c, short sh)
    {
        PacketSerialiser pack = new PacketSerialiser();
        pack.WriteToPacket(i);
        pack.WriteToPacket(f);
        pack.WriteToPacket(s);
        pack.WriteToPacket(b);
        pack.WriteToPacket(c);
        pack.WriteToPacket(sh);
        Debug.Log("before: int: " + i + ", float: " + f + ", string: " + s + ", bool: " + b + ", char: " + c + ", short: " + sh);
        i = pack.ReadIntFromPacket();
        f = pack.ReadFloatFromPacket();
        s = pack.ReadStringFromPacket();
        b = pack.ReadBoolFromPacket();
        c = pack.ReadCharFromPacket();
        sh = pack.ReadShortFromPacket();

        Debug.Log("after: int: " + i + ", float: " + f + ", string: " + s + ", bool: " + b + ", char: " + c + ", short: " + sh);
        AddPacketHeadersAndSend(pack.m_packet);
    }
    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        int randomint = BitConverter.ToInt32(packetData, 0);
        Debug.Log("received random value: " + randomint);
    }

}
