using UnityEngine;
using System.Collections;

public class ClearInitiated : MonoBehaviour {
    public GameObject IPAddressText;

    void OnSelect()
    {
        IPAddressText.SendMessage("Clear");
    }
}
