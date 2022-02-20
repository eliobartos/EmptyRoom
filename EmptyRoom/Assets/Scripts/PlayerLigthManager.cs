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

    // Start is called before the first frame update
    void Start()
    {
        playerLight = GetComponentInChildren<Light2D>();
    }

    public void Update() {
        SetRadius(targetRadius);
    }

    public void SetTargetRadius(float radius) {
        targetRadius = radius;
    }
    
    void SetRadius(float targetRadius) {
        float radius = Mathf.Lerp(playerLight.pointLightOuterRadius, targetRadius, radiusLerp);

        playerLight.pointLightOuterRadius = radius;
        playerLight.pointLightInnerRadius = radius * innerRadiusPercentage;
    }
}
