using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitMovement : MonoBehaviour
{
    public Func<Vector2Int, bool> WantToMoveOnCell; // вызывается до обновления координат на локальной сетке
    public Action MovedToCell; // вызывается после обновления координат на локальной сетке
    private HighlightingController _highlightedController;
    private UnitDescription _unitDescription;
    private TurnManager _turnManager;
    private bool _isHighlightedNeighbour;
    private ObjectOnGrid _objectOnGrid;
    private PlacementManager _placementManager;
    private int _maxSpeed;
    [NonSerialized] public int spentSpeed = 0;
    private Transform _highlighted;
    private MouseSelection _mouseSelection;
    private HexGrid _hexGrid;

    private void Awake()
    {
        InitComponentLinks();
    }
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
        UpdateSpeedValueOnTurnChanged();
    }



    void Update()
    {
        if (Input.GetMouseButtonDown(1) && _unitDescription.IsSelected && _unitDescription.TeamAffiliation == _turnManager.currentTeam)
        {
            if(_isHighlightedNeighbour && !_highlightedController.isAnyUnitHighlighted)
            {
                bool? _canIMove = WantToMoveOnCell?.Invoke(_hexGrid.InLocalCoords(_highlighted.position));
                if (!(bool)_canIMove || !(_maxSpeed - spentSpeed > 0)) { return; }
                transform.position = _highlighted.parent.transform.position;
                _placementManager.UpdateGrid(_objectOnGrid.LocalCoords, _hexGrid.InLocalCoords(_highlighted.position), _objectOnGrid);
                MovedToCell?.Invoke();
                _objectOnGrid.LocalCoords = _hexGrid.InLocalCoords(_highlighted.position);
                spentSpeed += 1;
                _mouseSelection.SetSelection(_highlighted);
            }
        }
    }

    private void NeighboursFind(Transform highlighted)
    {
        _isHighlightedNeighbour = false;
        if (_mouseSelection.selected == null || highlighted == null) return;
        var _neighbours = _hexGrid.Neighbours(_mouseSelection.selected.position);
        for (int i = 0; i < _neighbours.Length; i++)
        {
            if (highlighted.position == _hexGrid.hexCells[_neighbours[i].x, _neighbours[i].y].transform.position)
            {
                _isHighlightedNeighbour = true;
                break;
            }
        }
    }
    private void UpdateSpeedValueOnTurnChanged()
    {
        _maxSpeed = _unitDescription.MovementSpeed;
        spentSpeed = 0;
    }

    private void InitComponentLinks()
    {
        _highlightedController = FindObjectOfType<HighlightingController>();
        _unitDescription = GetComponent<UnitDescription>();
        _turnManager = FindObjectOfType<TurnManager>();
        _objectOnGrid = GetComponent<ObjectOnGrid>();
        _placementManager = FindObjectOfType<PlacementManager>();
        _mouseSelection = FindObjectOfType<MouseSelection>();
        _hexGrid = FindObjectOfType<HexGrid>();
    }
}
