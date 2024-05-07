using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WorkerUnit : UnitDescription
{
    public float miningModifier;
    public Dictionary<string, float> ResourcesWeights = new Dictionary<string, float>();
    public Dictionary<string, int> Inventory = new Dictionary<string, int>();
    [NonSerialized] public float _weightCapacityRemaining;
    [SerializeField] private float WeightCapacityMax;
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();
    private UnitMovement _unitMovement => GetComponent<UnitMovement>();
    private UnitHealth _unitHealth => GetComponent<UnitHealth>();
    private UnitActions _unitActions => GetComponent<UnitActions>();
    private Transform _highlighted => FindObjectOfType<MouseSelection>().highlighted;
    private HexGrid _hexGrid => FindObjectOfType<HexGrid>();
    private CollectableItem _itemToCollect = null;
    private CollectableItem _itemToReturnReference = null;

    private bool CollectItem(Vector2Int _CellToReceiveItem) {
        int _canTakeItems;
        var _objectOnMyWay = _placementManager.gridWithObjectsInformation[_CellToReceiveItem.x, _CellToReceiveItem.y];
        if (_itemToCollect != null ) { _itemToReturnReference = _itemToCollect; }
        if (_objectOnMyWay != null) {_itemToCollect = _objectOnMyWay.GetComponent<CollectableItem>(); 
            if (_itemToCollect != null) { _canTakeItems = Mathf.Clamp(Mathf.FloorToInt(_weightCapacityRemaining / _itemToCollect.WeightOfOneItem), 0, _itemToCollect.NumberOfItems);
                if (!Inventory.ContainsKey(_itemToCollect.Name)) { Inventory.Add(_itemToCollect.Name, _canTakeItems); ResourcesWeights.Add(_itemToCollect.Name, 0); }
                else if (Inventory.ContainsKey(_itemToCollect.Name)) { Inventory[_itemToCollect.Name] += _canTakeItems; }
                _itemToCollect.Taken(_canTakeItems); _weightCapacityRemaining -= _canTakeItems * _itemToCollect.WeightOfOneItem; ResourcesWeights[_itemToCollect.Name] += _canTakeItems * _itemToCollect.WeightOfOneItem; } }
        if (_itemToCollect != null || _objectOnMyWay == null) return true;
        else return false;
    }

    private bool EnterBuilding(Vector2Int _cellToEnter) {
        FirstFactionProductionBuildingDescription _buildingOnMyWay = null;
        var _objectOnMyWay = _placementManager.gridWithObjectsInformation[_cellToEnter.x, _cellToEnter.y];
        if (_objectOnMyWay != null) {       
            _buildingOnMyWay = _objectOnMyWay.GetComponent<FirstFactionProductionBuildingDescription>();
            if (_buildingOnMyWay != null && !_buildingOnMyWay.WorkerOnSite && TeamAffiliation == _buildingOnMyWay.TeamAffiliation && _unitActions.remainingActionsCount > 0) {
                if (_buildingOnMyWay.ActionsToFinalizeBuilding == 0) { 
                    _buildingOnMyWay.WorkerOnSite = true; _placementManager.UpdateGrid(LocalCoords, LocalCoords, null); _buildingOnMyWay.WorkerInsideMe = gameObject; _buildingOnMyWay.OnEnteredWorker?.Invoke(); gameObject.SetActive(false); }
                else if (_buildingOnMyWay.Administratum.OverallLight >= _buildingOnMyWay.LightConstructionCost && _buildingOnMyWay.Administratum.OverallOre >= _buildingOnMyWay.OreConstructionCost &&
                        _buildingOnMyWay.Administratum.OverallWood >= _buildingOnMyWay.WoodConstructionCost && _buildingOnMyWay.Administratum.OverallFood >= _buildingOnMyWay.FoodConstructionCost) {
                    _buildingOnMyWay.ActionsToFinalizeBuilding -= 1; _buildingOnMyWay.BuildingExpenses("Construction"); _unitActions.remainingActionsCount -= 1; } } }
        if (_buildingOnMyWay == null) return true;
        return false; }

    private void RepairBuilding() { 
        if (Input.GetKeyDown(KeyCode.R) && IsSelected && _highlighted != null) {
            Vector2Int _highlightedInLocalCoords = new Vector2Int(_hexGrid.InLocalCoords(_highlighted.position).x, _hexGrid.InLocalCoords(_highlighted.position).y);
            var _smthOnCell = _placementManager.gridWithObjectsInformation[_highlightedInLocalCoords.x, _highlightedInLocalCoords.y];
            if (_smthOnCell != null) {
                FacilityDescription _buildingToRepair = _smthOnCell.GetComponent<FacilityDescription>();
                if (_buildingToRepair == null) { return; }
                FacilityHealth _healthOfBuildingToRepair = _buildingToRepair.GetComponent<FacilityHealth>();
                if (_healthOfBuildingToRepair.currentHealth < _buildingToRepair.MaxHealth && TeamAffiliation == _buildingToRepair.TeamAffiliation && _unitActions.remainingActionsCount > 0 && IsNeededBuildingNear(_buildingToRepair)) {
                    _unitActions.remainingActionsCount -= 1;
                    _healthOfBuildingToRepair.Repairment(); } } } }

    private bool IsNeededBuildingNear(FacilityDescription _buildingToCheck) {
        foreach (Vector2Int _cell in _hexGrid.Neighbours(transform.position)) {
            ObjectOnGrid _objectOnCell = _placementManager.gridWithObjectsInformation[_cell.x, _cell.y];
            if (_objectOnCell != null) {
                FacilityDescription _building = _objectOnCell.GetComponent<FacilityDescription>();
                if (_building != null && _building == _buildingToCheck) { return true; } } } return false; }

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
                _unitActions.remainingActionsCount -= 1;
            }
            return false;
        }
        return true;
    }
    private void CollectToughItem(ToughResources _itemToCollect, int countOfResorces)
    {
        int _canTakeItems;
        if (_itemToCollect != null)
        {
            _canTakeItems = Mathf.Clamp(Mathf.FloorToInt(_weightCapacityRemaining / _itemToCollect.WeightOfOneItem), 0, countOfResorces);
            if (!Inventory.ContainsKey(_itemToCollect.Name)) { Inventory.Add(_itemToCollect.Name, _canTakeItems); ResourcesWeights.Add(_itemToCollect.Name, 0); }
            else if (Inventory.ContainsKey(_itemToCollect.Name)) { Inventory[_itemToCollect.Name] += _canTakeItems; }
            _weightCapacityRemaining -= _canTakeItems * _itemToCollect.WeightOfOneItem; ResourcesWeights[_itemToCollect.Name] += _canTakeItems * _itemToCollect.WeightOfOneItem;
        }
    }

    private void ItemReferenceReturner() { // т.к. рабочий, наступая на клетку вымещает собой collectableitem из списка всех предметов на сетке, то если он забрал не все и ушел, нам нужно вернуть collectableitem в список
        if (_itemToReturnReference != null && _placementManager.gridWithObjectsInformation[_itemToReturnReference.LocalCoords.x, _itemToReturnReference.LocalCoords.y] == null) { _itemToReturnReference.GoneAway(); _itemToReturnReference = null; } }
  
    private void Start() { _weightCapacityRemaining = WeightCapacityMax; }

    private void Update() { RepairBuilding(); if (Inventory.ContainsKey("Wood")) { Debug.Log(Inventory["Wood"]); } }

    
    private void OnEnable() { _unitMovement.MovedToCell += ItemReferenceReturner; _unitMovement.WantToMoveOnCell += EnterBuilding; _unitHealth.death += ItemReferenceReturner; _unitMovement.WantToMoveOnCell += GetToughResource; _unitMovement.WantToMoveOnCell += CollectItem; }
    private void OnDisable() { _unitMovement.MovedToCell -= ItemReferenceReturner; _unitMovement.WantToMoveOnCell -= EnterBuilding; _unitHealth.death -= ItemReferenceReturner; _unitMovement.WantToMoveOnCell -= GetToughResource; _unitMovement.WantToMoveOnCell -= CollectItem; }
}
