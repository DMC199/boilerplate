using UnityEngine;
using System.Collections;
using HoloToolkit.Unity;

public class CursorHighlight : MonoBehaviour {

    public GameObject FocusedGameObject { get; private set; }
    public GameObject oldFocusedGameObject = null;

	// Use this for initialization
	void Start () {
        FocusedGameObject = null;
	}
	
	// Update is called once per frame
	void Update () {
        oldFocusedGameObject = FocusedGameObject;

        if (GazeManager.Instance && GazeManager.Instance.Hit)
        {
            RaycastHit hitInfo = GazeManager.Instance.HitInfo;
            if (hitInfo.collider != null)
            {
                // Assign the hitInfo's collider gameObject to the FocusedGameObject.
                FocusedGameObject = hitInfo.collider.gameObject;
            }
            else
            {
                FocusedGameObject = null;
            }
        }
        else
        {
            FocusedGameObject = null;
        }

        if (FocusedGameObject != oldFocusedGameObject)
        {
            ResetFocusedInteractible();

            if (FocusedGameObject != null)
            {
                if (FocusedGameObject.GetComponent<Interactible>() != null)
                {
                    // Send a GazeEntered message to the FocusedGameObject.
                    FocusedGameObject.SendMessage("GazeEntered");
                }
            }
        }
    }

    private void ResetFocusedInteractible()
    {
        if (oldFocusedGameObject != null)
        {
            if (oldFocusedGameObject.GetComponent<Interactible>() != null)
            {
                // Send a GazeExited message to the oldFocusedGameObject.
                oldFocusedGameObject.SendMessage("GazeExited");
            }
        }
    }
}
