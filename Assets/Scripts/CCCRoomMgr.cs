using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;

#if !UNITY_EDITOR
using Windows;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
#endif

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
    private string hostIpAddress;
    private string localIpAddress;
    public int socketPort = 11764;
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
        //Debug.Log("Socket Open. SocketId is: " + myConnectionInfo.hostId);

        Connect();
    }

#if !UNITY_EDITOR

    public async Task<string> configJson()
    {
        Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        try
        {
            Windows.Storage.StorageFile configFile = await storageFolder.GetFileAsync("config.json");

            var buffer = await Windows.Storage.FileIO.ReadBufferAsync(configFile);


            using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
            {
                string text = dataReader.ReadString(buffer.Length);
                Debug.Log("Config.json reads: " + text);
                return text;
            }
        }
        catch (Exception e)
        {
            return "";
        }
    }

    public async void configureIpAddress()
    {
        string configJson = await this.configJson();
        if (!"".Equals(configJson))
        {
            Config configuration = JsonUtility.FromJson<Config>(configJson);
            if (!"".Equals(configuration.ipAddress))
            {
                this.hostIpAddress = configuration.ipAddress;
                Debug.Log("config.json hostIpAddress is " + this.hostIpAddress);
            }
        }


        foreach (HostName localHostName in NetworkInformation.GetHostNames())
        {
            if (localHostName.IPInformation != null)
            {
                if (localHostName.Type == HostNameType.Ipv4)
                {                    
                    this.localIpAddress = localHostName.ToString();
                    Debug.Log("localIpAddress is " + this.localIpAddress);
                    break;
                }
            }
        }

    }
#endif

    public void Connect()
    {
        byte error;
        this.localIpAddress = this.hostIpAddress = "127.0.0.1";
        

#if !UNITY_EDITOR
        configureIpAddress();
#endif
        myConnectionInfo.connectionId = NetworkTransport.Connect(myConnectionInfo.hostId, hostIpAddress, socketPort, 0, out error);

        LogNetworkError(error);

        Debug.LogFormat("Connected to server: {0} ConnectionId: {1} ", hostIpAddress,  myConnectionInfo.connectionId);
    }

    private static void LogNetworkError(byte error)
    {
        if (error != (byte)NetworkError.Ok)
        {
            NetworkError nerror = (NetworkError)error;
            Debug.LogError("Network Error " + nerror.ToString());
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
        bool serverIsLocal = localIpAddress.Equals(hostIpAddress);      
        return serverIsLocal;
    }

    public bool isConnected()
    {
        bool isConnected = clientConnections.Count > 0;
        return isConnected;
    }

    public bool isClient()
    {
        bool isClient = !localIpAddress.Equals(hostIpAddress);
        return isClient;
    }

    protected virtual void HandleIncomingRoomEvent(CCCRoomEvent e)
    {
        if (isServer() && !serverProcessedEvents.Contains(e.eventId))
        {
            serverProcessedEvents.Add(e.eventId);
            //propogate it to all the currently connected clients.
            foreach (ConnectionInfo client in clientConnections)
            {
                Debug.LogFormat("Send commande {0}, to {1}", e.eventType, client.hostId);
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
