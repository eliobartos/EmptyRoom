using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldTextBehaviour : MonoBehaviour
{

    string[] textArray;
    [SerializeField] private Text textUI;

    // Start is called before the first frame update
    void Start()
    {
        SetUpArray();

        int index = Random.Range(0, textArray.Length);
        textUI.text = textArray[index];
        textUI.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-40, 40));

        var newColor = textUI.color;
        newColor.a = Random.Range(0.3f, 0.9f);
        textUI.color = newColor;
    }

    void SetUpArray() {
        textArray = new string[] { 
            "happy thoughts happy thoughts happy!!!!",
            "the music art with a a a lot of a planet a lot",
            "ask about loneliness, the emotions relate but how",
            "keep moving remember",
            "Don't let them get you!",
            "you hear you hear? you hear",
            "Farwell...",
            "ever nevermore",
            "I've been here before you",
            "HELP",
            "all LIES",
            "voice IS LYING",
            "shiny shiny shiny",
            "trust the dont trust the arrows",
            "where is the rabbit",
            "stop STOP?!",
            "They are behind you",
            "NEVER close your eyes",
            "with a little?? bit more dreaaaam",
            "must collect shinies",
            "never collect shinies",
            "NO ONE SEES",
            "they KNOW when you CRY",
            "PAIN GOES NOT AWAY",
            "you read VERY WELL",
            "please make it stop",
            "RUNRUNRNUNRN",
            "Do not go gentle into that good night"
        };

    }

    
}
