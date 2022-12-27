using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class Client 
{

    public System.Net.IPEndPoint m_clientEndPoint { get; private set; }

    float m_connectionTimeoutVal;
    float m_timeSinceLastPacket;

    public bool m_inGame = false;

    // use one way latency stored in the clients for things like synching time 
    float m_OneWayLatency = 0;

    public int m_ID { get; private set; } = -1;

    // GameManager 
    public Client()
    {
        m_connectionTimeoutVal = 10;
        m_timeSinceLastPacket = 0;
    }



    public Client(System.Net.IPEndPoint clientEP, int newID)
    {
        m_clientEndPoint = clientEP;
        m_connectionTimeoutVal = 10;
        m_timeSinceLastPacket = 0;
        m_ID = newID;
    }

    public bool updateTimer(float deltaTime)
    {
        m_timeSinceLastPacket += deltaTime;
        if (m_timeSinceLastPacket >= m_connectionTimeoutVal)
        {
            return true;
        }
        return false;

    }

    public void ResetTimoutTimer()
    {
       // Debug.Log("client at endpoint: " + m_clientEndPoint + " had their idle timer reset");
        m_timeSinceLastPacket = 0;
    }

    ~Client()
    {
        Debug.Log("Client " + m_clientEndPoint + " deleted");
    }

}
