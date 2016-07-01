using UnityEngine;
using System.Collections;
using UnityEngine.Windows.Speech;
using System;
using UnityEngine.SceneManagement;

public class GrammarManager : MonoBehaviour {

    GrammarRecognizer grammar;
    public GearMovement gearMover;

    // Use this for initialization
    void Start () {

        String sceneName = SceneManager.GetActiveScene().name;
        String grammarFile = Application.streamingAssetsPath + "/" + sceneName + "_grammar.xml";

        if (System.IO.File.Exists(grammarFile))
        {
            PhraseRecognitionSystem.Shutdown();

            print("Attempting to load grammar file: " + grammarFile);

            grammar = new GrammarRecognizer(grammarFile, ConfidenceLevel.Low);
            grammar.OnPhraseRecognized += Grammar_onPhraseRecognized;

            grammar.Start();
        } else
        {
            print("No grammar XML file found for scene: " + sceneName);
        }
	}

    private void Grammar_onPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        // print("Recognized:" + args.text);
 
        if ((args.semanticMeanings != null) && (args.semanticMeanings.Length > 0)) {

            // print("Semantic values:" + args.semanticMeanings.Length);

            String gear = "";
            String action = "";

            for (int i = 0; i < args.semanticMeanings.Length; i++)
            {
                if (args.semanticMeanings[i].key == "Action")
                {
                    action = args.semanticMeanings[i].values[0];
                } else if (args.semanticMeanings[i].key == "Gear")
                {
                    gear = args.semanticMeanings[i].values[0];
                }
            }

            bool lockState = false;
            if(action == "lock")
            {
                lockState = true;
            }

            //print("Gear=" + gear + ", locked=" + lockState);

            if (gear == "ring")
            {
                gearMover.ringLocked = lockState;
               
            }

            if(gear == "planet")
            {
                gearMover.planetsLocked = lockState;
            }
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
