using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{

    public static Server Instance { get; private set; }

    //IP 143.226.212.81
    byte[] m_serverAddress;
    long m_serverAddressBigEndeanLong;
    // port number 32612
    int m_portNumber = 32612;
    System.Net.IPAddress m_serverIP;

    public int m_bufferSize { get; private set; } = 450;

    Socket m_listenSocket;

   List<Packet> m_packetRefs;

    int m_maxClients = 32;

    int m_newClientID = 0;

    float broadcasttimer = 0;
    int randomcounter = -2;

    List<Client> m_clients;
    Queue<KeyValuePair<int[], Byte[]>> m_pendingPackets;
    Queue<SocketAsyncEventArgs> m_pendingReads;

    SocketAsyncEventArgs m_asyncReceiveEventArgs;


    public Client FindClient(System.Net.IPEndPoint clientEP, out int clientListPos)
    {
        for (int i = 0; i < m_clients.Count; i++)
        {
            Client client = m_clients[i];
            if (client.m_clientEndPoint.Equals(clientEP))
            {
                clientListPos = i;
                return client;
            }
        }
        clientListPos = -1;
        return null;
    }

    public Client FindClient(int clientID, out int clientListPos)
    {
        for (int i = 0; i < m_clients.Count; i++)
        {
            Client client = m_clients[i];
            if (client.m_ID == clientID)
            {
                clientListPos = i;
                return client;
            }
        }
        clientListPos = -1;
        return null;
    }

    public bool CreateClient(System.Net.IPEndPoint clientEndPoint, out int clientID)
    {

        for (int i = 0; i < m_clients.Count; i++)
        {
            Client client = m_clients[i];

            if (client.m_clientEndPoint.Equals(clientEndPoint))
            {
                clientID = client.m_ID;
                return true;
            }
        }
        if (m_clients.Count < m_maxClients)
        {


            Client newClient = new Client(clientEndPoint, m_newClientID);

            m_clients.Add(newClient);
            clientID = m_newClientID;
            m_newClientID += 1;
            return true;
        }
        clientID = -1;
        return false;
    }

    public int[] GetAllClientIDs()
    {
        int[] clientIDs = new int[m_clients.Count];
        int index = 0;
        foreach (Client client in m_clients)
        {
            clientIDs[index] = client.m_ID;
            index += 1;
        }

        return clientIDs;
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

    public void AddPacketToQueue( int[] clients, byte[] newPacket)
    {
        m_pendingPackets.Enqueue(new KeyValuePair<int[], byte[]>(clients, newPacket));
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }

    }

    void Startreset()
    {

        m_asyncReceiveEventArgs = new SocketAsyncEventArgs();

        m_clients = new List<Client>();

        m_pendingPackets = new Queue<KeyValuePair<int[], byte[]>>();

        m_packetRefs = new List<Packet>();

        m_serverAddress = new byte[4];
        m_serverAddress[0] = 127;
        m_serverAddress[1] = 0;
        m_serverAddress[2] = 0;
        m_serverAddress[3] = 1;

        m_serverAddressBigEndeanLong = ConvertEndeannesBytes(m_serverAddress[0], m_serverAddress[1], m_serverAddress[2], m_serverAddress[3]);

        m_packetRefs.Add(new PositionPacket());
        m_packetRefs.Add(new AcknowledgePacket());
        m_packetRefs.Add(new ConnectPacket());
        m_packetRefs.Add(new IdlePacket());
        m_packetRefs.Add(new SendIDPacket());

        foreach (Packet packet in m_packetRefs)
        {
            packet.serverRef = this;
        }

        m_serverIP = new System.Net.IPAddress(m_serverAddress);

        System.Net.IPEndPoint socketEndPoint = new System.Net.IPEndPoint(m_serverIP, m_portNumber);

        m_listenSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);

        m_listenSocket.Bind(socketEndPoint);

         m_listenSocket.Close();
    }

    // Start is called before the first frame update
    void Start()
    {

        m_pendingReads = new Queue<SocketAsyncEventArgs>();

        m_asyncReceiveEventArgs = new SocketAsyncEventArgs();

        m_clients = new List<Client>();

        m_pendingPackets = new Queue<KeyValuePair<int[], byte[]>>();

       m_packetRefs = new List<Packet>();

        m_serverAddress = new byte[4];
        m_serverAddress[0] = 127;
        m_serverAddress[1] = 0;
        m_serverAddress[2] = 0;
        m_serverAddress[3] = 1;

        m_serverAddressBigEndeanLong = ConvertEndeannesBytes(m_serverAddress[0], m_serverAddress[1], m_serverAddress[2], m_serverAddress[3]);
        
        m_packetRefs.Add(new PositionPacket());
        m_packetRefs.Add(new AcknowledgePacket());
        m_packetRefs.Add( new ConnectPacket());
        m_packetRefs.Add(new IdlePacket());
        m_packetRefs.Add(new SendIDPacket());
        m_packetRefs.Add(new SpawnPlayerPacket());
        m_packetRefs.Add(new SpawnMinionPacket());
        m_packetRefs.Add(new SpawnTowerPacket());
        m_packetRefs.Add(new NewPlayerSpawnPacket());
        m_packetRefs.Add(new JoinGameDataPacket());
        m_packetRefs.Add(new DisconnectPacket());
        m_packetRefs.Add(new DisconnectBroadcastPacket());
        m_packetRefs.Add(new TowerUpdatePacket());
        m_packetRefs.Add(new MinionUpdatePacket());
        m_packetRefs.Add(new WorldUpdatePacket());
        m_packetRefs.Add(new PlayerDiedPacket());
        m_packetRefs.Add(new TowerShotPacket());
        m_packetRefs.Add(new LatencyPacket());

        foreach (Packet packet in m_packetRefs)
        {   
            packet.serverRef = this;
        }

        m_serverIP = new System.Net.IPAddress(m_serverAddress);

        Thread serverThread = new Thread(new ThreadStart(ServerCode));
        serverThread.Start();
    }
    void Update()
    {

        broadcasttimer += Time.deltaTime;
        if (broadcasttimer > 2.5)
        {
            broadcasttimer = 0;
            ((SendIDPacket)FindPacket((int)Packet.PacketID.Random)).SendPacket(randomcounter);
        }

        ReadQueuedPackets();

        // update client timers and remove any timed out
        for (int i = 0; i < m_clients.Count; i++)
        {
            Client client = m_clients[i];

            if(client.updateTimer(Time.deltaTime))
            {
                RemoveClient(client.m_ID);
            }
        }
        SendPackets();
       
       // int val = 4;

    }

    public void RemoveClient(int clientID)
    {
        for (int i = 0; i < m_clients.Count; i++)
        {
            if (m_clients[i].m_ID == clientID)
            {
                if (m_clients[i].m_inGame)
                {
                    
                    ((DisconnectBroadcastPacket)FindPacket((int)Packet.PacketID.DisconnectBroadcast)).SendPacket(clientID, GameManager.Instance.GetInGamePlayerIDs());
                }

                m_clients.RemoveAt(i);
                GameManager.Instance.RemoveDisconnectedPlayer(clientID);

                Debug.Log("Closed client " + clientID);


                return;
            }
        }
    }

    public void RemoveClient(System.Net.IPEndPoint clientEP)
    {
        int clientArrPos;
        Client client = FindClient(clientEP, out clientArrPos);
        //better to search for the client in game to see if they exist rather than have that data stored in a bool where its value might be incorrect
        if (client.m_inGame)
        {
            ((DisconnectBroadcastPacket)FindPacket((int)Packet.PacketID.DisconnectBroadcast)).SendPacket(m_clients[clientArrPos].m_ID, GameManager.Instance.GetInGamePlayerIDs());
        }
        GameManager.Instance.RemoveDisconnectedPlayer(client.m_ID);

        Debug.Log("Closed client " + m_clients[clientArrPos].m_ID);
        m_clients.RemoveAt(clientArrPos);

        
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
    
    public void SendPackets()
    {
        foreach (KeyValuePair<int[], byte[]> packet in m_pendingPackets)
        {
            AsyncSendPacket(packet.Value, packet.Key);
        }
        if (m_pendingPackets.Count > 0)
        {
            m_pendingPackets.Clear();
        }
    }
    public void AsyncSendPacket(byte[] packetData, int[] clientIDs)
    {
        //int [] newclientIDs = GetAllClientIDs();
        foreach (int clientID in clientIDs)
        {
            foreach (Client client in m_clients)
            {

                if (client.m_ID == clientID)
                {
                     SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                    args.SetBuffer(packetData, 0, packetData.Length);
                    args.Completed += OnCompleted;
                    args.RemoteEndPoint = client.m_clientEndPoint;

                    short packetID = BitConverter.ToInt16(packetData, 0);

                    m_listenSocket.SendToAsync(args);
                    break;
                }
            }
        }      
      
    }
    public void ReadQueuedPackets()
    {
        //cant use for or foreach loop in case the number of socketasynceventargs objects in the m_pendingreads queue is increased as other asynchronous operations queue more packets
        while (true)
        {
            SocketAsyncEventArgs args;
            // if there are more args inthe queue, take one while locked and handle
            lock (m_pendingReads)
            {
                if (m_pendingReads.Count > 0)
                {
                    args = m_pendingReads.Dequeue();
                }
                else
                {
                    break;
                }

            }

            int packetID = BitConverter.ToInt16(args.Buffer, 0);
            int messageLength = BitConverter.ToInt16(args.Buffer, 2);

            byte[] packetData = new byte[messageLength];
            Array.Copy(args.Buffer, 4, packetData, 0, messageLength);
            //get the appropriate packet handler to decipher the packet sent 
            Packet packet = FindPacket(packetID);
            if (packet != null)
            {
                packet.HandlePacket(packetData, args);
            }
            //keep updating the clients connection status when more packets are received by them
            int clientArrPos = -1;
            Client c = FindClient((System.Net.IPEndPoint)args.RemoteEndPoint, out clientArrPos);
            if (c != null)
            {
                c.ResetTimoutTimer();
            }
        }




    }
    void handleReceive(SocketAsyncEventArgs args)
    {

        lock (m_pendingReads)
        {
            m_pendingReads.Enqueue(args);          
        }

        SocketAsyncEventArgs newArg = new SocketAsyncEventArgs();

        newArg.SetBuffer(new byte[m_bufferSize], 0, m_bufferSize);

        newArg.Completed += OnCompleted;

        newArg.RemoteEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.IPv6Any, 0);

        if (!m_listenSocket.ReceiveFromAsync(newArg))
        {
            OnCompleted(m_listenSocket, newArg);
        }
    }



    void ServerCode()
    {

        System.Net.IPEndPoint socketEndPoint = new System.Net.IPEndPoint(m_serverIP, m_portNumber);

        m_listenSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);

        m_listenSocket.Bind(socketEndPoint);

        m_asyncReceiveEventArgs.SetBuffer(new byte[m_bufferSize], 0, m_bufferSize);
        m_asyncReceiveEventArgs.Completed += OnCompleted;
        m_asyncReceiveEventArgs.RemoteEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.IPv6Any, 0);

        if (!m_listenSocket.ReceiveFromAsync(m_asyncReceiveEventArgs))
        {
            OnCompleted(m_listenSocket, m_asyncReceiveEventArgs);
        }

        
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
