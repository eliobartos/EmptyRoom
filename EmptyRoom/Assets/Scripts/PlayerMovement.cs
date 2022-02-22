using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float movementSpeed = 0.01f;

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
    }

    
}
