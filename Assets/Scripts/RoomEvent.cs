using UnityEngine;
using System.Collections;

public class RoomEvent : MonoBehaviour {
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
}
