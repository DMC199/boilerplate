using UnityEngine;
using System.Collections;

public class VoiceModelSpawn : MonoBehaviour {

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
    }
}
