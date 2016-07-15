using UnityEngine;
using System.Collections;

public class CCCSpawner : MonoBehaviour {
    public CCCRoom room;

    public void SpawnCube()
    {
        if (room == null)
        {
            return;
        }

        Vector3 camTransform = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.forward.normalized;

        Vector3 targetPosition = camTransform + forward * 3;
        room.Create("cube", targetPosition);
    }

    public void Calibrate()
    {
        if (room == null)
        {
            return;
        }

        Vector3 camTransform = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.forward.normalized;

        Vector3 targetPosition = camTransform + forward * 1.75F;

        room.localAnchor.transform.position = targetPosition;
    }
}
