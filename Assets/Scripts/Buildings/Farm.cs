using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : FirstFactionProductionBuildingDescription 
{
    [SerializeField] private int LightForce=3;
    protected new void Start() 
    {
        base.Start(); 
       _hexGrid.hexCells[LocalCoords.x, LocalCoords.y].GetComponent<LightTransporter>().SetLight(LightForce); 
    }
}
