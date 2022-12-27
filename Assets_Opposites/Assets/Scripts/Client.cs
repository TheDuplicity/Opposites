using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Client : MonoBehaviour
{

    public static Client Instance { get; private set; }

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

    public int m_bufferSize { get; private set; } = 450;
    // 20 updates a second
    public float m_networkSendRate = (float)0.05;
    public int m_myID { get; private set; } = -1;

    Socket m_clientSocket;
    System.Net.IPAddress m_serverSocketIPAddress;
    System.Net.IPEndPoint m_serverEndPoint;

    Queue<byte[]> m_pendingSendPackets;
    Queue<SocketAsyncEventArgs> m_pendingReads;
    List<Packet> m_packetRefs;

    float m_timeSinceLastReceivedPacket = 0;

    // send a message after2.5 seconds
    List<KeyValuePair<float, bool>> m_idleMessageTimeThresholds;

    float m_timeSinceLastSentPacket = 0;

    public Queue<float> m_latencies;
    public List<KeyValuePair< int,float>> m_latencyIDAndSendTime;
    int totalLatencyPacketsSent = 0;

    public int m_maxLatencies { get; private set;} = 6;

    // Start is called before the first frame update
    void Start()
        {

        m_latencies = new Queue<float>();

        m_latencyIDAndSendTime = new List<KeyValuePair<int, float>>();

        m_idleMessageTimeThresholds = new List<KeyValuePair<float, bool>>();

        m_idleMessageTimeThresholds.Add(new KeyValuePair<float, bool>((float)2.5, false));
        m_idleMessageTimeThresholds.Add(new KeyValuePair<float, bool>((float)3.5, false));

        m_pendingSendPackets = new Queue<byte[]>();

        m_pendingReads = new Queue<SocketAsyncEventArgs>();

        m_packetRefs = new List<Packet>();
        m_packetRefs.Add(new AcknowledgePacket());
        m_packetRefs.Add(new IdlePacket());
        m_packetRefs.Add(new ConnectPacket()); 
        m_packetRefs.Add(new RandomPacket());
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

        SocketAsyncEventArgs arg = new SocketAsyncEventArgs();

        arg.SetBuffer(new byte[m_bufferSize], 0, m_bufferSize);

        arg.Completed += OnCompleted;

        arg.RemoteEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.IPv6Any, 0);

        if (!m_clientSocket.ReceiveFromAsync(arg))
        {
            OnCompleted(this, arg);
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
                if (m_timeSinceLastSentPacket >= timeThreshold.Key && !timeThreshold.Value)
                {
                    //queue up an idleCheck Packet
                    ((IdlePacket)(FindPacket((int)Packet.PacketID.Idle))).SendPacket();
                    m_idleMessageTimeThresholds[i] = new KeyValuePair<float, bool>(timeThreshold.Key, true);
                    break;
                }
            }

        if (m_pendingSendPackets.Count > 0)
        {
            SendPackets();
        }

        ReadQueuedPackets();
        

    }

    public void StartLatencyChecks(int numberOfSends = 4, float timeBetweenSends = 0.25f)
    {
        StartCoroutine(CreateLatencyPacket(numberOfSends, timeBetweenSends));
    }
    
    public void EndLatencyCheck(int latencyID)
    {
        foreach (var latencyTimePair in m_latencyIDAndSendTime)
        {
            if (latencyTimePair.Key == latencyID)
            {
                float latency = Time.realtimeSinceStartup - latencyTimePair.Value;
                m_latencies.Enqueue(latency);
                if (m_latencies.Count > m_maxLatencies)
                {
                    m_latencies.Dequeue();
                }
            }
        }
    }

    public float AverageLatency()
    {
        if (m_latencies.Count <= 0)
        {
            return 0;
        }
        float addedLatencies = 0.0f;
        foreach (float latency in m_latencies)
        {
            addedLatencies += latency;
        }
        addedLatencies /= (float)m_latencies.Count;
        return addedLatencies;
    }

    IEnumerator CreateLatencyPacket(int numberOfSends, float timeBetweenSends)
    {
        int i = 0;
        while (i < numberOfSends)
        {
            m_latencyIDAndSendTime.Add(new KeyValuePair<int, float>(totalLatencyPacketsSent, Time.realtimeSinceStartup));
            ((LatencyPacket)FindPacket(((int)Packet.PacketID.LatencyPacket))).SendPacket(totalLatencyPacketsSent);
            totalLatencyPacketsSent++;

            yield return new WaitForSeconds(timeBetweenSends); //wait 0.25 seconds per interval

            i++;
        }
    }

    public void setOurID(int newClientID)
    {
        m_myID = newClientID;
    }
    public void AddPacketToQueue(byte[] packet)
    {
        m_pendingSendPackets.Enqueue(packet);
    }

    void SendPackets()
    {

        foreach (byte[] packet in m_pendingSendPackets)
        {
            AsyncSendPacket(packet);
        }
        m_pendingSendPackets.Clear();

        for (int i = 0; i < m_idleMessageTimeThresholds.Count; i++)
        {
            if (m_idleMessageTimeThresholds[i].Value == true)
            {
                m_idleMessageTimeThresholds[i] = new KeyValuePair<float, bool>(m_idleMessageTimeThresholds[i].Key, false);
            }
        }

        m_timeSinceLastSentPacket = 0;

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
        }


        

    }
    void HandleReceive(SocketAsyncEventArgs args)
    {
        // use the async event args to handle the received data later on the main thread
        lock (m_pendingReads)
        {
            m_pendingReads.Enqueue(args);
        }
        //creating a new event argument for the next read allows the old argument to be passed into the functions for use in main thread functionality so packet handling can call gameobject related code
        SocketAsyncEventArgs newArg = new SocketAsyncEventArgs();

        newArg.SetBuffer(new byte[m_bufferSize], 0, m_bufferSize);

        newArg.Completed += OnCompleted;

        newArg.RemoteEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.IPv6Any, 0);
        // keep receiving with our new argument object for the new data
        if (!m_clientSocket.ReceiveFromAsync(newArg))
        {
            OnCompleted(this, newArg);
        }
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
        short packID = BitConverter.ToInt16(packet, 0);

        if (m_clientSocket != null)
        {
            m_clientSocket.SendToAsync(args);
        }


    }
    
    private void OnDestroy()
    {
        m_clientSocket.Shutdown(SocketShutdown.Both);
        m_clientSocket.Close();
        m_clientSocket = null;

    }


}


