using UnityEngine;
using System.Collections;

public class KeyPress : MonoBehaviour {
    public GameObject IPTextField;

    public string AppendText;

    void OnSelect()
    {
        IPTextField.SendMessage("AppendText", AppendText);
    }
}
