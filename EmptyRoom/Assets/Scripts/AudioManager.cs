using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections;
using System.Threading.Tasks;

public class AudioManager : NonPersistentSingleton<AudioManager>
{
    public Sound[] sounds;
    private int[] randomOrderIndex = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    private int currentIndex = 0;

    protected override void Awake() {
        base.Awake();
        
        foreach(Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
    }

    void Start() {
        Play("Theme1");
        Play("Theme2");
        SetVolume("Theme2", 0.0f);
        Play("Theme3");
        SetVolume("Theme3", 0.0f);

        // Create Random Order
        AudioManager.Shuffle(randomOrderIndex);
       
    }

    public void Play(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s.source.isPlaying == false) {
            s.source.Play();
        }
    }

    public void ForcePlay(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

    public void Stop(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if(s.source.isPlaying == true) {
            s.source.Stop();
        }  
    }

    public void StopWithFadeout(string name, float fadeOutDuration) {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if(s.source.isPlaying == true) {
            StartCoroutine(FadeOut(s, fadeOutDuration));
        }
    }

    public void StartWithFadeIn(string name, float fadeInDuration, float delay = 0.0f) {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if(s.source.isPlaying == false) {
            s.source.volume = 0;
            s.source.Play();
            StartCoroutine(FadeIn(s, fadeInDuration, delay));
        }

    }

    public void SetVolume(string name, float pct) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.volume = pct * s.volume;
    } 

    public void PlayOneOf(string[] names) {

        // Check if any voiceover is playing, return (do nothing)
        foreach(string name in names) {
            Sound s = Array.Find(sounds, sound => sound.name == name);

            if(s.source.isPlaying == true) {
                return;
            }
        }

        // Else choose one randomly and start playing it
        Play(names[randomOrderIndex[currentIndex]]);
        currentIndex++;


    }

    public void StopAllWithFadeout(string[] names, float duration) {
        foreach(string name in names) {
            Sound s = Array.Find(sounds, sound => sound.name == name);

            if(s.source.isPlaying == true) {
                StopWithFadeout(name, duration);
            }
        }
    }

    // Helper function for fading out
    IEnumerator FadeOut(Sound s, float duration) {
        float currentTime = 0;
        float start = s.source.volume;

        while(currentTime < duration) {
            currentTime += Time.deltaTime;
            s.source.volume = Mathf.Lerp(start, 0.0f, currentTime / duration);
            yield return null;
        }

        s.source.Stop();
        yield break;
    }

    // Helper function for fading in
    IEnumerator FadeIn(Sound s, float duration, float delay) {

        // Wait for delay time
        yield return new WaitForSeconds(delay);

        // Start Fade In
        float currentTime = 0;

        while(currentTime < duration) {
            currentTime += Time.deltaTime;
            s.source.volume = Mathf.Lerp(0.0f, s.volume, currentTime / duration);
            yield return null;
        }
        yield break;
    }


    public static void Shuffle<T> (T[] array)
    {
        var rng = new System.Random();
        int n = array.Length;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
 }

