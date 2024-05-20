using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedObjectInformationEnableController : MonoBehaviour
{
    private SelectionController _selectionController;
    private MouseSelection _mouseSelection;
    private GameObject _selectedObjectInformation;

    private void Awake()
    {
        InitComponentLinks();
        _selectedObjectInformation.SetActive(false);
    }

    private void OnEnable()
    {
        _selectionController.onSelectedInformationChanged += SelectedObjectInformationSetActive;
        EventBus.anyUnitSpendAction += OnAnyUnitSpendAction;
    }

    private void OnDisable()
    {
        _selectionController.onSelectedInformationChanged -= SelectedObjectInformationSetActive;
        EventBus.anyUnitSpendAction += OnAnyUnitSpendAction;
    }
    public void SelectedObjectInformationSetActive(Transform selected)
    {
        _selectedObjectInformation.SetActive(false);
        if (_selectionController.isAnyUnitSelected || _selectionController.isAnyFirstFactionFacilitySelected
            || _selectionController.isAnyCollectableItemSelected || _selectionController.isAnyToughResourceSelected)
        {
            _selectedObjectInformation.SetActive(true);
        }
    }

    private void OnAnyUnitSpendAction()
    {
        SelectedObjectInformationSetActive(transform);
    }

    private void InitComponentLinks()
    {
        _selectedObjectInformation = FindObjectOfType<SelectedObjectInformationDrawerConntroller>().gameObject;
        _selectionController = FindObjectOfType<SelectionController>();
        _mouseSelection = FindObjectOfType<MouseSelection>();
    }
}
