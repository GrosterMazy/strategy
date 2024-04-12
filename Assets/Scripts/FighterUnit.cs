using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterUnit : UnitDescription
{
    private ObjectOnGrid[,] _gridWithObjects => FindObjectOfType<PlacementManager>().gridWithObjectsInformation;
    private UnitMovement _unitMovement => GetComponent<UnitMovement>();
    private CollectableItem _itemToReturnReference;

    private void ItemToReturnReferenceUpdater(Vector2Int _coordsWithItem) { 
        var _objectOnMyWay = _gridWithObjects[_coordsWithItem.x, _coordsWithItem.y];
        if (_objectOnMyWay != null) { _itemToReturnReference = _objectOnMyWay.GetComponent<CollectableItem>(); } }
    

    private void ItemReferenceReturner() { 
        if (_itemToReturnReference != null && _gridWithObjects[_itemToReturnReference.LocalCoords.x, _itemToReturnReference.LocalCoords.y] == null) { _itemToReturnReference.GoneAway(); } }
    

    private void OnEnable() { _unitMovement.WantToMoveOnCell += ItemToReturnReferenceUpdater; _unitMovement.MovedToCell += ItemReferenceReturner; }
    private void OnDisable() { _unitMovement.WantToMoveOnCell -= ItemToReturnReferenceUpdater; _unitMovement.MovedToCell -= ItemReferenceReturner; }
}
