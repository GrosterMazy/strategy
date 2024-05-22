using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DarknessUnitSpawner : MonoBehaviour {
    [SerializeField] private int mobCap; // максимальное кол-во юнитов тьмы на карте
    [NonSerialized] public int mobCapFilled;

    [SerializeField] private DarknessUnitAI darknessUnitPrefab;

    [SerializeField] private int turnsToPowerup;
    [SerializeField] private int numberOfPowerups;
    [NonSerialized] public static Action onDarknessPowerup;

    [SerializeField] private int mobCapPowerup; // то, на сколько значение увеличится при усилении тьмы
    [SerializeField] private int maxMobCap = -1; // нет ограничения

    [SerializeField] private int unitActionsPowerup;
    [SerializeField] private int maxUnitActions = -1; // нет ограничения
    private int _additionalUnitActions = 0;

    [SerializeField] private int unitArmorPowerup;
    [SerializeField] private int maxUnitArmor = -1; // нет ограничения
    private int _additionalUnitArmor = 0;

    [SerializeField] private int unitSpeedPowerup;
    [SerializeField] private int maxUnitSpeed = -1; // нет ограничения
    private int _additionalUnitSpeed = 0;

    [SerializeField] private int unitAttackRangePowerup;
    [SerializeField] private int maxUnitAttackRange = -1; // нет ограничения
    private int _additionalUnitAttackRange = 0;

    [SerializeField] private int unitHealthPowerup;
    [SerializeField] private int maxUnitHealth = -1; // нет ограничения
    private int _additionalUnitHealth = 0;

    [SerializeField] private int unitAttackDamagePowerup;
    [SerializeField] private int maxUnitAttackDamage = -1; // нет ограничения
    private int _additionalUnitAttackDamage = 0;

    private HexGrid _hexGrid;
    private TurnManager _turnManager;
    private PlacementManager _placementManager;
    private int _turnCounter;

    private void Awake() {
        this._hexGrid = FindObjectOfType<HexGrid>();
        this._turnManager = FindObjectOfType<TurnManager>();
        this._placementManager = FindObjectOfType<PlacementManager>();

        this._turnCounter = 0;

        TurnManager.onTurnChanged += OnTurnChanged;
        DarknessUnitSpawner.onDarknessPowerup += OnDarknessPowerup;
    }

    private void OnDestroy() {
        TurnManager.onTurnChanged -= OnTurnChanged;
        DarknessUnitSpawner.onDarknessPowerup -= OnDarknessPowerup;
    }

    private void OnTurnChanged() {
        this._turnCounter++;

        if (this._turnCounter >= this.turnsToPowerup) {
            this._turnCounter -= this.turnsToPowerup;
            DarknessUnitSpawner.onDarknessPowerup?.Invoke();
        }

        if (this._turnManager.isDay) return;

        // заполняем mobCap
        int toSpawn = this.mobCap - this.mobCapFilled;

        Vector2Int pos;
        DarknessUnitAI darknessUnit;
        while (toSpawn > 0) {
            pos = new Vector2Int(
                UnityEngine.Random.Range(0, this._hexGrid.size.x),
                UnityEngine.Random.Range(0, this._hexGrid.size.y)
            );
            while (this._placementManager.gridWithObjectsInformation[pos.x, pos.y] != null
                    || !this._hexGrid.hexCells[pos.x, pos.y].InDarkness)
                pos = new Vector2Int(
                    UnityEngine.Random.Range(0, this._hexGrid.size.x),
                    UnityEngine.Random.Range(0, this._hexGrid.size.y)
                );
            
            darknessUnit = Instantiate(
                this.darknessUnitPrefab,
                this._hexGrid.pivots[pos.x, pos.y].transform.position,
                this.darknessUnitPrefab.transform.rotation
            );
            darknessUnit.LocalCoords = pos;
            this._placementManager.UpdateGrid(pos, pos, darknessUnit);

            darknessUnit.ActionsPerTurn += this._additionalUnitActions;
            darknessUnit.Armor += this._additionalUnitArmor;
            darknessUnit.MovementSpeed += this._additionalUnitSpeed;
            darknessUnit.AttackRange += this._additionalUnitAttackRange;
            darknessUnit.Health += this._additionalUnitHealth;
            darknessUnit.AttackDamage += this._additionalUnitAttackDamage;

            this.mobCapFilled++;

            toSpawn--;
        }
    }

    private void OnDarknessPowerup() {
        DarknessPowerup darknessPowerup;
        int powerupsRemaining = this.numberOfPowerups;

        while (powerupsRemaining > 0) {
            darknessPowerup = (DarknessPowerup)UnityEngine.Random.Range(
                (int)DarknessPowerup.MobCap,
                ((int)DarknessPowerup.UnitAttackDamage) + 1
            );

            switch (darknessPowerup) {
                case DarknessPowerup.MobCap:
                    if (this.maxMobCap != -1 && this.mobCap + this.mobCapPowerup > this.maxMobCap)
                        break;
                    this.mobCap += this.mobCapPowerup;

                    powerupsRemaining--;
                    break;
                case DarknessPowerup.UnitActions:
                    if (this.maxUnitActions != -1 && this._additionalUnitActions + this.unitActionsPowerup > this.maxUnitActions)
                        break;
                    this._additionalUnitActions += this.unitActionsPowerup;

                    powerupsRemaining--;
                    break;
                case DarknessPowerup.UnitArmor:
                    if (this.maxUnitArmor != -1 && this._additionalUnitArmor + this.unitArmorPowerup > this.maxUnitArmor)
                        break;
                    this._additionalUnitArmor += this.unitArmorPowerup;

                    powerupsRemaining--;
                    break;
                case DarknessPowerup.UnitSpeed:
                    if (this.maxUnitSpeed != -1 && this._additionalUnitSpeed + this.unitSpeedPowerup > this.maxUnitSpeed)
                        break;
                    this._additionalUnitSpeed += this.unitSpeedPowerup;

                    powerupsRemaining--;
                    break;
                case DarknessPowerup.UnitAttackRange:
                    if (this.maxUnitAttackRange != -1 && this._additionalUnitAttackRange + this.unitAttackRangePowerup > this.maxUnitAttackRange)
                        break;
                    this._additionalUnitAttackRange += this.unitAttackRangePowerup;

                    powerupsRemaining--;
                    break;
                case DarknessPowerup.UnitHealth:
                    if (this.maxUnitHealth != -1 && this._additionalUnitHealth + this.unitHealthPowerup > this.maxUnitHealth)
                        break;
                    this._additionalUnitHealth += this.unitHealthPowerup;

                    powerupsRemaining--;
                    break;
                case DarknessPowerup.UnitAttackDamage:
                    if (this.maxUnitAttackDamage != -1 && this._additionalUnitAttackDamage + this.unitAttackDamagePowerup > this.maxUnitAttackDamage)
                        break;
                    this._additionalUnitAttackDamage += this.unitAttackDamagePowerup;

                    powerupsRemaining--;
                    break;
            }
        }
    }
}
