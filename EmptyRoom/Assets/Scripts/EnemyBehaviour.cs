using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{

    List<Vector3> movementPoints;
    Vector3 currentPoint;
    int currentIndex;
    bool facingRigth = true;
    public float sanityReduction = 10.0f;
    Coroutine enableMovementCo;

    public float enemySpeed = 6.0f;

    public void Init(int nPoints, float minDistanceBetweenPoints) {

        movementPoints = new List<Vector3>();
        movementPoints.Add(transform.position);
        
        for (int i = 1; i < nPoints; i++) { // We already have starting point
            
            while(true) {
                int x = Random.Range(0, GameManager.instance.levelWidth);
                int y = Random.Range(0, GameManager.instance.levelHeigth);

                Vector3 potentialPoint = new Vector3(x, y, 0);

                bool pointIsGood = true;
                for (int j = 0; j < movementPoints.Count; j++) {
                    if(Vector3.Distance(potentialPoint, movementPoints[j]) <= minDistanceBetweenPoints) {
                        pointIsGood = false;
                    }
                }

                if(pointIsGood) {
                    movementPoints.Add(potentialPoint);
                    break;
                }
            }
        }

        currentIndex = 1;
        currentPoint = movementPoints[currentIndex];
        
      
    }

    public void Update() {
        Move();
        FlipIfNeeded();
    }

    public void Move() {
        transform.position = Vector3.MoveTowards(transform.position, currentPoint, enemySpeed * Time.deltaTime);

        if(Vector3.Distance(transform.position, currentPoint) < 0.001f) {
            
            currentIndex = (currentIndex + 1) % movementPoints.Count;
            currentPoint = movementPoints[currentIndex];
        }
    }

    public void FlipIfNeeded() {

        if(currentPoint.x > transform.position.x && facingRigth == false) {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
            facingRigth = true;
        } else if(currentPoint.x < transform.position.x && facingRigth == true) {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
            facingRigth = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {

        if(other.gameObject.tag == "Player") {

            var playerMovement = other.gameObject.GetComponent<PlayerMovement>();
            playerMovement.canMove = false;

            AudioManager.instance.Play("EnemyHit");

            var cameraShake = Camera.main.GetComponent<CameraShake>();
            if(cameraShake.enabled == true) {
                cameraShake.enabled = false;
                if(enableMovementCo != null) {
                    StopCoroutine(enableMovementCo);
                }
                
                cameraShake.enabled = true;
                enableMovementCo = StartCoroutine(EnableMovement(playerMovement, 2.0f));
            } {
                cameraShake.enabled = true;
                enableMovementCo = StartCoroutine(EnableMovement(playerMovement, 2.0f));
            }

            GameManager.instance.ReduceSanity(sanityReduction);

        }

        
    }

    IEnumerator EnableMovement(PlayerMovement playerMovement, float delay) {
        yield return new WaitForSeconds(delay);
        playerMovement.canMove = true;
        Camera.main.GetComponent<CameraShake>().enabled = false;
    }

    IEnumerator FadeOutForcePlay() {
        AudioManager.instance.StopWithFadeout("EnemyHit", 0.1f);
        yield return new WaitForSeconds(0.1f);
        AudioManager.instance.Play("EnemyHit");
    }
}
