using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CollectableItem : ObjectOnGrid
{
    public int NumberOfItems;
    [NonSerialized] public float WeightOfOneItem;
    public string Name;
    public ObjectOnGrid _coordsOnGrid;
    private PlacementManager _placementManager;

    public void GoneAway() { _placementManager.UpdateGrid(LocalCoords, LocalCoords, _coordsOnGrid); } // Возвращение collectableitem'a в список всех предметов на сетке

    public void Taken(int _amount) { 
        NumberOfItems -= _amount;
        if (NumberOfItems <= 0) Destroy(gameObject); }

    private void InitComponents() { _coordsOnGrid = GetComponent<CollectableItem>(); _placementManager = FindObjectOfType<PlacementManager>(); WeightOfOneItem = ResourcesWeights.ResourcesWeightsPerItemTable[Name]; }

    private void Start() { InitComponents();
        if (NumberOfItems <= 0 || WeightOfOneItem <= 0 || Name == null) throw new System.Exception("Указаны недопустимые данные у подбираемого предмета"); }
}
