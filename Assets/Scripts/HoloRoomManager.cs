using UnityEngine;
using System.Collections;
using WebSocketSharp;

public class HoloRoomManager : MonoBehaviour {
    public static HoloRoomManager Instance = null;

    public string ServerAddress = "ws://192.168.0.116";
    public int ServerPort = 9834;

    public WebSocket webSocket;

    private void Awake()
    {
        Debug.Log("HoloRoomManager.Awake()");

        Instance = this;

        // PlayerPrefs.GetString("server-ip-address")
        // config.SetServerPort(this.ServerPort);

        webSocket = new WebSocket(ServerAddress + ":" + ServerPort);
        webSocket.OnMessage += WebSocket_OnMessage;
        webSocket.Connect();

        RoomEvent re = new RoomEvent();
        re.id = System.Guid.NewGuid().ToString();

        WebSocket_SendMessage(re);
    }

    private void WebSocket_OnMessage(object sender, MessageEventArgs e)
    {
        if (e.IsText)
        {
            //myObject = JsonUtility.FromJson<MyClass>(json);

        }
    }

    public void WebSocket_SendMessage(RoomEvent anEvent)
    {
        Debug.Log("WebSocket_SendMessage");

        string json = JsonUtility.ToJson(anEvent);

        webSocket.Send(json as string);
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
