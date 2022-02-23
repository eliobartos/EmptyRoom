using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuBehaviour : MonoBehaviour
{

    [SerializeField] private Image titleImage;
    [SerializeField] private Text playText;

    bool canClick = false;

    // Represents the scene background loading. This is used to control
    // when the scene should switch over.
    AsyncOperation sceneLoadingOperation;

    // Start is called before the first frame update
    void Start()
    {
        // Begin loading the scene in the background
        sceneLoadingOperation = SceneManager.LoadSceneAsync("GameScene");

        // ... but don't actually switch to the new scene until we're ready
        sceneLoadingOperation.allowSceneActivation = false;

        // Start Animations
        StartCoroutine(FadeInAlphaImage(titleImage, 3.0f, 0.5f));
    }

    // Update is called once per frame
    void Update()
    {
        if(canClick) {
            if(Input.anyKey) {
                LoadLevelScene();
            }
        }
    }


    IEnumerator FadeInAlphaImage(Image img, float duration, float delay = 0.0f) {

        // Wait for delay time
        yield return new WaitForSeconds(delay);

        // Start Fade In
        float currentTime = 0;

        while(currentTime < duration) {
            currentTime += Time.deltaTime;
            var tempColor = img.color;
            tempColor.a = Mathf.Lerp(0.0f, 1.0f, currentTime / duration);
            img.color = tempColor;
            yield return null;
        }

        StartCoroutine(FadeInAlphaText(playText, 3.0f, 1.0f));
    }

    IEnumerator FadeInAlphaText(Text txt, float duration, float delay = 0.0f) {

        // Wait for delay time
        yield return new WaitForSeconds(delay);

        // Make clickable
        canClick = true;
        // Start Fade In
        float currentTime = 0;

        while(currentTime < duration) {
            currentTime += Time.deltaTime;
            var tempColor = txt.color;
            tempColor.a = Mathf.Lerp(0.0f, 1.0f, currentTime / duration);
            txt.color = tempColor;
            yield return null;
        }
    }

    void LoadLevelScene() {
       // Tell the scene loading operation to switch scenes when it's done loading
        sceneLoadingOperation.allowSceneActivation = true;
    }
}
