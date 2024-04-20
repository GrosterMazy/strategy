using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionBuildingFabrication : MonoBehaviour
{
    public Dictionary<string, int> Storage = new Dictionary<string, int>();
    [SerializeField] private string RawMaterialName;
    [SerializeField] private string ProcessedMaterialName;
    [SerializeField] private int RawMaterialProcessingPerTurn;
    [SerializeField] private int ProcessedMaterialProductionPerTurn;
    [SerializeField] private float WeightOfOneProcessedMaterial;
    private FacilityDescription _myFacility => GetComponent<FacilityDescription>();
    private WorkerUnit _myWorkingWorker => _myFacility.WorkerInsideMe.GetComponent<WorkerUnit>();

    private void UnloadWorker() {
        Dictionary<string, int> _inventoryToUnload = _myWorkingWorker.Inventory;
        if (_inventoryToUnload.ContainsKey(RawMaterialName)) { 
            Storage[RawMaterialName] += _inventoryToUnload[RawMaterialName];
            _inventoryToUnload[RawMaterialName] = 0;
            _myWorkingWorker._weightCapacityRemaining += _myWorkingWorker.ResourcesWeights[RawMaterialName];
            _myWorkingWorker.ResourcesWeights[RawMaterialName] = 0; } } 

    private void Production() {
        if (_myFacility.WorkerOnSite && Storage[RawMaterialName] >= RawMaterialProcessingPerTurn) {
            Storage[RawMaterialName] -= RawMaterialProcessingPerTurn;
            Storage[ProcessedMaterialName] += ProcessedMaterialProductionPerTurn; } }

    private void LoadWorker() {
        if (!_myFacility.WorkerOnSite) { return; }
        int _canLoadItems = Mathf.Clamp(Mathf.FloorToInt(_myWorkingWorker._weightCapacityRemaining / WeightOfOneProcessedMaterial), 0, Storage[ProcessedMaterialName]);
        Dictionary<string, int> _workerInventory = _myWorkingWorker.Inventory;
        if (!_workerInventory.ContainsKey(ProcessedMaterialName)) { _workerInventory.Add(ProcessedMaterialName, _canLoadItems); _myWorkingWorker.ResourcesWeights.Add(ProcessedMaterialName, 0); }
        else if (_workerInventory.ContainsKey(ProcessedMaterialName)) { _workerInventory[ProcessedMaterialName] += _canLoadItems; }
        Storage[ProcessedMaterialName] -= _canLoadItems; _myWorkingWorker._weightCapacityRemaining -= _canLoadItems * WeightOfOneProcessedMaterial; _myWorkingWorker.ResourcesWeights[ProcessedMaterialName] += _canLoadItems * WeightOfOneProcessedMaterial; }

    private void Start() {
        Storage.Add(RawMaterialName, 0); Storage.Add(ProcessedMaterialName, 0); }

    private void OnEnable() { _myFacility.OnEnteredWorker += UnloadWorker; TurnManager.onTurnChanged += Production; TurnManager.onTurnChanged += LoadWorker; }
    private void OnDisable() { _myFacility.OnEnteredWorker -= UnloadWorker; TurnManager.onTurnChanged -= Production; TurnManager.onTurnChanged -= LoadWorker; }
}
