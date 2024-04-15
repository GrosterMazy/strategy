﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WorkerUnit : UnitDescription
{
    public Dictionary<string, int> Inventory = new Dictionary<string, int>();
    [SerializeField] private float WeightCapacityMax;
    private float _weightCapacityRemaining;
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
                if (!Inventory.ContainsKey(_itemToCollect.Name)) { Inventory.Add(_itemToCollect.Name, _canTakeItems); }
                else if (Inventory.ContainsKey(_itemToCollect.Name)) { Inventory[_itemToCollect.Name] += _canTakeItems; }
                _itemToCollect.Taken(_canTakeItems); _weightCapacityRemaining -= _canTakeItems * _itemToCollect.WeightOfOneItem; } } return true;}

    private bool EnterBuilding(Vector2Int _cellToEnter) {    
        Building _buildingOnMyWay = null;
        var _objectOnMyWay = _placementManager.gridWithObjectsInformation[_cellToEnter.x, _cellToEnter.y];
        if (_objectOnMyWay != null) {       
            _buildingOnMyWay = _objectOnMyWay.GetComponent<Building>();
            if (_buildingOnMyWay != null && !_buildingOnMyWay.WorkerOnSite && TeamAffiliation == _buildingOnMyWay.TeamAffiliation && _unitActions.remainingActionsCount > 0) {
                if (_buildingOnMyWay.ActionsToFinalizeBuilding == 0) { 
                _buildingOnMyWay.WorkerOnSite = true; _placementManager.UpdateGrid(LocalCoords, LocalCoords, null); _buildingOnMyWay.WorkerInsideMe = gameObject; gameObject.SetActive(false); }
                else { _buildingOnMyWay.ActionsToFinalizeBuilding -= 1; _unitActions.remainingActionsCount -= 1; } } }
        if (_buildingOnMyWay == null) return true;
        return false; }

    private void RepairBuilding() { 
        if (Input.GetKeyDown(KeyCode.R) && IsSelected && _highlighted != null) {
            Vector2Int _highlightedInLocalCoords = new Vector2Int(_hexGrid.InLocalCoords(_highlighted.position).x, _hexGrid.InLocalCoords(_highlighted.position).y);
            var _smthOnCell = _placementManager.gridWithObjectsInformation[_highlightedInLocalCoords.x, _highlightedInLocalCoords.y];
            if (_smthOnCell != null) {
                FacilityDescription _buildingToRepair = _smthOnCell.GetComponent<FacilityDescription>();
                if (_buildingToRepair != null && _buildingToRepair.CurrentHealth < _buildingToRepair.MaxHealth && TeamAffiliation == _buildingToRepair.TeamAffiliation && _unitActions.remainingActionsCount > 0 && IsNeededBuildingNear(_buildingToRepair)) {
                    _unitActions.remainingActionsCount -= 1;
                    _buildingToRepair.Repairment(); } } } }

    private bool IsNeededBuildingNear(FacilityDescription _buildingToCheck) {
        foreach (Vector2Int _cell in _hexGrid.Neighbours(transform.position)) {
            ObjectOnGrid _objectOnCell = _placementManager.gridWithObjectsInformation[_cell.x, _cell.y];
            if (_objectOnCell != null) {
                FacilityDescription _building = _objectOnCell.GetComponent<FacilityDescription>();
                if (_building != null && _building == _buildingToCheck) { return true; } } } return false; }

    private void ItemReferenceReturner() { // т.к. рабочий, наступая на клетку вымещает собой collectableitem из списка всех предметов на сетке, то если он забрал не все и ушел, нам нужно вернуть collectableitem в список
        if (_itemToReturnReference != null && _placementManager.gridWithObjectsInformation[_itemToReturnReference.LocalCoords.x, _itemToReturnReference.LocalCoords.y] == null) { _itemToReturnReference.GoneAway(); _itemToReturnReference = null; } }
  
    private void Start() { _weightCapacityRemaining = WeightCapacityMax; }

    private void Update() { RepairBuilding(); }

    private void OnEnable() { _unitMovement.WantToMoveOnCell += CollectItem; _unitMovement.MovedToCell += ItemReferenceReturner; _unitMovement.WantToMoveOnCell += EnterBuilding; _unitHealth.death += ItemReferenceReturner; }
    private void OnDisable() { _unitMovement.WantToMoveOnCell -= CollectItem; _unitMovement.MovedToCell -= ItemReferenceReturner; _unitMovement.WantToMoveOnCell -= EnterBuilding; _unitHealth.death -= ItemReferenceReturner; }
}
