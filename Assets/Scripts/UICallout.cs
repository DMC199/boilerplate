using UnityEngine;
using System.Collections;

public class UICallout : MonoBehaviour {

    public GameObject targetCamera;
    public GameObject textLabel;
    public GameObject textBg;

    private GameObject prevBtn;
    private GameObject nextBtn;
    

    private Renderer txtRender;
    private Renderer bgRender;

    private Vector3 nextBtnSize;
    

    // Use this for initialization
    void Start () {
        prevBtn = transform.Find("Prev").gameObject;
        nextBtn = transform.Find("Next").gameObject;

        txtRender = textLabel.GetComponent<Renderer>();
        bgRender = textBg.GetComponent<Renderer>();

        nextBtnSize = nextBtn.GetComponent<Renderer>().bounds.size;
    }
	
	// Update is called once per frame
	void Update () {
     

        Vector3 txtWidth = txtRender.bounds.size;
        textBg.transform.localScale = txtWidth + (Vector3.right.normalized * 0.05f) + (Vector3.up.normalized * 0.005f);
        textBg.transform.localPosition = new Vector3( textLabel.transform.localPosition.x + (txtWidth.x / 2.0f), textBg.transform.localPosition.y, textBg.transform.localPosition.z);

        Vector3 bgWidth = bgRender.bounds.size;
        nextBtn.transform.localPosition = new Vector3( bgWidth.x - nextBtnSize.x - (Vector3.right.normalized * 0.004f).x, nextBtn.transform.localPosition.y, nextBtn.transform.localPosition.z);


    }
}
