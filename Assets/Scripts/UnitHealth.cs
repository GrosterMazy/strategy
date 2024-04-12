using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    public float regenerationPercent; // Процент от макс здоровья, который будет восстанавливаться, когда юнит стоит и ничего не делает и не получает урон
    public float currentHealth;
    private UnitDescription _unitDescription => GetComponent<UnitDescription>();
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
        TransformationHealthOnTurnChanged();
    }
    public void ApplyDamage(float _damage)
    { 
        currentHealth -= _damage * (100f - _damageReductionPercent);
    }


    private void TransformationHealthOnTurnChanged()
    {
        _maxHealth = _unitDescription.Health;
        _damageReductionPercent = _unitDescription.DamageReductionPercent;
        if (!_wasDamagedInThisTurn || _unitMovement.spentSpeed != 0 || _unitActions.remainingActionsCount != 0)
        {
            DefaultRegenerationPerTurn();
        }
    }
    private void DefaultRegenerationPerTurn()
    {
        currentHealth = Mathf.Clamp(currentHealth + _maxHealth * regenerationPercent, 0, _damageReductionPercent);
    }
}
