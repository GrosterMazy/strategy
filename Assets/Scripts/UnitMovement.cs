using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    private bool _isHighlightedNeighbour;
    private ObjectOnGrid _objectOnGrid => GetComponent<ObjectOnGrid>();
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();
    [SerializeField] private float _speed = 1;
    private Transform _selected;
    private Transform _highlighted;
    private MouseSelection _mouseSelection => FindObjectOfType<MouseSelection>();
    private HexGrid _hexGrid => FindObjectOfType<HexGrid>();
    private void OnEnable()
    {
        _selected = _mouseSelection.selected;
        MouseSelection.onSelectionChanged += OnSelectionChanged;
        MouseSelection.onHighlightChanged += NeighboursFind;
        MouseSelection.onHighlightChanged += OnHighlightChanged;
    }
    private void OnDisable()
    {
        MouseSelection.onSelectionChanged -= OnSelectionChanged;
        MouseSelection.onHighlightChanged -= NeighboursFind;
        MouseSelection.onHighlightChanged -= OnHighlightChanged;
    }

    private void OnSelectionChanged(Transform selected)
    {
        _selected = selected;
    }
    private void OnHighlightChanged(Transform highlighted)
    {
        _highlighted = highlighted;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
           if(_isHighlightedNeighbour)
           {
                transform.position = _highlighted.position;
                _placementManager.UpdateGrid(_objectOnGrid.LocalCoords, _hexGrid.InLocalCoords(_highlighted.position), _objectOnGrid);
                MouseSelection.onSelectionChanged.Invoke(_highlighted);
           }
        }
    }
    private void NeighboursFind(Transform highlighted)
    {
        _isHighlightedNeighbour = false;
        if (_selected == null || highlighted == null) return;
        var _localCoordsSelected = _hexGrid.InLocalCoords(_selected.position);
        var _neighbours = _hexGrid.Neighbours(_localCoordsSelected);
        for (int i = 0; i < _neighbours.Length; i++)
        {
            if (highlighted.position == _hexGrid.InUnityCoords(_neighbours[i]))
            {
                _isHighlightedNeighbour = true;
                break;
            }
        }
    }

}
