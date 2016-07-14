using UnityEngine;
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

    public string id;
    public string eventType;
    public string prefabName;
    public Vector3 relativeToCommonAnchor;
    public Quaternion facingDirection;
    //public byte[] data;

    public string asJson()
    {
        return JsonUtility.ToJson(this);
    }

    public CCCRoomEvent()
    {
        this.id = Guid.NewGuid().ToString();
    }
    public CCCRoomEvent(string eventType, string id)
    {
        this.id = id;
        this.eventType = eventType;
    }
    public CCCRoomEvent(string eventType, string id, string prefabName, Vector3 relativeToCommonAnchor)
    {
        this.id = id;
        this.eventType = eventType;
        this.prefabName = prefabName;
        this.relativeToCommonAnchor = relativeToCommonAnchor;
    }

    public CCCRoomEvent(string eventType, string prefabName, Vector3 relativeToCommonAnchor)
    {
        this.id = Guid.NewGuid().ToString();
        this.eventType = eventType;
        this.prefabName = prefabName;
        this.relativeToCommonAnchor = relativeToCommonAnchor;
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
