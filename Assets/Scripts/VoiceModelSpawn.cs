using UnityEngine;
using System.Collections;
using HoloToolkit.Sharing;

public class VoiceModelSpawn : MonoBehaviour
{
    [Tooltip("Duration of the animation rotation.  The duration multiplied by the rotation speed should be multiple of 360 to end at the start orientation.")]
    public float RotationDuration = 6.0f;

    [Tooltip("Rotation speed in degrees per second")]
    public float RotationSpeed = 60.0f;

    public AnimationCurve SpeedCurve = new AnimationCurve(new Keyframe(0f, 1.5f, 0, 0),
        new Keyframe(0.8f, 2.8f, -0.0012f, -0.0012f), new Keyframe(1.8f, 1.5f, -1.2f, -1.2f), new Keyframe(3f, 1f, 0, 0));


    public GameObject spherePrefab;
    public GameObject cubePrefab;
    public GameObject cylinderPrefab;

    public ArrayList objectsSpawned = new ArrayList();

    public void Start()
    {
        CustomMessages.Instance.MessageHandlers[CustomMessages.TestMessageID.OsbPlaceObject] = this.ProcessRemotePlaceObject;
    }

    public void SpawnSphere()
    {
        SpawnObject(0, spherePrefab, CustomMessages.OsbObjectType.Sphere);
    }

    public void SpawnCube()
    {
        SpawnObject(0, cubePrefab, CustomMessages.OsbObjectType.Cube);
    }

    public void SpawnCylinder()
    {
        SpawnObject(0, cylinderPrefab, CustomMessages.OsbObjectType.Cylinder);
    }

    void ProcessRemotePlaceObject(NetworkInMessage msg)
    {
        // Parse the message
        long userID = msg.ReadInt64();
        CustomMessages.OsbObjectType objType = (CustomMessages.OsbObjectType) msg.ReadByte();
        Vector3 targetPosition = CustomMessages.Instance.ReadVector3(msg);

        Transform anchor = ImportExportAnchorManager.Instance.gameObject.transform;

        switch (objType)
        {
            case CustomMessages.OsbObjectType.Cube:
                SpawnObject(cubePrefab, anchor.TransformPoint(targetPosition));
                break;
            case CustomMessages.OsbObjectType.Cylinder:
                SpawnObject(cylinderPrefab, anchor.TransformPoint(targetPosition));
                break;
            case CustomMessages.OsbObjectType.Sphere:
                SpawnObject(spherePrefab, anchor.TransformPoint(targetPosition));
                break;
        }
    }


    void SpawnObject(long UserId, GameObject spawnObject, CustomMessages.OsbObjectType objType)
    {
        // TODO clean up transforms
        Vector3 camTransform = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.forward.normalized;

        Vector3 targetPosition = camTransform + forward * 3;

        SpawnObject(spawnObject, targetPosition);

        Transform anchor = ImportExportAnchorManager.Instance.gameObject.transform;
        CustomMessages.Instance.SendPlaceObject(anchor.InverseTransformPoint(targetPosition), objType);
    }

    public void SpawnObject(GameObject spawnMe, Vector3 targetLocation)
    {
        Quaternion camRotation = Camera.main.transform.rotation;

        Object obj = Instantiate(spawnMe, targetLocation, camRotation);
        objectsSpawned.Add(obj);
        StartCoroutine(AnimatedOnSpawnRoutine(spawnMe));
    }

    public void DeleteAll()
    {
        foreach (Object obj in objectsSpawned)
        {
            Destroy(obj);
        }
        objectsSpawned.Clear();
    }

    //TODO: Add to some sort of static, animation routines class
    private IEnumerator AnimatedOnSpawnRoutine(GameObject spawnedObject)
    {
        var startTime = Time.time;
        var endTime = startTime + RotationDuration;
        while (startTime < endTime)
        {
            spawnedObject.transform.Rotate(Vector3.up * Time.deltaTime * RotationSpeed * SpeedCurve.Evaluate(Time.time));
            yield return null;
            startTime = Time.time;
        }
    }
}
