using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BallBehaviour : MonoBehaviour
{
    
    private void OnTriggerEnter2D(Collider2D other) {

        if(other.gameObject.tag == "Player") {

            GameManager.instance.balls.Remove(this.gameObject);
            Destroy(this.gameObject);
            GameManager.instance.BallCollected();

            AudioManager.instance.ForcePlay("PickUp");

            GameManager.instance.increaseSanity(GameManager.instance.ballIncreaseSanityAmount);
        }

        
    }

    
}


