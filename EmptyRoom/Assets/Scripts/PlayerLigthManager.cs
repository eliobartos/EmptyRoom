using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerLigthManager : MonoBehaviour
{

    // Reference to light
    Light2D playerLight;

    // Variables
    [SerializeField] private float innerRadiusPercentage = 0.2f;
    [SerializeField] private float targetRadius = 50.0f;
    [SerializeField] private float radiusLerp = 0.2f;

    public float[] lightRadiusLevels = {50.0f, 8.0f, 5.0f, 4.0f, 3.2f, 2.5f, 2.1f, 1.8f, 1.5f};

    // Start is called before the first frame update
    void Start()
    {
        playerLight = GetComponentInChildren<Light2D>();
    }

    public void Update() {
        SetRadius(targetRadius);
    }

    public void SetTargetRadius(int level) {
        targetRadius = lightRadiusLevels[level];
    }
    
    void SetRadius(float targetRadius) {
        float radius = Mathf.Lerp(playerLight.pointLightOuterRadius, targetRadius, radiusLerp);

        playerLight.pointLightOuterRadius = radius;
        playerLight.pointLightInnerRadius = radius * innerRadiusPercentage;
    }
}
