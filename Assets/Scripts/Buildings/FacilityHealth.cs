using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FacilityHealth : MonoBehaviour
{ 
    public static Action anyFacilityDie;
    public Action death;
    public float СurrentHealth;
    public float MaxHealth => _facilityDescription.MaxHealth;
    public float HealthPerRepairment;
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();
    private FacilityDescription _facilityDescription => GetComponent<FacilityDescription>();
    private float _damageReductionPercent => _facilityDescription.DamageReductionPercent;
    private bool _wasDamagedInThisTurn = false;

    void Start()
    {
        СurrentHealth = MaxHealth;
    }
    public void ApplyDamage(float _damage)
    { 
        СurrentHealth -= _damage * (100f - _damageReductionPercent) / 100;
        _wasDamagedInThisTurn = true;
        if (СurrentHealth <= 0)
        {
            Death();
        }
    }
    public void Repairment() {
        СurrentHealth = Mathf.Clamp(СurrentHealth + HealthPerRepairment, 0, MaxHealth); }
    private void Death()
    {
        _placementManager.gridWithObjectsInformation[_facilityDescription.LocalCoords.x, _facilityDescription.LocalCoords.y] = null;
        death?.Invoke();
        anyFacilityDie?.Invoke();
        Destroy(gameObject);
    }
}