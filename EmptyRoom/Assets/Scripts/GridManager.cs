using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    [SerializeField] private int _width, _height;

    [SerializeField] private Tile _tileNormalPrefab;
    [SerializeField] private Tile _tileWallPrefab;

    private Dictionary<Vector2, Tile> _tiles;
    private List<BallBehaviour> _balls;


    [SerializeField] private PlayerLigthManager playerLightManager;
    [SerializeField] private PlayerMovement playerMovement;

    public void GenerateGrid(int[,] gridArray) {

        _width = gridArray.GetLength(0);
        _height = gridArray.GetLength(1);

        _tiles = new Dictionary<Vector2, Tile>();

        // Generate all tiles
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {

                var spawnedTile = Instantiate(_tileNormalPrefab, new Vector3(x, y), Quaternion.identity);

                // Set tile properties
                spawnedTile.name = $"Tile x={x}, y={y}";
                spawnedTile.Init(TileTypes.Normal);

                // Save tile to dict
                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }
    }

    public Tile GetTile(Vector2 pos) {
        if(_tiles.TryGetValue(pos, out var tile))
            return tile;

        Debug.LogError("No tile found at location: " + pos);
        return null;
    }

    // Updates previously created grid (tiles only)
    public void UpdateGrid(int[,] gridArray, bool onlyUpdateNonVisibleTiles=false) {

        _width = gridArray.GetLength(0);
        _height = gridArray.GetLength(1);

        var visibilityRadius = playerLightManager.GetMaxLightRadius();
        Vector2 playerPosition = playerMovement.transform.position;

        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                if (onlyUpdateNonVisibleTiles & ((playerPosition - new Vector2(x, y)).magnitude < visibilityRadius) ) {
                    continue;
                }
                Tile tile = GetTile(new Vector2(x, y));

                switch (gridArray[x, y]) {
                    case 0:
                        if(tile.type != TileTypes.Normal)
                            SwapTiles(tile, _tileNormalPrefab, TileTypes.Normal, x, y);
                        break;
                    case 1:
                        if(tile.type != TileTypes.Wall)
                            SwapTiles(tile, _tileWallPrefab, TileTypes.Wall, x, y);
                        break;
                }
            }
        }
    }

    // Destroy the current tile and place a new one instead
    public void SwapTiles(Tile currentTile, Tile newTilePrefab,TileTypes newTileType, int x, int y) {

        Destroy(currentTile.gameObject);

        var newTile = Instantiate(newTilePrefab, new Vector3(x, y, 0), Quaternion.identity);
        newTile.name = $"Tile x={x}, y={y}";
        newTile.Init(newTileType);

        _tiles[new Vector2(x, y)] = newTile;
    }

    // Updated the grid based on list of changes in term of coordinates and tile types
    public void UpdateGrid(Dictionary<Vector2, TileTypes> changeDict) {
        foreach (KeyValuePair<Vector2, TileTypes> pair in changeDict) {
            Tile tile = GetTile(pair.Key);
        }
    }

    // Adds all placeable objects on the screen
    public List<GameObject> AddPlaceableObjects(List<PlaceableObject> placeableObjects) {

        List<GameObject> createdObjects = new List<GameObject>();

        foreach(PlaceableObject placeableObject in placeableObjects) {
            var plObj = Instantiate(placeableObject.prefab, placeableObject.position, placeableObject.rotation);
            plObj.name = placeableObject.name;

            createdObjects.Add(plObj);
        }

        return createdObjects;
    }
}
