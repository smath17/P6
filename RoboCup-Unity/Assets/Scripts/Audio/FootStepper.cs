using System;
using UnityEngine;

public class FootStepper : MonoBehaviour
{
    AudioSource source;
    AudioEvent footstep;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        footstep = Resources.Load<AudioEvent>("AudioEvents/footstep_default");
    }
    
    public void Footstep()
    {
        footstep.Play(source);
    }
}