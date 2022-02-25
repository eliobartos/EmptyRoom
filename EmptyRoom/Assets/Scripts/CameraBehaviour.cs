using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{

    int levelWidth;
    int levelHeight;

    float startingOrtographicZoom;
    public float targetZoom = 3.0f;
    public float zoomLerpPct = 0.01f;

    public float startDelayTime = 3.0f;
    public float startZoomSpeed = 0.1f;

    void Start() {
        levelWidth = GameManager.instance.levelWidth;
        levelHeight = GameManager.instance.levelHeigth;

        // Center the camera at the beginning
        this.transform.position = new Vector3((levelWidth - 0.5f) / 2.0f, (levelHeight - 0.5f) / 2.0f, this.transform.position.z);

        // Set the beginning zoom out
        //startingOrtographicZoom = Mathf.Min(levelHeight, levelWidth) / 4.0f;
        //Camera.main.orthographicSize = startingOrtographicZoom;

        StartCoroutine("StartingZoomIn");
    }

    // Update is called once per frame
    void Update()
    {
        // Make camera stay inside of the level
        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = halfHeight * Screen.width / Screen.height;

        float posX = Mathf.Clamp(this.transform.position.x, -0.5f + halfWidth, levelWidth - halfWidth - 0.5f);
        float posY = Mathf.Clamp(this.transform.position.y, -0.5f + halfHeight, levelHeight - halfHeight - 0.5f);

        this.transform.position = new Vector3(posX, posY, this.transform.position.z);
    }

    IEnumerator StartingZoomIn() {
        yield return new WaitForSeconds(startDelayTime);
        while(Camera.main.orthographicSize > targetZoom) {
            Camera.main.orthographicSize -= startZoomSpeed;
            yield return new WaitForFixedUpdate();
        }
        
        GameManager.instance.SetCanPlayerMove(true);
    }
}
