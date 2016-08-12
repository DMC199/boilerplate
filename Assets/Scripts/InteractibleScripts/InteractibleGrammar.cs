using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class InteractibleGrammar : MonoBehaviour {

    public GrammarManager gm;

    void GazeEntered()
    {
        gm.setGrammar("global_nav");
    }

    void GazeExited()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        gm.setGrammar(sceneName + "_grammar");
    }
}
