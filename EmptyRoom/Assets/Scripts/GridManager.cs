using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    [SerializeField] private int _width, _height;

    [SerializeField] private Tile _tilePrefab;

    private Dictionary<Vector2, Tile> _tiles;

    private int _ballsCollected = 0;

    void Start() {

        _ballsCollected = 0;

        // int[,] gridArray = new int[,] {{0, 0, 0, 0, 0},
        //                                {0, 0, 0, 0, 0},
        //                                {0, 0, 0, 0, 0}};


        // GenerateGrid(gridArray);

        // List<PlaceableObject> placeableObjects = new List<PlaceableObject>();
        // placeableObjects.Add(new PlaceableObject(_ballPrefab, PlaceableObjectType.Ball, 2, 0));
        // placeableObjects.Add(new PlaceableObject(_ballPrefab, PlaceableObjectType.Ball, 2, 4));

        // AddPlaceableObjects(placeableObjects);

    }

    public void GenerateGrid(int[,] gridArray) {

        _width = gridArray.GetLength(0);
        _height = gridArray.GetLength(1);

        _tiles = new Dictionary<Vector2, Tile>();

        // Generate all tiles
        for (int y = _height-1; y >= 0; y--) {
            for (int x = 0; x < _width; x++) {

                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity);

                // Set tile properties
                spawnedTile.name = $"Tile y={y}, x={x}";

                switch (gridArray[x, y]) {
                    case 0:
                        spawnedTile.SetType(TileTypes.Normal);
                        break;
                    case 1:
                        spawnedTile.SetType(TileTypes.Wall);
                        break;
                }

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
    public void UpdateGrid(int[,] gridArray) {

        _width = gridArray.GetLength(0);
        _height = gridArray.GetLength(1);

        for (int y = 0; y < _height; y++) {
            for (int x = 0; x < _width; x++) {

                Tile tile = GetTile(new Vector2(x, y));

                switch (gridArray[x, y]) {
                    case 0:
                        if(tile.tileType != TileTypes.Normal)
                            tile.SetType(TileTypes.Normal);
                        break;
                    case 1:
                        if(tile.tileType != TileTypes.Wall)
                            tile.SetType(TileTypes.Wall);
                        break;
                }
            }
        }
    }

    // Updated the grid based on list of changes in term of coordinates and tile types
    public void UpdateGrid(Dictionary<Vector2, TileTypes> changeDict) {
        foreach (KeyValuePair<Vector2, TileTypes> pair in changeDict) {
            Tile tile = GetTile(pair.Key);
            tile.SetType(pair.Value);
        }
    }

    // Adds all placeable objects on the screen
    public void AddPlaceableObjects(List<PlaceableObject> placeableObjects) {

        foreach(PlaceableObject placeableObject in placeableObjects) {
            var plObj = Instantiate(placeableObject.prefab, placeableObject.position, placeableObject.rotation);
            plObj.name = placeableObject.name;
            Debug.Log(plObj.name);
        }
    }
}
