using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightingController : MonoBehaviour
{
    public UnitDescription highlightedUnit;
    public bool isAnyUnitHighlighted;
    private Transform _highlighted;
    private UnitDescription _highlightedBeforeUnit;
    private MouseSelection _mouseSelection => FindObjectOfType<MouseSelection>();
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();
    private HexGrid _hexGrid => FindObjectOfType<HexGrid>();
    private void OnEnable()
    {
        MouseSelection.onHighlightChanged += OnHighlightedChanged;
    }
    private void OnDisable()
    {
        MouseSelection.onHighlightChanged -= OnHighlightedChanged;
    }


    private void IsUnitHighlighted(Transform highlighted)
    {
        if (_highlightedBeforeUnit != null) _highlightedBeforeUnit.IsHighlighted = false;
        if (_placementManager.gridWithObjectsInformation[_hexGrid.InLocalCoords(highlighted.position).x, _hexGrid.InLocalCoords(highlighted.position).y] == null) // На подсвеченной клетке ничего нет
        {
            isAnyUnitHighlighted = false;
            highlightedUnit = null;
        }
        else 
        {
            highlightedUnit = _placementManager.gridWithObjectsInformation[_hexGrid.InLocalCoords(highlighted.position).x, _hexGrid.InLocalCoords(highlighted.position).y].GetComponent<UnitDescription>();
            if (highlightedUnit != null) // На подсвеченной клетке есть юнит
            {
                isAnyUnitHighlighted = true;
                highlightedUnit.IsHighlighted = true;
                _highlightedBeforeUnit = highlightedUnit;
            }
            else
            {
                isAnyUnitHighlighted = false;
            }
        }

    }
    private void OnHighlightedChanged(Transform highlighted)
    {
        if (highlighted == null)
        {
            isAnyUnitHighlighted = false;
            highlightedUnit = null;
            if (_highlightedBeforeUnit != null) _highlightedBeforeUnit.IsHighlighted = false;
            return;
        }
        IsUnitHighlighted(highlighted);
    }
}
