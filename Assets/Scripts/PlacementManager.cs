using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlacementManager : MonoBehaviour
{
    private Vector2Int _size => FindObjectOfType<HexGrid>().size;
    [NonSerialized] public ObjectOnGrid[,] gridWithObjectsInformation;
    private ObjectOnGrid[] _objectsOnGrid;
    private HexGrid _hexGrid => FindObjectOfType<HexGrid>();
    void Awake()
    {
        CreateEmptyGridAwake();
    }
    private void CreateEmptyGridAwake()
    {
        gridWithObjectsInformation = new ObjectOnGrid[_size.x, _size.y];
        for (int x = 0; x < _size.x; x++)
        {
            for (int y = 0; y < _size.y; y++)
            {
                gridWithObjectsInformation[x, y] = null;
            }
        }
    }
    private void Start()
    {
        CreateGridStart();
    }
    private void CreateGridStart()
    {
        _objectsOnGrid = FindObjectsOfType<ObjectOnGrid>();
        foreach (ObjectOnGrid _objectOnGrid in _objectsOnGrid)
        {
            Vector2Int _unitLocalCoords = _objectOnGrid.LocalCoords;
            gridWithObjectsInformation[_unitLocalCoords.x, _unitLocalCoords.y] = _objectOnGrid;
            _objectOnGrid.transform.position = _hexGrid.InUnityCoords(_unitLocalCoords);
        }
    }
    public void UpdateGrid(Vector2Int oldPos, Vector2Int newPos, ObjectOnGrid objectOnGird)
    {
        gridWithObjectsInformation[oldPos.x, oldPos.y] = null;
        gridWithObjectsInformation[newPos.x, newPos.y] = objectOnGird;
    }
}
