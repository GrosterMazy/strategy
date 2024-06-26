﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WorkerUnit : UnitDescription
{
    public static Action<WorkerUnit, FacilityDescription> WantToOpenSingleTransferWindow;
    public int RangedLoadDistance = 1;
    [SerializeField] private int _maxHeightToStep;
    public float miningModifier;
    public Dictionary<string, int> Inventory = new Dictionary<string, int>();
    [NonSerialized] public float _weightCapacityRemaining;
    public float WeightCapacityMax;
    private PlacementManager _placementManager;
    private UnitMovement _unitMovement;
    private UnitHealth _unitHealth;
    [NonSerialized] public UnitActions _unitActions;
    private FacilityDescription _highlightedFacility;
    private Transform _highlighted;
    private CollectableItem _itemToCollect = null;
    private CollectableItem _itemToReturnReference = null;
    private WorkerUnit _thisWorkerUnit;
    private MouseSelection _mouseSelection;
    private HighlightingController _highlightingController;
    private HexGrid _hexGrid;
    private TargetsInDarkness _targetsInDarkness;
    private BuildingManager _buildingManager;

    private TurnManager _turnManager;

    private void TransferSingleResourcesWindow() {
        if (Input.GetKeyDown(KeyCode.G) && IsSelected && _highlightedFacility != null && _highlightedFacility.TeamAffiliation == TeamAffiliation && _hexGrid.Distance(LocalCoords, _highlightedFacility.LocalCoords) <= RangedLoadDistance && // Distance не работает так, как надо
            _highlightedFacility.ActionsToFinalizeBuilding == 0) { WantToOpenSingleTransferWindow?.Invoke(_thisWorkerUnit, _highlightedFacility); } }

    private bool CollectItem(Vector2Int _CellToReceiveItem) {
        int _canTakeItems;
        var _objectOnMyWay = _placementManager.gridWithObjectsInformation[_CellToReceiveItem.x, _CellToReceiveItem.y];
        if (_itemToCollect != null ) { _itemToReturnReference = _itemToCollect; }
        if (_objectOnMyWay != null) {_itemToCollect = _objectOnMyWay.GetComponent<CollectableItem>(); 
            if (_itemToCollect != null) { _canTakeItems = Mathf.Clamp(Mathf.FloorToInt(_weightCapacityRemaining / _itemToCollect.WeightOfOneItem), 0, _itemToCollect.NumberOfItems);
                if (!Inventory.ContainsKey(_itemToCollect.Name)) { Inventory.Add(_itemToCollect.Name, _canTakeItems); }
                else if (Inventory.ContainsKey(_itemToCollect.Name)) { Inventory[_itemToCollect.Name] += _canTakeItems; }
                _itemToCollect.Taken(_canTakeItems); _weightCapacityRemaining -= _canTakeItems * _itemToCollect.WeightOfOneItem; } }
        if (_itemToCollect != null || _objectOnMyWay == null) return NatureProhibitionInMove(_CellToReceiveItem);
        else return false;
    }

    private bool EnterBuilding(Vector2Int _cellToEnter) {
        FacilityDescription _buildingOnMyWay = null;
        FirstFactionProductionBuildingDescription _productionBuildingOnMyWay = null;
        var _objectOnMyWay = _placementManager.gridWithObjectsInformation[_cellToEnter.x, _cellToEnter.y];
        if (_objectOnMyWay != null) {       
            _buildingOnMyWay = _objectOnMyWay.GetComponent<FacilityDescription>();
            if (_buildingOnMyWay != null && !_buildingOnMyWay.WorkerOnSite && TeamAffiliation == _buildingOnMyWay.TeamAffiliation && _unitActions.remainingActionsCount > 0) {
                _productionBuildingOnMyWay = _buildingOnMyWay.GetComponent<FirstFactionProductionBuildingDescription>();
                if (_buildingOnMyWay.ActionsToFinalizeBuilding == 0) { 
                    _buildingOnMyWay.WorkerOnSite = true; _placementManager.UpdateGrid(LocalCoords, LocalCoords, null); _buildingOnMyWay.WorkerInsideMe = gameObject; _buildingOnMyWay.OnEnteredWorker?.Invoke(); gameObject.SetActive(false); }
                else if (_productionBuildingOnMyWay != null) { 
                    if (_productionBuildingOnMyWay.Administratum.Storage["Light"] >= _productionBuildingOnMyWay.LightConstructionCost && _productionBuildingOnMyWay.Administratum.Storage["Steel"] >= _productionBuildingOnMyWay.SteelConstructionCost &&
                        _productionBuildingOnMyWay.Administratum.Storage["Wood"] >= _productionBuildingOnMyWay.WoodConstructionCost && _productionBuildingOnMyWay.Administratum.Storage["Food"] >= _productionBuildingOnMyWay.FoodConstructionCost) {
                        _productionBuildingOnMyWay.ActionsToFinalizeBuilding -= 1; _productionBuildingOnMyWay.BuildingExpenses("Construction"); _unitActions.SpendAction(1); } } } }
        if (_buildingOnMyWay == null) return NatureProhibitionInMove(_cellToEnter); else return false; }

    private void RepairBuilding() { 
        if (Input.GetKeyDown(KeyCode.R) && IsSelected && _highlighted != null) {
            Vector2Int _highlightedInLocalCoords = new Vector2Int(_hexGrid.InLocalCoords(_highlighted.position).x, _hexGrid.InLocalCoords(_highlighted.position).y);
            var _smthOnCell = _placementManager.gridWithObjectsInformation[_highlightedInLocalCoords.x, _highlightedInLocalCoords.y];
            if (_smthOnCell != null) {
                FacilityDescription _buildingToRepair = _smthOnCell.GetComponent<FacilityDescription>();
                if (_buildingToRepair == null) { return; }
                FacilityHealth _healthOfBuildingToRepair = _buildingToRepair.GetComponent<FacilityHealth>();
                if (_healthOfBuildingToRepair.currentHealth < _buildingToRepair.MaxHealth && TeamAffiliation == _buildingToRepair.TeamAffiliation && _unitActions.remainingActionsCount > 0 && IsNeededBuildingNear(_buildingToRepair)) {
                    _unitActions.SpendAction(1);
                    _healthOfBuildingToRepair.Repairment(); } } } }

    private bool IsNeededBuildingNear(FacilityDescription _buildingToCheck) {
        foreach (Vector2Int _cell in _hexGrid.Neighbours(transform.position)) {
            ObjectOnGrid _objectOnCell = _placementManager.gridWithObjectsInformation[_cell.x, _cell.y];
            if (_objectOnCell != null) {
                FacilityDescription _building = _objectOnCell.GetComponent<FacilityDescription>();
                if (_building != null && _building == _buildingToCheck) { return true; } } } return false; }

    private void MaintenanceCosts() { _buildingManager.TeamsAdministratumsReferences[TeamAffiliation].WasteResources(0, 0, 0, FoodConsumption); }

    private bool GetToughResource(Vector2Int _cellToMove)
    {
        ToughResources _toughResourcesOnGrid = null;
        var _objectOnMyWay = _placementManager.gridWithObjectsInformation[_cellToMove.x, _cellToMove.y];
        if (_objectOnMyWay != null)
        {
            _toughResourcesOnGrid = _objectOnMyWay.GetComponent<ToughResources>();
            if (_toughResourcesOnGrid != null && _unitActions.remainingActionsCount > 0)
            {
                CollectToughItem(_toughResourcesOnGrid, _toughResourcesOnGrid.ApplyDamage(miningModifier));
                _unitActions.SpendAction(1);
            }
            return false;
        }
        return NatureProhibitionInMove(_cellToMove);
    }
    private void CollectToughItem(ToughResources _itemToCollect, int countOfResorces)
    {
        int _canTakeItems;
        if (_itemToCollect != null)
        {
            _canTakeItems = Mathf.Clamp(Mathf.FloorToInt(_weightCapacityRemaining / _itemToCollect.WeightOfOneItem), 0, countOfResorces);
            if (!Inventory.ContainsKey(_itemToCollect.Name)) { Inventory.Add(_itemToCollect.Name, _canTakeItems); }
            else if (Inventory.ContainsKey(_itemToCollect.Name)) { Inventory[_itemToCollect.Name] += _canTakeItems; }
            _weightCapacityRemaining -= _canTakeItems * _itemToCollect.WeightOfOneItem;
        }
    }

    private bool NatureProhibitionInMove(Vector2Int _cell) { return _hexGrid.hexCells[_cell.x, _cell.y].isWater || Mathf.Abs(_hexGrid.hexCells[_cell.x, _cell.y].height - _hexGrid.hexCells[LocalCoords.x, LocalCoords.y].height) > _maxHeightToStep ? false : true; }

    private void ItemReferenceReturner() { // т.к. рабочий, наступая на клетку вымещает собой collectableitem из списка всех предметов на сетке, то если он забрал не все и ушел, нам нужно вернуть collectableitem в список
        if (_itemToReturnReference != null && _placementManager.gridWithObjectsInformation[_itemToReturnReference.LocalCoords.x, _itemToReturnReference.LocalCoords.y] == null) { _itemToReturnReference.GoneAway(); _itemToReturnReference = null; } }


    private void UpdateMyTargetInDarknessCoords() { _targetsInDarkness.RemoveTarget(LocalCoords); _targetsInDarkness.AddTarget(_hexGrid.InLocalCoords(_highlighted.position)); }
  
    private void InitComponents() { _placementManager = FindObjectOfType<PlacementManager>(); _unitMovement = GetComponent<UnitMovement>(); _unitHealth = GetComponent<UnitHealth>();
        _unitActions = GetComponent<UnitActions>(); _thisWorkerUnit = GetComponent<WorkerUnit>(); _mouseSelection = FindObjectOfType<MouseSelection>();
        _highlightingController = FindObjectOfType<HighlightingController>(); _hexGrid = FindObjectOfType<HexGrid>(); _targetsInDarkness = FindObjectOfType<TargetsInDarkness>(); _buildingManager = FindObjectOfType<BuildingManager>(); }

    private new void Awake() {
        base.Awake();
        _turnManager = FindObjectOfType<TurnManager>();
        _weightCapacityRemaining = WeightCapacityMax;
        InitComponents();
    }

    private void Update() { RepairBuilding(); TransferSingleResourcesWindow();
        _highlightedFacility = _highlightingController.highlightedFirstFactionFacility; _highlighted = _mouseSelection.highlighted; } // нужны экшены смены хайлайтеда и хайлайтед фасилити
    
    private void OnEnable() { _unitMovement.MovedToCell += ItemReferenceReturner; _unitMovement.WantToMoveOnCell += EnterBuilding; _unitHealth.death += ItemReferenceReturner; _unitMovement.WantToMoveOnCell += GetToughResource;
        _unitMovement.WantToMoveOnCell += CollectItem; _unitMovement.MovedToCell += UpdateMyTargetInDarknessCoords; _turnManager.onTurnChanged += MaintenanceCosts; }
    private void OnDisable() { _unitMovement.MovedToCell -= ItemReferenceReturner; _unitMovement.WantToMoveOnCell -= EnterBuilding; _unitHealth.death -= ItemReferenceReturner; _unitMovement.WantToMoveOnCell -= GetToughResource;
        _unitMovement.WantToMoveOnCell -= CollectItem; _unitMovement.MovedToCell -= UpdateMyTargetInDarknessCoords; _turnManager.onTurnChanged -= MaintenanceCosts; }

    private void OnDestroy() {
        ItemReferenceReturner();
    }
}
