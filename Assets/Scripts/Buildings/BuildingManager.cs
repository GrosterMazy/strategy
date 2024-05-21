using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuildingManager : MonoBehaviour
{
    private TurnManager _turnManager;
    private MouseSelection _mouseSelection;
    private int CurrentTeamNumber;
    private Transform _highlighted;
    private PlacementManager _placementManager;
    private HexGrid _hexGrid;
    public Dictionary<int, Administratum> TeamsAdministratumsReferences = new Dictionary<int, Administratum>();
    [SerializeField] private Administratum Administratum;
    [SerializeField] private FirstFactionProductionBuildingDescription Sawmill;
    [SerializeField] private FirstFactionProductionBuildingDescription Foundry;
    [SerializeField] private FirstFactionProductionBuildingDescription Barracks;
    [SerializeField] private FirstFactionProductionBuildingDescription Farm;

    private void BuildBuildingsUpdate() {
                if (_highlighted == null) { return; }
        var _highlightedInLocalCoords = new Vector2Int(_hexGrid.InLocalCoords(_highlighted.position).x, _hexGrid.InLocalCoords(_highlighted.position).y);
        if (_placementManager.gridWithObjectsInformation[_highlightedInLocalCoords.x, _highlightedInLocalCoords.y] == null) {
            if (Input.GetKeyDown(KeyCode.Alpha1) && !TeamsAdministratumsReferences.ContainsKey(CurrentTeamNumber)) {
                var _administratum = Instantiate(Administratum, _highlighted.parent.transform.position, Quaternion.identity);
                _administratum.TeamAffiliation = CurrentTeamNumber;
                TeamsAdministratumsReferences.Add(CurrentTeamNumber, _administratum);
                _placementManager.UpdateGrid(_highlightedInLocalCoords, _highlightedInLocalCoords, _administratum); }
            if (TeamsAdministratumsReferences.ContainsKey(CurrentTeamNumber) && IsAllyWorkerNearby(_highlightedInLocalCoords)) {
                var _currentAdministratum = TeamsAdministratumsReferences[CurrentTeamNumber];
                FirstFactionProductionBuildingDescription _buildingToBuild;
                if (Input.GetKeyDown(KeyCode.Alpha2)) { _buildingToBuild = Sawmill; }
                else if (Input.GetKeyDown(KeyCode.Alpha3)) { _buildingToBuild = Foundry; }
                else if (Input.GetKeyDown(KeyCode.Alpha4)) { _buildingToBuild = Barracks; }
                else if (Input.GetKeyDown(KeyCode.Alpha5)) { _buildingToBuild = Farm; }
                else return;
                if (_currentAdministratum.Storage["Light"] >= _buildingToBuild.LightBuildingFoundationCost && _currentAdministratum.Storage["Steel"] >= _buildingToBuild.SteelBuildingFoundationCost &&
                        _currentAdministratum.Storage["Wood"] >= _buildingToBuild.WoodBuildingFoundationCost && _currentAdministratum.Storage["Food"] >= _buildingToBuild.FoodBuildingFoundationCost) {
                    var _building = Instantiate(_buildingToBuild, _highlighted.parent.transform.position, Quaternion.identity);
                    _placementManager.UpdateGrid(_highlightedInLocalCoords, _highlightedInLocalCoords, _building);
                    _building.TeamAffiliation = CurrentTeamNumber;
                    _building.Administratum = TeamsAdministratumsReferences[_building.TeamAffiliation];
                    _building.BuildingExpenses("Foundation"); } } } }

    private bool IsAllyWorkerNearby(Vector2Int _cell) {
        foreach (Vector2Int _neighbourCell in _hexGrid.Neighbours(_cell)) {
            ObjectOnGrid _neighbour = _placementManager.gridWithObjectsInformation[_neighbourCell.x, _neighbourCell.y];
            if (_neighbour != null) {
                WorkerUnit _worker = _neighbour.GetComponent<WorkerUnit>();
                if (_worker != null && _worker.TeamAffiliation == CurrentTeamNumber) return true; } } return false; }

    private void InitComponents() {_placementManager = FindObjectOfType<PlacementManager>(); _hexGrid = FindObjectOfType<HexGrid>(); _turnManager = FindObjectOfType<TurnManager>(); _mouseSelection = FindObjectOfType<MouseSelection>(); }
    
    private void Start() { InitComponents(); }

    private void Update() { _highlighted = _mouseSelection.highlighted; // нужен экшен смены хайлайтеда
        CurrentTeamNumber = _turnManager.currentTeam; // нужен экшен смены команды (для полной оптимизации, но и так в целом норм)
        BuildBuildingsUpdate();}
}
