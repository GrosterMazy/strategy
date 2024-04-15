using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuildingManager : MonoBehaviour
{
    private int CurrentTeamNumber => FindObjectOfType<TurnManager>().currentTeam;
    private Transform _highlighted => FindObjectOfType<MouseSelection>().highlighted;
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();
    private HexGrid _hexGrid => FindObjectOfType<HexGrid>();
    private Dictionary<int, Administratum> _teamsAdministratumsReferences = new Dictionary<int, Administratum>();
    [SerializeField] private Administratum Administratum;
    [SerializeField] private Building Sawmill;
    [SerializeField] private Building Foundry;
    [SerializeField] private Building Barracks;

    private void BuildBuildingsUpdate() {
                if (_highlighted == null) { return; }
        var _highlightedInLocalCoords = new Vector2Int(_hexGrid.InLocalCoords(_highlighted.position).x, _hexGrid.InLocalCoords(_highlighted.position).y);
        if (_placementManager.gridWithObjectsInformation[_highlightedInLocalCoords.x, _highlightedInLocalCoords.y] == null) {
            if (Input.GetKeyDown(KeyCode.Alpha1) && !_teamsAdministratumsReferences.ContainsKey(CurrentTeamNumber)) {
                var _administratum = Instantiate(Administratum, _highlighted.position, Quaternion.identity);
                _administratum.TeamAffiliation = CurrentTeamNumber;
                _teamsAdministratumsReferences.Add(CurrentTeamNumber, _administratum);
                _placementManager.UpdateGrid(_highlightedInLocalCoords, _highlightedInLocalCoords, _administratum); }
            if (_teamsAdministratumsReferences.ContainsKey(CurrentTeamNumber) && IsAllyWorkerNearby(_highlightedInLocalCoords)) {
                var _currentAdministratum = _teamsAdministratumsReferences[CurrentTeamNumber];
                Building _buildingToBuild;
                if (Input.GetKeyDown(KeyCode.Alpha2)) { _buildingToBuild = Sawmill; }
                else if (Input.GetKeyDown(KeyCode.Alpha3)) { _buildingToBuild = Foundry; }
                else if (Input.GetKeyDown(KeyCode.Alpha4)) { _buildingToBuild = Barracks; }
                else return;
                if (_currentAdministratum.OverallLight >= _buildingToBuild.LightBuildingCost && _currentAdministratum.OverallOre >= _buildingToBuild.OreBuildingCost &&
                        _currentAdministratum.OverallWood >= _buildingToBuild.WoodBuildingCost && _currentAdministratum.OverallFood >= _buildingToBuild.FoodBuildingCost) {
                    var _building = Instantiate(_buildingToBuild, _highlighted.position, Quaternion.identity);
                    _placementManager.UpdateGrid(_highlightedInLocalCoords, _highlightedInLocalCoords, _building);
                    _building.TeamAffiliation = CurrentTeamNumber;
                    _building.Administratum = _teamsAdministratumsReferences[_building.TeamAffiliation];
                    _building.Administratum.WasteResources(_building.LightBuildingCost, _building.OreBuildingCost, _building.WoodBuildingCost, _building.FoodBuildingCost); } } } }

    private bool IsAllyWorkerNearby(Vector2Int _cell) {
        foreach (Vector2Int _neighbourCell in _hexGrid.Neighbours(_cell)) {
            ObjectOnGrid _neighbour = _placementManager.gridWithObjectsInformation[_neighbourCell.x, _neighbourCell.y];
            if (_neighbour != null) {
                WorkerUnit _worker = _neighbour.GetComponent<WorkerUnit>();
                if (_worker != null && _worker.TeamAffiliation == CurrentTeamNumber) return true; } } return false; }
    
    private void Update() {
        BuildBuildingsUpdate();}
}
