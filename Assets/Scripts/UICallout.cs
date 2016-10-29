using UnityEngine;
using System.Collections;

public class UICallout : MonoBehaviour {

    public GameObject targetCamera;
    public GameObject textLabel;
    public GameObject textBg;

    private GameObject prevBtn;
    private GameObject nextBtn;
    private GameObject label;


    private Renderer txtRender;
    private Renderer textBgRender;

    private Vector3 nextBtnSize;
    private Vector3 prevBtnSize;
    private float lastWidth;

    // Use this for initialization
    void Start () {
        prevBtn = transform.Find("Prev").gameObject;
        nextBtn = transform.Find("Next").gameObject;
        label = transform.Find("Label").gameObject;

        txtRender = textLabel.GetComponent<Renderer>();
        textBgRender = textBg.GetComponent<Renderer>();

        prevBtnSize = prevBtn.GetComponent<Renderer>().bounds.size;
        nextBtnSize = nextBtn.GetComponent<Renderer>().bounds.size;

        lastWidth = 0.0f;
    }
	
	// Update is called once per frame
	void Update () {
     
        Vector3 txtWidth = txtRender.bounds.size;
        if( txtWidth.x != lastWidth)
        {
            layout(txtWidth.x);
        }
    }

    void layout(float width)
    {
        textBg.transform.localScale = new Vector3(width + 0.05f, textBg.transform.localScale.y, textBg.transform.localScale.z);
        textBg.transform.localPosition = new Vector3(textLabel.transform.localPosition.x + (width / 2.0f), textBg.transform.localPosition.y, textBg.transform.localPosition.z);

        Vector3 bgWidth = textBgRender.bounds.size;
        nextBtn.transform.localPosition = new Vector3(label.transform.localPosition.x + bgWidth.x + nextBtnSize.x / 2.0f + 0.02f, nextBtn.transform.localPosition.y, nextBtn.transform.localPosition.z);

        lastWidth = width;
    }
}
