using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitHealth : Health
{
    public float regenerationPercent; // Процент от макс здоровья, который будет восстанавливаться, когда юнит стоит и ничего не делает и не получает урон
    private UnitDescription _unitDescription;
    private UnitMovement _unitMovement;
    private UnitActions _unitActions;


    new private void Awake()
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
        TransformationHealthOnTurnChanged();
        currentHealth = maxHealth;
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
        currentHealth = Mathf.Clamp(currentHealth + maxHealth * regenerationPercent / 100, 0, maxHealth);
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
        _placementManager.gridWithObjectsInformation[_unitDescription.LocalCoords.x, _unitDescription.LocalCoords.y] = null;
        death?.Invoke();
        EventBus.anyUnitDie?.Invoke();
        Destroy(gameObject);
    }


    private void InitComponentLinks()
    {
        _unitDescription = GetComponent<UnitDescription>();
        _unitMovement = GetComponent<UnitMovement>();
        _unitActions = GetComponent<UnitActions>();
        maxHealth = _unitDescription.Health;
        damageReductionPercent = _unitDescription.DamageReductionPercent;
    }
}
