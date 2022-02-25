using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;

public class GameManager : NonPersistentSingleton<GameManager>
{

    bool gameOver = false;

    // Game Manager Variables
    [Header("Level Generating Variables")]
    public int levelWidth = 25;
    public int levelHeigth = 5;
    public int maxBallsOnLevel = 9;
    public int[] arrowsPerLevel;
    public int minArrowDistanceToPlayer = 5;
    public int maxArrowDistanceToPlayer = 10;
    [SerializeField] private bool onlyUpdateNonVisibleObject=true;
    [SerializeField] private double prefered_rewards_distance = 15;
    public List<double> percolationStages;
    public float enemyGenerationPct = 0.3f;


    [Header("Sound Variables")]
    public int secondMusicBallsNeeded = 4;
    public int thirdMusicBallsNeeded = 7;
    public float FadeInOutTime = 1.0f;
    public string[] voiceOverList;
    public float ballIncreaseSanityAmount = 20.0f;

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
    List<GameObject> enemies;
    List<GameObject> decorations;

    // References to other objects
    [SerializeField] private GridManager gridManager;
    [SerializeField] private PlayerLigthManager playerLightManager;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Light2D globalLight;
    [SerializeField] private SubtitleManager subtitleManager;
    [SerializeField] private Image transitionImage;

    // Prefabs
    [Header("Prefabs")]
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private GameObject _arrowPrefab;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _decorationPrefab;

    List<int[,]> worldStages;

    public void Start() {

        // Initialise object containers
        arrows = new List<GameObject>();
        enemies = new List<GameObject>();

        // Set up gameplayer
        sanityBar.SetUp(maxSanity);

        // Spawn player randomly on a map
        playerTransform.position = new Vector3(Random.Range(1, levelWidth - 1), Random.Range(1, levelHeigth - 1));

        // Generate world the world
        // var gameWorld = new GameWorld(levelWidth, levelHeigth, maxBallsOnLevel, percolation_steps: maxBallsOnLevel);
        var gameWorld = new GameWorld(percolationStages, levelWidth, levelHeigth, maxBallsOnLevel, prefered_rewards_distance: prefered_rewards_distance);
        gameWorld.generate_world();

        worldStages = gameWorld.stages;

        gridManager.GenerateGrid(worldStages[0]);
        
        // Add Balls
        var ballsList = CoordinatesToPlaceableObject(gameWorld.rewards, PlaceableObjectType.Ball, "Ball", _ballPrefab);
        balls = gridManager.AddPlaceableObjects(ballsList);

        // Start scene transition
        StartCoroutine(ChangeImageAlphaAnim(transitionImage, 1.0f, 0.0f, 1.0f));

        // Start Subtitles
        subtitleManager.DisplaySubtitle(ballsCollected, 3.0f);

    }

    void Update() {
        ReduceSanityOverTime();
        sanityBar.UpdateBar(currentSanity);

        if(currentSanity <= 0.0f && gameOver == false) {
            LevelLost();
            gameOver = true;
        }
    }

    // Called by BallBehaviour when the ball is collected
    public void BallCollected() {
        ballsCollected += 1;

        if(ballsCollected == 1) {
            globalLight.gameObject.SetActive(false);

            // Add Fixed Walkable Decorations
            AddDecorationsToWorld(10);

        } else if(ballsCollected == 9) {
            globalLight.gameObject.SetActive(true);
        }

        // Reduce player light
        playerLightManager.SetTargetRadius(ballsCollected);

        // Update the world
        if(ballsCollected == 9) {
            gridManager.UpdateGrid(worldStages[0], onlyUpdateNonVisibleObject);
        } else {
            gridManager.UpdateGrid(worldStages[ballsCollected], onlyUpdateNonVisibleObject);
        }


        AddArrowsToWorld(arrowsPerLevel[ballsCollected], onlyUpdateNonVisibleObject);

        // Handle Sound Changes
        HandleSoundChanges();

        // Show Subtitles
        subtitleManager.DisplaySubtitle(ballsCollected);

        // Generate Enemy
        float rUnif = Random.value;
        Debug.Log(rUnif);
        if(rUnif < enemyGenerationPct) {
            GenerateEnemy();
        }

        // End the level
        if(ballsCollected == maxBallsOnLevel) {
            DestroyGameObjects(enemies, false);
            DestroyGameObjects(decorations, false);
            LevelWon();
            return;
        }

    }

