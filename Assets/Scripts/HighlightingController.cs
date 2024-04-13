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
                if (highlightedUnit != null) highlightedUnit.IsHighlighted = false;
            }
        }

    }
    private void DistanceBetweenSelectedAndHighlighted(Transform highlighted)
    {
        if (_mouseSelection.selected == null) return;
        int deltaY = _hexGrid.InLocalCoords(highlighted.position).y - _hexGrid.InLocalCoords(_mouseSelection.selected.position).y;
        int deltaX = _hexGrid.InLocalCoords(highlighted.position).x - _hexGrid.InLocalCoords(_mouseSelection.selected.position).x;
        float absDeltaY = Mathf.Abs(deltaY);
        float absDeltaX = Mathf.Abs(deltaX);
        distanceBetweenSelectedAndHighlighted = (int)absDeltaX + (int)absDeltaY;
        Debug.Log(_hexGrid.InLocalCoords(_mouseSelection.selected.position).y);
        if (deltaX == 0 || deltaY == 0) return;
        if (_hexGrid.InLocalCoords(_mouseSelection.selected.position).y % 2 == 0) // Стоим на чётной строке
        {
            if (deltaX < 0) distanceBetweenSelectedAndHighlighted -= Mathf.CeilToInt(absDeltaY / 2);
            else distanceBetweenSelectedAndHighlighted -= Mathf.FloorToInt(absDeltaY / 2);
        }
        else
        {
            if (deltaX > 0) distanceBetweenSelectedAndHighlighted -= Mathf.CeilToInt(absDeltaY / 2);
            else distanceBetweenSelectedAndHighlighted -= Mathf.FloorToInt(absDeltaY / 2);
        }
        Debug.Log((absDeltaX, absDeltaY, Mathf.Clamp(Mathf.FloorToInt(Mathf.Max(absDeltaX, absDeltaY) / 2), 0, (int)Mathf.Min(absDeltaX, absDeltaY))));
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
