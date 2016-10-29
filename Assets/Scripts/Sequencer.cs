using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Windows.Speech;
using System.IO;
using System;

public class Sequencer : MonoBehaviour {

    //UnityEditor
    public CCCRoom room;
    public int currentStep = -1;
    public float lerpTime = 1.0f;
    public string jsonFileName;
    public GameObject stepLabel;

    public bool xRayState =false;
    private float xRayAlpha = 0.3f;

    public GameObject callout;

    private int lastCurrentStep;
    private bool doLerp = false;
    private TextMesh stepLabelMesh;

    private List<string> xrayModels;

    JSONNode root;
    List<StepInfo> steps;

    public class StepInfo
    {

        public string name;

        public bool hasCallout;
        public string calloutText;
        public Vector3 calloutTarget;

        public int stepid;
        public string label;
        public List<ObjectStepInfo> state = new List<ObjectStepInfo>();
        
    }

    public class ObjectStepInfo
    {
        public string name;
        public bool visible;
        public Vector3 position;
        public Vector3 defaultPosition;
        public Vector3 fixedPosition;
        public float tweentime;
        public string animation;
        public float alpha;
    }

	// Use this for initialization
	void Start () {

    
        Application.SetStackTraceLogType( LogType.Log, StackTraceLogType.None );
        xRayState = false;

        string path = Application.dataPath + "/StreamingAssets/" + jsonFileName + ".json";
        string jsonContents = File.ReadAllText(path);
        root = JSON.Parse(jsonContents);


        xrayModels = new List<string>();
        JSONArray jsonXray = root["xray"].AsArray;
        for (int i = 0; i < jsonXray.Count; i++)
        {
            xrayModels.Add(jsonXray[i]);
        }

        print("xray models count: " + xrayModels.Count );

        steps = new List<StepInfo>();
        JSONArray jsonSteps = root["steps"].AsArray;

        print("Step count: " + jsonSteps.Count);

        for (int i = 0; i < jsonSteps.Count; i++)
        {

            JSONNode jStep = jsonSteps[i];
            StepInfo step = new StepInfo();
            step.stepid = jStep["stepid"].AsInt;
            step.label = jStep["label"];
            step.state = new List<ObjectStepInfo>();

            JSONArray objectsInStep = jStep["state"].AsArray;
            
            for (int o = 0; o < objectsInStep.Count; o++)
            {
                ObjectStepInfo objInfo = new ObjectStepInfo();

                JSONNode infoNode = objectsInStep[o];
                
                if (infoNode["visible"] != null)
                {
                    objInfo.visible = infoNode["visible"].AsBool; 
                } else
                {
                    objInfo.visible = true;
                }


                if (infoNode["name"] != null)
                {
                    objInfo.name = infoNode["name"].Value;
                }
                else
                {
                    print("ERROR!! All objects must have a name!");
                }

                GameObject go = GameObject.Find(objInfo.name);

                if (infoNode["animation"] != null)
                {
                    objInfo.animation = infoNode["animation"].Value;
                }
                else
                {
                    objInfo.animation = "";
                }


                if (infoNode["position"] != null)
                {
                    objInfo.position = parseVector(infoNode["position"].AsArray);
                } else
                {
                    objInfo.position = Vector3.zero;
                }

                if (infoNode["defaultPosition"] != null)
                {
                    objInfo.defaultPosition = parseVector(infoNode["defaultPosition"].AsArray);
                    if (infoNode["position"] == null) objInfo.position = objInfo.defaultPosition;
                }
                else
                {
                    objInfo.defaultPosition = Vector3.zero;
                }

                if (infoNode["fixedPosition"] != null)
                {
                    Debug.Log("found fixedPosition " + objInfo.name);
                    objInfo.fixedPosition = parseVector(infoNode["fixedPosition"].AsArray);
                }else
                {
                    objInfo.fixedPosition = Vector3.zero;
                }


                if (infoNode["tweentime"] != null)
                {
                    objInfo.tweentime = infoNode["tweentime"].AsFloat;
                }
                else
                {
                    objInfo.tweentime = lerpTime;
                }
             

                if (infoNode["alpha"] != null)
                {
                    Debug.LogFormat("Obj {0} alpha {1}", objInfo.name, infoNode["alpha"]);
                    objInfo.alpha = infoNode["alpha"].AsFloat;
                } else
                {
                    objInfo.alpha = 1.0f;
                }

                step.state.Add(objInfo);

            }

            steps.Add(step);
        }

        currentStep = -1;
        setActiveStep(7);
    }

    public void Awake()
    {
        if (room != null)
        {
            room.OnPropagateRoomEvent += Room_OnPropagateRoomEvent;

            if (stepLabel != null)
            {
                stepLabelMesh = stepLabel.GetComponent<TextMesh>();
            }
        }
        else
        {
            Debug.LogError("No Room defined for at Sequencer.Awake");
        }

    }

