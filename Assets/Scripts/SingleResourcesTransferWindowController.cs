using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public sealed class SingleResourcesTransferWindowController : MonoBehaviour
{
    private GameObject Window;
    private SingleResourcesTransferWindow _transferWindowScript;

    private void OpenWindow(WorkerUnit _transferingUnit, FacilityDescription _targetFacility) { Window.SetActive(true); _transferWindowScript.ChangeTransferInformation(_transferingUnit, _targetFacility); }

    private void InitComponents() { _transferWindowScript = FindObjectOfType<SingleResourcesTransferWindow>(); Window = _transferWindowScript.gameObject; }

    private void Start() { InitComponents();  Window.SetActive(false); }

    private void OnEnable() { WorkerUnit.WantToOpenSingleTransferWindow += OpenWindow; }
    private void OnDisable() { WorkerUnit.WantToOpenSingleTransferWindow -= OpenWindow; }
}
