using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightingController : MonoBehaviour
{
    public UnitDescription highlightedUnit;
    public bool isAnyUnitHighlighted;
    public FirstFactionFacilities highlightedFirstFactionFacility;
    public bool isAnyFirstFactionFacilityHighlighted;
    private Transform _highlighted;
    private UnitDescription _highlightedBeforeUnit;
    private FirstFactionFacilities _highlightedBeforeFirstFactionFacility;
    private MouseSelection _mouseSelection;
    private PlacementManager _placementManager;
    private HexGrid _hexGrid;

    private void Awake()
    {
        InitComponentLinks();
    }

    private void OnEnable()
    {
        MouseSelection.onHighlightChanged += OnHighlightedChanged;
        EventBus.anyUnitDie += OnAnyUnitDeath;
    }
    private void OnDisable()
    {
        MouseSelection.onHighlightChanged -= OnHighlightedChanged;
        EventBus.anyUnitDie -= OnAnyUnitDeath;
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

    private void IsFirstFactionFacilityHighlighted(Transform highlighted)
    {
        if (_highlightedBeforeFirstFactionFacility != null) _highlightedBeforeFirstFactionFacility.IsHighlighted = false;
        if (_placementManager.gridWithObjectsInformation[_hexGrid.InLocalCoords(highlighted.position).x, _hexGrid.InLocalCoords(highlighted.position).y] == null) // На подсвеченной клетке ничего нет
        {
            isAnyFirstFactionFacilityHighlighted = false;
            highlightedFirstFactionFacility = null;
        }
        else
        {
            highlightedFirstFactionFacility = _placementManager.gridWithObjectsInformation[_hexGrid.InLocalCoords(highlighted.position).x, _hexGrid.InLocalCoords(highlighted.position).y].GetComponent<FirstFactionFacilities>();
            if (highlightedFirstFactionFacility != null) // На подсвеченной клетке есть юнит
            {
                isAnyFirstFactionFacilityHighlighted = true;
                highlightedFirstFactionFacility.IsHighlighted = true;
                _highlightedBeforeFirstFactionFacility = highlightedFirstFactionFacility;
            }
            else
            {
                isAnyFirstFactionFacilityHighlighted = false;
            }
        }
    }

    private void OnAnyUnitDeath()
    {
        OnHighlightedChanged(_mouseSelection.highlighted);
    }
    private void OnHighlightedChanged(Transform highlighted)
    {
        if (highlighted == null)
        {
            isAnyUnitHighlighted = false;
            highlightedUnit = null;
            if (_highlightedBeforeUnit != null) _highlightedBeforeUnit.IsHighlighted = false;

            isAnyFirstFactionFacilityHighlighted = false;
            highlightedFirstFactionFacility = null;
            if (_highlightedBeforeFirstFactionFacility != null) _highlightedBeforeFirstFactionFacility.IsHighlighted = false;

            return;
        }
        IsUnitHighlighted(highlighted);
        IsFirstFactionFacilityHighlighted(highlighted);
    }

    private void InitComponentLinks()
    {
        _mouseSelection = FindObjectOfType<MouseSelection>();
        _placementManager = FindObjectOfType<PlacementManager>();
        _hexGrid = FindObjectOfType<HexGrid>();
    }
}
