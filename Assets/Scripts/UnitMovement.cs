using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitMovement : MonoBehaviour
{
    public Action<Vector2Int> WantToMoveOnCell; // вызывается до обновления координат на локальной сетке
    public Action MovedToCell; // вызывается после обновления координат на локальной сетке
    public bool canMove;
    private UnitCharacteristicsTransformationController _unitCharacteristicsTransformationController => GetComponent<UnitCharacteristicsTransformationController>();
    private UnitDescription _unitDescription;
    private TurnManager _turnManager => FindObjectOfType<TurnManager>();
    private bool _isHighlightedNeighbour;
    private ObjectOnGrid _objectOnGrid => GetComponent<ObjectOnGrid>();
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();
    private short _maxSpeed;
    private short _speed;
    private Transform _highlighted;
    private MouseSelection _mouseSelection => FindObjectOfType<MouseSelection>();
    private HexGrid _hexGrid => FindObjectOfType<HexGrid>();
    private void OnEnable()
    {
        TurnManager.onTurnChanged += UpdateSpeedValueOnTurnChanged;
        MouseSelection.onHighlightChanged += NeighboursFind;
        MouseSelection.onHighlightChanged += OnHighlightChanged;
    }
    private void OnDisable()
    {
        TurnManager.onTurnChanged -= UpdateSpeedValueOnTurnChanged;
        MouseSelection.onHighlightChanged -= NeighboursFind;
        MouseSelection.onHighlightChanged -= OnHighlightChanged;
    }
    private void OnHighlightChanged(Transform highlighted)
    {
        _highlighted = highlighted;
    }
    private void Start()
    {
        _unitDescription = _unitCharacteristicsTransformationController.unitDescription;
        UpdateSpeedValueOnTurnChanged();
    }



    void Update()
    {
        if (Input.GetMouseButtonDown(1) && canMove && _maxSpeed + _speed > 0)
        {
            if(_isHighlightedNeighbour)
            {
                transform.position = _highlighted.position;
                WantToMoveOnCell?.Invoke(_hexGrid.InLocalCoords(_highlighted.position));
                _placementManager.UpdateGrid(_objectOnGrid.LocalCoords, _hexGrid.InLocalCoords(_highlighted.position), _objectOnGrid);
                MovedToCell?.Invoke();
                _objectOnGrid.LocalCoords = _hexGrid.InLocalCoords(_highlighted.position);
                _speed -= 1;
                _mouseSelection.SetSelection(_highlighted);
            }
        }
    }
    private void NeighboursFind(Transform highlighted)
    {
        _isHighlightedNeighbour = false;
        if (_mouseSelection.selected == null || highlighted == null) return;
        var _localCoordsSelected = _hexGrid.InLocalCoords(_mouseSelection.selected.position);
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
    private void UpdateSpeedValueOnTurnChanged()
    {
        _maxSpeed = _unitDescription.MovementSpeed;
        _speed = 0;
    }
}
