using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Administratum : FirstFactionFacilities
{
    [SerializeField] private int InitialLight;
    [SerializeField] private int InitialSteel;
    [SerializeField] private int InitialWood;
    [SerializeField] private int InitialFood;

    public void WasteResources(int _lightConsume, int _steelConsume, int _woodConsume, int _foodConsume) { 
        Storage["Light"] -= _lightConsume; Storage["Steel"] -= _steelConsume; Storage["Wood"] -= _woodConsume; Storage["Food"] -= _foodConsume; }

    private void Awake() { Storage.Add("Light", InitialLight); Storage.Add("Steel", InitialSteel); Storage.Add("Wood", InitialWood); Storage.Add("Food", InitialFood); }
}
