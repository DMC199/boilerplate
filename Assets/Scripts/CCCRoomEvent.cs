﻿using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class CCCRoomEvent :EventArgs
{
    //object created
    //{ eventType="create", prefab="prefab-name", id="UUID1", relativeToCommonAnchor="x,y,z"} 
    //object destroyed
    //{ eventType="destroy", id="UUID1"}
    //object (or user) moved
    //{ eventType="move", id="UUID1", relativeToCommonAnchor="x,y,z" }

    //custom event
    //{ eventType="custom", data="BASE64-ENCODED-DATA" }

    public string eventId;
    public string objectRef;
    public string eventType;
    public string prefabName;
    public Vector3 relativeToCommonAnchor;
    public Quaternion facingDirection;
    public string data;

    public string asJson()
    {
        return JsonUtility.ToJson(this);
    }

    public CCCRoomEvent()
    {
        this.eventId = Guid.NewGuid().ToString();
        this.objectRef = Guid.NewGuid().ToString();
    }
    public CCCRoomEvent(string eventType, string id)
    {
        this.eventId = Guid.NewGuid().ToString();
        this.objectRef = id;
        this.eventType = eventType;
    }
    public CCCRoomEvent(string eventType, string id, string prefabName, Vector3 relativeToCommonAnchor)
    {
        this.eventId = Guid.NewGuid().ToString();
        this.objectRef = id;
        this.eventType = eventType;
        this.prefabName = prefabName;
        this.relativeToCommonAnchor = relativeToCommonAnchor;
    }

    public CCCRoomEvent(string eventType, string prefabName, Vector3 relativeToCommonAnchor, Quaternion facingDirection)
    {
        this.eventId = Guid.NewGuid().ToString();
        this.objectRef = Guid.NewGuid().ToString();
        this.eventType = eventType;
        this.prefabName = prefabName;
        this.relativeToCommonAnchor = relativeToCommonAnchor;
        this.facingDirection = facingDirection;
        this.data = "";
    }

    public CCCRoomEvent(string eventType, string objRef, string actionDetails)
    {
        this.eventId = Guid.NewGuid().ToString();
        this.objectRef = objRef;
        this.eventType = eventType;
        this.prefabName = "";
        this.relativeToCommonAnchor = Vector3.zero;
        this.facingDirection = Quaternion.identity;
        this.data = actionDetails;
    }

    public static CCCRoomEvent fromJson(string json)
    {
        return JsonUtility.FromJson<CCCRoomEvent>(json);
    }

    public override string ToString()
    {
        return asJson();
    }
}
