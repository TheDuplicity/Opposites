using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Client : MonoBehaviour
{

    Socket m_clientSocket;
    System.Net.IPAddress m_serverSocketIPAddress;
    System.Net.IPEndPoint m_serverEndPoint;

    Queue<byte[]> m_pendingPackets;
    List<Packet> m_packetRefs;
    // send a message after2.5 seconds
    float m_timeSinceLastReceivedPacket = 0;

    List<KeyValuePair<float, bool>> m_idleMessageTimeThresholds;
    float m_timeoutVal = 5;
    float m_timeSinceLastSentPacket = 0;

    // Start is called before the first frame update
    void Start()
    {

        m_idleMessageTimeThresholds = new List<KeyValuePair<float, bool>>();

        m_idleMessageTimeThresholds.Add(new KeyValuePair<float, bool>((float)2.5, false));
        m_idleMessageTimeThresholds.Add(new KeyValuePair<float, bool>((float)3.5, false));

        m_pendingPackets = new Queue<byte[]>();

        m_packetRefs = new List<Packet>();
        m_packetRefs.Add(new AcknowledgePacket());
        m_packetRefs.Add(new IdlePacket());

        foreach (Packet packet in m_packetRefs)
        {
            packet.clientRef = this;
        }

        m_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        byte[] m_serverIP = new byte[4];
        m_serverIP[0] = 127;
        m_serverIP[1] = 0;
        m_serverIP[2] = 0;
        m_serverIP[3] = 1;

        m_serverSocketIPAddress = new System.Net.IPAddress(m_serverIP);
        m_serverEndPoint = new System.Net.IPEndPoint(m_serverSocketIPAddress, 32612);

        List<byte> sendData = new List<byte>();
        short packetID = 1, clientID = 1;
        sendData.AddRange(BitConverter.GetBytes(packetID));

        int x = 10, y = 839, z = 123;
        sendData.AddRange(BitConverter.GetBytes(clientID));
        sendData.AddRange(BitConverter.GetBytes(x));
        sendData.AddRange(BitConverter.GetBytes(y));
        sendData.AddRange(BitConverter.GetBytes(z));
        short packetLength = (short)(sendData.Count - 2);
        sendData.InsertRange(2, BitConverter.GetBytes(packetLength));
        byte[] check = new byte[sendData.Count];
        check = sendData.ToArray();
        m_clientSocket.SendTo(sendData.ToArray(), m_serverEndPoint);


        SocketAsyncEventArgs arg = new SocketAsyncEventArgs();

        arg.SetBuffer(new byte[256], 0, 256);

        arg.Completed += OnCompleted;

        arg.RemoteEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.IPv6Any, 0);

        if (!m_clientSocket.ReceiveFromAsync(arg))
        {
            OnCompleted(this, arg);
        }

    }

    public void AddPacketToQueue(byte[] packet)
    {
        m_pendingPackets.Enqueue(packet);
    }

    void SendPackets()
    {
        for (int i = 0; i < m_idleMessageTimeThresholds.Count; i++)
        {
            if (m_idleMessageTimeThresholds[i].Value == true)
            {
                m_idleMessageTimeThresholds[i] = new KeyValuePair<float, bool>(m_idleMessageTimeThresholds[i].Key, false);
            }
        }

    }

    void OnCompleted(object sender, SocketAsyncEventArgs args)
    {

        switch (args.LastOperation)
        {
            case SocketAsyncOperation.Accept:
                break;
            case SocketAsyncOperation.Connect:
                break;
            case SocketAsyncOperation.Disconnect:
                break;
            case SocketAsyncOperation.None:
                break;
            case SocketAsyncOperation.Receive:
                HandleReceive(args);
                break;
            case SocketAsyncOperation.ReceiveFrom:
                HandleReceive(args);
                break;
            case SocketAsyncOperation.ReceiveMessageFrom:
                break;
            case SocketAsyncOperation.Send:
                break;
            case SocketAsyncOperation.SendPackets:
                break;
            case SocketAsyncOperation.SendTo:
                break;
            default:
                break;
        }
    }



    void HandleReceive(SocketAsyncEventArgs args)
    {

        short packetReceivedID = BitConverter.ToInt16(args.Buffer, 0);
        short messageLength = BitConverter.ToInt16(args.Buffer, 2);

        byte[] packetData = new byte[messageLength];
        Array.Copy(args.Buffer, 4, packetData, 0, messageLength);

        foreach (Packet packet in m_packetRefs)
        {
            if ((short)packet.packetID == packetReceivedID)
            {
                packet.HandlePacket(packetData, args);
            }
        }

       // Debug.Log("PacketID: " + packetReceivedID + ", number sent: " + numbersend);

        if (!m_clientSocket.ReceiveFromAsync(args))
        {
            OnCompleted(this, args);
        }
    }


    // Update is called once per frame
    void Update()
    {
        m_timeSinceLastSentPacket += Time.deltaTime;
        m_timeSinceLastReceivedPacket += Time.deltaTime;

            for (int i = 0; i < m_idleMessageTimeThresholds.Count; i++)
            {
                KeyValuePair<float, bool> timeThreshold = m_idleMessageTimeThresholds[i];
                if (m_timeSinceLastReceivedPacket >= timeThreshold.Key && !timeThreshold.Value)
                {
                    //queue up an idleCheck Packet
                    IdlePacket p = (IdlePacket)(FindPacket((int)Packet.PacketID.Idle));
                    p.SendPacket();
                    m_idleMessageTimeThresholds[i] = new KeyValuePair<float, bool>(timeThreshold.Key, true);
                    break;
                }
            }

        

        foreach (byte[] packet in m_pendingPackets)
        {
            AsyncSendPacket(packet);
        }
        m_pendingPackets.Clear();

    }

    public Packet FindPacket(int packetID)
    {
        foreach (Packet packet in m_packetRefs)
        {
            if ((int)packet.packetID == packetID)
            {
                return packet;
            }
        }
        return null;
    }

    public void AsyncSendPacket(byte[] packet)
    {
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();

        args.SetBuffer(packet, 0, packet.Length);
        args.Completed += OnCompleted;
        args.RemoteEndPoint = m_serverEndPoint;

        Debug.Log("sending packet to server endpoint: " + m_serverEndPoint);


        m_clientSocket.SendToAsync(args);
    }

}


