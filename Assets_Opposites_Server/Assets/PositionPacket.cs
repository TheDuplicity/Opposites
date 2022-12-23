using System;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionPacket : Packet
{
    // Start is called before the first frame update
    public PositionPacket()
    {
        packetID = PacketID.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public override void Unpack()
    {
        base.Unpack();
        return;
    }

    public override void Pack()
    {
        base.Pack();
        return;
    }

    public override void HandlePacket(byte[] packetData, SocketAsyncEventArgs asyncEvent)
    {
        base.HandlePacket(packetData, asyncEvent);
        int position = 0;

        short clientID = BitConverter.ToInt16(packetData, position);
        position += 2;
    

        int xPos = BitConverter.ToInt32(packetData, position);
         position += 4;
        int yPos = BitConverter.ToInt32(packetData, position);
        position += 4;
        int zPos = BitConverter.ToInt32(packetData, position);
        position += 4;
        //unpack

        Debug.Log("x: " + xPos + ". y: " + yPos + ". z: " + zPos + ".");
        return;
    }
}
