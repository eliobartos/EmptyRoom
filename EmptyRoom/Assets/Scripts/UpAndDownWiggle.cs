using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpAndDownWiggle : MonoBehaviour
{
    public float wiggleDistance = 1;
    public float wiggleSpeed = 1;

    float initPosX;
    float initPosY;
    float initPosZ;

    void Start() {
        initPosX = transform.position.x;
        initPosY = transform.position.y;
        initPosZ = transform.position.z;
    }

    void Update()
    {
        float yPosition = Mathf.Sin(Time.time * wiggleSpeed) * wiggleDistance;
        transform.localPosition = new Vector3(initPosX, initPosY + yPosition, initPosZ);
    }
}
