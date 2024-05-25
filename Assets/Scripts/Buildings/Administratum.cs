using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Administratum : FirstFactionFacilities
{
    [SerializeField] private int InitialLight;
    [SerializeField] private int InitialSteel;
    [SerializeField] private int InitialWood;
    [SerializeField] private int InitialFood;
    private GameObject _looseScreen;

    public void WasteResources(int _lightConsume, int _steelConsume, int _woodConsume, int _foodConsume) { 
        Storage["Light"] -= _lightConsume; Storage["Steel"] -= _steelConsume; Storage["Wood"] -= _woodConsume; Storage["Food"] -= _foodConsume; }

    new private void InitComponents() { _looseScreen = FindObjectOfType<LooseScreen>().gameObject; }

    private void LoseScreen() { _looseScreen.SetActive(true); }

    private void Awake() { Storage.Add("Light", InitialLight); Storage.Add("Steel", InitialSteel); Storage.Add("Wood", InitialWood); Storage.Add("Food", InitialFood); }

    new private void Start() { base.Start(); InitComponents(); _looseScreen.SetActive(false); }

    new private void Update() { base.Update(); if (Storage["Food"] <= 0) LoseScreen(); }

    private void OnDestroy() { LoseScreen(); } // При выключении игры возникает ошибка, потому что объекта _looseScreen уже нет, а мы пытаемся его включить
}
