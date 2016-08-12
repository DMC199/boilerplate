using SimpleJSON;
using UnityEngine;
using System.Collections;

public class JsonTest : MonoBehaviour {



	// Use this for initialization
	void Start () {

        JSONNode test = JSON.Parse("{\"foo\" : \"bar\"}");
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
