using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GlitchBehaviour : MonoBehaviour
{
    float offset;
    public float glitchProb = 0.05f;
    public float initialTime = 1.0f;
    public float shortTime = 0.1f;

    SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if(Random.value < glitchProb) {
            offset = Random.value;
            StartCoroutine(StartGlitch());
        }
        
    }

    IEnumerator StartGlitch() {
        yield return new WaitForSeconds(offset);
        while(true) {
            yield return new WaitForSeconds(initialTime);
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(shortTime);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(shortTime);
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(shortTime);
            spriteRenderer.enabled = true;
        }
        
    }
}
