using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlaceableObjectType {
    Ball,
    Decoration
}


public class PlaceableObject
{
    public GameObject prefab;
    public Vector3 position;
    public Quaternion rotation;
    public PlaceableObjectType type;

    public PlaceableObject(GameObject _prefab, PlaceableObjectType _type, int row, int column) {
        
        position = new Vector3(column, row, 0);
        prefab = _prefab;
        rotation = Quaternion.identity;
        type = _type;
    }

    public PlaceableObject(GameObject _prefab, PlaceableObjectType _type, int row, int column, float offsetX, float offsetY) {
        
        position = new Vector3(column + offsetX, row + offsetY, 0);
        prefab = _prefab;
        rotation = Quaternion.identity;
        type = _type;
    }
}
