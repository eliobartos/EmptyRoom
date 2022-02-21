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
    public string name;

    public PlaceableObject(GameObject _prefab, PlaceableObjectType _type, string _name, int row, int column, float offsetX, float offsetY) {

        position = new Vector3(column + offsetX, row + offsetY, 0);
        prefab = _prefab;
        rotation = Quaternion.identity;
        type = _type;
        name = _name + $" y={row}, x={column}";
    }
    public PlaceableObject(GameObject _prefab, PlaceableObjectType _type, string _name, int row, int column) : this(_prefab, _type, _name, row, column, 0, 0) {
    }
}
