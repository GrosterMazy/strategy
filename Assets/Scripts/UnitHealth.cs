using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitHealth : MonoBehaviour
{
    public static Action anyUnitDie;
    public Action death;
    public float regenerationPercent; // Процент от макс здоровья, который будет восстанавливаться, когда юнит стоит и ничего не делает и не получает урон
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();
    private PlayableObjectDescription _unitDescription => GetComponent<PlayableObjectDescription>();
    private UnitMovement _unitMovement => GetComponent<UnitMovement>();
    private float _damageReductionPercent;
    private UnitActions _unitActions => GetComponent<UnitActions>();
    private float _maxHealth;
    private bool _wasDamagedInThisTurn = false;
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
        _unitDescription.CurrentHealth = _maxHealth;
    }
    public void ApplyDamage(float _damage)
    { 
        _unitDescription.CurrentHealth -= _damage * (100f - _damageReductionPercent) / 100;
        _wasDamagedInThisTurn = true;
        if (_unitDescription.CurrentHealth <= 0)
        {
            Death();
        }
    }


    private void TransformationHealthOnTurnChanged()
    {
        _maxHealth = _unitDescription.MaxHealth;
        _damageReductionPercent = _unitDescription.DamageReductionPercent;
        _wasDamagedInThisTurn = false;
        if (!_wasDamagedInThisTurn && _unitMovement.spentSpeed == 0 && _unitActions.remainingActionsCount != 0)
        {
            DefaultRegenerationPerTurn();
        }
    }
    private void DefaultRegenerationPerTurn()
    {
        _unitDescription.CurrentHealth = Mathf.Clamp(_unitDescription.CurrentHealth + _maxHealth * regenerationPercent, 0, _maxHealth);
    }
    private void Death()
    {
        _placementManager.gridWithObjectsInformation[_unitDescription.LocalCoords.x, _unitDescription.LocalCoords.y] = null;
        death?.Invoke();
        anyUnitDie?.Invoke();
        Destroy(gameObject);
    }
}
