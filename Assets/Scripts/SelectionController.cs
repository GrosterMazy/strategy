using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionController : MonoBehaviour
{
    public UnitDescription selectedUnit;
    public bool isAnyUnitSelected;
    private UnitDescription _selectedBeforeUnit;
    private MouseSelection _mouseSelection => FindObjectOfType<MouseSelection>();
    private HexGrid _hexGrid => FindObjectOfType<HexGrid>();
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();

    private void OnEnable()
    {
        MouseSelection.onSelectionChanged += OnSelectionChanged;
        UnitHealth.anyUnitDie += OnAnyUnitDeath;
    }
    private void OnDisable()
    {
        MouseSelection.onSelectionChanged -= OnSelectionChanged;
        UnitHealth.anyUnitDie -= OnAnyUnitDeath;
    }

    private void OnSelectionChanged(Transform selected)
    {
        if (selected == null)
        {
            isAnyUnitSelected = false;
            selectedUnit = null;
            if (_selectedBeforeUnit != null) _selectedBeforeUnit.IsSelected = false;
            return; 
        }
        IsUnitSelected(selected);
    }
    private void IsUnitSelected(Transform selected)
    {
        if (selected != null)
        {
            ObjectOnGrid _selectedObject = _placementManager.gridWithObjectsInformation[_hexGrid.InLocalCoords(selected.position).x, _hexGrid.InLocalCoords(selected.position).y];

            if (_selectedObject != null) // Если на выделенной клетке стоит что-то
            {
                selectedUnit = _selectedObject.GetComponent<UnitDescription>();
                if (selectedUnit != null) // Если на выделенной клетке стоит юнит
                {
                    if (_selectedBeforeUnit != null) _selectedBeforeUnit.IsSelected = false;
                    selectedUnit.IsSelected = true;
                    _selectedBeforeUnit = selectedUnit;
                    return;
                }
            }
        }
        if (_selectedBeforeUnit != null) _selectedBeforeUnit.IsSelected = false;
    }
    private void OnAnyUnitDeath()
    {
        OnSelectionChanged(_mouseSelection.selected);
    }
}
