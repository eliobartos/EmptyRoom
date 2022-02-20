using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    private Image filling;
    private float max = 1.0f;
    private float current = 1.0f;

    public void Start() {
        filling = this.GetComponent<Image>();
    }

    public void UpdateBar(float currentFill) {
        current = currentFill;
        filling.fillAmount = current / max;
    }

    public void SetUp(float maxFill) {
        max = maxFill;
        current = maxFill;
    }
}