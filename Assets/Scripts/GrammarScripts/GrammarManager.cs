using UnityEngine;
using System.Collections;
using UnityEngine.Windows.Speech;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GrammarManager : MonoBehaviour {
    
    Dictionary<String, GrammarRecognizer> grammars = new Dictionary<String, GrammarRecognizer>();

    [System.Serializable]
    public class GrammarEvent : UnityEvent<PhraseRecognizedEventArgs> { }

    [Tooltip("Optional, specify a grammar XML file")]
    public string grammarFile = "";

    public GrammarEvent grammarRecognized;

    // Use this for initialization
    void Start() {

        String sceneName = SceneManager.GetActiveScene().name;
        String grammarFileToLoad = "";
        
        if (grammarFile != "") {
            grammarFileToLoad = grammarFile;
        } else {
            grammarFileToLoad = sceneName + "_grammar";
        }

        loadGrammar("global_nav");

        if (loadGrammar(grammarFileToLoad))
        {
            setGrammar(grammarFileToLoad);
        }
    }

    public void setGrammar(String name)
    {
        if (grammars.ContainsKey(name))
        {
            PhraseRecognitionSystem.Shutdown();
            GrammarRecognizer gm;

            if (grammars.TryGetValue(name, out gm))
            {
                print("Activating grammar: " + name);
                gm.Start();
            } else
            {
                print("Unable to find grammar: " + name);
            }
        }
    }

    Boolean loadGrammar(String name)
    {
        String path = Application.streamingAssetsPath + "/" + name + ".xml";
        if (System.IO.File.Exists(path))
        { 
            GrammarRecognizer gm = new GrammarRecognizer(path, ConfidenceLevel.Low);
            gm.OnPhraseRecognized += Grammar_onPhraseRecognized;

            grammars.Add(name, gm);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Grammar_onPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        grammarRecognized.Invoke(args);
    }
}
