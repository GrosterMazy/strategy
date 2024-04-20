using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class FirstFactionProductionBuildingDescription : FirstFactionFacilities
{
    public Administratum Administratum;

    public float LightBuildingFoundationCost;
    public float FoodBuildingFoundationCost;
    public float OreBuildingFoundationCost;
    public float WoodBuildingFoundationCost;

    public float LightConstructionCost;
    public float FoodConstructionCost;
    public float OreConstructionCost;
    public float WoodConstructionCost;

    public void ResourcesConsumption() { 
        if (WorkerOnSite) { 
            Administratum.WasteResources(LightConsumption, OreConsumption, WoodConsumption, FoodConsumption); } }

    public void BuildingExpenses(string _typeOfExpense) {
        float _lightExpense; float _oreExpense; float _woodExpense; float _foodExpense;
        if (_typeOfExpense == "Foundation") {
            _lightExpense = LightBuildingFoundationCost; _oreExpense = OreBuildingFoundationCost; _woodExpense = WoodBuildingFoundationCost; _foodExpense = FoodBuildingFoundationCost; }     
        else if (_typeOfExpense == "Construction") {
            _lightExpense = LightConstructionCost; _oreExpense = OreConstructionCost; _woodExpense = WoodConstructionCost; _foodExpense = FoodConstructionCost; }
        else { throw new Exception("Unknown expense type"); }
        Administratum.WasteResources(_lightExpense, _oreExpense, _woodExpense, _foodExpense); }

    private void OnEnable() { TurnManager.onTurnChanged += ResourcesConsumption; }
    private void OnDisable() { TurnManager.onTurnChanged -= ResourcesConsumption; }
}
