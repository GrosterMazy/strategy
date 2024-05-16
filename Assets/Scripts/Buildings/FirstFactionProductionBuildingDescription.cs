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
    public int OreBuildingFoundationCost;
    public int WoodBuildingFoundationCost;

    public int LightConstructionCost;
    public int FoodConstructionCost;
    public int OreConstructionCost;
    public int WoodConstructionCost;

    public void ResourcesConsumption() { 
        if (WorkerOnSite) { 
            Administratum.WasteResources(LightConsumption, OreConsumption, WoodConsumption, FoodConsumption); } }

    public void BuildingExpenses(string _typeOfExpense) {
        int _lightExpense; int _oreExpense; int _woodExpense; int _foodExpense;
        if (_typeOfExpense == "Foundation") {
            _lightExpense = LightBuildingFoundationCost; _oreExpense = OreBuildingFoundationCost; _woodExpense = WoodBuildingFoundationCost; _foodExpense = FoodBuildingFoundationCost; }     
        else if (_typeOfExpense == "Construction") {
            _lightExpense = LightConstructionCost; _oreExpense = OreConstructionCost; _woodExpense = WoodConstructionCost; _foodExpense = FoodConstructionCost; }
        else { throw new Exception("Unknown expense type"); }
        Administratum.WasteResources(_lightExpense, _oreExpense, _woodExpense, _foodExpense); }

    private void OnEnable() { TurnManager.onTurnChanged += ResourcesConsumption; }
    private void OnDisable() { TurnManager.onTurnChanged -= ResourcesConsumption; }
}
