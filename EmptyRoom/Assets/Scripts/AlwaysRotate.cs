using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysRotate : MonoBehaviour
{
    public float rotationSpeed;

    public void SetRotationSpeed(float newSpeed) {
        rotationSpeed = newSpeed;
    }

    void Update() {
        transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
    }
     
}
