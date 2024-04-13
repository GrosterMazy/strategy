using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterUnit : UnitDescription
{
    private ObjectOnGrid[,] _gridWithObjects => FindObjectOfType<PlacementManager>().gridWithObjectsInformation;
    private UnitMovement _unitMovement => GetComponent<UnitMovement>();
    private UnitHealth _unitHealth => GetComponent<UnitHealth>();
    private CollectableItem _itemToReturnReference;

    private bool ItemToReturnReferenceUpdater(Vector2Int _coordsWithItem) { 
        var _objectOnMyWay = _gridWithObjects[_coordsWithItem.x, _coordsWithItem.y];
        if (_objectOnMyWay != null) { _itemToReturnReference = _objectOnMyWay.GetComponent<CollectableItem>(); } return true; }
    

    private void ItemReferenceReturner() { 
        if (_itemToReturnReference != null && _gridWithObjects[_itemToReturnReference.LocalCoords.x, _itemToReturnReference.LocalCoords.y] == null) { _itemToReturnReference.GoneAway(); } }
    

    private void OnEnable() { _unitMovement.WantToMoveOnCell += ItemToReturnReferenceUpdater; _unitMovement.MovedToCell += ItemReferenceReturner; _unitHealth.death += ItemReferenceReturner; }
    private void OnDisable() { _unitMovement.WantToMoveOnCell -= ItemToReturnReferenceUpdater; _unitMovement.MovedToCell -= ItemReferenceReturner; _unitHealth.death -= ItemReferenceReturner; }
}
