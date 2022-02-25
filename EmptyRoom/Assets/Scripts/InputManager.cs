using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        // Close the app, does not work from editor.
        if (Input.GetKeyDown(KeyCode.Escape)) {
        Application.Quit();
    }
    }
}
