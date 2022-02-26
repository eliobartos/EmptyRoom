using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleManager : MonoBehaviour
{
    Dictionary<int, string[]> subtitlesDict;
    public Text subtitleUI;
    public float subtitleDuration = 3.0f;

    Coroutine currentCoroutine = null;

    void Awake() {
        SetUpDict();
    }

    public void DisplaySubtitle(int level, float delay = 0.0f) {

        int index = Random.Range(0, subtitlesDict[level].Length);
        var subtitleText = subtitlesDict[level][index];

        subtitleUI.text = subtitleText;
        
        if(currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(DisplayForXSeconds(subtitleUI.gameObject, subtitleDuration, delay));

    }

    IEnumerator DisplayForXSeconds(GameObject obj, float duration, float delay) {
        
        yield return new WaitForSeconds(delay);

        // Activate subtitles and play voiceover
        AudioManager.instance.PlayOneOf(GameManager.instance.voiceOverList);
        obj.SetActive(true);
        
        yield return new WaitForSeconds(duration);
        
        // Deactive subtitles after duration
        obj.SetActive(false);
    }

    void SetUpDict() {
        // Assing subtitles here
        subtitlesDict = new Dictionary<int, string[]>();

        subtitlesDict[0] = new string[] { 
            "Remember: The room is empty. The room is empty. There is nothing in here.", 
            "Glad to see you here again. Don't forget, there's nothing here but you!", 
            "This room has always been empty. Nothing can persist here." };

        subtitlesDict[1] = new string[] { 
            "Whatever you think you're seeing around you is just an illusion.", 
            "This will change nothing. The room is empty, and it will remain so.", 
            "Nothing is in here to get you. You are perfectly safe." };

        subtitlesDict[2] = new string[] { 
            "Why are you walking in a strange pattern? There are no walls in here.", 
            "Just relax here. Nothing can hurt you here.", 
            "These whispers aren't real. They have never existed." };

        subtitlesDict[3] = new string[] { 
            "You shouldn't trust your eyes, trust me instead.", 
            "Remember - the room is empty.", 
            "No one is talking about you. No one is laughing at you." };

        subtitlesDict[4] = new string[] { 
            "What are you hoping to achieve?", 
            "It's pointless anyway.", 
            "Nothing is trying to hurt you here." };

        subtitlesDict[5] = new string[] { 
            "Honestly, you should just let go.", 
            "This won't bring anyone back. This won't bring you back.", 
            "You think I don't know what's on your mind?" };

        subtitlesDict[6] = new string[] { 
            "It's all in your head.", 
            "Nothing is seen - everything has been seen.", 
            "Hear that sound? You do, right? I know you do." };

        subtitlesDict[7] = new string[] { 
            "No one is following you. You know that, right?", 
            "What a remarkable empty room this is.", 
            "You can leave any time you wish. You know the way out." };

        subtitlesDict[8] = new string[] { 
            "This will change nothing!", 
            "Stop! Or you'll end up hurting yourself!", 
            "Just give up already! You won't get rid of me!" };

        subtitlesDict[9] = new string[] { 
            "None of this was real anyway. We knew that from the start.", 
            "Unreal experience. We should do it again.", 
            "You can't escape me. I am you after all." };

        // When you loose
        subtitlesDict[10] = new string[] { 
            "This is what happens when you try to escape.", 
            "Guess you aren't good enough.", 
            "It's not the walls that are keeping you here. They aren't real anyway."};
    }
}