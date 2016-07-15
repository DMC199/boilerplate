using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System;

public class CCCRoomMgr : MonoBehaviour
{
    int channelId;
    int socketId;
    int clientId;
    public int socketPort = 8935;

    int connectionId;

    public static CCCRoomMgr Instance = null;


    // Use this for initialization
    void Awake()
    {
        Instance = this;

        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();
        channelId = config.AddChannel(QosType.Reliable);

        int maxConnections = 10;
        HostTopology topology = new HostTopology(config, maxConnections);

        socketId = NetworkTransport.AddHost(topology, socketPort);
        clientId = NetworkTransport.AddHost(topology); 
        Debug.Log("Socket Open. SocketId is: " + socketId);

        Connect();
    }

    public void Connect()
    {
        byte error;
        string hostIpAddress = PlayerPrefs.GetString("server-ip-address");
        connectionId = NetworkTransport.Connect(socketId, hostIpAddress, socketPort, 0, out error);
        //todo trying to understand why there is a client and socket.  
        //see https://gist.github.com/LinaAdkins/a3bc0cee6f39bfd80110
        //connectionId = NetworkTransport.Connect(clientId, hostIpAddress, socketPort, 0, out error);
        Debug.Log("Connected to server. ConnectionId: " + connectionId);
    }

    public void SendMessage(CCCRoomEvent eventToSend)
    {
        Debug.Log("Sending Message: " + eventToSend);

        byte error;
        byte[] buffer = Encoding.UTF8.GetBytes(eventToSend.asJson());

        NetworkTransport.Send(socketId, connectionId, channelId, buffer, buffer.Length, out error);
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
        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);

        do
        {

            switch (recNetworkEvent)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    Debug.Log("incoming connection event received");
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
                    break;
            }

        } while (recNetworkEvent != NetworkEventType.Nothing);


    }

    protected virtual void HandleIncomingRoomEvent(CCCRoomEvent e)
    {
        EventHandler<CCCRoomEvent> handler = OnIncomingRoomEvent;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    public event EventHandler<CCCRoomEvent> OnIncomingRoomEvent;
}
