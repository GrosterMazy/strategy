using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Barracks : FirstFactionProductionBuildingDescription
{
    public Material Basic;
    public Material Ready;
    public Material InProgress;
    public Material Reject;
    private List<UnitDescription> _trainQueue = new List<UnitDescription>();
    private List<UnitDescription> _readyUnits = new List<UnitDescription>();
    private UnitDescription _currentUnit;
    private int _currentUnitRemainingTrainingTurns;
    private MeshRenderer _meshRenderer;
    private Color _neededColor;

    public void AddToQueue(UnitDescription unit) { if (Administratum.Storage["Light"] >= unit.LightCost && Administratum.Storage["Steel"] >= unit.SteelCost && Administratum.Storage["Wood"] >= unit.WoodCost && Administratum.Storage["Food"] >= unit.FoodCost) {
                                                    _trainQueue.Add(unit); Administratum.WasteResources(unit.LightCost, unit.SteelCost, unit.WoodCost, unit.FoodCost); }
                                                    else RejectSignal(); }

    private void Train() { if (_currentUnit == null && _trainQueue.Count == 0 || !WorkerOnSite) return;
        if (_currentUnit == null && _trainQueue.Count > 0) { _currentUnit = _trainQueue[0]; _trainQueue.RemoveAt(0); _currentUnitRemainingTrainingTurns = _currentUnit.TurnsToTrain; }
        else if (_currentUnit != null) { _currentUnitRemainingTrainingTurns -= 1;
            if (_currentUnitRemainingTrainingTurns == 0) { _readyUnits.Add(_currentUnit); _currentUnit = null; } } }

    private void ThrowReadyUnit() { if (_highlighted == null) return;
            Vector2Int highlightedPos = new Vector2Int(_hexGrid.InLocalCoords(_highlighted.position).x, _hexGrid.InLocalCoords(_highlighted.position).y);
            if (Input.GetMouseButtonDown(1) && IsSelected && _placementManager.gridWithObjectsInformation[highlightedPos.x, highlightedPos.y] == null &&
                !_hexGrid.hexCells[highlightedPos.x, highlightedPos.y].isWater && IsSelectedDestinationNearby(highlightedPos) && _readyUnits.Count > 0 ) { UnitDescription unit = Instantiate(_readyUnits[0], _highlighted.parent.position, Quaternion.identity);
            _readyUnits.RemoveAt(0); unit.LocalCoords = highlightedPos; _placementManager.UpdateGrid(highlightedPos, highlightedPos, unit); unit.TeamAffiliation = TeamAffiliation; } }

    private void SignalManager() { bool canChange = _neededColor != Reject.color;
        if (_readyUnits.Count > 0 && canChange) _neededColor = Ready.color;
        else if (_currentUnit != null && canChange) _neededColor = InProgress.color;
        else if (canChange) _neededColor = Basic.color;
            _meshRenderer.material.color = _neededColor; }

    private void RejectSignal() { _neededColor = Reject.color; Invoke("ReturnMaterial", 0.6f); }

    private void ReturnMaterial() => _neededColor = Basic.color;

    private void Awake() { _meshRenderer = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>(); }

    private new void Update() { base.Update(); ThrowReadyUnit(); SignalManager(); }

    private void OnEnable() { TurnManager.onTurnChanged += Train; }
    private void OnDisable() { TurnManager.onTurnChanged -= Train; }
}
