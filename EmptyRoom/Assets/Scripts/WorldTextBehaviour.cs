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
    }

    void SetUpArray() {
        textArray = new string[] { 
            "Ar  you sc red?",
            "Pain", 
            "T is is real", 
            "Alone", 
            "domi ation obsessed with?? my fever", 
            "happy thoughts",
            "up or down",
            "come here",
            "this way... maybe",
            "just let it go..",
            "feel",
            "stop, stop",
            "noo ooo!",
            ":D"
        };
    }

    
}
