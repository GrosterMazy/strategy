using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterUnit : UnitDescription
{
    private PlacementManager _placementManager;
    private ObjectOnGrid[,] _gridWithObjects;
    private UnitMovement _unitMovement;
    private UnitHealth _unitHealth;
    private CollectableItem _itemToReturnReference;

    private bool ItemToReturnReferenceUpdater(Vector2Int _coordsWithItem)
    {
        var _objectOnMyWay = _gridWithObjects[_coordsWithItem.x, _coordsWithItem.y];
        if (_objectOnMyWay != null)
        { 
            _itemToReturnReference = _objectOnMyWay.GetComponent<CollectableItem>();
            if (_itemToReturnReference != null) return true;
            else return false;
        }
        else return true;
    }

    private void InitComponents() { _unitMovement = GetComponent<UnitMovement>(); _unitHealth = GetComponent<UnitHealth>(); _placementManager = FindObjectOfType<PlacementManager>(); }

    private void ItemReferenceReturner() {  
        if (_itemToReturnReference != null && _gridWithObjects[_itemToReturnReference.LocalCoords.x, _itemToReturnReference.LocalCoords.y] == null) { _itemToReturnReference.GoneAway(); } }
    
    private void Start() { InitComponents(); }

    private void Update() { _gridWithObjects = _placementManager.gridWithObjectsInformation; } // нужен экшен изменения gridwoi для полной оптимизации

    private void OnEnable() { _unitMovement.WantToMoveOnCell += ItemToReturnReferenceUpdater; _unitMovement.MovedToCell += ItemReferenceReturner; _unitHealth.death += ItemReferenceReturner; }
    private void OnDisable() { _unitMovement.WantToMoveOnCell -= ItemToReturnReferenceUpdater; _unitMovement.MovedToCell -= ItemReferenceReturner; _unitHealth.death -= ItemReferenceReturner; }
}
