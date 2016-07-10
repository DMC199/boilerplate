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
    //public byte[] data;

    public string asJson()
    {
        return JsonUtility.ToJson(this);
    }

    public CCCRoomEvent()
    {
        id = Guid.NewGuid().ToString();
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
