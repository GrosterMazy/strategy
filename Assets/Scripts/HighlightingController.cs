using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightingController : MonoBehaviour
{
    public UnitDescription highlightedUnit;
    public int distanceBetweenSelectedAndHighlighted;
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
        if (_placementManager.gridWithObjectsInformation[_hexGrid.InLocalCoords(highlighted.position).x, _hexGrid.InLocalCoords(highlighted.position).y] == null) // На выделенной клетке ничего нет
        {
            isAnyUnitHighlighted = false;
            highlightedUnit = null;
        }
        else 
        {
            highlightedUnit = _placementManager.gridWithObjectsInformation[_hexGrid.InLocalCoords(highlighted.position).x, _hexGrid.InLocalCoords(highlighted.position).y].GetComponent<UnitDescription>();
            if (highlightedUnit != null) // На выделенной клетке есть юнит
            {
                isAnyUnitHighlighted = true;
                highlightedUnit.IsHighlighted = true;
                _highlightedBeforeUnit = highlightedUnit;
            }
            else
            {
                isAnyUnitHighlighted = false;
                highlightedUnit.IsHighlighted = false;
            }
        }

    }
    private void DistanceBetweenSelectedAndHighlighted(Transform highlighted)
    {
        if (_mouseSelection.selected == null) return;
        distanceBetweenSelectedAndHighlighted =
            Mathf.Abs(_hexGrid.InLocalCoords(_mouseSelection.selected.position).x - _hexGrid.InLocalCoords(highlighted.position).x)
            + Mathf.Abs(_hexGrid.InLocalCoords(_mouseSelection.selected.position).y - _hexGrid.InLocalCoords(highlighted.position).y);
    }
    private void OnHighlightedChanged(Transform highlighted)
    {
        if (highlighted == null)
        {
            isAnyUnitHighlighted = false;
            highlightedUnit = null;
            if (_highlightedBeforeUnit != null) _highlightedBeforeUnit.IsHighlighted = false;
            distanceBetweenSelectedAndHighlighted = -1;
            return;
        }
        IsUnitHighlighted(highlighted);
        DistanceBetweenSelectedAndHighlighted(highlighted);
    }
}
