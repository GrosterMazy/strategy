using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WorkerUnit : UnitDescription
{
    public static Action<WorkerUnit, FacilityDescription> WantToOpenSingleTransferWindow;
    public int RangedLoadDistance = 1;
    public float miningModifier;
    public Dictionary<string, int> Inventory = new Dictionary<string, int>();
    [NonSerialized] public float _weightCapacityRemaining;
    [SerializeField] private float WeightCapacityMax;
    private PlacementManager _placementManager;
    private UnitMovement _unitMovement;
    private UnitHealth _unitHealth;
    [NonSerialized] public UnitActions _unitActions;
    private FacilityDescription _highlightedFacility;
    private Transform _highlighted;
    private HexGrid _hexGrid;
    private CollectableItem _itemToCollect = null;
    private CollectableItem _itemToReturnReference = null;
    private WorkerUnit _thisWorkerUnit;
    private MouseSelection _mouseSelection;
    private HighlightingController _highlightingController;

    private void TransferSingleResourcesWindow() {
        if (Input.GetKeyDown(KeyCode.G) && IsSelected && _highlightedFacility != null && _highlightedFacility.TeamAffiliation == TeamAffiliation && _hexGrid.Distance(LocalCoords, _highlightedFacility.LocalCoords) <= RangedLoadDistance && // Distance не работает так, как надо
            _highlightedFacility.ActionsToFinalizeBuilding == 0) { WantToOpenSingleTransferWindow?.Invoke(_thisWorkerUnit, _highlightedFacility); } }

    private bool CollectItem(Vector2Int _CellToReceiveItem) {
        int _canTakeItems;
        var _objectOnMyWay = _placementManager.gridWithObjectsInformation[_CellToReceiveItem.x, _CellToReceiveItem.y];
        if (_itemToCollect != null ) { _itemToReturnReference = _itemToCollect; }
        if (_objectOnMyWay != null) {_itemToCollect = _objectOnMyWay.GetComponent<CollectableItem>(); 
            if (_itemToCollect != null) { _canTakeItems = Mathf.Clamp(Mathf.FloorToInt(_weightCapacityRemaining / _itemToCollect.WeightOfOneItem), 0, _itemToCollect.NumberOfItems);
                if (!Inventory.ContainsKey(_itemToCollect.Name)) { Inventory.Add(_itemToCollect.Name, _canTakeItems); }
                else if (Inventory.ContainsKey(_itemToCollect.Name)) { Inventory[_itemToCollect.Name] += _canTakeItems; }
                _itemToCollect.Taken(_canTakeItems); _weightCapacityRemaining -= _canTakeItems * _itemToCollect.WeightOfOneItem; } }
        if (_itemToCollect != null || _objectOnMyWay == null) return true;
        else return false;
    }

    private bool EnterBuilding(Vector2Int _cellToEnter) {
        FacilityDescription _buildingOnMyWay = null;
        FirstFactionProductionBuildingDescription _productionBuildingOnMyWay = null;
        var _objectOnMyWay = _placementManager.gridWithObjectsInformation[_cellToEnter.x, _cellToEnter.y];
        if (_objectOnMyWay != null) {       
            _buildingOnMyWay = _objectOnMyWay.GetComponent<FacilityDescription>();
            if (_buildingOnMyWay != null && !_buildingOnMyWay.WorkerOnSite && TeamAffiliation == _buildingOnMyWay.TeamAffiliation && _unitActions.remainingActionsCount > 0) {
                _productionBuildingOnMyWay = _buildingOnMyWay.GetComponent<FirstFactionProductionBuildingDescription>();
                if (_buildingOnMyWay.ActionsToFinalizeBuilding == 0) { 
                    _buildingOnMyWay.WorkerOnSite = true; _placementManager.UpdateGrid(LocalCoords, LocalCoords, null); _buildingOnMyWay.WorkerInsideMe = gameObject; _buildingOnMyWay.OnEnteredWorker?.Invoke(); gameObject.SetActive(false); }
                else if (_productionBuildingOnMyWay != null) { 
                    if (_productionBuildingOnMyWay.Administratum.Storage["Light"] >= _productionBuildingOnMyWay.LightConstructionCost && _productionBuildingOnMyWay.Administratum.Storage["Ore"] >= _productionBuildingOnMyWay.OreConstructionCost &&
                        _productionBuildingOnMyWay.Administratum.Storage["Wood"] >= _productionBuildingOnMyWay.WoodConstructionCost && _productionBuildingOnMyWay.Administratum.Storage["Food"] >= _productionBuildingOnMyWay.FoodConstructionCost) {
                        _productionBuildingOnMyWay.ActionsToFinalizeBuilding -= 1; _productionBuildingOnMyWay.BuildingExpenses("Construction"); _unitActions.remainingActionsCount -= 1; } } } }
        if (_buildingOnMyWay == null) return true;
        return false; }

    private void RepairBuilding() { 
        if (Input.GetKeyDown(KeyCode.R) && IsSelected && _highlighted != null) {
            Vector2Int _highlightedInLocalCoords = new Vector2Int(_hexGrid.InLocalCoords(_highlighted.position).x, _hexGrid.InLocalCoords(_highlighted.position).y);
            var _smthOnCell = _placementManager.gridWithObjectsInformation[_highlightedInLocalCoords.x, _highlightedInLocalCoords.y];
            if (_smthOnCell != null) {
                FacilityDescription _buildingToRepair = _smthOnCell.GetComponent<FacilityDescription>();
                if (_buildingToRepair == null) { return; }
                FacilityHealth _healthOfBuildingToRepair = _buildingToRepair.GetComponent<FacilityHealth>();
                if (_healthOfBuildingToRepair.СurrentHealth < _buildingToRepair.MaxHealth && TeamAffiliation == _buildingToRepair.TeamAffiliation && _unitActions.remainingActionsCount > 0 && IsNeededBuildingNear(_buildingToRepair)) {
                    _unitActions.remainingActionsCount -= 1;
                    _healthOfBuildingToRepair.Repairment(); } } } }

    private bool IsNeededBuildingNear(FacilityDescription _buildingToCheck) {
        foreach (Vector2Int _cell in _hexGrid.Neighbours(transform.position)) {
            ObjectOnGrid _objectOnCell = _placementManager.gridWithObjectsInformation[_cell.x, _cell.y];
            if (_objectOnCell != null) {
                FacilityDescription _building = _objectOnCell.GetComponent<FacilityDescription>();
                if (_building != null && _building == _buildingToCheck) { return true; } } } return false; }

    private bool GetToughResource(Vector2Int _cellToMove)
    {
        ToughResources _toughResourcesOnGrid = null;
        var _objectOnMyWay = _placementManager.gridWithObjectsInformation[_cellToMove.x, _cellToMove.y];
        if (_objectOnMyWay != null)
        {
            _toughResourcesOnGrid = _objectOnMyWay.GetComponent<ToughResources>();
            if (_toughResourcesOnGrid != null && _unitActions.remainingActionsCount > 0)
            {
                CollectToughItem(_toughResourcesOnGrid, _toughResourcesOnGrid.ApplyDamage(miningModifier));
                _unitActions.remainingActionsCount -= 1;
            }
            return false;
        }
        return true;
    }
    private void CollectToughItem(ToughResources _itemToCollect, int countOfResorces)
    {
        int _canTakeItems;
        if (_itemToCollect != null)
        {
            _canTakeItems = Mathf.Clamp(Mathf.FloorToInt(_weightCapacityRemaining / _itemToCollect.WeightOfOneItem), 0, countOfResorces);
            if (!Inventory.ContainsKey(_itemToCollect.Name)) { Inventory.Add(_itemToCollect.Name, _canTakeItems); }
            else if (Inventory.ContainsKey(_itemToCollect.Name)) { Inventory[_itemToCollect.Name] += _canTakeItems; }
            _weightCapacityRemaining -= _canTakeItems * _itemToCollect.WeightOfOneItem;
        }
    }

    private void ItemReferenceReturner() { // т.к. рабочий, наступая на клетку вымещает собой collectableitem из списка всех предметов на сетке, то если он забрал не все и ушел, нам нужно вернуть collectableitem в список
        if (_itemToReturnReference != null && _placementManager.gridWithObjectsInformation[_itemToReturnReference.LocalCoords.x, _itemToReturnReference.LocalCoords.y] == null) { _itemToReturnReference.GoneAway(); _itemToReturnReference = null; } }
  
    private void InitComponents() { _placementManager = FindObjectOfType<PlacementManager>(); _unitMovement = GetComponent<UnitMovement>(); _unitHealth = GetComponent<UnitHealth>();
        _unitActions = GetComponent<UnitActions>(); _hexGrid = FindObjectOfType<HexGrid>(); _thisWorkerUnit = GetComponent<WorkerUnit>(); _mouseSelection = FindObjectOfType<MouseSelection>();
        _highlightingController = FindObjectOfType<HighlightingController>(); }

    private void Awake() { _weightCapacityRemaining = WeightCapacityMax; InitComponents(); }

    private void Update() { RepairBuilding(); TransferSingleResourcesWindow();
        _highlightedFacility = _highlightingController.highlightedFirstFactionFacility; _highlighted = _mouseSelection.highlighted; } // нужны экшены смены хайлайтеда и хайлайтед фасилити
    
    private void OnEnable() { _unitMovement.MovedToCell += ItemReferenceReturner; _unitMovement.WantToMoveOnCell += EnterBuilding; _unitHealth.death += ItemReferenceReturner; _unitMovement.WantToMoveOnCell += GetToughResource; _unitMovement.WantToMoveOnCell += CollectItem; }
    private void OnDisable() { _unitMovement.MovedToCell -= ItemReferenceReturner; _unitMovement.WantToMoveOnCell -= EnterBuilding; _unitHealth.death -= ItemReferenceReturner; _unitMovement.WantToMoveOnCell -= GetToughResource; _unitMovement.WantToMoveOnCell -= CollectItem; }
}
