using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections;
using System.Threading.Tasks;

public class AudioManager : NonPersistentSingleton<AudioManager>
{
    public Sound[] sounds;

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

    public void PlayOneOf(string[] names) {

        // Check if any voiceover is playing, return (do nothing)
        foreach(string name in names) {
            Sound s = Array.Find(sounds, sound => sound.name == name);

            if(s.source.isPlaying == true) {
                return;
            }
        }

        // Else choose one randomly and start playing it
        int chosenSoundIndex = UnityEngine.Random.Range(0, names.Length);
        Play(names[chosenSoundIndex]);


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
 }

