using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BallBehaviour : MonoBehaviour
{
    
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "Player") {
            Destroy(this.gameObject);
        }

        GameManager.instance.BallCollected();
    }

    
}


