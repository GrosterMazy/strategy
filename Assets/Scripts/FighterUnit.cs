using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FighterUnit : UnitDescription
{
    [SerializeField] private int _maxHeightToStep;
    private PlacementManager _placementManager;
    private ObjectOnGrid[,] _gridWithObjects;
    private UnitMovement _unitMovement;
    private UnitHealth _unitHealth;
    private CollectableItem _itemToReturnReference;
    private HexGrid _hexGrid;
    private TargetsInDarkness _targetsInDarkness;
    private Transform _highlighted;
    private MouseSelection _mouseSelection;
    private BuildingManager _buildingManager;
    
    private TurnManager _turnManager;

    private bool ItemToReturnReferenceUpdater(Vector2Int _coordsWithItem)
    {
        var _objectOnMyWay = _gridWithObjects[_coordsWithItem.x, _coordsWithItem.y];
        if (_objectOnMyWay != null)
        { 
            _itemToReturnReference = _objectOnMyWay.GetComponent<CollectableItem>();
            if (_itemToReturnReference != null) return true;
            else return false;
        }
        else return NatureProhibitionInMove(_coordsWithItem);
    }

    private void MaintenanceCosts() { _buildingManager.TeamsAdministratumsReferences[TeamAffiliation].WasteResources(0, 0, 0, FoodConsumption); }

    private void InitComponents() { _unitMovement = GetComponent<UnitMovement>(); _unitHealth = GetComponent<UnitHealth>(); _placementManager = FindObjectOfType<PlacementManager>(); _hexGrid = FindObjectOfType<HexGrid>();
        _targetsInDarkness = FindObjectOfType<TargetsInDarkness>(); _mouseSelection = FindObjectOfType<MouseSelection>(); _buildingManager = FindObjectOfType<BuildingManager>(); }

    private void ItemReferenceReturner() {  
        if (_itemToReturnReference != null && _gridWithObjects[_itemToReturnReference.LocalCoords.x, _itemToReturnReference.LocalCoords.y] == null) { _itemToReturnReference.GoneAway(); } }


    private bool NatureProhibitionInMove(Vector2Int _cell) { return _hexGrid.hexCells[_cell.x, _cell.y].isWater || Mathf.Abs(_hexGrid.hexCells[_cell.x, _cell.y].height - _hexGrid.hexCells[LocalCoords.x, LocalCoords.y].height) > _maxHeightToStep ? false : true; }

    private void UpdateMyTargetInDarknessCoords() { _targetsInDarkness.RemoveTarget(LocalCoords); _targetsInDarkness.AddTarget(_hexGrid.InLocalCoords(_highlighted.position)); }

    private new void Awake() {
        _turnManager = FindObjectOfType<TurnManager>();
        base.Awake();
        InitComponents();
    }

    private void Update() { _gridWithObjects = _placementManager.gridWithObjectsInformation; _highlighted = _mouseSelection.highlighted; } // нужен экшен изменения gridwoi для полной оптимизации

    private void OnEnable() { _unitMovement.MovedToCell += ItemReferenceReturner; _unitMovement.WantToMoveOnCell += ItemToReturnReferenceUpdater; _unitHealth.death += ItemReferenceReturner;
         _unitMovement.MovedToCell += UpdateMyTargetInDarknessCoords; _turnManager.onTurnChanged += MaintenanceCosts; }
    private void OnDisable() { _unitMovement.MovedToCell -= ItemReferenceReturner; _unitMovement.WantToMoveOnCell -= ItemToReturnReferenceUpdater; _unitHealth.death -= ItemReferenceReturner;
         _unitMovement.MovedToCell -= UpdateMyTargetInDarknessCoords; _turnManager.onTurnChanged -= MaintenanceCosts; }
}
