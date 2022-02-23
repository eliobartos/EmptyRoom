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
            "Glad to see you here again. Remember, there is nothing in this room except you.", 
            "Nothing exists here. Nothing can exist here. The room has always been empty." };

        subtitlesDict[1] = new string[] { 
            "I know you might think you're seeing things around you, but it's all just an illusion.", 
            "This will change nothing. The room is empty, and it will remain so.", 
            "Nothing is in here to get you. You are perfectly safe." };

        subtitlesDict[2] = new string[] { 
            "Why are you walking in a strange pattern? There are no walls in here.", 
            "Just relax here. Nothing can hurt you here.", 
            "These whispers aren't real. They have never existed." };

        subtitlesDict[3] = new string[] { 
            "You should trust your mind, not your eyes.", 
            "Remember - the room is empty.", 
            "No one is talking about you. No one is laughing at you." };

        subtitlesDict[4] = new string[] { 
            "What are you hoping to achieve?", 
            "It's useless anyway.", 
            "Nothing is trying to hurt you here." };

        subtitlesDict[5] = new string[] { 
            "Honestly, you should just let go.", 
            "This won't bring anything or anyone back. This won't bring you back.", 
            "Don't you think they all know what's on your mind?" };

        subtitlesDict[6] = new string[] { 
            "It's all in your head anyway.", 
            "Nothing is seen - everything has been seen.", 
            "Hear that sound? You do, right? I know you do." };

        subtitlesDict[7] = new string[] { 
            "No one is following you, and you know that.", 
            "What a remarkable empty room this is.", 
            "You can leave any time you wish. You know the way out." };

        subtitlesDict[8] = new string[] { 
            "You know this will change nothing.", 
            "You will only end up hurting yourself more.", 
            "It will be easier if you just close your eyes and give up." };

        subtitlesDict[9] = new string[] { 
            "Until we meet again.", 
            "I was never keeping you in here anyway.", 
            "You will be back. We all do." };

        // When you loose
        subtitlesDict[10] = new string[] { 
            "You Lost 1", 
            "You Lost 2", 
            "You Lost 3"};
    }
}
