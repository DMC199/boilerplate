using UnityEngine;
using System.Collections;

public class CCCRoom : MonoBehaviour {

    public GameObject localAnchor;

	// Use this for initialization
	void Start () {
        CCCRoomMgr.Instance.OnIncomingRoomEvent += OnIncomingRoomEvent;
	}

    private void OnIncomingRoomEvent(object sender, CCCRoomEvent e)
    {
        Debug.Log("CCCRoom.OnIncomingRoomEvent");

        if (e.eventType == null)
        {
            return;
        }
        if ("create".Equals(e.eventType))
        {
            if (e.prefabName != null)
            {
                GameObject instance = (GameObject) Instantiate(Resources.Load(e.prefabName));
            }
        }
        else if ("destroy".Equals(e.eventType))
        {

        }
        else if ("move".Equals(e.eventType))
        {

        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
