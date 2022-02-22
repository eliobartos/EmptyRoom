using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlaceableObjectType {
    Ball,
    Decoration,
    Arrow
}


public class PlaceableObject
{
    public GameObject prefab;
    public Vector3 position;
    public Quaternion rotation;
    public PlaceableObjectType type;
    public string name;

    public PlaceableObject(GameObject _prefab, PlaceableObjectType _type, string _name, int x, int y) : this(_prefab, _type, _name, x, y, 0, 0) {}

    public PlaceableObject(GameObject _prefab, PlaceableObjectType _type, string _name, int x, int y, float offsetX, float offsetY) {
        
        position = new Vector3(x + offsetX, y + offsetY, 0);
        prefab = _prefab;
        rotation = Quaternion.identity;
        type = _type;
        name = _name + $" x={x}, y={y}";
    }
}
