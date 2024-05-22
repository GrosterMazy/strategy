using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BarracksController : MonoBehaviour
{
    private Dictionary<string, UnitDescription> _unitsLinks = new Dictionary<string, UnitDescription>();
    [SerializeField] private List<UnitDescription> _units;
    private GameObject _window;
    private SelectionController _selectionController;
    private TurnManager _turnManager;
    private Barracks _targetBarracks;

    public void Order(string name) { _targetBarracks.AddToQueue(_unitsLinks[name]); }

    private void ButtonsActivate() { GameObject bg = _window.transform.GetChild(0).gameObject;
                                    for (int _ = 0; _ < _units.Count; _++) { GameObject button = bg.transform.GetChild(_).gameObject; 
                                                                            button.SetActive(true); button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = _units[_].name;
                                                                            _unitsLinks.Add(_units[_].name, _units[_]); }
                                                                            _window.SetActive(false); }

    private void OpenWindow(Transform pos) { FirstFactionFacilities facility = _selectionController.selectedFacility; 
        if (facility != null) { Barracks barracks = facility.GetComponent<Barracks>();
            if (barracks != null) { if (barracks.TeamAffiliation == _turnManager.currentTeam && barracks.ActionsToFinalizeBuilding == 0) _window.SetActive(true); _targetBarracks = barracks; }
            else _window.SetActive(false); } 
        else _window.SetActive(false); }

    private void InitComponentsLinks() { _window = FindObjectOfType<BarracksWindow>().gameObject; _selectionController = FindObjectOfType<SelectionController>(); _turnManager = FindObjectOfType<TurnManager>(); }

    private void Start() { InitComponentsLinks(); ButtonsActivate(); }

    private void OnEnable() { MouseSelection.onSelectionChanged += OpenWindow; }
    private void OnDisable() {  MouseSelection.onSelectionChanged -= OpenWindow; }
}
