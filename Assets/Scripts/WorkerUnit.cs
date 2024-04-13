using System.Collections;
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
    private CollectableItem _itemToCollect = null;
    private CollectableItem _itemToReturnReference = null;

    private bool CollectItem(Vector2Int _CellToReceiveItem)
    {
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
                _buildingOnMyWay.WorkerOnSite = true; _placementManager.UpdateGrid(LocalCoords, LocalCoords, null); Destroy(gameObject); }
                else { _buildingOnMyWay.ActionsToFinalizeBuilding -= 1; _unitActions.remainingActionsCount -= 1; } } }
        if (_buildingOnMyWay == null) return true;
        return false; }          

    private void ItemReferenceReturner() { // т.к. рабочий, наступая на клетку вымещает собой collectableitem из списка всех предметов на сетке, то если он забрал не все и ушел, нам нужно вернуть collectableitem в список
        if (_itemToReturnReference != null && _placementManager.gridWithObjectsInformation[_itemToReturnReference.LocalCoords.x, _itemToReturnReference.LocalCoords.y] == null) { _itemToReturnReference.GoneAway(); _itemToReturnReference = null; } }
  
    private void Start() { _weightCapacityRemaining = WeightCapacityMax; }

    private void OnEnable() { _unitMovement.WantToMoveOnCell += CollectItem; _unitMovement.MovedToCell += ItemReferenceReturner; _unitMovement.WantToMoveOnCell += EnterBuilding; _unitHealth.death += ItemReferenceReturner; }
    private void OnDisable() { _unitMovement.WantToMoveOnCell -= CollectItem; _unitMovement.MovedToCell -= ItemReferenceReturner; _unitMovement.WantToMoveOnCell -= EnterBuilding; _unitHealth.death -= ItemReferenceReturner; }
}
