using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{

    [SerializeField] private GameObject target;
    [SerializeField] private float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 targetPos = target.transform.position;
        targetPos.z = -10.0f;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}
