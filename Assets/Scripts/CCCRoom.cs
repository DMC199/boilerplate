﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CCCRoom : MonoBehaviour {

    public GameObject localAnchor;
    private Dictionary<string, GameObject> roomObjectsById = new Dictionary<string, GameObject>();
    private List<CCCRoomEvent> eventPlayByPlay = new List<CCCRoomEvent>();

    //right now it keeps track of the most recently spawned...could be set to most recently focused.  
    public string mostRecentObjectUUID;

    // Use this for initialization
    void Start () {
        CCCRoomMgr.Instance.OnIncomingRoomEvent += OnIncomingRoomEvent;
	}

    public void Replay()
    {
        //set that we are replaying (grab a lock against play by play)
        //destroy all existing objects
        //recreate the roomObjectById data structure by processing each event
        //unset that we are replaying all messages.  
        if (localAnchor == null)
        {
            Debug.Log("Set Local Anchor before handling incoming request");
            return;
        }

        lock (eventPlayByPlay)
        {
            foreach (KeyValuePair<string, GameObject> keyValue in roomObjectsById)
            {
                Destroy(keyValue.Value);
            }

            this.roomObjectsById = new Dictionary<string, GameObject>();
            foreach (CCCRoomEvent cccRoomEvent in eventPlayByPlay)
            {
                ProcessEvent(cccRoomEvent, localAnchor);
            }
        }
    }

    private void OnIncomingRoomEvent(object sender, CCCRoomEvent e)
    {
        if (e.eventType == null)
        {
            return;
        }
        Debug.Log("CCCRoom.OnIncomingRoomEvent");
        if (localAnchor == null)
        {
            Debug.Log("Set Local Anchor before handling incoming request");
            return;
        }

        lock (eventPlayByPlay)
        {
            eventPlayByPlay.Add(e);            
            ProcessEvent(e, this.localAnchor);
        }
    }

    private void ProcessEvent(CCCRoomEvent e, GameObject localAnchor)
    {
        if ("create".Equals(e.eventType))
        {
            if (e.prefabName != null && e.objectRef != null && e.relativeToCommonAnchor != null && !roomObjectsById.ContainsKey(e.objectRef))
            {

                GameObject instance = (GameObject)Instantiate(Resources.Load(e.prefabName), TranslateAnchor(e, localAnchor), TranslateQuaternion(localAnchor, e.facingDirection));
                mostRecentObjectUUID = e.objectRef;
                roomObjectsById.Add(e.objectRef, instance);
            }
        }
        else if ("destroy".Equals(e.eventType))
        {
            if (e.objectRef != null && roomObjectsById.ContainsKey(e.objectRef))
            {
                GameObject instance = roomObjectsById[e.objectRef];
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
            //todo purge all previoius events for this object id from the play by play.
        }
        else if ("move".Equals(e.eventType))
        {
            if (e.objectRef != null && roomObjectsById.ContainsKey(e.objectRef) && e.relativeToCommonAnchor != null)
            {
                GameObject instance = roomObjectsById[e.objectRef];
                instance.transform.position = TranslateAnchor(e, localAnchor);
                //todo purge all previoius move events for this object id from the play by play.
            }
        }
        else if ("animation".Equals(e.eventType))
        {
            if (e.objectRef != null && roomObjectsById.ContainsKey(e.objectRef))
            {
                GameObject instance = roomObjectsById[e.objectRef];
                Animation animation = instance.GetComponent<Animation>();
                if (e.data != null && "play".Equals(e.data) && animation != null) {
                    animation.Play("Default Take");
                } else if (e.data != null && "stop".Equals(e.data))
                {
                    animation.Stop();
                } else if (animation == null)
                {
                    Debug.Log("animation not found");
                }
            }
        }
        else
        {

        }
    }

    public string Create(string prefabName, Vector3 targetPosition, Quaternion camRotation)
    {
        Transform anchor = localAnchor.transform;
        Vector3 relativeAnchorPosition = anchor.InverseTransformPoint(targetPosition);
        //todo add a warning if distance of vector is more than 3 meters

        CCCRoomEvent ev = new CCCRoomEvent("create", prefabName, relativeAnchorPosition, CalculateInverseQP(localAnchor, camRotation));
        CCCRoomMgr.Instance.SendMessage(ev);
        return ev.objectRef;
    }

    public void Destroy(string uuid)
    {
        //todo test this.  
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

    public void RunAnimationCommand(string uuid, string animationName)
    {
        CCCRoomEvent ev = new CCCRoomEvent("animation", uuid, animationName);
        CCCRoomMgr.Instance.SendMessage(ev);
    }

    private static Vector3 TranslateAnchor(CCCRoomEvent e, GameObject localAnchor)
    {
        Transform anchor = localAnchor.transform;
        Vector3 localPosition = anchor.TransformPoint(e.relativeToCommonAnchor);
        return localPosition;
    }

    private static Quaternion CalculateInverseQP(GameObject localAnchor, Quaternion localRotation)
    {
        //from http://answers.unity3d.com/questions/35541/problem-finding-relative-rotation-from-one-quatern.html
        //also https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation which says  p ↦ q p q−1
        return Quaternion.Inverse(localAnchor.transform.rotation) * localRotation;
    }

    private static Quaternion TranslateQuaternion(GameObject localAnchor, Quaternion qp)
    {
        return qp * localAnchor.transform.localRotation;
    }
}
