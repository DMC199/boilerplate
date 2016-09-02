using UnityEngine;
using System.Collections;

public class UICallout : MonoBehaviour {

    public GameObject targetCamera;
    public GameObject textLabel;
    public GameObject textBg;

    //public GameObject indicatorLine;
    //public GameObject indicatorMesh;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        /*
        Vector2 cameraPos = new Vector2(targetCamera.transform.position.z, targetCamera.transform.position.x);
        Vector2 billboardPos = new Vector2(textLabel.transform.position.z, textLabel.transform.position.x);
        Vector2 delta = cameraPos - billboardPos;
        
        float angle = Vector2.Angle(delta, new Vector2(0, 1)) - 90;

        if (cameraPos.x > billboardPos.x)
        {
            angle *= -1;
        }

        textLabel.transform.rotation = Quaternion.AngleAxis(angle, targetCamera.transform.up);
        */

        Vector3 sizeCalculated = textLabel.GetComponent<Renderer>().bounds.size;
        textBg.transform.localScale = sizeCalculated + (Vector3.right.normalized * 0.05f) + (Vector3.up.normalized * 0.005f);
        textBg.transform.position = new Vector3( textLabel.transform.position.x + ( sizeCalculated.x / 2.0f), textBg.transform.position.y, textBg.transform.position.z);

        //textBg.transform.rotation = textLabel.transform.rotation;

        /*
        float indicatorScale = Vector3.Distance(textLabel.transform.position, targetCamera.transform.position);
        print(indicatorScale);
        
        indicatorMesh.transform.lossyScale.Set(0.001f, indicatorScale / 2.0f, 0.001f);

        indicatorLine.transform.LookAt(targetCamera.transform);
        */
    }
}
