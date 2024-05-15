using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Administratum : FirstFactionFacilities
{
    [SerializeField] private int InitialLight;
    [SerializeField] private int InitialOre;
    [SerializeField] private int InitialWood;
    [SerializeField] private int InitialFood;

    public void WasteResources(int _lightConsume, int _oreConsume, int _woodConsume, int _foodConsume) { 
        Storage["Light"] -= _lightConsume; Storage["Ore"] -= _oreConsume; Storage["Wood"] -= _woodConsume; Storage["Food"] -= _foodConsume; }

    private void Awake() { Storage.Add("Light", InitialLight); Storage.Add("Ore", InitialOre); Storage.Add("Wood", InitialWood); Storage.Add("Food", InitialFood); }
}
