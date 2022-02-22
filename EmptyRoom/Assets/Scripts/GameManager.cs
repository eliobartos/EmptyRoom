using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NonPersistentSingleton<GameManager>
{

    // Game Manager Variables
    [Header("Level Generating Variables")]
    public int levelWidth = 25;
    public int levelHeigth = 5;
    public int maxBallsOnLevel = 9;
    public int[] arrowsPerLevel;

    // Player Variables
    [Header("Player Variables")]
    public int ballsCollected = 0;
    public float currentSanity = 100.0f;
    public float maxSanity = 100.0f;

    // UI Objects
    [Header("References to Objects")]
    [SerializeField] private Bar sanityBar;

    // Keep Track of objects
    public List<GameObject> balls;
    List<GameObject> arrows;

    // References to other objects
    [SerializeField] private GridManager gridManager;
    [SerializeField] private PlayerLigthManager playerLightManager;
    [SerializeField] private Transform playerTransform;

    // Prefabs
    [Header("Prefabs")]
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private GameObject _arrowPrefab;

    List<int[,]> worldStages;
  
    public void Start() {
        sanityBar.SetUp(maxSanity);
        arrows = new List<GameObject>();

        // Generate world the world
        var gameWorld = new GameWorld(levelWidth, levelHeigth, maxBallsOnLevel, percolation_steps: maxBallsOnLevel);
        gameWorld.generate_world();

        worldStages = gameWorld.stages;
        var ballsList = CoordinatesToPlaceableObject(gameWorld.rewards, PlaceableObjectType.Ball, "Ball", _ballPrefab);

        gridManager.GenerateGrid(worldStages[0]);
        balls = gridManager.AddPlaceableObjects(ballsList);

    }

    void Update() {
        ReduceSanityOverTime();
        sanityBar.UpdateBar(currentSanity);

        if(currentSanity <= 0.0f) {
            LevelLost();
        }
    }

    // Called by BallBehaviour when the ball is collected
    public void BallCollected() {
        ballsCollected += 1;

        if(ballsCollected == maxBallsOnLevel) {
            LevelWon();
        }

        // Update the world
        gridManager.UpdateGrid(worldStages[ballsCollected]);

        // Reduce player light
        playerLightManager.SetTargetRadius(ballsCollected);

        AddArrowsToWorld(arrowsPerLevel[ballsCollected]);
        Debug.Log(arrows.Count);

    }

    private void AddArrowsToWorld(int n, bool destroyPrevious = true) {

        if(destroyPrevious) DestroyGameObjects(arrows);

        List<PlaceableObject> placeableObjects = new List<PlaceableObject>();

        for (int i = 0; i < n; i++) {
            // Call Game World Utils
            IntCoordinates playerPosition = new IntCoordinates(Mathf.RoundToInt(playerTransform.position.x), Mathf.RoundToInt(playerTransform.position.y));
            ArrowData nextArrow = GameWorldUtils.generate_arrow(worldStages[ballsCollected], playerPosition, GameObjectsToIntCoordinates(balls));

            // Convert coordinates to real object
            PlaceableObject ArrowObject = new PlaceableObject(_arrowPrefab, PlaceableObjectType.Arrow, $"Arrow {i}", nextArrow.origin.x, nextArrow.origin.y);
            Debug.Log(nextArrow.origin.x);
            Debug.Log(nextArrow.origin.y);
            // Add object to list for grid manager
            placeableObjects.Add(ArrowObject);
        }

        List<GameObject> newArrows = gridManager.AddPlaceableObjects(placeableObjects);

        foreach(GameObject newArrow in newArrows) {
            arrows.Add(newArrow);
        }

        return;
    }

    private void DestroyGameObjects(List<GameObject> gameObjects) {
        foreach(GameObject gameObject in gameObjects) {
            Destroy(gameObject);
        }
    }

    void ReduceSanityOverTime() {
        currentSanity -= Time.deltaTime;
    }

    // Convert list of coordinats into meaningful game object to be placed by gridManager
    private List<PlaceableObject> CoordinatesToPlaceableObject(List<IntCoordinates> corList, PlaceableObjectType type, string name, GameObject prefab) {

        List<PlaceableObject> plObjList = new List<PlaceableObject>();

        foreach(IntCoordinates cor in corList) {
            PlaceableObject plObj = new PlaceableObject(prefab, type, name, cor.x, cor.y);
            plObjList.Add(plObj);
        }

        return plObjList;
    }

    private List<IntCoordinates> GameObjectsToIntCoordinates(List<GameObject> gameObjects) {
        List<IntCoordinates> objectsCor = new List<IntCoordinates>();

        foreach(GameObject gameObject in gameObjects) {
            IntCoordinates intCor = new IntCoordinates(Mathf.RoundToInt(gameObject.transform.position.x), Mathf.RoundToInt(gameObject.transform.position.y));
            objectsCor.Add(intCor);
        }
        return objectsCor;
    }

    void LevelWon() {
        SceneManager.LoadScene("GameScene");
    }

    void LevelLost() {
        SceneManager.LoadScene("GameScene");
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
