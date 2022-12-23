using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
    //IP 143.226.212.81
    byte[] m_serverAddress;
    long m_serverAddressBigEndeanLong;
    // port number 32612
    int m_portNumber = 32612;
    System.Net.IPAddress m_serverIP;

    Socket m_listenSocket;
    byte[] m_listenSocketBuffer;

   List<Packet> packetRefs;

    int m_maxClients = 32;

    List<Client> m_clients;
    Queue<Byte[]> m_pendingPackets;

    SocketAsyncEventArgs m_asyncReceiveEventArgs;

    public Client FindClient(System.Net.IPEndPoint clientEP)
    {
        foreach (Client client in m_clients)
        {
            if (client.m_clientEndPoint.Address == clientEP.Address)
            {
                Debug.Log("addresses match");
            }
            if (client.m_clientEndPoint.Port == clientEP.Port)
            {
                Debug.Log("ports match");
            }
            if (client.m_clientEndPoint.Address == clientEP.Address && client.m_clientEndPoint.Port == clientEP.Port)
            {
                return client;
            }
        }
        return null;
    }

    public bool CreateClient(System.Net.IPEndPoint clientEndPoint)
    {
        foreach (Client client in m_clients)
        {
            if (client.m_clientEndPoint == clientEndPoint)
            {
                return true;
            }
        }
        if (m_clients.Count < m_maxClients)
        {

            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();

            eventArgs.SetBuffer(new byte[256], 0, 256);
            eventArgs.Completed += OnCompleted;
            eventArgs.RemoteEndPoint = clientEndPoint;
            Client newClient = new Client(clientEndPoint, eventArgs);
            m_clients.Add(newClient);
            return true;
        }

        return false;
    }

    public Packet FindPacket(int packetID)
    {
        foreach (Packet packet in packetRefs)
        {
            if ((int)packet.packetID == packetID)
            {
                return packet;
            }
        }
        return null;
    }

    public void AddPacketToQueue(byte[] newPacket)
    {
        m_pendingPackets.Enqueue(newPacket);
    }

    // Start is called before the first frame update
    void Start()
    {

        m_asyncReceiveEventArgs = new SocketAsyncEventArgs();

        m_clients = new List<Client>();

        m_pendingPackets = new Queue<byte[]>();

       packetRefs = new List<Packet>();

        m_listenSocketBuffer = new byte[264];
        m_serverAddress = new byte[4];
        m_serverAddress[0] = 127;
        m_serverAddress[1] = 0;
        m_serverAddress[2] = 0;
        m_serverAddress[3] = 1;

        m_serverAddressBigEndeanLong = ConvertEndeannesBytes(m_serverAddress[0], m_serverAddress[1], m_serverAddress[2], m_serverAddress[3]);

        //m_listenSocket.Listen(0);
        
        packetRefs.Add(new PositionPacket());
        packetRefs.Add(new AcknowledgePacket());
        packetRefs.Add( new ConnectPacket());
        packetRefs.Add(new IdlePacket());
       // packetRefs.Add(new Packet());
        foreach (Packet packet in packetRefs)
        {   
            packet.serverRef = this;
        }

        m_serverIP = new System.Net.IPAddress(m_serverAddress);

        Thread serverThread = new Thread(new ThreadStart(ServerCode));
        serverThread.Start();
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
                handleReceive(args);
                break;
            case SocketAsyncOperation.ReceiveFrom:
                handleReceive(args);
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
    public void AsyncSendPacket(byte[] packetData)
    {


        foreach (Client client in m_clients)
        {
            //send loads of packets to each client 
            for (int i = 0; i < 1; i++) {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                args.SetBuffer(packetData, 0, packetData.Length);
                args.Completed += OnCompleted;
                args.RemoteEndPoint = client.m_clientEndPoint;

                Debug.Log("sending packet to endpoint: " + client.m_clientEndPoint);

                m_listenSocket.SendToAsync(args);
            }
        }
      
    }
    void handleReceive(SocketAsyncEventArgs args)
    {

        // if there's an eror, close the socket
        if (args.SocketError != SocketError.Success)
        {
            args.AcceptSocket.Shutdown(SocketShutdown.Both);
            args.AcceptSocket.Close();
        }
        
        int packetID = BitConverter.ToInt16(args.Buffer, 0);
        int messageLength = BitConverter.ToInt16(args.Buffer, 2);

        byte[] packetData = new byte[messageLength];
        Array.Copy(args.Buffer, 4, packetData, 0, messageLength);

        Packet packet = FindPacket(packetID);
        if(packet != null)
        {
            packet.HandlePacket(packetData, args);
        }

        if (!m_listenSocket.ReceiveFromAsync(args))
        {
            OnCompleted(m_listenSocket, args);
        }
    }

    void ServerCode()
    {

        System.Net.IPEndPoint socketEndPoint = new System.Net.IPEndPoint(m_serverIP, m_portNumber);

        m_listenSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);

        m_listenSocket.Bind(socketEndPoint);

        m_asyncReceiveEventArgs.SetBuffer(new byte[256], 0, 256);
        m_asyncReceiveEventArgs.Completed += OnCompleted;
        m_asyncReceiveEventArgs.RemoteEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.IPv6Any, 0);

        if (!m_listenSocket.ReceiveFromAsync(m_asyncReceiveEventArgs))
        {
            OnCompleted(m_listenSocket, m_asyncReceiveEventArgs);
        }

          //m_listenSocket.Close();
        
    }

    // Update is called once per frame
    void Update()
    {
        // update client timers and remove any timed out
        for (int i = 0; i < m_clients.Count; i++)
        {
            Client client = m_clients[i];
            if(client.updateTimer(Time.deltaTime))
            {
                Debug.Log("client timed out");
                m_clients.RemoveAt(i);
            }
        }

        foreach (byte[] packet in m_pendingPackets) 
        {
            AsyncSendPacket(packet);
        }
        m_pendingPackets.Clear();
       
       // int val = 4;

    }
    long FourBytesToLong(byte left, byte middleLeft, byte middleRight, byte right)
    {
        long newIP = 0;
        newIP +=  left<< 24;
        newIP += middleLeft << 16;
        newIP +=  middleRight << 8;
        newIP += right;

        return newIP;

    }
    byte[] LongToFourBytes(long value)
    {
        long left, middleLeft, middleRight, right;
        //extract values for each 8 bit number in the long
        left = (value >> 24) & 0x000000FF;
        middleLeft = (value >> 16) & 0x000000FF;
        middleRight = (value >> 8) & 0x000000FF;
        right = value & 0x000000FF;

        byte[] bytes = new byte[4];
        bytes[0] = (byte)left;
        bytes[1] = (byte)middleLeft;
        bytes[2] = (byte)middleRight;
        bytes[3] = (byte)right;
        return bytes;
    }

    long ConvertEndeannesBytes(byte left, byte middleLeft, byte middleRight, byte right)
    {
        // swap order from L-R to R-L
        long newIP = 0;
        newIP += right << 24;
        newIP += middleRight << 16;
        newIP += middleLeft << 8;
        newIP += left;

        return newIP;
    }
    long ConvertEndeannesLong(long value)
    {

        long left, middleLeft, middleRight, right;
        //extract values for each 8 bit number in the long
        left = (value >> 24) & 0x000000FF;
        middleLeft = (value >> 16) & 0x000000FF;
        middleRight = (value >> 8) & 0x000000FF;
        right = value & 0x000000FF;

        return ConvertEndeannesBytes((byte)left, (byte)middleLeft, (byte)middleRight, (byte)right);

    }

}
