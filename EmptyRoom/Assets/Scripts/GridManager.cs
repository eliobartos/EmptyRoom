using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    [SerializeField] private int _width, _height;

    [SerializeField] private Tile _tileNormalPrefab;
    [SerializeField] private Tile _tileWallPrefab;

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

        _height = gridArray.GetLength(0);
        _width = gridArray.GetLength(1);

        _tiles = new Dictionary<Vector2, Tile>();

        // Generate all tiles
        for (int y = _height-1; y >= 0; y--) {
            for (int x = 0; x < _width; x++) {
                
                var spawnedTile = Instantiate(_tileNormalPrefab, new Vector3(x, y), Quaternion.identity);

                // Set tile properties
                spawnedTile.name = $"Tile y={y}, x={x}";
                spawnedTile.Init(TileTypes.Normal);

                // Save tile to dict
                _tiles[new Vector2(y, x)] = spawnedTile;
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

        _height = gridArray.GetLength(0);
        _width = gridArray.GetLength(1);

        for (int y = 0; y < _height; y++) {
            for (int x = 0; x < _width; x++) {

                Tile tile = GetTile(new Vector2(_height-1 - y, x));

                switch (gridArray[y, x]) {
                    case 0:
                        if(tile.type != TileTypes.Normal)
                            SwapTiles(tile, _tileNormalPrefab, TileTypes.Normal, _height - 1 - y, x);
                        break;
                    case 1:
                        if(tile.type != TileTypes.Wall)
                            SwapTiles(tile, _tileWallPrefab, TileTypes.Wall, _height - 1 - y, x);    
                        break;
                }
            }
        }
    }

    // Destroy the current tile and place a new one instead
    public void SwapTiles(Tile currentTile, Tile newTilePrefab,TileTypes newTileType, int y, int x) {
        
        Destroy(currentTile.gameObject);
        
        var newTile = Instantiate(newTilePrefab, new Vector3(x, y, 0), Quaternion.identity);
        newTile.name = $"Tile y={y}, x={x}";
        newTile.Init(newTileType);
        
        _tiles[new Vector2(y, x)] = newTile;
    }

    // Updated the grid based on list of changes in term of coordinates and tile types
    public void UpdateGrid(Dictionary<Vector2, TileTypes> changeDict) {
        foreach (KeyValuePair<Vector2, TileTypes> pair in changeDict) {
            Tile tile = GetTile(pair.Key);
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
