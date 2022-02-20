using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NonPersistentSingleton<GameManager>
{
    // Game Manager Variables
    public int ballsCollected = 0;
    public float currentSanity = 100.0f;
    public float maxSanity = 100.0f;

    // UI Objects
    [SerializeField] private Bar sanityBar;

    // References to other objects
    private GridManager gridManager;

    

    public void Start() {
        gridManager = FindObjectOfType<GridManager>();
        sanityBar.SetUp(maxSanity);
    }

    void Update() {
        ReduceSanityOverTime();
        sanityBar.UpdateBar(currentSanity);
    }

    // Called by BallBehaviour when the ball is collected
    public void BallCollected() {
        ballsCollected += 1;

        int[,] gridArray = new int[,] { {0, 0, 1, 0, 0},
                                        {0, 1, 1, 0, 0},
                                        {0, 0, 0, 0, 0}};

        gridManager.UpdateGrid(gridArray);
    }

    void ReduceSanityOverTime() {
        currentSanity -= Time.deltaTime;
    }
}