    private void AddDecorationsToWorld(int n) {
        
        IntCoordinates playerCoordinates = new IntCoordinates(Mathf.RoundToInt(playerMovement.transform.position.x), Mathf.RoundToInt(playerMovement.transform.position.y));
        List<IntCoordinates> decorationsCoord = GameWorldUtils.find_space_for_n_objects(worldStages[8], 10, 3.0, min_distance_to_player: 10.0f, player_position: playerCoordinates);
        var decorationsList = CoordinatesToPlaceableObject(decorationsCoord, PlaceableObjectType.Decoration, "Decoration", _decorationPrefab);
        decorations = gridManager.AddPlaceableObjects(decorationsList);
    }

    private void AddArrowsToWorld(int n, bool destroyPrevious = true, bool doNotDestroyVisibleArrows=false) {

        if(destroyPrevious) DestroyGameObjects(arrows, doNotDestroyVisibleArrows);

        List<PlaceableObject> placeableObjects = new List<PlaceableObject>();

        for (int i = 0; i < n; i++) {
            // Call Game World Utils
            IntCoordinates playerPosition = new IntCoordinates(Mathf.RoundToInt(playerTransform.position.x), Mathf.RoundToInt(playerTransform.position.y));
            ArrowData nextArrow = GameWorldUtils.generate_arrow(
                worldStages[ballsCollected], playerPosition, GameObjectsToIntCoordinates(balls),
                min_distance_to_player: minArrowDistanceToPlayer,
                max_distance_to_player: maxArrowDistanceToPlayer);

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
        playerMovement.canMove = false;
        AudioManager.instance.StopWithFadeout("Theme3", 8.0f);
        AudioManager.instance.StopAllWithFadeout(voiceOverList, 8.0f);
        StartCoroutine(ChangeImageAlphaAnim(transitionImage, 0.0f, 1.0f, 4.0f, 4.0f));
        StartCoroutine(LoadSceneDelay("MainMenu", 8.0f));
    }

    IEnumerator LoadSceneDelay(string sceneName, float delay) {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    void LevelLost() {
        subtitleManager.DisplaySubtitle(10);
        playerLightManager.SetTargetRadiusDirectly(0.0f);
        playerMovement.canMove = false;
        AudioManager.instance.StopWithFadeout("Sanity", 8.0f);
        AudioManager.instance.StopAllWithFadeout(new string[] {"Theme1", "Theme2", "Theme3"}, 8.0f);
        AudioManager.instance.StopAllWithFadeout(voiceOverList, 8.0f);
        StartCoroutine(ChangeImageAlphaAnim(transitionImage, 0.0f, 1.0f, 4.0f, 4.0f));
        StartCoroutine(LoadSceneDelay("MainMenu", 8.0f));

    }

    void GenerateEnemy() {
        int x = Random.Range(0, levelWidth + 1);
        int y = Random.Range(0, levelHeigth + 1);
        Vector3 startPosition = new Vector3(x, y, 0);

        var enemy = (GameObject)Instantiate(_enemyPrefab, startPosition, Quaternion.identity);
        var enemyBehaviourComp = enemy.GetComponent<EnemyBehaviour>();
        enemyBehaviourComp.Init(3, 5);

        enemies.Add(enemy);
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

    public void increaseSanity(float amount) {
        currentSanity += amount;
        if(currentSanity > maxSanity) {
            currentSanity = maxSanity;
        }
    }

    // Helper function for scene transitions
    public static IEnumerator ChangeImageAlphaAnim(Image img, float startAlpha, float endAlpha, float duration = 1.0f, float delay = 0.0f) {

        // Wait for delay time
        yield return new WaitForSeconds(delay);

        // Start Fade In
        float currentTime = 0;

        while(currentTime < duration) {
            currentTime += Time.deltaTime;
            var tempColor = img.color;
            tempColor.a = Mathf.Lerp(startAlpha, endAlpha, currentTime / duration);
            img.color = tempColor;
            yield return null;
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
