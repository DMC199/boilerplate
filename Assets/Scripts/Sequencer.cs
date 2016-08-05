using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class Sequencer : MonoBehaviour {


    /*public int CurrentStep {
        get
        {
            return _currentStep;
        }

        set
        {
            print("CHANGE IS COMING!");
            _currentStep = value;
            setActiveStep(_currentStep);
        }
      
    }*/

    //UnityEditor
    public int currentStep;
    public string jsonFileName;

    private int lastCurrentStep;

    JSONNode root;
    List<StepInfo> steps;

    public class StepInfo
    {
        public List<ObjectStepInfo> objects;
    }

    public class ObjectStepInfo
    {
        public string name;
        public bool visible;
        public Vector3 position;
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
            JSONArray objectsInStep = jsonSteps[i].AsArray;
            StepInfo step = new StepInfo();
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


                if (infoNode["position"] != null)
                {
                    objInfo.position = parseVector(infoNode["position"].AsArray);
                } else
                {
                    objInfo.position = new Vector3();
                }
                step.objects.Add(objInfo);
            }

            //StepInfo info = new StepInfo();
            steps.Add(step);
        }

        print("I'm awake! Steps: " + steps.Count);
        setActiveStep(0);
    }

    public void Awake()
    {
       
    }

    public void setActiveStep(int index)
    {
        StepInfo step = steps[index];
        
        foreach(ObjectStepInfo obj in step.objects)
        {
            GameObject go = GameObject.Find(obj.name);
            if(go != null)
            {
                print("Setting " + obj.name + " to visible:" + obj.visible);
                go.GetComponent<Renderer>().enabled = obj.visible;
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
	
	// Update is called once per frame
	void Update () {
	    if(currentStep != lastCurrentStep)
        {
            setActiveStep(currentStep);
            lastCurrentStep = currentStep;
        }
	}
}
