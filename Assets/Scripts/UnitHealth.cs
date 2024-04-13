using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    public float regenerationPercent; // Процент от макс здоровья, который будет восстанавливаться, когда юнит стоит и ничего не делает и не получает урон
    public float currentHealth;
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();
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
        regenerationPercent /= 100;
//        currentHealth = _maxHealth;
        TransformationHealthOnTurnChanged();
    }
    public void ApplyDamage(float _damage)
    { 
        currentHealth -= _damage * (100f - _damageReductionPercent);
        _wasDamagedInThisTurn = true;
        if (currentHealth <= 0)
        {
            Death();
        }
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
    private void Death()
    {
        _placementManager.gridWithObjectsInformation[_unitDescription.LocalCoords.x, _unitDescription.LocalCoords.y] = null;
        Destroy(gameObject);
    }
}
