using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FacilityHealth : Health
{ 
    public static Action anyFacilityDie;
    public float HealthPerRepairment;
    private FacilityDescription _facilityDescription;

    new void Start()
    {
        base.Start();
        currentHealth = _maxHealth;
    }
    public void Repairment() {
        currentHealth = Mathf.Clamp(currentHealth + HealthPerRepairment, 0, _maxHealth); }
    private void TransformationHealthOnTurnChanged()
    {
        _wasDamagedInThisTurn = false;
    }
    new private void IsDead()
    {
        if (currentHealth <= 0)
        {
            Death();
        }
    }
    private void Death()
    {
        _placementManager.gridWithObjectsInformation[_facilityDescription.LocalCoords.x, _facilityDescription.LocalCoords.y] = null;
        death?.Invoke();
        anyFacilityDie?.Invoke();
        Destroy(gameObject);
    }

    private void InitComponentLinks()
    {
        _facilityDescription = GetComponent<FacilityDescription>();
        _maxHealth = _facilityDescription.MaxHealth;
    }
}