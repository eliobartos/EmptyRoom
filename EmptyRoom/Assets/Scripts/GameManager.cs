using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NonPersistentSingleton<GameManager>
{

    // Game Manager Variables
    [Tooltip("Level Generating Variables")]
    public int levelWidth = 25;
    public int levelHeigth = 25;
    public int maxBallsOnLevel = 9;

    // Player Variables
    [Tooltip("Player Variables")]
    public int ballsCollected = 0;
    public float currentSanity = 100.0f;
    public float maxSanity = 100.0f;

    // UI Objects
    [Tooltip("References to Objects")]
    [SerializeField] private Bar sanityBar;

    // References to other objects
    [SerializeField] private GridManager gridManager;
    [SerializeField] private PlayerLigthManager playerLightManager;

    // Prefabs
    [Tooltip("Prefabs")]
    [SerializeField] private GameObject _ballPrefab;

    List<int[,]> worldStages;
    List<Coordinates> ballsList;

    public void Start() {
        sanityBar.SetUp(maxSanity);
        
        // Generate world the world
        var gameWorld = new GameWorld(levelWidth, levelWidth, maxBallsOnLevel, 1.0, 0.64, maxBallsOnLevel);
        gameWorld.generate_world(24);
        worldStages = gameWorld.stages;
        var ballsList = CoordinatesToPlaceableObject(gameWorld.rewards, PlaceableObjectType.Ball, "Ball", _ballPrefab);

        // foreach(Coordinates cor in gameWorld.rewards) {
        //     Debug.Log(cor.x + "," + cor.y);
        // }
        gridManager.GenerateGrid(worldStages[0]);
        gridManager.AddPlaceableObjects(ballsList);
    }

    void Update() {
        ReduceSanityOverTime();
        sanityBar.UpdateBar(currentSanity);
    }

    // Called by BallBehaviour when the ball is collected
    public void BallCollected() {
        ballsCollected += 1;

        // Update the world
        gridManager.UpdateGrid(worldStages[ballsCollected]);

        // Reduce player light
        playerLightManager.SetTargetRadius(ballsCollected);
    }

    void ReduceSanityOverTime() {
        currentSanity -= Time.deltaTime;
    }

    // Convert list of coordinats into meaningful game object to be placed by gridManager
    private List<PlaceableObject> CoordinatesToPlaceableObject(List<Coordinates> corList, PlaceableObjectType type, string name, GameObject prefab) {

        List<PlaceableObject> plObjList = new List<PlaceableObject>();

        foreach(Coordinates cor in corList) {
            PlaceableObject plObj = new PlaceableObject(prefab, type, name, cor.y, cor.x);
            plObjList.Add(plObj);
        }

        return plObjList;
    }

    // Debug function to plot the matrics
    void printMatrix(int[,] array) {
        for (int i = 0; i < array.GetLength(0); i++) {
            string row = "";
            for (int j= 0; j < array.GetLength(1); j++) {
                row += array[i, j];
            }
            Debug.Log("Row (" + i + ") " + row);
        } 
    }
}
