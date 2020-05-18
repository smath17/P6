using System;
using Unity.Mathematics;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    public static void EmitSound(AudioEvent sound, Vector3 position)
    {
        GameObject soundObj = new GameObject();
        soundObj.transform.position = position;
        AudioSource source = soundObj.AddComponent<AudioSource>();
        sound.Play(source);
        Destroy(soundObj, source.clip.length);
    }
    
    public static void EmitSound(AudioEvent sound)
    {
        EmitSound(sound, Vector3.zero);
    }
}