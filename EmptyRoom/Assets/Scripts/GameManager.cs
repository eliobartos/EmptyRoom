using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering.Universal;

public class GameManager : NonPersistentSingleton<GameManager>
{

    // Game Manager Variables
    [Header("Level Generating Variables")]
    public int levelWidth = 25;
    public int levelHeigth = 5;
    public int maxBallsOnLevel = 9;
    public int[] arrowsPerLevel;
    public int minArrowDistanceToPlayer = 5;
    public int maxArrowDistanceToPlayer = 10;
    [SerializeField] private bool onlyUpdateNonVisibleObject=true;

    [Header("Sound Variables")]
    public int secondMusicBallsNeeded = 4;
    public int thirdMusicBallsNeeded = 7;
    public float FadeInOutTime = 1.0f;

    // Player Variables
    [Header("Player Variables")]
    public int ballsCollected = 0;
    public float currentSanity = 100.0f;
    public float maxSanity = 100.0f;
    public float lowSanityPct = 0.2f;

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
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Light2D globalLight;

    // Prefabs
    [Header("Prefabs")]
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private GameObject _arrowPrefab;

    List<int[,]> worldStages;

    public void Start() {

        // Initialise object containers
        arrows = new List<GameObject>();

        // Set up gameplayer
        sanityBar.SetUp(maxSanity);

        // Spawn player randomly on a map
        playerTransform.position = new Vector3(Random.Range(1, levelWidth - 1), Random.Range(1, levelHeigth - 1));

        // Generate world the world
        // var gameWorld = new GameWorld(levelWidth, levelHeigth, maxBallsOnLevel, percolation_steps: maxBallsOnLevel);
        var stagesProbs = new List<double>() {1.0, 0.89, 0.81, 0.75, 0.72, 0.69, 0.67, 0.65, 0.63, 0.62};
        var gameWorld = new GameWorld(stagesProbs, levelWidth, levelHeigth, maxBallsOnLevel);
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
            return;
        }

        if(ballsCollected == 1) {
            globalLight.gameObject.SetActive(false);
        }

        // Reduce player light
        playerLightManager.SetTargetRadius(ballsCollected);

        // Update the world
        gridManager.UpdateGrid(worldStages[ballsCollected], onlyUpdateNonVisibleObject);

        AddArrowsToWorld(arrowsPerLevel[ballsCollected], onlyUpdateNonVisibleObject);

        // Handle Sound Changes
        HandleSoundChanges();

    }

    private void AddArrowsToWorld(int n, bool destroyPrevious = true, bool doNotDestroyVisibleArrows=false) {

        if(destroyPrevious) DestroyGameObjects(arrows, doNotDestroyVisibleArrows);

        List<PlaceableObject> placeableObjects = new List<PlaceableObject>();

        for (int i = 0; i < n; i++) {
            // Call Game World Utils
            IntCoordinates playerPosition = new IntCoordinates(Mathf.RoundToInt(playerTransform.position.x), Mathf.RoundToInt(playerTransform.position.y));
            ArrowData nextArrow = GameWorldUtils.generate_arrow(worldStages[ballsCollected], playerPosition, GameObjectsToIntCoordinates(balls), minArrowDistanceToPlayer, maxArrowDistanceToPlayer);

            // Convert coordinates to real object
            PlaceableObject ArrowObject = new PlaceableObject(_arrowPrefab, PlaceableObjectType.Arrow, $"Arrow {i}", nextArrow.origin.x, nextArrow.origin.y, Quaternion.FromToRotation(Vector3.left, new Vector3(nextArrow.direction.x, nextArrow.direction.y, 0)));

            // Add object to list for grid manager
            placeableObjects.Add(ArrowObject);
        }

        List<GameObject> newArrows = gridManager.AddPlaceableObjects(placeableObjects);

        foreach(GameObject newArrow in newArrows) {
            arrows.Add(newArrow);
        }

        return;
    }

    private void DestroyGameObjects(List<GameObject> gameObjects, bool doNotDestroyVisibleObjects=false) {
        var playerPosition = playerMovement.transform.position;
        foreach(GameObject gameObject in gameObjects) {
            if (doNotDestroyVisibleObjects) {
                if ((gameObject.transform.position - playerPosition).magnitude < playerLightManager.GetMaxLightRadius()) {
                    continue;
                }
            }
            Destroy(gameObject);
        }
    }

    void ReduceSanityOverTime() {
        currentSanity -= Time.deltaTime;

        if(currentSanity/maxSanity < lowSanityPct) {
            AudioManager.instance.StartWithFadeIn("Sanity", currentSanity);
        } else {
            AudioManager.instance.Stop("Sanity");
        }
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

    // Used by Camera Behaviour to call this after zoom in animation finishes
    public void SetCanPlayerMove(bool _canMove) {
        playerMovement.canMove = _canMove;
    }

    private void HandleSoundChanges() {
        if(ballsCollected == secondMusicBallsNeeded) {
            AudioManager.instance.StopWithFadeout("Theme1", FadeInOutTime);
            AudioManager.instance.StartWithFadeIn("Theme2", FadeInOutTime, FadeInOutTime);
        } else if(ballsCollected == thirdMusicBallsNeeded) {
            AudioManager.instance.StopWithFadeout("Theme2", FadeInOutTime);
            AudioManager.instance.StartWithFadeIn("Theme3", FadeInOutTime, FadeInOutTime);
        }
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
