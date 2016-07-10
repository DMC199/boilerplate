using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ActionInitiated : MonoBehaviour {
    public GameObject IPAddressText;

    void OnSelect()
    {
        string serverIpAddress = IPAddressText.GetComponent<TextMesh>().text;
        PlayerPrefs.SetString("server-ip-address", serverIpAddress);
        SceneManager.LoadScene("jhager_scene_0");
    }
}
