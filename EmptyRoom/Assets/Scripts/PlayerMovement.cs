using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float sanityPctForPlayerSpriteChange = 0.2f;
    public float movementSpeed = 0.01f;
    Animator animator;

    public void Start() {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector2 moveVector = new Vector2(inputX, inputY);
        moveVector = moveVector.normalized;

        Vector3 pos = this.transform.position;
        pos.x = pos.x + moveVector.x * movementSpeed * Time.deltaTime;
        pos.y = pos.y + moveVector.y * movementSpeed * Time.deltaTime;

        this.transform.position = pos;

        HandleAnimations(inputX, inputY);
    }

    private void HandleAnimations(float inputX, float inputY) {

        if(inputX == 0 & inputY == 0) {
            animator.speed = 0;
        } else {
            animator.speed = 1;
        }

        if(GameManager.instance.ballsCollected == 0) {
            if(inputX < 0) {
                animator.Play("Left", 0);
            } else if(inputX > 0) {
                animator.Play("Right", 0);
            } else if(inputY > 0) {
                animator.Play("Up");
            } else if(inputY < 0) {
                animator.Play("Down");
            }

        } else if(GameManager.instance.ballsCollected > 0 & GameManager.instance.currentSanity/GameManager.instance.maxSanity > sanityPctForPlayerSpriteChange) {

            if(inputX < 0) {
                animator.Play("Left-Lantern", 0);
            } else if(inputX > 0) {
                animator.Play("Right-Lantern", 0);
            } else if(inputY > 0) {
                animator.Play("Up-Lantern");
            } else if(inputY < 0) {
                animator.Play("Down-Lantern");
            }
        } else {

            if(inputX < 0) {
                animator.Play("Left-Red", 0);
            } else if(inputX > 0) {
                animator.Play("Right-Red", 0);
            } else if(inputY > 0) {
                animator.Play("Up-Red");
            } else if(inputY < 0) {
                animator.Play("Down-Red");
            }
        }
        
    }

    
}
