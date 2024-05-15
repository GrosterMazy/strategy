using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitHealth : MonoBehaviour
{
    public static Action anyUnitDie;
    public Action death;
    public float regenerationPercent; // Процент от макс здоровья, который будет восстанавливаться, когда юнит стоит и ничего не делает и не получает урон
    public float currentHealth;
    private PlacementManager _placementManager;
    private UnitDescription _unitDescription;
    private UnitMovement _unitMovement;
    private float _damageReductionPercent;
    private UnitActions _unitActions;
    private float _maxHealth;
    private bool _wasDamagedInThisTurn = false;

    private void Awake()
    {
        InitComponentLinks();
    }

    private void OnEnable()
    {
        TurnManager.onTurnChanged += TransformationHealthOnTurnChanged;
    }
    private void OnDisable()
    {
        TurnManager.onTurnChanged -= TransformationHealthOnTurnChanged;
    }
    void Start()
    {
        regenerationPercent /= 100;
        TransformationHealthOnTurnChanged();
        currentHealth = _maxHealth;
    }
    public void ApplyDamage(float _damage)
    { 
        currentHealth -= _damage * (100f - _damageReductionPercent) / 100;
        _wasDamagedInThisTurn = true;
        IsDead();
    }
    public void ApplyPercentageDamageOfMaxHealth(float percent)
    {
        currentHealth -= _maxHealth * percent / 100;
        _wasDamagedInThisTurn = true;
        IsDead();
    }
    public void ApplyPercentageDamageOfCurrentHealth(float percent)
    {
        currentHealth -= _maxHealth * percent / 100;
        _wasDamagedInThisTurn = true;
        IsDead();
    }
    public void ApplyPercentageDamageOfMissingHealth(float percent)
    {
        currentHealth -= (_maxHealth - currentHealth) * percent / 100;
        _wasDamagedInThisTurn = true;
        IsDead();
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


    private void TransformationHealthOnTurnChanged()
    {
        _maxHealth = _unitDescription.Health;
        _damageReductionPercent = _unitDescription.DamageReductionPercent;
        _wasDamagedInThisTurn = false;
        if (!_wasDamagedInThisTurn && _unitMovement.spentSpeed == 0 && _unitActions.remainingActionsCount != 0)
        {
            DefaultRegenerationPerTurn();
        }
    }
    private void DefaultRegenerationPerTurn()
    {
        currentHealth = Mathf.Clamp(currentHealth + _maxHealth * regenerationPercent, 0, _maxHealth);
    }

    private void IsDead()
    {
        if (currentHealth <= 0)
        {
            Death();
        }
    }
    private void Death()
    {
        _placementManager.gridWithObjectsInformation[_unitDescription.LocalCoords.x, _unitDescription.LocalCoords.y] = null;
        death?.Invoke();
        anyUnitDie?.Invoke();
        Destroy(gameObject);
    }


    private void InitComponentLinks()
    {
        _placementManager = FindObjectOfType<PlacementManager>();
        _unitDescription = GetComponent<UnitDescription>();
        _unitMovement = GetComponent<UnitMovement>();
        _unitActions = GetComponent<UnitActions>();
    }
}
