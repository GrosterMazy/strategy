using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessUnitHealth : Health
{
    public float regeneration; // Здоровье, которое будет восстанавливаться, когда юнит находится во тьме
    public float damageOnLight; // Урон, который юнит будет получать на свету
    private UnitDescription _unitDescription;
    private HexGrid _hexGrid;
    new private void Awake()
    {
        base.Awake();
    }
    private void OnEnable()
    {
        _turnManager.onTurnChanged += RegenerationInDarknessOnTurnChange;
        _turnManager.onTurnChanged += GetDamageOnLight;
    }
    private void OnDisable()
    {
        _turnManager.onTurnChanged -= RegenerationInDarknessOnTurnChange;
        _turnManager.onTurnChanged -= GetDamageOnLight;
    }
    new protected void Start()
    {
        base.Start();
        InitComponentLinks();
        currentHealth = maxHealth;
    }

    private void RegenerationInDarknessOnTurnChange()
    {
        if (_hexGrid.hexCells[_objectOnGrid.LocalCoords.x, _objectOnGrid.LocalCoords.y].InDarkness)
        {
            ApplyHeal(regeneration);
        }
    }
    public void GetDamageOnLight()
    {
        if (!_hexGrid.hexCells[_objectOnGrid.LocalCoords.x, _objectOnGrid.LocalCoords.y].InDarkness)
        {
            ApplyDamageIgnoringArmour(damageOnLight);
        }
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
    protected void InitComponentLinks()
    {
        _unitDescription = GetComponent<UnitDescription>();
        _hexGrid = FindObjectOfType<HexGrid>();
        maxHealth = _unitDescription.Health;
        damageReductionPercent = _unitDescription.DamageReductionPercent;
    }
}