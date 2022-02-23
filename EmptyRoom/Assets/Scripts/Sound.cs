using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public float volume;
    public bool loop;

    public AudioClip clip; // actuall audio file/clip
    [HideInInspector]
    public AudioSource source; // component
    
    
}
