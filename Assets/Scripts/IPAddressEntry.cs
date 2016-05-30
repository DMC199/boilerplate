using UnityEngine;
using System.Collections;
using HoloToolkit.Sharing;

public class IPAddressEntry : MonoBehaviour {

    public string FullIpAddress = "";

    void AppendText(string text)
    {
        if (text.Equals("\b") || text.Equals("\\b")) //backspace-code
        {
            if (FullIpAddress.Length > 0)
            {
                FullIpAddress = FullIpAddress.Substring(0, FullIpAddress.Length - 1);
            }
        }
        else
        {
            FullIpAddress += text;
        }
        GetComponent<TextMesh>().text = FullIpAddress;
    }

    void Clear()
    {
        FullIpAddress = "";
        GetComponent<TextMesh>().text = FullIpAddress;
    }

    void Start()
    {
        //saved from last run.
        if (PlayerPrefs.HasKey("server-ip-address"))
        {
            FullIpAddress = PlayerPrefs.GetString("server-ip-address");
            GetComponent<TextMesh>().text = FullIpAddress;
        }
    }
}
