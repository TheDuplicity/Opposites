using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketSerialiser
{
    public List<byte> m_packet { get; private set; }
    int m_writePositionPointer = 0;
    int m_readPositionPointer = 0;
    // Start is called before the first frame update
    public PacketSerialiser()
    {

        m_packet = new List<byte>();

    }
    public PacketSerialiser(byte[] packetToRead)
    {

        m_packet = new List<byte>();
        m_packet.AddRange(packetToRead);
    }
    #region writing functions

    //write to the position stated unless it is outside of the bounds of the array, in which case write to the end
    public void WriteToPacketEnd(ref int packetPos)
    {
        if (packetPos < 0 || packetPos > m_packet.Count)
        {
            m_writePositionPointer = m_packet.Count;
            packetPos = m_writePositionPointer;
        }
        else
        {
            m_writePositionPointer = packetPos;
        }

    }
    public void WriteToPacket(float writeFloat, int packetPosition = -1)
    {
        //if we dont specify a position, we will write to the end of the packet
        WriteToPacketEnd(ref packetPosition);
        m_packet.InsertRange(m_writePositionPointer, BitConverter.GetBytes(writeFloat));
        m_writePositionPointer += sizeof(float);
    }
    public void WriteToPacket(int writeInt, int packetPosition = -1)
    {
        //if we dont specify a position, we will write to the end of the packet
        WriteToPacketEnd(ref packetPosition);
        m_packet.InsertRange(m_writePositionPointer, BitConverter.GetBytes(writeInt));
        m_writePositionPointer += sizeof(int);
    }
    public void WriteToPacket(short writeShort, int packetPosition = -1)
    {
        //if we dont specify a position, we will write to the end of the packet
        WriteToPacketEnd(ref packetPosition);
        m_packet.InsertRange(m_writePositionPointer, BitConverter.GetBytes(writeShort));
        m_writePositionPointer += sizeof(short);
    }
    public void WriteToPacket(bool writeBool, int packetPosition = -1)
    {
        //if we dont specify a position, we will write to the end of the packet
        WriteToPacketEnd(ref packetPosition);
        m_packet.InsertRange(m_writePositionPointer, BitConverter.GetBytes(writeBool));
        m_writePositionPointer += sizeof(bool);
    }
    public void WriteToPacket(char writeCharacter, int packetPosition = -1)
    {
        //if we dont specify a position, we will write to the end of the packet
        WriteToPacketEnd(ref packetPosition);
        m_packet.InsertRange(m_writePositionPointer, BitConverter.GetBytes(writeCharacter));
        m_writePositionPointer += sizeof(char);
    }
    public void WriteToPacket(string writeString, int packetPosition = -1)
    {
        //if we dont specify a position, we will write to the end of the packet
        WriteToPacketEnd(ref packetPosition);

        //write the byte size of the string at the start of the message so when reading we know how many characters
        int stringByteLength = (sizeof(char) * writeString.Length);
        WriteToPacket(stringByteLength, m_writePositionPointer);

        //write the byte size of the characters so the reader knows how big the characters are supposed to be
        short characterSize = (short)sizeof(char);
        WriteToPacket(characterSize, m_writePositionPointer);

        foreach (char stringChar in writeString)
        {
            m_packet.InsertRange(m_writePositionPointer, BitConverter.GetBytes(stringChar));
            m_writePositionPointer += sizeof(char);
        }
    }
    #endregion

    #region reading functions

    public float ReadFloatFromPacket(int readPosition = -1)
    {
        if (readPosition < 0 || readPosition >= m_packet.Count)
        {
            //normal read
            readPosition = m_readPositionPointer;
            //increment the read position only if no valid position supplied
            m_readPositionPointer += sizeof(float);
        }

        return BitConverter.ToSingle(m_packet.ToArray(), readPosition);

    }

    public int ReadIntFromPacket(int readPosition = -1)
    {
        if (readPosition < 0 || readPosition >= m_packet.Count)
        {
            //normal read
            readPosition = m_readPositionPointer;
            //increment the read position only if no valid position supplied
            m_readPositionPointer += sizeof(int);
        }

        return BitConverter.ToInt32(m_packet.ToArray(), readPosition);

    }

    public bool ReadBoolFromPacket(int readPosition = -1)
    {
        if (readPosition < 0 || readPosition >= m_packet.Count)
        {
            //normal read
            readPosition = m_readPositionPointer;
            //increment the read position only if no valid position supplied
            m_readPositionPointer += sizeof(bool);
        }

        return BitConverter.ToBoolean(m_packet.ToArray(), readPosition);

    }

    public short ReadShortFromPacket(int readPosition = -1)
    {
        if (readPosition < 0 || readPosition >= m_packet.Count)
        {
            //normal read
            readPosition = m_readPositionPointer;
            //increment the read position only if no valid position supplied
            m_readPositionPointer += sizeof(short);
        }

        return BitConverter.ToInt16(m_packet.ToArray(), readPosition);

    }
    public char ReadCharFromPacket(int readPosition = -1)
    {
        if (readPosition < 0 || readPosition >= m_packet.Count)
        {
            //normal read
            readPosition = m_readPositionPointer;
            //increment the read position only if no valid position supplied
            m_readPositionPointer += sizeof(char);
        }

        return BitConverter.ToChar(m_packet.ToArray(), readPosition);

    }

    public string ReadStringFromPacket(int readPosition = -1)
    {
        int stringByteLength;
        short charSize;
        int stringLength;
        if (readPosition < 0 || readPosition >= m_packet.Count)
        {
            stringByteLength = ReadIntFromPacket();
            charSize = ReadShortFromPacket();

            readPosition = m_readPositionPointer;
            //increment the read position only if no valid position supplied
            m_readPositionPointer += stringByteLength;
        }
        else
        {
            stringByteLength = ReadIntFromPacket();
            readPosition += sizeof(int);
            charSize = ReadShortFromPacket();
            readPosition += sizeof(short);
        }

        stringLength = stringByteLength / charSize;

        string str = "";
        for (int i = 0; i < stringLength; i++)
        {
            str += BitConverter.ToChar(m_packet.ToArray(), readPosition);
            readPosition += sizeof(char);
        }

        return str;

    }
    #endregion
}
