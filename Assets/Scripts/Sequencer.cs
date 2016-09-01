using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Windows.Speech;
using System.IO;

public class Sequencer : MonoBehaviour {

    //UnityEditor
    public CCCRoom room;
    public int currentStep;
    public float lerpTime = 1.0f;
    public string jsonFileName;
    public GameObject stepLabel;


    public GameObject callout;

    private int lastCurrentStep;
    private bool doLerp = false;
    private TextMesh stepLabelMesh;

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
        public float tweentime;
        public string animation;
        public float alpha;
    }

	// Use this for initialization
	void Start () {
        string path = Application.dataPath + "/StreamingAssets/" + jsonFileName + ".json";
        string jsonContents = File.ReadAllText(path);
        root = JSON.Parse(jsonContents);

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

			/*
            if (jStep["name"] != null)
            {
                step.name = stepInfo["name"];
            }

            if (jStep["callout"] != null)
            {
                JSONNode calloutInfo = stepInfo["callout"];
                step.hasCallout = true;
                step.calloutText = calloutInfo["text"];
                step.calloutTarget = parseVector(calloutInfo["target"].AsArray);
            } else
            {
                step.hasCallout = false;
            }*/
            
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
                    objInfo.position = new Vector3();
                }

                if (infoNode["tweentime"] != null)
                {
                    objInfo.tweentime = infoNode["tweentime"].AsFloat;
                }
                else
                {
                    objInfo.tweentime = lerpTime;
                }
                step.state.Add(objInfo);

                if (infoNode["alpha"] != null)
                {
                    objInfo.alpha = infoNode["alpha"].AsFloat;
                } else
                {
                    objInfo.alpha = 1;
                }

                step.state.Add(objInfo);

            }

            //StepInfo info = new StepInfo();
            steps.Add(step);
        }

        setActiveStep(0);
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
        if (room != null)
        {
            room.RunStepChangeCommand(step);

        }else
        {
            Debug.LogError("No Room defined at Sequencer.triggerActiveStep " + step );
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
        currentStep++;

        if (currentStep >= steps.Count)
        {
            currentStep = 0;          
        }

        triggerActiveStep(currentStep);
    }

    public void LastStep()
    {
        currentStep--;

        if(currentStep < 0)
        {
            currentStep = steps.Count - 1;
        }

        triggerActiveStep(currentStep);
    }

    public void NavigationGrammarRecognized(PhraseRecognizedEventArgs args)
    {
        foreach (SemanticMeaning meaning in args.semanticMeanings) {
            if(meaning.key == "Direction")
            {
                string direction = meaning.values[0];
                if(direction == "next")
                {
                    NextStep();
                } else
                {
                    LastStep();
                }
            }

            if(meaning.key == "GoToStep")
            {
                string stepNumberString = meaning.values[0];
                currentStep = (int.Parse(stepNumberString) - 1);

                triggerActiveStep(currentStep);
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
            }
        }
        

    }

    public void setActiveStep(int index)
    {
        StepInfo step = steps[index];

        if(stepLabelMesh!=null) stepLabelMesh.text = step.stepid + ": " + step.label;

        foreach (ObjectStepInfo obj in step.state)
        {
            GameObject go = GameObject.Find(obj.name);
            if (go != null)
            {
                Debug.Log("Setting " + obj.name + " to visible:" + obj.visible);

                Renderer parentRenderer = go.GetComponent<Renderer>();
                if (parentRenderer == null)
                {
                    foreach (Transform child in go.transform)
                    {
                        child.GetComponent<Renderer>().enabled = obj.visible;
                    }
                }
                else
                {
                    parentRenderer.enabled = obj.visible;
                }


                if (!obj.position.Equals(Vector3.zero) && go.transform.localPosition.Equals(Vector3.zero))
                {
                    go.transform.localPosition = obj.position;
                }
                else if (obj.position.Equals(Vector3.zero) && !go.transform.localPosition.Equals(Vector3.zero))
                {
                    doLerp = true;

                    LerpPart(go.transform, go.transform.localPosition, obj.position, obj.tweentime);

                }

                if (!string.IsNullOrEmpty(obj.animation))
                {
                    //print("Setting animation: " + obj.animation);
                    Animation anim = go.GetComponent<Animation>();
                    if (anim != null)
                    {
                        //print("Found animation component, playing.. " + obj.animation);
                        anim.Play(obj.animation);
                    }
                    else
                    {
                        print("Couldn't find animation component on game object!");
                    }
                }
                else
                {
                    Animation anim = go.GetComponent<Animation>();
                    if (anim != null)
                    {
                        //print("Stopping anim...");
                        anim.Stop();
                    }
                }

                Renderer renderer = go.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material[] mats = go.GetComponent<Renderer>().materials;

                    for (int m = 0; m < mats.Length; m++)
                    {
                        if (mats[m].name != "tic mark (Instance)")
                        {
                            Color newColor = new Color(mats[m].color.r, mats[m].color.g, mats[m].color.b, obj.alpha);
                            mats[m].color = newColor;
                            if (obj.alpha != 1)
                            {
                                mats[m].SetFloat("_Mode", 3.0f);
                            }
                            else
                            {
                                mats[m].SetFloat("_Mode", 0.0f);
                            }
                        }
                    }
                }
            }
        }
        /*
        // Update the callout text overlay.
        if (step.hasCallout)
        {
            TextMesh mesh = callout.GetComponent<TextMesh>();
            mesh.text = step.calloutText;
            mesh.color = new Color(1,1,1,1);
            //mesh.transform.transform.
        } else
        {
            TextMesh mesh = callout.GetComponent<TextMesh>();
            mesh.color = new Color(1, 1, 1, 0);
        }*/
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
            LastStep();
        }

	    /*if(currentStep != lastCurrentStep)
        {
            lastCurrentStep = currentStep;
            setActiveStep(currentStep);
        }*/
	}
}
