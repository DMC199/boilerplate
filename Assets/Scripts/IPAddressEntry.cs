using UnityEngine;
using System.Collections;
using HoloToolkit.Sharing;

public class IPAddressEntry : MonoBehaviour {

    public string FullIpAddress = "172.16.0.130";  //default should be set here.  

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
        SuggestCharacter(text);
        GetComponent<TextMesh>().text = FullIpAddress;
    }

    void SuggestCharacter(string lastCharacter)
    {
        int length = FullIpAddress.Length;
        int periodCount = FullIpAddress.Split('.').Length - 1;

        if (length >= 3 && periodCount < 3 && !(lastCharacter.Equals("\b") || lastCharacter.Equals("\\b")))
        {
            if (!".".Equals(FullIpAddress.Substring(length-1, 1)) &&
                !".".Equals(FullIpAddress.Substring(length-2, 1)) &&
                !".".Equals(FullIpAddress.Substring(length-3, 1))
                )
            {
                FullIpAddress += ".";
            }
        }
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
        else
        {
            GetComponent<TextMesh>().text = FullIpAddress;
        }
    }
}
