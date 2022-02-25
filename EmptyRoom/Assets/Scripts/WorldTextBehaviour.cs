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
        textArray = new string[] { "Fear Me", "Pain is here", "T is is real", "Alone", "Sca y", "domi ation obsessed w th?? my fever", "happy thoughts"  };
    }

    
}
