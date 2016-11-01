using UnityEngine;
using System.Collections;

public class UICallout : MonoBehaviour {

    public GameObject textLabel;
    public GameObject textBg;

    public GameObject label;
    
    private TextMesh txtMesh;
    private MeshFilter textBgMesh;

    private GameObject nextBtn;
    private Vector3 nextBtnSize;
    
    // Use this for initialization
    void Start () {
    
        txtMesh = textLabel.GetComponent<TextMesh>();
        textBgMesh = textBg.GetComponent<MeshFilter>();


        nextBtn = transform.Find("Next").gameObject;
        nextBtnSize = nextBtn.transform.Find("bg").GetComponent<Renderer>().bounds.size;

    }
	
	// Update is called once per frame
	void Update () {
        
    }

    void layout(float width)
    {
        Debug.LogFormat("width {0} scale {1} ", width, textBg.transform.localScale);
        
        textBg.transform.localScale = new Vector3(width + 0.05f, textBg.transform.localScale.y, textBg.transform.localScale.z);
        textBg.transform.localPosition = new Vector3(textLabel.transform.localPosition.x + (width / 2.0f), textBg.transform.localPosition.y, textBg.transform.localPosition.z);

        nextBtn.transform.localPosition = new Vector3(label.transform.localPosition.x + (width + 0.05f) + (nextBtnSize.x / 2.0f) + 0.008f, nextBtn.transform.localPosition.y, nextBtn.transform.localPosition.z);
        
    }

    public void SetText(string text)
    {
        txtMesh.text = text;
        float width = GetWidth(txtMesh);

        layout(width /2);
    }

    public static float GetWidth(TextMesh mesh)
    {
        float width = 0;
        foreach (char symbol in mesh.text)
        {
            CharacterInfo info;
            if (mesh.font.GetCharacterInfo(symbol, out info, mesh.fontSize, mesh.fontStyle))
            {
                width += info.advance;
            }
        }
        return width * (mesh.characterSize * 0.001f);
    }
}
