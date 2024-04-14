using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Building : PlayableObjectDescription
{
    public Administratum Administratum;
    public bool WorkerOnSite = false;
    public int ActionsToFinalizeBuilding;
    public Dictionary<string, int> Storage = new Dictionary<string, int>();

    public float OreConsumption;
    public float WoodConsumption;
    public float LightConsumption;

    public float LightBuildingCost;
    public float FoodBuildingCost;
    public float OreBuildingCost;
    public float WoodBuildingCost;

    public void ResourcesConsumption() { 
        if (WorkerOnSite) { 
            Administratum.WasteResources(LightConsumption, OreConsumption, WoodConsumption, FoodConsumption); } }

    private void OnEnable() { TurnManager.onTurnChanged += ResourcesConsumption; }
    private void OnDisable() { TurnManager.onTurnChanged -= ResourcesConsumption; }
}
