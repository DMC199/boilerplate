using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;

public class CCCRoomMgr : MonoBehaviour
{
    private class ConnectionInfo
    {
        public int hostId;
        public int connectionId;
        public int channelId;

        public override bool Equals(object obj)
        {
            return hostId == ((ConnectionInfo) obj).hostId && 
                connectionId == ((ConnectionInfo) obj).connectionId &&
                channelId == ((ConnectionInfo) obj).channelId;
        }

        public override int GetHashCode()
        {
            return hostId + 37 * connectionId;
        }
    };

    ConnectionInfo myConnectionInfo = new ConnectionInfo();
    List<ConnectionInfo> clientConnections = new List<ConnectionInfo>();
    HashSet<string> serverProcessedEvents = new HashSet<string>();
    public int socketPort = 8935;
    int mServerSocket = -1;

    public static CCCRoomMgr Instance = null;

    // Use this for initialization
    void Awake()
    {
        Instance = this;

        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();
        myConnectionInfo.channelId = config.AddChannel(QosType.Reliable);

        int maxConnections = 10;
        HostTopology topology = new HostTopology(config, maxConnections);

        //this device will listen on the specified socket for clients.       
        mServerSocket = NetworkTransport.AddHost(topology, socketPort);
        //it will also attach to the specified host, as a client.  
        myConnectionInfo.hostId = NetworkTransport.AddHost(topology);
        Debug.Log("Socket Open. SocketId is: " + myConnectionInfo.hostId);

        Connect();
    }

    public void Connect()
    {
        byte error;
        //todo is there another way besides PlayerPrefs this should be grabbed to make this class more generic. (see also isServer)
        string hostIpAddress = PlayerPrefs.GetString("server-ip-address");
        myConnectionInfo.connectionId = NetworkTransport.Connect(myConnectionInfo.hostId, hostIpAddress, socketPort, 0, out error);

        LogNetworkError(error);

        Debug.Log("Connected to server. ConnectionId: " + myConnectionInfo.connectionId);
    }

    private static void LogNetworkError(byte error)
    {
        if (error != (byte)NetworkError.Ok)
        {
            NetworkError nerror = (NetworkError)error;
            Debug.Log("Error " + nerror.ToString());
        }
    }

    public void SendMessage(CCCRoomEvent eventToSend)
    {
        SendMessage(eventToSend, myConnectionInfo.hostId, myConnectionInfo.connectionId, myConnectionInfo.channelId);       
    }

    private void SendMessage(CCCRoomEvent eventToSend, int host, int connection, int channel)
    {
        Debug.Log("Sending Message: " + eventToSend + " to " + host + ":" + connection + ":" + channel);

        byte error;
        byte[] buffer = Encoding.UTF8.GetBytes(eventToSend.asJson());

        NetworkTransport.Send(host, connection, channel, buffer, buffer.Length, out error);

        LogNetworkError(error);
    }


    // Update is called once per frame
    void Update()
    {
        int recHostId;
        int recConnectionId;
        int recChannelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recNetworkEvent;
        do
        {
            recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);

            LogNetworkError(error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    Debug.Log("incoming connection event received " + recHostId + ":" + recConnectionId + ":" + recChannelId);
                    ConnectionInfo clientInfo = extractClientConnectionInfo(recHostId, recConnectionId, recChannelId);
                    clientConnections.Add(clientInfo);
                    //todo send the current state of the shared room.  
                    break;
                case NetworkEventType.DataEvent:
                    Debug.Log("incoming data");
                    string result = Encoding.UTF8.GetString(recBuffer);
                    CCCRoomEvent myEvent = CCCRoomEvent.fromJson(result);

                    Debug.Log("incoming message event received: " + myEvent);
                    HandleIncomingRoomEvent(myEvent);

                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("remote client event disconnected");
                    ConnectionInfo disconnectedClientInfo = extractClientConnectionInfo(recHostId, recConnectionId, recChannelId);
                    clientConnections.Remove(disconnectedClientInfo);
                    break;
            }

        } while (recNetworkEvent != NetworkEventType.Nothing);


    }

    private ConnectionInfo extractClientConnectionInfo(int recHostId, int recConnectionId, int recChannelId)
    {
        ConnectionInfo clientInfo = new ConnectionInfo();
        clientInfo.hostId = recHostId;
        clientInfo.connectionId = recConnectionId;
        clientInfo.channelId = recChannelId;
        return clientInfo;
    }

    public bool isServer()
    {
        string hostIpAddress = PlayerPrefs.GetString("server-ip-address");
        return "127.0.0.1".Equals(hostIpAddress);
    }

    protected virtual void HandleIncomingRoomEvent(CCCRoomEvent e)
    {
        if (isServer() && !serverProcessedEvents.Contains(e.eventId))
        {
            serverProcessedEvents.Add(e.eventId);
            //propogate it to all the currently connected clients.
            foreach (ConnectionInfo client in clientConnections)
            {
                SendMessage(e, client.hostId, client.connectionId, client.channelId);
            }
        }
        EventHandler<CCCRoomEvent> handler = OnIncomingRoomEvent;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    public event EventHandler<CCCRoomEvent> OnIncomingRoomEvent;
}
