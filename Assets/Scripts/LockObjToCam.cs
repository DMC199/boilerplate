using UnityEngine;
using System.Collections;

public class LockObjToCam : MonoBehaviour {
    [Tooltip("Move Dapening threshold")]
    [SerializeField]
    private float m_ObjectMoveDampening = 0.5f;

    [Tooltip("Defaults to Object this is attached to")]
    [SerializeField]
    private Transform m_Object;

    [Tooltip("Defaults to MainCam")]
    [SerializeField]
    private Transform m_Camera;

    private const float k_ExpDampingCoef = -20f;

	// Use this for initialization
	void Awake () {
        if (!m_Object)
            m_Object = transform;
        if (!m_Camera)
            m_Camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>().transform;
	}

    void Start()
    {
        StartCoroutine(MoveObj());
    }

    public IEnumerator MoveObj()
    {
        while (true)
        {
            Quaternion rotation = m_Camera.localRotation;
            float TargetDistance = Vector3.Distance(m_Object.position, m_Camera.position);
            Vector3 MoveTo = m_Camera.position + (rotation * Vector3.forward) * TargetDistance;

            m_Object.position = Vector3.Lerp(m_Object.position, MoveTo, m_ObjectMoveDampening * (1f - Mathf.Exp(k_ExpDampingCoef * Time.deltaTime)));

            yield return null;
        }
    }
}
