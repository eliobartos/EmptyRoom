using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TileTypes {
        Normal,
        Wall
    }

public class Tile : MonoBehaviour
{
    public TileTypes type;

    public void Init(TileTypes _type) {
        type = _type;
    }
}
