using UnityEngine;
using System.Collections;

public class AnimationTest : MonoBehaviour {

    Animation anim;
    string[] clipNames;
    int lastClipIndex = 0;
    public int clipIndex = 0;
	// Use this for initialization
	void Start () {
        anim = GetComponent<Animation>();
        int i = 0;
        clipNames = new string[anim.GetClipCount()];
        foreach (AnimationState clip in anim)
        {
            print(clip.name);
            clip.wrapMode = WrapMode.Loop;
            clipNames[i++] = clip.name;
        }

        anim.Play(clipNames[0]);
	}

	
	// Update is called once per frame
	void Update () {
        if (clipIndex != lastClipIndex)
        {
            anim.Play(clipNames[clipIndex]);
            lastClipIndex = clipIndex;
        }
	}
}
