using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlacementManager : MonoBehaviour
{
    private Vector2Int _size => FindObjectOfType<HexGrid>().size;
    [NonSerialized] public ObjectOnGrid[,] gridWithObjectsInformation; // Сетка с информацией об объектах, которую можно получить, обратившись к этому массиву по координатам.
    private ObjectOnGrid[] _objectsOnGrid;
    private HexGrid _hexGrid => FindObjectOfType<HexGrid>();
    void Awake()
    {
        CreateEmptyGridAwake();
    }
    private void CreateEmptyGridAwake()
    {
        gridWithObjectsInformation = new ObjectOnGrid[_size.x, _size.y];
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
    public void UpdateGrid(Vector3 oldPos, Vector3 newPos, ObjectOnGrid objectOnGrid)
    {
        gridWithObjectsInformation[_hexGrid.InLocalCoords(oldPos).x, _hexGrid.InLocalCoords(oldPos).y] = null;
        gridWithObjectsInformation[_hexGrid.InLocalCoords(oldPos).x, _hexGrid.InLocalCoords(oldPos).y] = objectOnGrid;
    }
    public void UpdateGrid(Transform oldPos, Transform newPos, ObjectOnGrid objectOnGrid)
    {
        gridWithObjectsInformation[_hexGrid.InLocalCoords(oldPos.position).x, _hexGrid.InLocalCoords(oldPos.position).y] = null;
        gridWithObjectsInformation[_hexGrid.InLocalCoords(oldPos.position).x, _hexGrid.InLocalCoords(oldPos.position).y] = objectOnGrid;
    }
}
