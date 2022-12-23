using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class Client 
{

    public System.Net.IPEndPoint m_clientEndPoint { get; private set; }

    public SocketAsyncEventArgs m_asyncSendEventArgs;

    float m_connectionTimeoutVal;
    float m_timeSinceLastPacket;

    // GameManager 
    public Client()
    {
        m_connectionTimeoutVal = 5;
        m_timeSinceLastPacket = 0;
    }

    public Client(System.Net.IPEndPoint clientEP, SocketAsyncEventArgs setArgs)
    {
        m_clientEndPoint = clientEP;
        m_asyncSendEventArgs = setArgs;
        m_connectionTimeoutVal = 5;
        m_timeSinceLastPacket = 0;
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
        Debug.Log("client at endpoint: " + m_clientEndPoint + " had their idle timer reset");
        m_timeSinceLastPacket = 0;
    }

    ~Client()
    {
        Debug.Log("Client " + m_clientEndPoint + " closed");
        m_asyncSendEventArgs.Dispose();
    }

}
