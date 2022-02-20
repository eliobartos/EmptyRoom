using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TileTypes {
        Normal,
        Wall
    }

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _wallColor;
    private SpriteRenderer _renderer;
    private BoxCollider2D _collider;

    public TileTypes tileType;

    // Start is called before the first frame update
    void Awake()
    {
        _renderer = this.GetComponent<SpriteRenderer>();
        _collider = this.GetComponent<BoxCollider2D>();
    }

    public void SetType(TileTypes tileType) {

        switch (tileType) {
            
            case TileTypes.Normal:
                SetNormalTile();
                break;
            
            case TileTypes.Wall:
                SetWallTile();
                break;
        }

    }

    void SetNormalTile() {
        _renderer.color = _baseColor;
        _collider.enabled = false;
        tileType = TileTypes.Normal;
    }

    void SetWallTile() {
        _collider.enabled = true;
        _renderer.color = _wallColor;
        tileType = TileTypes.Wall;
    }

}
