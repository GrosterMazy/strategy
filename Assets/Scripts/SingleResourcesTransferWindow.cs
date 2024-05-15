using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SingleResourcesTransferWindow : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private TMP_InputField _amountInput;
    private WorkerUnit _transferingUnit;
    private FacilityDescription _targetFacility;

    public void ChangeTransferInformation(WorkerUnit _unit, FacilityDescription _facility) { _transferingUnit = _unit; _targetFacility = _facility; }

    public void ApplyTransfer() {
        if (_transferingUnit.Inventory.ContainsKey(_nameInput.text) && _targetFacility.Storage.ContainsKey(_nameInput.text) && _transferingUnit._unitActions.remainingActionsCount > 0) { 
            int _amountOfTransferingItems = Mathf.Clamp(int.Parse(_amountInput.text), 0, _transferingUnit.Inventory[_nameInput.text]);
            _transferingUnit.Inventory[_nameInput.text] -= _amountOfTransferingItems; 
            _transferingUnit._weightCapacityRemaining += ResourcesWeights.ResourcesWeightsPerItemTable[_nameInput.text] * _amountOfTransferingItems;
            _targetFacility.Storage[_nameInput.text] += _amountOfTransferingItems;
            _transferingUnit._unitActions.remainingActionsCount -= 1; }
        gameObject.SetActive(false); }
}
