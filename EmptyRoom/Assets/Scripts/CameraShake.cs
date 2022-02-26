using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	public Transform camTransform;

    // How long the object should shake for.
    public float origShakeDuration = 1.0f;
    float shakeDuration = 0f;
	
	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount = 0.7f;
	public float decreaseFactor = 1.0f;

    float halfHeight;
    float halfWidth;

    float levelWidth;
    float levelHeight;

    Vector3 originalPos;
	
	void Awake()
	{
		if (camTransform == null)
		{
			camTransform = GetComponent(typeof(Transform)) as Transform;
		}
	}

    void Start() {
        halfHeight = Camera.main.orthographicSize;
        halfWidth = halfHeight * Screen.width / Screen.height;

        levelWidth = GameManager.instance.levelWidth;
        levelHeight = GameManager.instance.levelHeigth;
    }
	
	void OnEnable()
	{
		originalPos = camTransform.localPosition;
        shakeDuration = origShakeDuration;
    }

	void Update()
	{
		if (shakeDuration > 0)
		{
			camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

            float posX = Mathf.Clamp(this.transform.position.x, -0.5f + halfWidth, levelWidth - halfWidth - 0.5f);
            float posY = Mathf.Clamp(this.transform.position.y, -0.5f + halfHeight, levelHeight - halfHeight - 0.5f);

            this.transform.position = new Vector3(posX, posY, this.transform.position.z);

			shakeDuration -= Time.deltaTime * decreaseFactor;
		}
		else
		{
			shakeDuration = 0.0f;
		}
	}
}