using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class FirstFactionProductionBuildingDescription : FirstFactionFacilities
{
    public Administratum Administratum;

    public int LightBuildingFoundationCost;
    public int FoodBuildingFoundationCost;
    public int SteelBuildingFoundationCost;
    public int WoodBuildingFoundationCost;

    public int LightConstructionCost;
    public int FoodConstructionCost;
    public int SteelConstructionCost;
    public int WoodConstructionCost;

    protected TurnManager _turnManager;

    protected void Awake() {
        _turnManager = FindObjectOfType<TurnManager>();
    }

    public void ResourcesConsumption() { 
        if (WorkerOnSite) { 
            Administratum.WasteResources(LightConsumption, SteelConsumption, WoodConsumption, FoodConsumption); } }

    public void BuildingExpenses(string _typeOfExpense) {
        int _lightExpense; int _steelExpense; int _woodExpense; int _foodExpense;
        if (_typeOfExpense == "Foundation") {
            _lightExpense = LightBuildingFoundationCost; _steelExpense = SteelBuildingFoundationCost; _woodExpense = WoodBuildingFoundationCost; _foodExpense = FoodBuildingFoundationCost; }     
        else if (_typeOfExpense == "Construction") {
            _lightExpense = LightConstructionCost; _steelExpense = SteelConstructionCost; _woodExpense = WoodConstructionCost; _foodExpense = FoodConstructionCost; }
        else { throw new Exception("Unknown expense type"); }
        Administratum.WasteResources(_lightExpense, _steelExpense, _woodExpense, _foodExpense); }

    protected void OnEnable() {
        _turnManager.onTurnChanged += ResourcesConsumption;
    }
    protected void OnDisable() { _turnManager.onTurnChanged -= ResourcesConsumption; }
}
