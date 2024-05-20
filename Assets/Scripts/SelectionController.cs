using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SelectionController : MonoBehaviour
{
    public Action<Transform> onSelectedInformationChanged;
    public UnitDescription selectedUnit;
    public FirstFactionFacilities selectedFacility;
    public ToughResources selectedToughResource;
    public CollectableItem selectedCollectableItem;
    public bool isAnyUnitSelected;
    public bool isAnyFirstFactionFacilitySelected;
    public bool isAnyToughResourceSelected;
    public bool isAnyCollectableItemSelected;
    private UnitDescription _selectedBeforeUnit;
    private FirstFactionFacilities _selectedBeforeFacility;
    private MouseSelection _mouseSelection;
    private HexGrid _hexGrid;
    private PlacementManager _placementManager;

    private void Awake()
    {
        InitComponentLinks();
    }

    private void OnEnable()
    {
        MouseSelection.onSelectionChanged += OnSelectionChanged;
        EventBus.anyUnitDie += OnAnyUnitDeath;
    }
    private void OnDisable()
    {
        MouseSelection.onSelectionChanged -= OnSelectionChanged;
        EventBus.anyUnitDie -= OnAnyUnitDeath;
    }

    private void OnSelectionChanged(Transform selected)
    {
        if (selected == null)
        {
            isAnyUnitSelected = false;
            selectedUnit = null;
            if (_selectedBeforeUnit != null) _selectedBeforeUnit.IsSelected = false;

            isAnyFirstFactionFacilitySelected = false;
            selectedFacility = null;
            if (_selectedBeforeFacility != null) _selectedBeforeFacility.IsSelected = false;

            isAnyToughResourceSelected = false;
            selectedToughResource = null;

            onSelectedInformationChanged?.Invoke(selected);
            return;
        }
        IsUnitSelected(selected);
        IsFirstFactionFacilitySelected(selected);
        IsToughResourceSelected(selected);
        IsCollectableItemSelected(selected);
        onSelectedInformationChanged?.Invoke(selected);
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
                    isAnyUnitSelected = true;
                    if (_selectedBeforeUnit != null) _selectedBeforeUnit.IsSelected = false;
                    selectedUnit.IsSelected = true;
                    _selectedBeforeUnit = selectedUnit;
                    return;
                }
            }
        }
        isAnyUnitSelected = false;
        selectedUnit = null;
        if (_selectedBeforeUnit != null) _selectedBeforeUnit.IsSelected = false;
    }

    private void IsFirstFactionFacilitySelected(Transform selected)
    {
        if (selected != null)
        {
            ObjectOnGrid _selectedObject = _placementManager.gridWithObjectsInformation[_hexGrid.InLocalCoords(selected.position).x, _hexGrid.InLocalCoords(selected.position).y];

            if (_selectedObject != null) // Если на выделенной клетке стоит что-то
            {
                selectedFacility = _selectedObject.GetComponent<FirstFactionFacilities>();
                if (selectedFacility != null) // Если на выделенной клетке стоит здание первой фракции
                {
                    isAnyFirstFactionFacilitySelected = true;
                    if (_selectedBeforeFacility != null) _selectedBeforeFacility.IsSelected = false;
                    selectedFacility.IsSelected = true;
                    _selectedBeforeFacility = selectedFacility;
                    return;
                }
            }
        }
        isAnyFirstFactionFacilitySelected = false;
        selectedFacility = null;
        if (_selectedBeforeFacility != null) _selectedBeforeFacility.IsSelected = false;
    }
    private void IsToughResourceSelected(Transform selected)
    {
        if (selected != null)
        {
            ObjectOnGrid _selectedObject = _placementManager.gridWithObjectsInformation[_hexGrid.InLocalCoords(selected.position).x, _hexGrid.InLocalCoords(selected.position).y];

            if (_selectedObject != null) // Если на выделенной клетке стоит что-то
            {
                selectedToughResource = _selectedObject.GetComponent<ToughResources>();
                if (selectedToughResource != null) // Если на выделенной клетке стоит добываемый ресурс
                {
                    isAnyToughResourceSelected = true;
                    return;
                }
            }
        }
        isAnyToughResourceSelected = false;
        selectedToughResource = null;
    }
    private void IsCollectableItemSelected(Transform selected)
    {
        if (selected != null)
        {
            ObjectOnGrid _selectedObject = _placementManager.gridWithObjectsInformation[_hexGrid.InLocalCoords(selected.position).x, _hexGrid.InLocalCoords(selected.position).y];

            if (_selectedObject != null) // Если на выделенной клетке стоит что-то
            {
                selectedCollectableItem = _selectedObject.GetComponent<CollectableItem>();
                if (selectedCollectableItem != null) // Если на выделенной клетке стоит подбираемый ресурс
                {
                    isAnyCollectableItemSelected = true;
                    return;
                }
            }
        }
        isAnyCollectableItemSelected = false;
        selectedCollectableItem = null;
    }

    private void OnAnyUnitDeath()
    {
        OnSelectionChanged(_mouseSelection.selected);
    }


    private void InitComponentLinks()
    {
        _mouseSelection = FindObjectOfType<MouseSelection>();
        _hexGrid = FindObjectOfType<HexGrid>();
        _placementManager = FindObjectOfType<PlacementManager>();
    }
}
