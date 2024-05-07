using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitHealth : Health
{
    public static Action anyUnitDie;
    public float regenerationPercent; // Процент от макс здоровья, который будет восстанавливаться, когда юнит стоит и ничего не делает и не получает урон
    private UnitDescription _unitDescription;
    private UnitMovement _unitMovement;
    private UnitActions _unitActions;

    new private void Awake()
    {
        base.Awake();
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
    new void Start()
    {
        base.Start();
        TransformationHealthOnTurnChanged();
        currentHealth = _maxHealth;
    }

    private void TransformationHealthOnTurnChanged()
    {
        
        if (!_wasDamagedInThisTurn && _unitMovement.spentSpeed == 0 && _unitActions.remainingActionsCount != 0)
        {
            DefaultRegenerationPerTurn();
        }
        _wasDamagedInThisTurn = false;
    }
    private void DefaultRegenerationPerTurn()
    {
        currentHealth = Mathf.Clamp(currentHealth + _maxHealth * regenerationPercent / 100, 0, _maxHealth);
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
        _placementManager.gridWithObjectsInformation[_unitDescription.LocalCoords.x, _unitDescription.LocalCoords.y] = null;
        death?.Invoke();
        anyUnitDie?.Invoke();
        Destroy(gameObject);
    }


    private void InitComponentLinks()
    {
        _unitDescription = GetComponent<UnitDescription>();
        _unitMovement = GetComponent<UnitMovement>();
        _unitActions = GetComponent<UnitActions>();
        _maxHealth = _unitDescription.Health;
        damageReductionPercent = _unitDescription.DamageReductionPercent;
    }
}
