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
    [SerializeField] private FirstFactionProductionBuildingDescription Sawmill;
    [SerializeField] private FirstFactionProductionBuildingDescription Foundry;
    [SerializeField] private FirstFactionProductionBuildingDescription Barracks;

    private void BuildBuildingsUpdate() {
                if (_highlighted == null) { return; }
        var _highlightedInLocalCoords = new Vector2Int(_hexGrid.InLocalCoords(_highlighted.position).x, _hexGrid.InLocalCoords(_highlighted.position).y);
        if (_placementManager.gridWithObjectsInformation[_highlightedInLocalCoords.x, _highlightedInLocalCoords.y] == null) {
            if (Input.GetKeyDown(KeyCode.Alpha1) && !_teamsAdministratumsReferences.ContainsKey(CurrentTeamNumber)) {
                var _administratum = Instantiate(Administratum, _highlighted.parent.transform.position, Quaternion.identity);
                _administratum.TeamAffiliation = CurrentTeamNumber;
                _teamsAdministratumsReferences.Add(CurrentTeamNumber, _administratum);
                _placementManager.UpdateGrid(_highlightedInLocalCoords, _highlightedInLocalCoords, _administratum); }
            if (_teamsAdministratumsReferences.ContainsKey(CurrentTeamNumber) && IsAllyWorkerNearby(_highlightedInLocalCoords)) {
                var _currentAdministratum = _teamsAdministratumsReferences[CurrentTeamNumber];
                FirstFactionProductionBuildingDescription _buildingToBuild;
                if (Input.GetKeyDown(KeyCode.Alpha2)) { _buildingToBuild = Sawmill; }
                else if (Input.GetKeyDown(KeyCode.Alpha3)) { _buildingToBuild = Foundry; }
                else if (Input.GetKeyDown(KeyCode.Alpha4)) { _buildingToBuild = Barracks; }
                else return;
                if (_currentAdministratum.OverallLight >= _buildingToBuild.LightBuildingFoundationCost && _currentAdministratum.OverallOre >= _buildingToBuild.OreBuildingFoundationCost &&
                        _currentAdministratum.OverallWood >= _buildingToBuild.WoodBuildingFoundationCost && _currentAdministratum.OverallFood >= _buildingToBuild.FoodBuildingFoundationCost) {
                    var _building = Instantiate(_buildingToBuild, _highlighted.parent.transform.position, Quaternion.identity);
                    _placementManager.UpdateGrid(_highlightedInLocalCoords, _highlightedInLocalCoords, _building);
                    _building.TeamAffiliation = CurrentTeamNumber;
                    _building.Administratum = _teamsAdministratumsReferences[_building.TeamAffiliation];
                    _building.BuildingExpenses("Foundation"); } } } }

    private bool IsAllyWorkerNearby(Vector2Int _cell) {
        foreach (Vector2Int _neighbourCell in _hexGrid.Neighbours(_cell)) {
            ObjectOnGrid _neighbour = _placementManager.gridWithObjectsInformation[_neighbourCell.x, _neighbourCell.y];
            if (_neighbour != null) {
                WorkerUnit _worker = _neighbour.GetComponent<WorkerUnit>();
                if (_worker != null && _worker.TeamAffiliation == CurrentTeamNumber) return true; } } return false; }
    
    private void Update() {
        BuildBuildingsUpdate();}
}
