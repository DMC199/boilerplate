using UnityEngine;
using System.Collections;

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

    public void SpawnSphere()
    {
        SpawnObject(spherePrefab);
    }

    public void SpawnCube()
    {
        SpawnObject(cubePrefab);
    }

    public void SpawnCylinder()
    {
        SpawnObject(cylinderPrefab);
    }

    public void SpawnObject(GameObject spawnMe)
    {
        Vector3 camTransform = Camera.main.transform.position;
        Quaternion camRotation = Camera.main.transform.rotation;

        Vector3 targetPosition = new Vector3(camTransform.x, camTransform.y, camTransform.z + 3);

        Instantiate(spawnMe, targetPosition, camRotation);
        StartCoroutine(AnimatedOnSpawnRoutine(spawnMe));
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
