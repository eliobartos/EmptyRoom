using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    private Image filling;
    [SerializeField] private Image backgroundFill;
    private float max = 1.0f;
    private float current = 1.0f;
    private bool changeBackgroundFill = true;

    public void Start() {
        filling = this.GetComponent<Image>();
    }

    public void UpdateBar(float currentFill) {
        current = currentFill;
        filling.fillAmount = current / max;

        if(changeBackgroundFill) {
            backgroundFill.fillAmount = current / max;
        }
    }

    public void UpdateBarWithEffect(float currentFill) {
        changeBackgroundFill = false;
        UpdateBar(currentFill);
        StartCoroutine(FadeEffect(2.0f));
        
    }

    public void SetUp(float maxFill) {
        max = maxFill;
        current = maxFill;
    }

    IEnumerator FadeEffect(float delay) {
        
        yield return new WaitForSeconds(delay);
        
        while(backgroundFill.color.a > 0.0f) {
            var tmpColor = backgroundFill.color;
            tmpColor.a -= 0.02f;
            yield return new WaitForSeconds(0.02f);
            backgroundFill.color = tmpColor;
        }

        backgroundFill.fillAmount = filling.fillAmount;

        var tmpColor2 = backgroundFill.color;
        tmpColor2.a = 1.0f;
        backgroundFill.color = tmpColor2;
        changeBackgroundFill = true;
    }
}