using UnityEngine;
using System.Collections;
using UnityEngine.Windows.Speech;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GrammarManager : MonoBehaviour {

    GrammarRecognizer grammar;

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
            grammarFileToLoad = Application.streamingAssetsPath + "/" + grammarFile;
        } else {
            grammarFileToLoad = Application.streamingAssetsPath + "/" + sceneName + "_grammar.xml";
        }

       if (System.IO.File.Exists(grammarFileToLoad))
        {
            PhraseRecognitionSystem.Shutdown();

            print("Attempting to load grammar file: " + grammarFile);

            grammar = new GrammarRecognizer(grammarFile, ConfidenceLevel.Low);
            grammar.OnPhraseRecognized += Grammar_onPhraseRecognized;

            grammar.Start();
        } else
        {
            print("No grammar XML file loaded.");
        }
	}

    private void Grammar_onPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        grammarRecognized.Invoke(args);
    }
}
