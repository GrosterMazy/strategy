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
    }

    private void OnDisable()
    {
        _selectionController.onSelectedInformationChanged -= SelectedObjectInformationSetActive;
    }
    public void SelectedObjectInformationSetActive(Transform selected)
    {
        _selectedObjectInformation.SetActive(false);
        if (_selectionController.isAnyUnitSelected || _selectionController.isAnyFirstFactionFacilitySelected)
        {
            _selectedObjectInformation.SetActive(true);
        }
    }

    private void InitComponentLinks()
    {
        _selectedObjectInformation = FindObjectOfType<SelectedObjectInformationDrawerConntroller>().gameObject;
        _selectionController = FindObjectOfType<SelectionController>();
        _mouseSelection = FindObjectOfType<MouseSelection>();
    }
}
