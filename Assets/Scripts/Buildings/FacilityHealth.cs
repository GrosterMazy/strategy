using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FacilityHealth : Health
{ 
    public static Action anyFacilityDie;
    public float HealthPerRepairment;
    private FacilityDescription _facilityDescription;

    new void Awake()
    {
        base.Awake();
    }
    private void OnEnable()
    {
        _turnManager.onTurnChanged += TransformationHealthOnTurnChanged;
    }
    private void OnDisable()
    {
        _turnManager.onTurnChanged -= TransformationHealthOnTurnChanged;
    }
    new void Start()
    {
        base.Start();
        InitComponentLinks();
        currentHealth = maxHealth;
    }
    public void Repairment() {
        currentHealth = Mathf.Clamp(currentHealth + HealthPerRepairment, 0, maxHealth); }
    private void TransformationHealthOnTurnChanged()
    {
        _wasDamagedInThisTurn = false;
    }
    override protected void IsDead()
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
        maxHealth = _facilityDescription.MaxHealth;
        damageReductionPercent = _facilityDescription.DamageReductionPercent;
    }
}