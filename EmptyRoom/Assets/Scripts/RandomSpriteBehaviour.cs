using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// If we want some object to have random sprite selecet from the list of sprits on
// init give this class enables that
[RequireComponent(typeof(SpriteRenderer))]
public class RandomSpriteBehaviour : MonoBehaviour
{

    [SerializeField] private Sprite[] spriteList;

    void Start()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();

        int n = spriteList.Length;
        int sample = Random.Range(0, n);
        renderer.sprite = spriteList[sample];
    }
}
