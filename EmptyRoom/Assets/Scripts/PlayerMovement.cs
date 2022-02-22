using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float sanityPctForPlayerSpriteChange = 0.2f;
    public float movementSpeed = 0.01f;
    public bool canMove = false;
    Animator animator;

    int levelWidth;
    int levelHeight;

    float inputX;
    float inputY;

    public void Start() {
        animator = GetComponent<Animator>();

        levelWidth = GameManager.instance.levelWidth;
        levelHeight = GameManager.instance.levelHeigth;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(canMove) {
            inputX = Input.GetAxisRaw("Horizontal");
            inputY = Input.GetAxisRaw("Vertical");
        } else {
            inputX = 0.0f;
            inputY = 0.0f;
        }
        

        MoveThePlayer(inputX, inputY);
        HandleAnimations(inputX, inputY);
    }

    private void MoveThePlayer(float inputX, float inputY) {

        // Get and normalize input
        Vector2 moveVector = new Vector2(inputX, inputY);
        moveVector = moveVector.normalized;

        // Calculate new position
        Vector3 pos = this.transform.position;
        pos.x = pos.x + moveVector.x * movementSpeed * Time.deltaTime;
        pos.y = pos.y + moveVector.y * movementSpeed * Time.deltaTime;

        // Check if outside of world
        if(pos.x < 0) {
            pos.x = 0;
        }

        if(pos.x > levelWidth-1) {
            pos.x = levelWidth - 1;
        }

        if(pos.y < 0) {
            pos.y = 0;
        }

        if(pos.y > levelHeight-1) {
            pos.y = levelHeight - 1;
        }

        // Apply movement
        this.transform.position = pos;
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
