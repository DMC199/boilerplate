﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CCCRoom : MonoBehaviour {

    public GameObject localAnchor;
    private Dictionary<string, GameObject> roomObjectsById = new Dictionary<string, GameObject>();

    // Use this for initialization
    void Start () {
        CCCRoomMgr.Instance.OnIncomingRoomEvent += OnIncomingRoomEvent;
	}

    private void OnIncomingRoomEvent(object sender, CCCRoomEvent e)
    {
        Debug.Log("CCCRoom.OnIncomingRoomEvent");
        if (localAnchor == null)
        {
            Debug.Log("Set Local Anchor before handling incoming request");
        }

        if (e.eventType == null)
        {
            return;
        }
        if ("create".Equals(e.eventType))
        {
            if (e.prefabName != null && e.id != null && e.relativeToCommonAnchor != null)
            {
                Quaternion camRotation = Camera.main.transform.rotation;

                GameObject instance = (GameObject)Instantiate(Resources.Load(e.prefabName), TranslateAnchor(e), camRotation);
                roomObjectsById.Add(e.id, instance);
            }
        }
        else if ("destroy".Equals(e.eventType))
        {
            if (e.id != null && roomObjectsById.ContainsKey(e.id))
            {
                GameObject instance = roomObjectsById[e.id];
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
        }
        else if ("move".Equals(e.eventType))
        {
            if (e.id != null && roomObjectsById.ContainsKey(e.id) && e.relativeToCommonAnchor != null)
            {
                GameObject instance = roomObjectsById[e.id];
                instance.transform.position = TranslateAnchor(e);
            }
        }
        else
        {

        }
    }

    public string Create(string prefabName, Vector3 targetPosition)
    {
        Transform anchor = localAnchor.transform;
        Vector3 relativeAnchorPosition = anchor.InverseTransformPoint(targetPosition);
        //todo add a warning if distance of vector is more than 3 meters

        CCCRoomEvent ev = new CCCRoomEvent("create", prefabName, relativeAnchorPosition);
        CCCRoomMgr.Instance.SendMessage(ev);
        return ev.id;
    }

    public void Destroy(string uuid)
    {
        CCCRoomMgr.Instance.SendMessage(new CCCRoomEvent("destroy", uuid));
    }

    public void Move(string uuid, Vector3 newTargetPosition)
    {
        Transform anchor = localAnchor.transform;
        Vector3 relativeAnchorPosition = anchor.InverseTransformPoint(newTargetPosition);
        //todo add a warning if distance of vector is more than 3 meters

        CCCRoomEvent ev = new CCCRoomEvent("move", uuid, null, relativeAnchorPosition);
        CCCRoomMgr.Instance.SendMessage(ev);
    }

    private Vector3 TranslateAnchor(CCCRoomEvent e)
    {
        Transform anchor = localAnchor.transform;
        Vector3 localPosition = anchor.TransformPoint(e.relativeToCommonAnchor);
        return localPosition;
    }

    int count = 0;
    string lastUUID = null;

    // Update is called once per frame
    void Update () {
        //simple test code to make it look like the cube is flying at you.  
        count++;
        if (count % 300 == 0)
        {
            count = 0;
            if (lastUUID != null)
            {
                Destroy(lastUUID);
            }
            Vector3 camTransform = Camera.main.transform.position;
            Vector3 forward = Camera.main.transform.forward.normalized;

            Vector3 targetPosition = camTransform + forward * 3;
            lastUUID = Create("cube", targetPosition);
        }
        if (lastUUID != null && count % 30 == 5)
        {
            Vector3 camTransform = Camera.main.transform.position;
            Vector3 forward = Camera.main.transform.forward.normalized;
            float  f = 2 * ((300 - count) / 300.0F) + 1;
            Vector3 targetPosition = camTransform + forward * f;
            Move(lastUUID, targetPosition);
        }
    }
}
