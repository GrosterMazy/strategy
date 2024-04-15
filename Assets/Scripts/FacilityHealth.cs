using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FacilityHealth : MonoBehaviour
{ 
    public static Action anyFacilityDie;
    public Action death;
    public float currentHealth;
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();
    private FacilityDescription _facilityDescription => GetComponent<FacilityDescription>();
    private float _damageReductionPercent => _facilityDescription.DamageReductionPercent;
    private float _maxHealth => _facilityDescription.MaxHealth;
    private bool _wasDamagedInThisTurn = false;

    void Start()
    {
        currentHealth = _maxHealth;
    }
    public void ApplyDamage(float _damage)
    { 
        currentHealth -= _damage * (100f - _damageReductionPercent) / 100;
        _wasDamagedInThisTurn = true;
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
}