﻿using UnityEngine;
using System.Collections;

public class GearMovement : MonoBehaviour {

    public Transform sunGear;
    public Transform ringGear;

    public Transform planetGear;

    public Transform[] planets;

    private MeshRenderer sunRenderer;
    private MeshRenderer ringRenderer;
    private MeshRenderer[] planetRenderers;

    public Material lockedMaterial;
    public Material unlockedMaterial;

    public bool planetsLocked = false;
    public bool ringLocked = false;


    // Use this for initialization
    void Start() {
        print(planets.Length);

        sunRenderer = sunGear.GetComponent<MeshRenderer>();
        ringRenderer = ringGear.GetComponent<MeshRenderer>();

        planetRenderers = new MeshRenderer[planets.Length];

        for (int i = 0; i < planets.Length; i++)
        {
            planetRenderers[i] = planets[i].GetComponent<MeshRenderer>();
        }
    }


	// Update is called once per frame
	void Update () {

        float amount = 0.25f;
        
        ringRenderer.material = ringLocked ? lockedMaterial : unlockedMaterial;
        for (int i = 0; i < planets.Length; i++)
        {
            planetRenderers[i].material = planetsLocked ? lockedMaterial : unlockedMaterial;
        }

        if (planetsLocked)
        {
            sunGear.rotation *= Quaternion.AngleAxis(amount, new Vector3(0, 0, 1));
            planetGear.rotation *= Quaternion.AngleAxis(amount, new Vector3(0, 1, 0));
            ringGear.rotation *= Quaternion.AngleAxis(amount, new Vector3(0, 0, 1));
        }
        else
        {
            if (!ringLocked)
            {
                sunGear.rotation *= Quaternion.AngleAxis(amount, new Vector3(0, 0, 1));
                ringGear.rotation *= Quaternion.AngleAxis(-amount * 0.3333f, new Vector3(0, 0, 1));
                foreach (Transform p in planets)
                {
                    // cancel out planet rotation
                    p.rotation *= Quaternion.AngleAxis(-amount, new Vector3(0, 0, 1));
                    // p.rotation *= Quaternion.AngleAxis(-amount, new Vector3(0, 0, 1));
                }
            }
            else
            {
                sunGear.rotation *= Quaternion.AngleAxis(amount, new Vector3(0, 0, 1));
                planetGear.rotation *= Quaternion.AngleAxis(amount * 0.3333f, new Vector3(0, 1, 0));
                foreach (Transform p in planets)
                {
                    // cancel out planet rotation
                    p.rotation *= Quaternion.AngleAxis(-amount, new Vector3(0, 0, 1));
                    // p.rotation *= Quaternion.AngleAxis(-amount, new Vector3(0, 0, 1));
                }
            }
        }
       //ringGear.rotation *= Quaternion.AngleAxis(amount, new Vector3(0, 0, 1));
    }
}
