using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Administratum : ObjectOnGrid
{
    public bool WorkerOnSite;
    public int TeamAffiliation;
    public float OverallLight;
    public float OverallOre;
    public float OverallWood;
    public float OverallFood;

    public void WasteResources(float _lightConsume, float _oreConsume, float _woodConsume, float _foodConsume) { 
        OverallLight -= _lightConsume; OverallOre -= _oreConsume; OverallWood -= _woodConsume; OverallFood -= _foodConsume; }       
}