    public void triggerActiveStep(int step)
    {
        if (room != null && CCCRoomMgr.Instance.isConnected() )
        {
            room.RunStepChangeCommand(step);

        }else
        {
            if( room == null ) Debug.LogError("No Room defined at Sequencer.triggerActiveStep " + step );
			setActiveStep(step);
        }
    }

    private void Room_OnPropagateRoomEvent(object sender, CCCRoomEvent e)
    {
        if ("step-changed".Equals(e.eventType))
        {
            this.setActiveStep(e.intData);
        }
    }

    public void NextStep()
    {
        int step = currentStep + 1;
        
        if (step >= steps.Count)
        {
            step = 0;          
        }

        triggerActiveStep(step);
    }

    public void PrevStep()
    {
        int step = currentStep - 1;

        if(step < 0)
        {
            step = steps.Count - 1;
        }

        triggerActiveStep(step);
    }

    public void ToggleXray()
    {
        xRayState = !xRayState;

        foreach ( string model in xrayModels)
        {
            GameObject go = GameObject.Find(model);
            Renderer renderer = go.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material[] mats = go.GetComponent<Renderer>().materials;

                for (int m = 0; m < mats.Length; m++)
                {
                    Material mat = mats[m];

                    if (mat.name != "stripe (Instance)")
                    {
                        
                        if (xRayState)
                        {
                            Color newColor = new Color(mat.color.r, mat.color.g, mat.color.b, xRayAlpha);
                            mat.SetColor("_Color", newColor);

                            //Debug.LogFormat("Transparency for Material {0} {1} ", mat.name, mat.GetFloat("_Mode"));
                            mat.SetFloat("_Mode", 2.0f);

                            SetupMaterialWithBlendMode(mat, 2);
                        }
                        else
                        {
                            Color newColor = new Color(mat.color.r, mat.color.g, mat.color.b, 1.0f);
                            mat.SetColor("_Color", newColor);
                            
                            //Debug.LogFormat("Opaque for Material {0} {1} ", mat.name, mat.GetFloat("_Mode"));
                            mat.SetFloat("_Mode", 0.0f);

                            SetupMaterialWithBlendMode(mat, 0);
                        }
                    }
                }
            }
        }
    }

    public void NavigationGrammarRecognized(PhraseRecognizedEventArgs args)
    {

        try
        {
            foreach (SemanticMeaning meaning in args.semanticMeanings)
            {
                //process commands if you are a server or if you are a client but not connected
                if (CCCRoomMgr.Instance.isServer() || (CCCRoomMgr.Instance.isClient() && !CCCRoomMgr.Instance.isConnected() ))
                {
                    if (meaning.key == "Direction")
                    {
                        string direction = meaning.values[0];
                        if (direction == "next")
                        {
                            NextStep();
                        }
                        else
                        { 
                            PrevStep();
                        }
                    }

                    if (meaning.key == "GoToStep")
                    {
                        string stepNumberString = meaning.values[0];
                        int step  = (int.Parse(stepNumberString) - 1);

                        triggerActiveStep(step);
                    }
                }

                if (meaning.key == "Action")
                {
                    Debug.Log("Calibrating");
                    if ("calibrate".Equals(meaning.values[0]))
                    {
                        if (room != null)
                        {
                            room.Calibrate();
                        }
                    }

                    if ("xray".Equals(meaning.values[0]))
                    {
                        ToggleXray();
                    }
                }
            }
        }catch( Exception ex)
        {
            Debug.LogError("Exception in Sequencer NavigationGrammarRecognized " + ex.Message);
        }
    }

    public void setActiveStep(int index)
    {
        Debug.Log("step " + index + " " + currentStep);
        if (currentStep == index) return;

        if (index >= steps.Count) index = 0;
        if (index < 0) index = 0;

        currentStep = index;
        
        StepInfo step = steps[currentStep];

        if(stepLabelMesh!=null) stepLabelMesh.text = step.stepid + ": " + step.label;
        
        foreach (ObjectStepInfo obj in step.state)
        {
            GameObject go = GameObject.Find(obj.name);
            if (go != null)
            {
                Debug.Log("Setting " + obj.name + "," + go.name + " to visible:" + obj.visible + " step:" + index);

                //enable up and down the hierarchy
                Renderer parentRenderer = go.GetComponent<Renderer>();
                if (parentRenderer == null)
                {
                    foreach (Transform child in go.transform)
                    {
                        Renderer childRender = child.GetComponent<Renderer>();
                        if(childRender== null)
                        {
                            foreach (Transform child2 in child.transform)
                            {
                                child2.GetComponent<Renderer>().enabled = obj.visible;
                            }

                        } else {
                            childRender.enabled = obj.visible;
                        }
                    }
                }
                else
                {
                    parentRenderer.enabled = obj.visible;
                }

                //set the location                
                Vector3 currentPos = go.transform.localPosition;
                if (!obj.position.Equals(obj.defaultPosition) && currentPos.Equals( obj.defaultPosition ))
                {
                    Debug.Log("set the position for " + go.name+ " to " + obj.position);
                    go.transform.localPosition = obj.position;
                }
                else if (obj.position.Equals(obj.defaultPosition) && !currentPos.Equals(obj.defaultPosition))
                {
                    Debug.Log("Move the position for " + go.name + " to " + obj.position + " from " + currentPos + " default " + obj.defaultPosition);
                    doLerp = true;
                    LerpPart(go.transform, currentPos, obj.position, obj.tweentime);
                }
                else if(!obj.position.Equals(obj.defaultPosition) && !currentPos.Equals(obj.defaultPosition))
                {
                    Debug.Log("Sync the position for " + go.name + " to " + obj.position + " from " + currentPos + " default " + obj.defaultPosition);
                    go.transform.localPosition = obj.position;
                    //doLerp = true;
                    //LerpPart(go.transform, currentPos, obj.position, obj.tweentime);
                }
            
                //animate, blend multiple animations
                if (!string.IsNullOrEmpty(obj.animation))
                {
                    if (obj.animation.IndexOf(',') > 0)
                    {
                        //it's mutliple
                        string[] animations = obj.animation.Split(',');
                        Animation anim = go.GetComponent<Animation>();

                        foreach ( string a in animations)
                        {
                            AnimationState s = anim[a];
                            if(s != null) {
                                if( index != 6) s.wrapMode = WrapMode.Loop;//step 6 we want to apply only once
                                s.enabled = true;
                                s.blendMode = AnimationBlendMode.Blend;
                                anim.Blend(a);
                            }
                            else
                            {
                                print("Couldn't find animation " + a);
                            }
                        }
                    }
                    else
                    { 
                        Animation anim = go.GetComponent<Animation>();
                        if (anim != null)
                        {
                            AnimationState s = anim[obj.animation];
                            if(s != null) {
                                s.wrapMode = WrapMode.Loop;
                                s.enabled = true;
                                anim.Play(obj.animation);
                            }
                        }
                        else
                        {
                            print("Couldn't find animation "+ obj.animation);
                        }
                    }
                }
                else
                {
                    Animation anim = go.GetComponent<Animation>();
                    if (anim != null)
                    {
                        anim.Stop();
                    }
                }

                //set the color
                if (obj.visible)
                {
                    Renderer renderer = go.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material[] mats = go.GetComponent<Renderer>().materials;

                        for (int m = 0; m < mats.Length; m++)
                        {
                            Material mat = mats[m];

                            if (mat.name != "stripe (Instance)")
                            {
                               
                                if (xRayState && xrayModels.Contains( obj.name ) )
                                {
                                    Color newColor = new Color(mat.color.r, mat.color.g, mat.color.b, xRayAlpha);
                                    mat.SetColor("_Color", newColor);
                                } else {
                                    Color newColor = new Color(mat.color.r, mat.color.g, mat.color.b, obj.alpha);
                                    mat.SetColor("_Color", newColor);

                                    if (obj.alpha != 1.0f)
                                    {
                                        //Debug.LogFormat("Transparency for Material {0} {1} ", mat.name, mat.GetFloat("_Mode"));
                                        mat.SetFloat("_Mode", 2.0f);

                                        SetupMaterialWithBlendMode(mat, 2);
                                    }
                                    else
                                    {
                                        //Debug.LogFormat("Opaque for Material {0} {1} ", mat.name, mat.GetFloat("_Mode"));
                                        mat.SetFloat("_Mode", 0.0f);

                                        SetupMaterialWithBlendMode(mat, 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    Vector3 parseVector(JSONArray values)
    {
        Vector3 result = new Vector3();
        result.x = values[0].AsFloat;
        result.y = values[1].AsFloat;
        result.z = values[2].AsFloat;
        return result;
    }

    void LerpPart(Transform tTrans, Vector3 start, Vector3 end, float duration)
    {
        if(doLerp == true)
        {
            StartCoroutine(LerpMachine(tTrans, start, end, duration));
            doLerp = false;
        }
    }

    IEnumerator LerpMachine(Transform targetTransform, Vector3 startPos, Vector3 endPos, float time)
    {
        var i = 0.0f;
        var rate = 1.0f / time;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            targetTransform.localPosition = Vector3.Lerp(startPos, endPos, i);
            yield return null;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextStep();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PrevStep();
        }


        if (Input.GetKeyDown(KeyCode.X))
        {
            ToggleXray();
        }

    }

    //helper function to move between render modes
    public static void SetupMaterialWithBlendMode(Material material, int blendMode)
    {
        switch (blendMode)
        {
            case 0://opaque
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case 1://cutout:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;
                break;
            case 2://fade
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case 3://transparent
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
        }
    }
}
