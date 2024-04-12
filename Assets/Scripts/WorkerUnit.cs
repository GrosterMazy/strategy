using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WorkerUnit : UnitDescription
{
    public Dictionary<string, int> Inventory = new Dictionary<string, int>();
    [SerializeField] private float WeightCapacityMax;
    private float WeightCapacityCurrent;
    private ObjectOnGrid[,] _gridWithObjects => FindObjectOfType<PlacementManager>().gridWithObjectsInformation;
    private UnitMovement _unitMovement => GetComponent<UnitMovement>();
    private CollectableItem _itemToCollect = null;
    private CollectableItem _itemToReturnReference = null;

    private void CollectItem(Vector2Int _CellToReceiveItem)
    {
        int _canTakeItems;
        var _objectOnMyWay = _gridWithObjects[_CellToReceiveItem.x, _CellToReceiveItem.y];
        if (_itemToCollect != null ) { _itemToReturnReference = _itemToCollect; }
        if (_objectOnMyWay != null) {_itemToCollect = _objectOnMyWay.GetComponent<CollectableItem>(); 
            if (_itemToCollect != null) { _canTakeItems = Mathf.Clamp(Mathf.FloorToInt(WeightCapacityCurrent / _itemToCollect.WeightOfOneItem), 0, _itemToCollect.NumberOfItems);
                if (!Inventory.ContainsKey(_itemToCollect.Name)) { Inventory.Add(_itemToCollect.Name, _canTakeItems); }
                else if (Inventory.ContainsKey(_itemToCollect.Name)) { Inventory[_itemToCollect.Name] += _canTakeItems; }
                _itemToCollect.Taken(_canTakeItems); WeightCapacityCurrent -= _canTakeItems * _itemToCollect.WeightOfOneItem; } }
            }

    private void ItemReferenceReturner() { // т.к. рабочий, наступая на клетку вымещает собой collectableitem из списка всех предметов на сетке, то если он забрал не все и ушел, нам нужно вернуть collectableitem в список
        if (_itemToReturnReference != null && _gridWithObjects[_itemToReturnReference._coordsOnGrid.LocalCoords.x, _itemToReturnReference._coordsOnGrid.LocalCoords.y] == null) { _itemToReturnReference.GoneAway(); _itemToReturnReference = null; } }
  
    private void Start() { WeightCapacityCurrent = WeightCapacityMax; }

    private void OnEnable() { _unitMovement.WantToMoveOnCell += CollectItem; _unitMovement.MovedToCell += ItemReferenceReturner; }
    private void OnDisable() { _unitMovement.WantToMoveOnCell -= CollectItem; _unitMovement.MovedToCell -= ItemReferenceReturner; }
}
