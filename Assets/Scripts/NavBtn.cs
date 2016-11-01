using UnityEngine;
using System.Collections;

public class NavBtn : MonoBehaviour {

    public Sequencer sequencer;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnSelect()
    {
        Debug.Log("OnSelect " + gameObject.name);
        if( gameObject.name == "Prev")
        {
            sequencer.PrevStep();
        }
        else if(gameObject.name == "Next")
        {
            sequencer.NextStep();
        }else
        {
            Debug.Log("Unknown button");
        }
    }
}
