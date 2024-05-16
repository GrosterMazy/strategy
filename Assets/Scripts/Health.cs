using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public Action death;
    public float currentHealth;
    [NonSerialized] public float damageReductionPercent;
    protected float _maxHealth;
    protected PlacementManager _placementManager;
    protected ObjectOnGrid _objectOnGrid;
    protected bool _wasDamagedInThisTurn = false;
    protected void Awake()
    {
        InitComponentLinks();
    }
    protected void Start()
    {

    }


    public void ApplyDamage(float _damage)
    {
        currentHealth -= _damage * (100f - damageReductionPercent) / 100;
        _wasDamagedInThisTurn = true;
        IsDead();
    }
    public void ApplyDamageIgnoringArmour(float _damage)
    {
        currentHealth -= _damage;
        _wasDamagedInThisTurn = true;
        IsDead();
    }
    public void ApplyPercentageDamageOfMaxHealth(float percent)
    {
        ApplyDamageIgnoringArmour(_maxHealth * percent / 100);
    }
    public void ApplyPercentageDamageOfCurrentHealth(float percent)
    {
        ApplyDamageIgnoringArmour(_maxHealth * percent / 100);
    }
    public void ApplyPercentageDamageOfMissingHealth(float percent)
    {
        ApplyDamageIgnoringArmour((_maxHealth - currentHealth) * percent / 100);
    }

    public void ApplyHeal(float _heal)
    {
        currentHealth = Mathf.Clamp(currentHealth + _heal, 0, _maxHealth);
    }
    public void ApplyPercentageHealOfMaxHealth(float percent)
    {
        currentHealth = Mathf.Clamp(currentHealth + _maxHealth * percent / 100, 0, _maxHealth);
    }
    public void ApplyPercentageHealOfCurrentHealth(float percent)
    {
        currentHealth = Mathf.Clamp(currentHealth + currentHealth * percent / 100, 0, _maxHealth);
    }
    public void ApplyPercentageHealOfMissingHealth(float percent)
    {
        currentHealth = Mathf.Clamp(currentHealth + (_maxHealth - currentHealth) * percent / 100, 0, _maxHealth);
    }
    protected void IsDead()
    {
        if (currentHealth <= 0)
        {
            _placementManager.gridWithObjectsInformation[_objectOnGrid.LocalCoords.x, _objectOnGrid.LocalCoords.y] = null;
            death?.Invoke();
            Destroy(gameObject);
        }
    }


    private void InitComponentLinks()
    {
        _placementManager = FindObjectOfType<PlacementManager>();
        _objectOnGrid = GetComponent<ObjectOnGrid>();
    }
}
