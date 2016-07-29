using UnityEngine;
using System.Collections;

public class CCCSpawner : MonoBehaviour {
    public CCCRoom room;

    void Start()
    {
        Calibrate();

        //uncomment for debugging on simulator
        //StartCoroutine(SpawnCube(1.0f));
        //StartCoroutine(SpawnCube(5.0f));
        //StartCoroutine(Calibrate(15.0f));
    }

    IEnumerator SpawnCube(float time)
    {
        yield return new WaitForSeconds(time);

        SpawnCube();
    }

    IEnumerator Calibrate(float time)
    {
        yield return new WaitForSeconds(time);
        Calibrate();
    }

    public void SpawnCube()
    {
        if (room == null)
        {
            return;
        }

        Vector3 camTransform = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.forward.normalized;

        //we will spawn a cube 
        //  3 meters from me (the camera)
        Vector3 targetPosition = camTransform + forward * 3;
        //  and have it facing the camera.  
        Quaternion towardsMe = Quaternion.LookRotation(Camera.main.transform.forward);

        //  then put it in the room (which in turn should propogate it to everyone in the room)
        room.Create("cube", targetPosition, towardsMe);
    }

    public void SpawnTransmission()
    {
        if (room == null)
        {
            return;
        }

        Vector3 camTransform = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.forward.normalized;

        //we will spawn the gear animation 
        //  3 meters from me (the camera)
        Vector3 targetPosition = camTransform + forward * 3;
        //  and have it facing the camera.  
        Quaternion towardsMe = Quaternion.LookRotation(Camera.main.transform.forward);

        //  then put it in the room (which in turn should propogate it to everyone in the room)
        room.Create("transmission", targetPosition, towardsMe);
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

        //we may want to use the real world surface normal.
        Quaternion targetRotation = Camera.main.transform.rotation;        
        room.localAnchor.transform.rotation = targetRotation;

        //replay all the events we have captured with the new calibration
        room.Replay();
    }
}
