using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionBuildingFabrication : MonoBehaviour
{
    [SerializeField] private string RawMaterialName;
    [SerializeField] private string ProcessedMaterialName;
    [SerializeField] private int RawMaterialProcessingPerTurn;
    [SerializeField] private int ProcessedMaterialProductionPerTurn;
    private float _weightOfOneProcessedMaterial;
    private FacilityDescription _myFacility;
    private WorkerUnit _myWorkingWorker;

    public void ApplyResource(string _resource, int _amount) { if (_resource != RawMaterialName) { return; }
        _myFacility.Storage[_resource] += _amount; }

    private void UnloadWorker() { 
        Dictionary<string, int> _inventoryToUnload = _myWorkingWorker.Inventory;
        if (_inventoryToUnload.ContainsKey(RawMaterialName)) { 
            _myFacility.Storage[RawMaterialName] += _inventoryToUnload[RawMaterialName];
            _myWorkingWorker._weightCapacityRemaining += ResourcesWeights.ResourcesWeightsPerItemTable[RawMaterialName] * _inventoryToUnload[RawMaterialName];
            _inventoryToUnload[RawMaterialName] = 0; } } 

    private void Production() {
        if (_myFacility.WorkerOnSite && _myFacility.Storage[RawMaterialName] >= RawMaterialProcessingPerTurn) {
            _myFacility.Storage[RawMaterialName] -= RawMaterialProcessingPerTurn;
            _myFacility.Storage[ProcessedMaterialName] += ProcessedMaterialProductionPerTurn; } }

    private void LoadWorker() {
        if (!_myFacility.WorkerOnSite) { return; }
        int _canLoadItems = Mathf.Clamp(Mathf.FloorToInt(_myWorkingWorker._weightCapacityRemaining / _weightOfOneProcessedMaterial), 0, _myFacility.Storage[ProcessedMaterialName]);
        Dictionary<string, int> _workerInventory = _myWorkingWorker.Inventory;
        if (!_workerInventory.ContainsKey(ProcessedMaterialName)) { _workerInventory.Add(ProcessedMaterialName, _canLoadItems); }
        else if (_workerInventory.ContainsKey(ProcessedMaterialName)) { _workerInventory[ProcessedMaterialName] += _canLoadItems; }
        _myFacility.Storage[ProcessedMaterialName] -= _canLoadItems; _myWorkingWorker._weightCapacityRemaining -= _canLoadItems * _weightOfOneProcessedMaterial; }

    private void MyWorkerLinkChange() { _myWorkingWorker = _myFacility.WorkerInsideMe.GetComponent<WorkerUnit>(); }

    private void InitComponents() { _myFacility = GetComponent<FacilityDescription>();
        _weightOfOneProcessedMaterial = ResourcesWeights.ResourcesWeightsPerItemTable[ProcessedMaterialName]; }

    private void Awake() { InitComponents();
        _myFacility.Storage.Add(RawMaterialName, 0); _myFacility.Storage.Add(ProcessedMaterialName, 0); }

    private void OnEnable() { _myFacility.OnEnteredWorker += MyWorkerLinkChange; _myFacility.OnEnteredWorker += UnloadWorker; TurnManager.onTurnChanged += Production; TurnManager.onTurnChanged += LoadWorker; }
    private void OnDisable() { _myFacility.OnEnteredWorker -= MyWorkerLinkChange; _myFacility.OnEnteredWorker -= UnloadWorker; TurnManager.onTurnChanged -= Production; TurnManager.onTurnChanged -= LoadWorker; }
}
