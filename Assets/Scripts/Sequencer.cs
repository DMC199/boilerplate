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

    private int lastCurrentStep;
    private bool doLerp = false;

    JSONNode root;
    List<StepInfo> steps;

    public class StepInfo
    {
        public string name;
        public List<ObjectStepInfo> objects;
    }

    public class ObjectStepInfo
    {
        public string name;
        public bool visible;
        public Vector3 position;
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
            JSONNode stepInfo = jsonSteps[i];
            StepInfo step = new StepInfo();

            if (stepInfo["name"] != null)
            {
                step.name = stepInfo["name"];
            }

            JSONArray objectsInStep = stepInfo["state"].AsArray;
           
            step.objects = new List<ObjectStepInfo>();

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

                if (infoNode["alpha"] != null)
                {
                    objInfo.alpha = infoNode["alpha"].AsFloat;
                } else
                {
                    objInfo.alpha = 1;
                }

                step.objects.Add(objInfo);
            }

            //StepInfo info = new StepInfo();
            steps.Add(step);
        }

        setActiveStep(0);
    }

    public void Awake()
    {
        room.OnPropagateRoomEvent += Room_OnPropagateRoomEvent;
    }

    public void triggerActiveStep(int step)
    {
        if (room != null)
        {
            room.RunStepChangeCommand(step);
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
        print("Setting active step to: " + steps[index].name);
        foreach(ObjectStepInfo obj in step.objects)
        {
            GameObject go = GameObject.Find(obj.name);
            if (go != null)
            {
               // print("Setting " + obj.name + " to visible:" + obj.visible);
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
                    LerpPart(go.transform, go.transform.localPosition, obj.position);
                   /// print("Lerp me right round baby, right round");
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
    }

    Vector3 parseVector(JSONArray values)
    {
        Vector3 result = new Vector3();
        result.x = values[0].AsFloat;
        result.y = values[1].AsFloat;
        result.z = values[2].AsFloat;
        return result;
    }

    void LerpPart(Transform tTrans, Vector3 start, Vector3 end)
    {
        if(doLerp == true)
        {
            StartCoroutine(LerpMachine(tTrans, start, end, lerpTime));
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
