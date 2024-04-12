using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionController : MonoBehaviour
{
    private UnitMovement _selectedBeforeUnitMovement;
    private MouseSelection _mouseSelection => FindObjectOfType<MouseSelection>();
    private HexGrid _hexGrid => FindObjectOfType<HexGrid>();
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();

    private void OnEnable()
    {
        MouseSelection.onSelectionChanged += OnSelectionChanged;
    }
    private void OnDisable()
    {
        MouseSelection.onSelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(Transform selected)
    {
        if (selected != null)
        {
            ObjectOnGrid _selectedObject = _placementManager.gridWithObjectsInformation[_hexGrid.InLocalCoords(selected.position).x, _hexGrid.InLocalCoords(selected.position).y];

            if (_selectedObject != null) // Если на выделенной клетке стоит что-то
            {
                UnitDescription _selectedUnit = _selectedObject.GetComponent<UnitDescription>();
                if (_selectedUnit != null) // Если на выделенной клетке стоит юнит
                {
                    if (_selectedBeforeUnitMovement != null) _selectedBeforeUnitMovement.canMove = false;
                    UnitMovement _selectedUnitMovement = _selectedUnit.GetComponent<UnitMovement>();
                    _selectedUnitMovement.canMove = true;
                    _selectedBeforeUnitMovement = _selectedUnitMovement;
                    return;
                }
            }
        }
        if (_selectedBeforeUnitMovement != null) _selectedBeforeUnitMovement.canMove = false;

    }
}
