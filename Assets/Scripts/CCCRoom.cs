using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CCCRoom : MonoBehaviour {

    public GameObject localAnchor;
    private Dictionary<string, GameObject> roomObjectsById = new Dictionary<string, GameObject>();
    private List<CCCRoomEvent> eventPlayByPlay = new List<CCCRoomEvent>();

    //right now it keeps track of the most recently spawned...could be set to most recently focused.  
    public string mostRecentObjectUUID;

    //propagate unknown events.  
    public event EventHandler<CCCRoomEvent> OnPropagateRoomEvent;


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
                if (e.data != null && e.data.StartsWith("play:") && animation != null) {
                    string animationName = e.data.Substring("play:".Length);
                    animation.Play(animationName);
                } else if (e.data != null && e.data.StartsWith("stop:"))
                {
                    animation.Stop();
                } else if (animation == null)
                {
                    Debug.Log("animation not found");
                }
            }
        }
        else if ("clear-all".Equals(e.eventType))
        {
            foreach (KeyValuePair<string, GameObject> entry in roomObjectsById)
            {
                Destroy(entry.Value);              
            }
            roomObjectsById.Clear();
            eventPlayByPlay.Clear();
        }
        else
        {
            EventHandler<CCCRoomEvent> handler = OnPropagateRoomEvent;
            if (handler != null)
            {
                handler(this, e);
            }
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

    public void ClearAll()
    {
        CCCRoomMgr.Instance.SendMessage(new CCCRoomEvent("clear-all"));
    }

    public void Move(string uuid, Vector3 newTargetPosition)
    {
        Transform anchor = localAnchor.transform;
        Vector3 relativeAnchorPosition = anchor.InverseTransformPoint(newTargetPosition);
        //todo add a warning if distance of vector is more than 3 meters

        CCCRoomEvent ev = new CCCRoomEvent("move", uuid, null, relativeAnchorPosition);
        CCCRoomMgr.Instance.SendMessage(ev);
    }

    public void RunStepChangeCommand(int step)
    {
        CCCRoomEvent ev = new CCCRoomEvent(step);
        CCCRoomMgr.Instance.SendMessage(ev);
    }

    /**
     * On what object to play or stop the animation.   
     **/
    public void RunAnimationCommand(string uuid, bool play, string animationName)
    {
        CCCRoomEvent ev = new CCCRoomEvent("animation", uuid, play ? "play:" : "stop:" + animationName);
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

    public void Calibrate()
    {
        Vector3 camTransform = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.forward.normalized;

        Vector3 targetPosition = camTransform + forward * 1.0F;

        localAnchor.transform.position = targetPosition;

        //we may want to use the real world surface normal.
        Quaternion targetRotation = Camera.main.transform.rotation;
        localAnchor.transform.rotation = targetRotation;

        //replay all the events we have captured with the new calibration
        Replay();
    }
}
