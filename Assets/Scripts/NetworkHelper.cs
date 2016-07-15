using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEngine.Networking;

public class NetworkHelper {

    public static string localIPAddress()
    {
        return NetworkManager.singleton.networkAddress;
    }


}
